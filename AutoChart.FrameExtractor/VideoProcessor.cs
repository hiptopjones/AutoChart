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
            double frameIntervalInSeconds = options.FrameIntervalInSeconds;
            double skipDurationInSeconds = options.SkipDurationInSeconds;
            double takeDurationInSeconds = options.TakeDurationInSeconds;

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            Logger.Info($"Processing '{inputFilePath}'");
            using (Engine engine = new Engine())
            {
                MediaFile inputMediaFile = new MediaFile { Filename = inputFilePath };
                engine.GetMetadata(inputMediaFile);

                double totalDurationInSeconds = inputMediaFile.Metadata.Duration.TotalSeconds;
                if (skipDurationInSeconds > totalDurationInSeconds)
                {
                    throw new Exception($"Skip duration ({skipDurationInSeconds}) exceeds total duration ({totalDurationInSeconds})");
                }

                if (takeDurationInSeconds == default)
                {
                    takeDurationInSeconds = totalDurationInSeconds - skipDurationInSeconds;
                }

                double maxDurationInSeconds = takeDurationInSeconds + skipDurationInSeconds;
                if (maxDurationInSeconds > totalDurationInSeconds)
                {
                    throw new Exception($"Skip and take duration total ({maxDurationInSeconds}) exceeds total duration ({totalDurationInSeconds})");
                }

                int takeFrameCount = (int)(takeDurationInSeconds / frameIntervalInSeconds);

                Logger.Info($"Extracting {takeFrameCount} individual frames");

                int frameIndex = 0;
                double i = skipDurationInSeconds;
                while (i < maxDurationInSeconds)
                {
                    long sequenceId = (long)(i * 1000);
                    string outputFilePath = Path.Combine(outputDirectoryPath, $"frame-{sequenceId:0000000}.jpg");
                    Logger.Info($"{frameIndex++}/{takeFrameCount}: {outputFilePath}");

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
