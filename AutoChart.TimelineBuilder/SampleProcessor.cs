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

namespace AutoChart.TimelineBuilder
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

        private StringEnumConverter StringEnumConverter { get; set; } = new StringEnumConverter();

        public void BuildTimeline(CommandLineOptions options)
        {
            string inputDirectoryPath = options.InputDirectoryPath;
            string outputFilePath = options.OutputFilePath;
            int skipFrameCount = options.SkipFramesCount;
            int takeFrameCount = options.TakeFramesCount;

            // TODO: Would be great to try and auto-calculate these values from the frames
            int beatIntervalInPixels = options.BeatIntervalInPixels;
            double frameIntervalInSeconds = options.FrameIntervalInSeconds;
            double beatsPerMinute = options.BeatsPerMinute;
            int divisionsPerBeat = options.DivisionsPerBeat;

            // Precision is not required here, since it's just a guide to put us in the right general area
            int frameIntervalInPixels = (int)((beatsPerMinute / 60d) * frameIntervalInSeconds * beatIntervalInPixels);

            Logger.Info($"Calculated frameIntervalInPixels: {frameIntervalInPixels}");

            FeatureDetector featureDetector = new FeatureDetector();

            // Timeline for each lane in notes (based on divisions per beat, assuming constant 4/4 time)
            Dictionary<string, List<bool>> NoteTimeline = new Dictionary<string, List<bool>>();
            foreach (string columnName in ColumnNames)
            {
                NoteTimeline[columnName] = new List<bool>();
            }
            NoteTimeline["Kick"] = new List<bool>();


            int nextNoteLocation = 0;
            bool isSynchronized = false;

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
                    
                    Dictionary<string, List<Feature>> columnFeatures = new Dictionary<string, List<Feature>>();
                    foreach (string columnName in ColumnNames)
                    {
                        List<Sample> columnSamples = samples.Where(x => x.Column == columnName).ToList();
                        columnFeatures[columnName] = featureDetector.DetectFeaturesInSamples(columnSamples);
                    }

                    if (!isSynchronized)
                    {
                        // If this is the first frame, we need to orient ourselves to the beat
                        // Look for a full beat marker in the first (red) column and jump in!

                        Feature feature = columnFeatures[ColumnNames.First()].Where(x => x.FeatureType == FeatureType.FullBeat).LastOrDefault();
                        if (feature == null)
                        {
                            throw new Exception($"Unable to find {nameof(FeatureType.FullBeat)} during synchronization");
                        }

                        nextNoteLocation = feature.EffectiveLocation;
                        isSynchronized = true;
                    }
                    else
                    {
                        nextNoteLocation += frameIntervalInPixels;
                    }

                    // Features about this point can be unreliable
                    const int upperLocationCutoff = 100;

                    // Allow the actual feature to be offset slightly
                    const int featureLocationTolerance = 15;

                    while (nextNoteLocation > upperLocationCutoff)
                    {
                        int totalNoteLocation = 0;

                        bool isKickNote = false;
                        foreach (string columnName in ColumnNames)
                        {
                            Feature feature = columnFeatures[columnName].LastOrDefault(x => Math.Abs(x.EffectiveLocation - nextNoteLocation) < featureLocationTolerance);

                            NoteTimeline[columnName].Add(feature != null && feature.FeatureType == FeatureType.StickNote);
                            isKickNote |= (feature != null && feature.FeatureType == FeatureType.KickNote);

                            totalNoteLocation += (feature != null ? feature.EffectiveLocation : nextNoteLocation);
                        }

                        NoteTimeline["Kick"].Add(isKickNote);

                        // Account for drift and cumulative error by following actual locations where possible
                        int actualNoteLocation = totalNoteLocation / (ColumnNames.Count);
                        Console.WriteLine($"{Path.GetFileName(inputFilePath)} @ {nextNoteLocation} / {actualNoteLocation}: {NoteTimeline["Red"].Last():10} {NoteTimeline["Yellow"].Last():10} {NoteTimeline["Blue"].Last():10} {NoteTimeline["Green"].Last():10} {NoteTimeline["Kick"].Last():10}");

                        nextNoteLocation = actualNoteLocation;
                        nextNoteLocation -= (beatIntervalInPixels / divisionsPerBeat);
                    }
                }
            }

            string jsonText = JsonConvert.SerializeObject(NoteTimeline, Formatting.Indented, StringEnumConverter);
            File.WriteAllText(outputFilePath, jsonText);
        }
    }
}
