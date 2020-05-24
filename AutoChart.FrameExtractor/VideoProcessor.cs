using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.FrameExtractor
{
    class VideoProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void ConvertVideoToStills(CommandLineOptions options)
        {
            string inputFilePath = options.InputFilePath;
            string outputDirectoryPath = options.OutputDirectoryPath;
            double frameIntervalInSeconds = Convert.ToDouble(options.FrameIntervalInSeconds);

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            using (Engine engine = new Engine())
            {
                MediaFile inputMediaFile = new MediaFile { Filename = inputFilePath };
                engine.GetMetadata(inputMediaFile);

                double totalDurationInSeconds = inputMediaFile.Metadata.Duration.TotalSeconds;
                int totalFrameCount = (int)(totalDurationInSeconds / frameIntervalInSeconds);

                Logger.Info($"Extracting {totalFrameCount} individual frames");

                int frameIndex = 0;
                double i = 0;
                while (i < inputMediaFile.Metadata.Duration.TotalSeconds)
                {
                    long sequenceId = (long)(i * 1000);
                    string outputFilePath = Path.Combine(outputDirectoryPath, $"frame-{sequenceId:0000000}.jpg");
                    Logger.Info($"{frameIndex++}/{totalFrameCount}: {outputFilePath}");

                    MediaFile outputMediaFile = new MediaFile { Filename = outputFilePath };
                    ConversionOptions conversionOptions = new ConversionOptions { Seek = TimeSpan.FromSeconds(i) };
                    engine.GetThumbnail(inputMediaFile, outputMediaFile, conversionOptions);

                    // Try to avoid aggregration of precision errors by rounding to the nearest hundredth
                    i = Math.Round(i + frameIntervalInSeconds, 2);
                }
            }
        }
    }
}
