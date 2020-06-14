using AutoChart.Common;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.TableWriter
{
    class TimelineProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // TODO: Move to a shared enum
        private List<string> TimelineColumnNames { get; } = new List<string>
        {
            "Kick",
            "Red",
            "Yellow",
            "Blue",
            "Green"
        };

        private List<string> TableColumnNames { get; } = new List<string>
        {
            nameof(DrumType.BassDrum),
            nameof(DrumType.SnareDrum),
            nameof(DrumType.ClosedHiHat),
            nameof(DrumType.OpenHiHat),
            nameof(DrumType.HighTom),
            nameof(DrumType.MediumTom),
            nameof(DrumType.FloorTom),
            nameof(DrumType.RideCymbal),
            nameof(DrumType.CrashCymbal),
        };

        private StringEnumConverter StringEnumConverter { get; set; } = new StringEnumConverter();

        public void GenerateTable(CommandLineOptions options)
        {
            string inputFilePath = options.InputFilePath;
            string outputFilePath = options.OutputFilePath;

            Logger.Info($"Processing '{inputFilePath}'");

            string jsonTimeline = File.ReadAllText(inputFilePath);
            List<Dictionary<string, bool>> songTimeline = JsonConvert.DeserializeObject<List<Dictionary<string, bool>>>(jsonTimeline);

            List<Dictionary<string, bool>> songTable = new List<Dictionary<string, bool>>();

            foreach (Dictionary<string, bool> timelineEntry in songTimeline)
            {
                Dictionary<string, bool> tableEntry = new Dictionary<string, bool>();

                foreach (string timelineColumnName in TimelineColumnNames)
                {
                    bool isNotePresent = timelineEntry[timelineColumnName];
                    if (isNotePresent)
                    {
                        string tableColumnName = GetTableColumnNameFromTimelineColumnName(timelineColumnName);
                        tableEntry[tableColumnName] = true;
                    }
                }

                songTable.Add(tableEntry);
            }

            using (var writer = new StreamWriter(outputFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField("Index");

                foreach (string tableColumnName in TableColumnNames)
                {
                    csv.WriteField(tableColumnName);
                }

                csv.WriteField("SectionName");

                csv.NextRecord();

                int entryIndex = 0;
                foreach (Dictionary<string, bool> tableEntry in songTable)
                {
                    csv.WriteField(entryIndex.ToString());

                    foreach (string tableColumnName in TableColumnNames)
                    {
                        csv.WriteField(tableEntry.ContainsKey(tableColumnName) ? "X" : string.Empty);
                    }

                    // The chart author can fill in this column before running the next step
                    csv.WriteField(string.Empty);

                    csv.NextRecord();
                    entryIndex++;
                }
            }
        }

        private string GetTableColumnNameFromTimelineColumnName(string timelineColumnName)
        {
            // These are the most common mappings
            switch (timelineColumnName)
            {
                case "Kick":
                    return nameof(DrumType.BassDrum);

                case "Red":
                    return nameof(DrumType.SnareDrum);

                case "Yellow":
                    return nameof(DrumType.ClosedHiHat);

                case "Blue":
                    return nameof(DrumType.HighTom);

                case "Green":
                    return nameof(DrumType.CrashCymbal);

                default:
                    throw new Exception($"Unexpected timeline column name: '{timelineColumnName}'");
            }
        }
    }
}
