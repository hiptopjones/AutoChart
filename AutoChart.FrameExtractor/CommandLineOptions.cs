using System;
using NLog;

namespace AutoChart.FrameExtractor
{
    public class CommandLineOptions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool PromptUser { get; private set; }
        public string InputFilePath { get; private set; }
        public string OutputDirectoryPath { get; private set; }
        public double FrameIntervalInSeconds { get; set; } = 0;
        
        public bool ParseArguments(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "--InputFilePath":
                            InputFilePath = args[++i];
                            break;

                        case "--OutputDirectoryPath":
                            OutputDirectoryPath = args[++i];
                            break;

                        case "--FrameIntervalInSeconds":
                            FrameIntervalInSeconds = Convert.ToDouble(args[++i]);
                            break;

                        case "--PromptUser":
                            PromptUser = true;
                            break;

                        default:
                            Logger.Error($"Unrecognized parameter: {args[i]}");
                            return false;
                    }
                }

                if (string.IsNullOrEmpty(InputFilePath))
                {
                    Logger.Error("InputDirectoryPath must be specified");
                }

                if (string.IsNullOrEmpty(OutputDirectoryPath))
                {
                    Logger.Error("OutputDirectoryPath must be specified");
                }

                Logger.Info($"Application configuration:");
                Logger.Info($"  InputFilePath:                  '{InputFilePath}'");
                Logger.Info($"  OutputDirectoryPath:            '{OutputDirectoryPath}'");
                Logger.Info($"  FrameIntervalInSeconds:         {FrameIntervalInSeconds}");
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
