using System;
using NLog;

namespace AutoChart.ChartWriter
{
    public class CommandLineOptions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool PromptUser { get; private set; }
        public string InputFilePath { get; private set; }
        public string OutputFilePath { get; private set; }
        public string SongFilePath { get; private set; }

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

                        case "--OutputFilePath":
                            OutputFilePath = args[++i];
                            break;

                        case "--SongFilePath":
                            SongFilePath = args[++i];
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
                    Logger.Error("InputFilePath must be specified");
                }

                if (string.IsNullOrEmpty(OutputFilePath))
                {
                    Logger.Error("OutputFilePath must be specified");
                }

                Logger.Info($"Application configuration:");
                Logger.Info($"  InputFilePath:                  '{InputFilePath}'");
                Logger.Info($"  OutputFilePath:                 '{OutputFilePath}'");
                Logger.Info($"  SongFilePath:                   '{SongFilePath}'");
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
