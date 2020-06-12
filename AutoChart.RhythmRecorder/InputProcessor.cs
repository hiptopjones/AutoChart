using AutoChart.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.RhythmRecorder
{
    class InputProcessor
    {
        public class DrumBeat
        {
            public DrumType DrumType { get; set; }
            public long RelativeTicks { get; set; }
        }

        public void HandleKeyboardInput(CommandLineOptions options)
        {
            List<DrumBeat> drumBeats = new List<DrumBeat>();

            // If an input file is provided, merge with it
            if (File.Exists(options.InputFilePath))
            {
                string inputDrumBeatsJson = File.ReadAllText(options.InputFilePath);
                drumBeats = JsonConvert.DeserializeObject<List<DrumBeat>>(inputDrumBeatsJson);
            }

            long startTicks = 0;
            bool isFirstKey = true;

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                long currentTicks = DateTime.Now.Ticks;

                // Start tick counter on the first key
                if (isFirstKey)
                {
                    startTicks = currentTicks;
                    isFirstKey = false;
                }

                // Escape key exits
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    break;
                }

                DrumBeat drumBeat = new DrumBeat
                {
                    DrumType = GetDrumTypeFromKeyInfo(keyInfo),
                    RelativeTicks = currentTicks - startTicks
                };

                drumBeats.Add(drumBeat);
            }

            string outputDrumBeatsJson = JsonConvert.SerializeObject(drumBeats, Formatting.Indented);
            File.WriteAllText(options.OutputFilePath, outputDrumBeatsJson);
        }

        private static DrumType GetDrumTypeFromKeyInfo(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.C:
                    return DrumType.CrashCymbal;

                case ConsoleKey.F:
                    return DrumType.FloorTom;

                case ConsoleKey.H:
                    return DrumType.ClosedHiHat;

                case ConsoleKey.K:
                    return DrumType.BassDrum;

                case ConsoleKey.M:
                    return DrumType.MediumTom;

                case ConsoleKey.O:
                    return DrumType.OpenHiHat;

                case ConsoleKey.R:
                    return DrumType.RideCymbal;

                case ConsoleKey.S:
                    return DrumType.SnareDrum;

                case ConsoleKey.T:
                    return DrumType.HighTom;

                default:
                    throw new Exception($"Unsupported key code: {keyInfo}");
            }
        }
    }
}
