using AutoChart.Common;
using CsvHelper;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.FeatureDetector
{
    class SampleProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // TODO: Move to a shared enum
        private List<string> ColumnNames { get; } = new List<string>
        {
            "Red",
            "Yellow",
            "Blue",
            "Green"
        };

        // To be treated as part of a feature, a sample must be this tall
        private int FeatureGrayThreshold { get; set; } = 40;

        // To treat the group of samples as a feature, at least one of them must be this tall
        private int FeatureMaxGrayThreshold { get; set; } = 50;

        public void DetectFeaturesInSamples(CommandLineOptions options)
        {
            string inputDirectoryPath = options.InputDirectoryPath;
            string outputDirectoryPath = options.OutputDirectoryPath;
            int skipFrameCount = options.SkipFramesCount;
            int takeFrameCount = options.TakeFramesCount;

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            string[] inputFilePaths = Directory.GetFiles(inputDirectoryPath);
            foreach (string inputFilePath in inputFilePaths)
            {
                // Allow subsetting the input frames
                if (skipFrameCount > 0)
                {
                    skipFrameCount--;
                    continue;
                }

                if (takeFrameCount > 0)
                {
                    takeFrameCount--;
                }
                else
                {
                    break;
                }

                Logger.Info($"Processing '{inputFilePath}'");

                using (StreamReader reader = new StreamReader(inputFilePath))
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    List<Sample> samples = csv.GetRecords<Sample>().ToList();
                    Dictionary<string, List<Feature>> features = new Dictionary<string, List<Feature>>();

                    // Detect features for each column
                    foreach (string columnName in ColumnNames)
                    {
                        List<Sample> columnSamples = samples.Where(x => x.Column == columnName).ToList();
                        List<Feature> detectedFeatures = DetectFeatures(columnSamples);
                        List<Feature> filteredFeatures = new List<Feature>();
                        List<Feature> removedFeatures = new List<Feature>();

                        foreach (Feature detectedFeature in detectedFeatures)
                        {
                            int rowCutoff = 30;

                            Logger.Info($"{columnName}, {detectedFeature.FeatureLocation} {detectedFeature.FeatureType} - #: {detectedFeature.Samples.Count}; max: {detectedFeature.Samples.Max(x => x.Gray)} [{detectedFeature.Samples.First().Row} - {detectedFeature.Samples.Last().Row}]");

                            // Ignore features near the edges of the frame, since they can be distorted and unreliable
                            if (detectedFeature.FeatureLocation < rowCutoff || detectedFeature.FeatureLocation > (columnSamples.Last().Row - rowCutoff))
                            {
                                Logger.Info("Ignoring feature near the edges");
                                removedFeatures.Add(detectedFeature);
                            }
                            else if (detectedFeature.FeatureType == "Unknown")
                            {
                                Logger.Info("Ignoring feature that could not be identified");
                                removedFeatures.Add(detectedFeature);
                            }
                            else
                            {
                                filteredFeatures.Add(detectedFeature);
                            }
                        }

                        features[columnName] = filteredFeatures;
                    }

                    string inputFileName = Path.GetFileName(inputFilePath);
                    string inputFileExtension = Path.GetExtension(inputFileName);

                    string outputFileName = inputFileName.Substring(0, inputFileName.IndexOf('.')) + ".features.json";
                    string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);

                    string jsonText = JsonConvert.SerializeObject(features, Formatting.Indented);
                    File.WriteAllText(outputFilePath, jsonText);
                }
            }
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
                    columnFeature.FeatureType = "HalfBeat";
                }
                else if (featureSampleCount < 10)
                {
                    columnFeature.FeatureType = "FullBeat";
                }
                else if (featureSampleCount < 20)
                {
                    // Some features are wider than they should be, due to noise in the background
                    // Use the max value to help adjust classification
                    if (featureMaxValue > 100)
                    {
                        columnFeature.FeatureType = "KickNote";
                    }
                    else
                    {
                        columnFeature.FeatureType = "FullBeat";
                    }
                }
                else if (featureSampleCount < 40)
                {
                    // Some features are wider than they should be, due to noise in the background
                    // Use the max value to help adjust classification
                    if (featureMaxValue > 100)
                    {
                        columnFeature.FeatureType = "HandNote";
                    }
                    else
                    {
                        columnFeature.FeatureType = "Unknown";
                        Logger.Warn("Looks like a hand note, but does not meet the max value threshold");
                    }
                }
                else
                {
                    columnFeature.FeatureType = "Unknown";
                    Logger.Warn("Large sample count in the feature suggests notes are probably lumped together");
                }
            }
        }

        private void LocateFeatures(List<Feature> columnFeatures)
        {
            // Calculate the effective location of the feature
            // Based on a simple "center of mass" calculation
            // NOTE: Could account for the locaiton of the value peak, but not doing that now
            foreach (Feature columnFeature in columnFeatures)
            {
                columnFeature.FeatureLocation = (int)Math.Round(columnFeature.Samples.Average(x => x.Row), 0);
            }
        }
    }
}
