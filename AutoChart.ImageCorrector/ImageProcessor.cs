﻿using ImageMagick;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.ImageCorrector
{
    class ImageProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void CorrectStillsPerspectiveAndCrop(CommandLineOptions options)
        {
            string inputDirectoryPath = options.InputDirectoryPath;
            string outputDirectoryPath = options.OutputDirectoryPath;
            int skipFrameCount = options.SkipFramesCount;
            int takeFrameCount = options.TakeFramesCount;

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            Logger.Info($"Processing '{inputDirectoryPath}'");
            string[] inputFilePaths = Directory.GetFiles(inputDirectoryPath);

            int inputFileCount = inputFilePaths.Length;
            Logger.Info($"Correcting {inputFileCount} images");

            int imageIndex = 0;
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

                string inputFileName = Path.GetFileName(inputFilePath);
                string inputFileExtension = Path.GetExtension(inputFileName);

                string outputFileName = inputFileName.Substring(0, inputFileName.IndexOf('.')) + ".corrected" + inputFileExtension;
                string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);

                Logger.Info($"{imageIndex++}/{inputFileCount}: {outputFilePath}");

                using (MagickImage image = new MagickImage(inputFilePath))
                {
                    image.VirtualPixelMethod = VirtualPixelMethod.Tile;
                    image.MatteColor = Color.DodgerBlue;

                    int blX1 = 405;
                    int blY1 = 571;
                    int blX2 = 405;
                    int blY2 = 571;

                    int tlX1 = 513;
                    int tlY1 = 305;
                    int tlX2 = 405;
                    int tlY2 = 305;

                    int trX1 = 768;
                    int trY1 = 305;
                    int trX2 = 879;
                    int trY2 = 305;

                    int brX1 = 879;
                    int brY1 = 571;
                    int brX2 = 879;
                    int brY2 = 571;

                    image.Distort(DistortMethod.Perspective, new double[]
                    {
                        blX1, blY1, blX2, blY2,
                        tlX1, tlY1, tlX2, tlY2,
                        trX1, trY1, trX2, trY2,
                        brX1, brY1, brX2, brY2,
                    });

                    int cropX = tlX2;
                    int cropY = tlY2;
                    int cropWidth = brX2 - tlX2;
                    int cropHeight = brY2 - tlY2;

                    MagickGeometry geometry = new MagickGeometry(cropX, cropY, cropWidth, cropHeight);
                    image.Crop(geometry);

                    image.Write(outputFilePath);
                }
            }
        }
    }
}
