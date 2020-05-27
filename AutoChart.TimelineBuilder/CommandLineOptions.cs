using System;
using NLog;

namespace AutoChart.TimelineBuilder
{
    public class CommandLineOptions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public bool PromptUser { get; private set; }
        public string InputDirectoryPath { get; private set; }
        public string OutputFilePath { get; private set; }
        public int SkipFramesCount { get; set; } = 0;
        public int TakeFramesCount { get; set; } = Int32.MaxValue;
        public double FrameIntervalInSeconds { get; set; } = 0;
        public int BeatIntervalInPixels { get; set; } = 0;
        public int BeatsPerMinute { get; set; } = 0;
        public int DivisionsPerBeat { get; set; } = 0;

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

                        case "--OutputFilePath":
                            OutputFilePath = args[++i];
                            break;

                        case "--SkipFramesCount":
                            SkipFramesCount = Convert.ToInt32(args[++i]);
                            break;

                        case "--TakeFramesCount":
                            TakeFramesCount = Convert.ToInt32(args[++i]);
                            break;

                        case "--FrameIntervalInSeconds":
                            FrameIntervalInSeconds = Convert.ToDouble(args[++i]);
                            break;

                        case "--BeatIntervalInPixels":
                            BeatIntervalInPixels = Convert.ToInt32(args[++i]);
                            break;

                        case "--BeatsPerMinute":
                            BeatsPerMinute = Convert.ToInt32(args[++i]);
                            break;

                        case "--DivisionsPerBeat":
                            DivisionsPerBeat = Convert.ToInt32(args[++i]);
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
                    return false;
                }

                if (string.IsNullOrEmpty(OutputFilePath))
                {
                    Logger.Error("OutputFilePath must be specified");
                    return false;
                }

                if (FrameIntervalInSeconds == 0)
                {
                    Logger.Error("FrameIntervalInSeconds must be specified and non-zero");
                    return false;
                }

                if (BeatIntervalInPixels == 0)
                {
                    Logger.Error("BeatIntervalInPixels must be specified and non-zero");
                    return false;
                }

                if (BeatsPerMinute == 0)
                {
                    Logger.Error("BeatsPerMinute must be specified and non-zero");
                    return false;
                }

                if (DivisionsPerBeat == 0)
                {
                    Logger.Error("DivisionsPerBeat must be specified and non-zero");
                    return false;
                }

                Logger.Info($"Application configuration:");
                Logger.Info($"  InputDirectoryPath:             '{InputDirectoryPath}'");
                Logger.Info($"  OutputFilePath:                 '{OutputFilePath}'");
                Logger.Info($"  SkipFramesCount:                {SkipFramesCount}");
                Logger.Info($"  TakeFramesCount:                {TakeFramesCount}");
                Logger.Info($"  FrameIntervalInSeconds:         {FrameIntervalInSeconds}");
                Logger.Info($"  BeatIntervalInPixels:           {BeatIntervalInPixels}");
                Logger.Info($"  BeatsPerMinute:                 {BeatsPerMinute}");
                Logger.Info($"  DivisionsPerBeat:               {DivisionsPerBeat}");
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
