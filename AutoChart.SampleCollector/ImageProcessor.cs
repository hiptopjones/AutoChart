using AutoChart.Common;
using CsvHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.SampleCollector
{
    class ImageProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        const int redLeft = 40;
        const int redRight = 90;
        const int yellowLeft = 145;
        const int yellowRight = 210;
        const int blueLeft = 260;
        const int blueRight = 325;
        const int greenLeft = 380;
        const int greenRight = 430;

        List<int> sampleColumns = new List<int>
        {
            (redLeft + redRight) / 2,
            (yellowLeft + yellowRight) / 2,
            (blueLeft + blueRight) / 2,
            (greenLeft + greenRight) / 2,
        };

        List<string> sampleNames = new List<string>
        {
            "Red",
            "Yellow",
            "Blue",
            "Green"
        };

        public void SampleNoteColumnsInStills(CommandLineOptions options)
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
                Bitmap bitmap = new Bitmap(inputFilePath);

                List<Sample> samples = new List<Sample>();
                for (int sampleIndex = 0; sampleIndex < sampleColumns.Count; sampleIndex++)
                {
                    int x = sampleColumns[sampleIndex];

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);

                        samples.Add(new Sample
                        {
                            Column = sampleNames[sampleIndex],
                            Row = y,
                            Gray = (pixel.R + pixel.G + pixel.B) / 3,
                            Red = pixel.R,
                            Green = pixel.G,
                            Blue = pixel.B,
                            Hue = pixel.GetHue(),
                            Saturation = pixel.GetSaturation(),
                            Brightness = pixel.GetBrightness(),
                            Luminence = 0.2126 * pixel.R + 0.7152 * pixel.G + 0.0722 * pixel.B
                        });
                    }
                }

                string inputFileName = Path.GetFileName(inputFilePath);
                string outputFileName = inputFileName.Substring(0, inputFileName.IndexOf('.')) + ".samples.csv";
                string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);

                using (var writer = new StreamWriter(outputFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(samples);
                }
            }
        }
    }
}
