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

namespace AutoChart.AudioExtractor
{
    class VideoProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void ConvertVideoToAudio(CommandLineOptions options)
        {
            string inputFilePath = options.InputFilePath;
            string outputFilePath = options.OutputFilePath;

            Logger.Info($"Processing '{inputFilePath}'");
            Logger.Info($"Extracting audio file");

            MediaFile inputMediaFile = new MediaFile { Filename = inputFilePath };
            MediaFile outputMediaFile = new MediaFile { Filename = outputFilePath };

            using (Engine engine = new Engine())
            {
                engine.GetMetadata(inputMediaFile);
                engine.Convert(inputMediaFile, outputMediaFile);
            }
        }
    }
}
