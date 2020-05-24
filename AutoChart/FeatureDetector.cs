using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart
{
    class FeatureDetector
    {
        class ColumnSample
        {
            public string Column { get; set; }
            public int Row { get; set; }
            public int Gray { get; set; }
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }
            public double Hue { get; set; }
            public double Saturation { get; set; }
            public double Brightness { get; set; }
        }

        class Feature
        {
            public string FeatureType { get; set; }
            public int FeatureLocation { get; set; }
            public List<ColumnSample> Samples { get; set; } = new List<ColumnSample>();
        }

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

        public void DetectFeaturesInSamples(string[] args)
        {
            // DetectFeaturesInSamples "C:\Users\hipto\Desktop\NewKidInSchool\samples" "C:\Users\hipto\Desktop\NewKidInSchool\features"
            string inputDirectoryPath = args[0];
            string outputDirectoryPath = args[1];

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            string[] inputFilePaths = Directory.GetFiles(inputDirectoryPath);
            foreach (string inputFilePath in inputFilePaths)
            {
                Console.WriteLine($"Processing '{inputFilePath}'");

                using (StreamReader reader = new StreamReader(inputFilePath))
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    List<ColumnSample> allSamples = csv.GetRecords<ColumnSample>().ToList();

                    // Detect features for each column
                    foreach (string columnName in ColumnNames)
                    {
                        List<ColumnSample> columnSamples = allSamples.Where(x => x.Column == columnName).ToList();
                        List<Feature> columnFeatures = DetectFeatures(columnSamples);

                        foreach (Feature feature in columnFeatures)
                        {
                            int rowCutoff = 30;

                            // Ignore features near the edges of the frame, since they can be distorted and unreliable
                            if (feature.FeatureLocation < rowCutoff || feature.FeatureLocation > (columnSamples.Last().Row - rowCutoff))
                            {
                                continue;
                            }

                            Console.WriteLine($"{columnName}, {feature.FeatureLocation} {feature.FeatureType} - #: {feature.Samples.Count}; max: {feature.Samples.Max(x => x.Gray)} [{feature.Samples.First().Row} - {feature.Samples.Last().Row}]");
                        }
                    }
                }
            }
        }

        private List<Feature> DetectFeatures(List<ColumnSample> columnSamples)
        {
            List<Feature> columnFeatures = new List<Feature>();
            Feature currentFeature = new Feature();

            foreach (ColumnSample columnSample in columnSamples)
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
            while (true)
            {
                ClassifyFeatures(columnFeatures);

                List<Feature> unclassifiedFeatures = columnFeatures.Where(x => string.IsNullOrEmpty(x.FeatureType)).ToList();
                if (!unclassifiedFeatures.Any())
                {
                    break;
                }

                foreach (Feature unclassifiedFeature in unclassifiedFeatures)
                {
                    const int trimCount = 8;

                    int sampleCount = unclassifiedFeature.Samples.Count;

                    // Find the min value in the feature and split on it
                    // (dropping the samples at the start and end, since they may be smaller than the min value)
                    int minValue = unclassifiedFeature.Samples.Skip(trimCount).Take(sampleCount - trimCount).Min(x => x.Gray);

                    List<ColumnSample> beforeSamples = unclassifiedFeature.Samples.Take(trimCount).TakeWhile(x => x.Gray != minValue).ToList();
                    List<ColumnSample> afterSamples = unclassifiedFeature.Samples.Skip(trimCount).SkipWhile(x => x.Gray != minValue).ToList();

                    // Update the original feature with the first half of the original sample list
                    unclassifiedFeature.Samples = beforeSamples;

                    // Insert a new feature after the original feature with the second half of the original sample list
                    int unclassifiedFeatureIndex = columnFeatures.IndexOf(unclassifiedFeature);
                    columnFeatures.Insert(unclassifiedFeatureIndex + 1, new Feature { Samples = afterSamples });
                }
            }

            LocateFeatures(columnFeatures);
        }

        private void ClassifyFeatures(List<Feature> columnFeatures)
        {
            List<Feature> clumpedFeatures = new List<Feature>();

            // Classify features based on the number of samples, which is how large it is
            // NOTE: Could also take max value into account, but not doing that now
            foreach (Feature columnFeature in columnFeatures)
            {
                // NOTE: These thresholds are tied very tightly to the image size
                if (columnFeature.Samples.Count < 5)
                {
                    columnFeature.FeatureType = "HalfBeat";
                }
                else if (columnFeature.Samples.Count < 10)
                {
                    columnFeature.FeatureType = "FullBeat";
                }
                else if (columnFeature.Samples.Count < 20)
                {
                    columnFeature.FeatureType = "KickNote";
                }
                else if (columnFeature.Samples.Count < 40)
                {
                    columnFeature.FeatureType = "HandNote";
                }
                else
                {
                    // Too many samples, so notes must be clumped together -- leave them unclassified
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
