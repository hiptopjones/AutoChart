
using AutoChart.Common;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.TimelineBuilder
{
    class FeatureDetector
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // To be treated as part of a feature, a sample must be this tall
        private int FeatureGrayThreshold { get; set; } = 40;

        // To treat the group of samples as a feature, at least one of them must be this tall
        private int FeatureMaxGrayThreshold { get; set; } = 50;

        public List<Feature> DetectFeaturesInSamples(List<Sample> columnSamples)
        {
            List<Feature> detectedFeatures = DetectFeatures(columnSamples);
            List<Feature> filteredFeatures = new List<Feature>();
            List<Feature> removedFeatures = new List<Feature>();

            foreach (Feature detectedFeature in detectedFeatures)
            {
                int topSampleCutoff = 100;
                int bottomSampleCutoff = 20;

                //Logger.Info($"{columnName}, {detectedFeature.EffectiveRow} {detectedFeature.FeatureType} - #: {detectedFeature.Samples.Count}; max: {detectedFeature.Samples.Max(x => x.Gray)} [{detectedFeature.Samples.First().Row} - {detectedFeature.Samples.Last().Row}]");

                // Ignore features toward the edges of the frame, since they can be distorted, cut-off and unreliable
                if (detectedFeature.EffectiveLocation < topSampleCutoff || detectedFeature.EffectiveLocation > (columnSamples.Last().Row - bottomSampleCutoff))
                {
                    //Logger.Info("Ignoring feature near the edges");
                    removedFeatures.Add(detectedFeature);
                }
                else if (detectedFeature.FeatureType == FeatureType.Unknown)
                {
                    //Logger.Info("Ignoring feature that could not be identified");
                    removedFeatures.Add(detectedFeature);
                }
                else
                {
                    filteredFeatures.Add(detectedFeature);
                }
            }

            return filteredFeatures;
        }

        private List<Feature> DetectFeatures(List<Sample> columnSamples)
        {
            List<Feature> columnFeatures = new List<Feature>();
            Feature currentFeature = new Feature();

            foreach (Sample columnSample in columnSamples)
            {
                if (columnSample.Gray > FeatureGrayThreshold)
                {
                    if (currentFeature.Samples.Count > 0)
                    {
                        // If not contiguous, start a new feature
                        if (currentFeature.Samples.Last().Row < (columnSample.Row - 1))
                        {
                            // At least one sample must be over some threshold to actually count the whole set as a feature
                            if (currentFeature.Samples.Max(x => x.Gray) > FeatureMaxGrayThreshold)
                            {
                                columnFeatures.Add(currentFeature);
                            }

                            currentFeature = new Feature();
                        }
                    }

                    currentFeature.Samples.Add(columnSample);
                }
            }

            // Capture any in-progress feature before we return
            if (currentFeature.Samples.Count > 0)
            {
                // At least one sample must be over some threshold to actually count the whole set as a feature
                if (currentFeature.Samples.Max(x => x.Gray) > FeatureMaxGrayThreshold)
                {
                    columnFeatures.Add(currentFeature);
                }
            }

            ProcessFeatures(columnFeatures);
            return columnFeatures;
        }

        private void ProcessFeatures(List<Feature> columnFeatures)
        {
            ClassifyFeatures(columnFeatures);
            LocateFeatures(columnFeatures);
        }

        private void ClassifyFeatures(List<Feature> columnFeatures)
        {
            List<Feature> clumpedFeatures = new List<Feature>();

            // Classify features based on the number of samples
            foreach (Feature columnFeature in columnFeatures)
            {
                int featureSampleCount = columnFeature.Samples.Count;
                double featureMaxValue = columnFeature.Samples.Max(x => x.Gray);

                // NOTE: These thresholds are tied very tightly to the image size
                // TODO: Make these thresholds configurable on the command-line
                if (featureSampleCount < 5)
                {
                    columnFeature.FeatureType = FeatureType.OffBeat;
                }
                else if (featureSampleCount < 10)
                {
                    columnFeature.FeatureType = FeatureType.FullBeat;
                }
                else if (featureSampleCount < 20)
                {
                    // Some features are wider than they should be, due to noise in the background
                    // Use the max value to help adjust classification
                    if (featureMaxValue > 100)
                    {
                        columnFeature.FeatureType = FeatureType.KickNote;
                    }
                    else
                    {
                        columnFeature.FeatureType = FeatureType.FullBeat;
                    }
                }
                else
                {
                    columnFeature.FeatureType = FeatureType.StickNote;
                }
            }
        }

        private void LocateFeatures(List<Feature> columnFeatures)
        {
            // Determine the effective location of the feature
            foreach (Feature columnFeature in columnFeatures)
            {
                switch (columnFeature.FeatureType)
                {
                    // The effective location of these items is basically the maximum brightness point
                    case FeatureType.OffBeat:
                    case FeatureType.FullBeat:
                    case FeatureType.KickNote:
                        columnFeature.EffectiveLocation = columnFeature.Samples.First(x => x.Gray == columnFeature.Samples.Max(y => y.Gray)).Row;
                        break;

                    // The effective location of a stick note is near the bottom of the sprite
                    case FeatureType.StickNote:
                        columnFeature.EffectiveLocation = columnFeature.Samples.Skip(columnFeature.Samples.Count - 7).First().Row;
                        break;

                    // The effective location of unknown items is the physical center of the feature
                    case FeatureType.Unknown:
                        columnFeature.EffectiveLocation = (int)Math.Round(columnFeature.Samples.Average(x => x.Row), 0);
                        break;

                    default:
                        throw new Exception($"Unexpected feature type: {columnFeature.FeatureType}");
                }
            }
        }
    }
}