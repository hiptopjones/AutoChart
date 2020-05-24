using System;
using NLog;

namespace AutoChart.FeatureDetector
{
    public class CommandLineOptions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool PromptUser { get; private set; }
        public string InputDirectoryPath { get; private set; }
        public string OutputDirectoryPath { get; private set; }
        public int SkipFramesCount { get; set; } = 0;
        public int TakeFramesCount { get; set; } = Int32.MaxValue;

        public bool ParseArguments(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--InputDirectoryPath":
                            InputDirectoryPath = args[++i];
                            break;

                        case "--OutputDirectoryPath":
                            OutputDirectoryPath = args[++i];
                            break;

                        case "--SkipFramesCount":
                            SkipFramesCount = Convert.ToInt32(args[++i]);
                            break;

                        case "--TakeFramesCount":
                            TakeFramesCount = Convert.ToInt32(args[++i]);
                            break;

                        case "--PromptUser":
                            PromptUser = true;
                            break;

                        default:
                            Logger.Error($"Unrecognized parameter: {args[i]}");
                            return false;
                    }
                }

                if (string.IsNullOrEmpty(InputDirectoryPath))
                {
                    Logger.Error("InputDirectoryPath must be specified");
                }

                if (string.IsNullOrEmpty(OutputDirectoryPath))
                {
                    Logger.Error("OutputDirectoryPath must be specified");
                }

                Logger.Info($"Application configuration:");
                Logger.Info($"  InputDirectoryPath:             '{InputDirectoryPath}'");
                Logger.Info($"  OutputDirectoryPath:            '{OutputDirectoryPath}'");
                Logger.Info($"  SkipFramesCount:                {SkipFramesCount}");
                Logger.Info($"  TakeFramesCount:                {TakeFramesCount}");
                Logger.Info($"  PromptUser:                     {PromptUser}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Unable to parse parameters: {ex.Message}");
                return false;
            }

            return true;
        }
    }
}
