using AutoChart.Common;
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

namespace AutoChart.ChartWriter
{
    class TimelineProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // TODO: Move to a shared enum
        private List<string> ColumnNames { get; } = new List<string>
        {
            "Kick",
            "Red",
            "Yellow",
            "Blue",
            "Green"
        };

        private StringEnumConverter StringEnumConverter { get; set; } = new StringEnumConverter();

        public void GenerateChart(CommandLineOptions options)
        {
            string inputFilePath = options.InputFilePath;
            string outputFilePath = options.OutputFilePath;
            string songFilePath = options.SongFilePath;

            Logger.Info($"Processing '{inputFilePath}'");

            string jsonTimeline = File.ReadAllText(inputFilePath);
            List<Dictionary<string, bool>> songTimeline = JsonConvert.DeserializeObject<List<Dictionary<string, bool>>>(jsonTimeline);

            int beatsPerMinute = 139;
            int timelineDivisionsPerBeat = 4;
            int chartDivisionsPerBeat = 192;
            int chartStartingDivisionIndex = 768;

            ChartFormat chart = new ChartFormat();

            // Copying default values from a chart that was created in Moonscraper
            chart.Song.Add(new KeyValuePair<string, string>("Offset", "0"));
            chart.Song.Add(new KeyValuePair<string, string>("Resolution", Convert.ToString(chartDivisionsPerBeat)));
            chart.Song.Add(new KeyValuePair<string, string>("Player2", "bass"));
            chart.Song.Add(new KeyValuePair<string, string>("Difficulty", "0"));
            chart.Song.Add(new KeyValuePair<string, string>("PreviewStart", "0"));
            chart.Song.Add(new KeyValuePair<string, string>("PreviewEnd", "0"));
            chart.Song.Add(new KeyValuePair<string, string>("Genre", Quote("rock")));
            chart.Song.Add(new KeyValuePair<string, string>("MediaType", Quote("cd")));
            chart.Song.Add(new KeyValuePair<string, string>("MusicStream", Quote(songFilePath)));

            chart.SyncTrack.Add(new KeyValuePair<string, string>("0", "TS 4"));
            chart.SyncTrack.Add(new KeyValuePair<string, string>("0", $"B {beatsPerMinute * 1000}"));

            int timelineDivisionIndex = 0;

            foreach (Dictionary<string, bool> beatDivision in songTimeline)
            {
                int chartDivisionIndex = (int)(chartStartingDivisionIndex + (timelineDivisionIndex / (double) timelineDivisionsPerBeat) * chartDivisionsPerBeat);

                for (int i = 0; i < ColumnNames.Count; i++)
                {
                    string columnName = ColumnNames[i];

                    bool isNotePresent = beatDivision[columnName];
                    if (isNotePresent)
                    {
                        int noteIndex = GetNoteIndexFromColumnNameIndex(i);
                        chart.ExpertDrums.Add(new KeyValuePair<string, string>(Convert.ToString(chartDivisionIndex), $"N {noteIndex} 0"));
                    }
                }

                timelineDivisionIndex++;
            }

            StringBuilder builder = new StringBuilder();

            builder.AppendLine("[Song]");
            builder.AppendLine("{");
            foreach (KeyValuePair<string, string> kvp in chart.Song)
            {
                builder.AppendLine($"  {kvp.Key} = {kvp.Value}");
            }
            builder.AppendLine("}");

            builder.AppendLine("[SyncTrack]");
            builder.AppendLine("{");
            foreach (KeyValuePair<string, string> kvp in chart.SyncTrack)
            {
                builder.AppendLine($"  {kvp.Key} = {kvp.Value}");
            }
            builder.AppendLine("}");

            builder.AppendLine("[Events]");
            builder.AppendLine("{");
            foreach (KeyValuePair<string, string> kvp in chart.Events)
            {
                builder.AppendLine($"  {kvp.Key} = {kvp.Value}");
            }
            builder.AppendLine("}");

            builder.AppendLine("[ExpertDrums]");
            builder.AppendLine("{");
            foreach (KeyValuePair<string, string> kvp in chart.ExpertDrums)
            {
                builder.AppendLine($"  {kvp.Key} = {kvp.Value}");
            }
            builder.AppendLine("}");

            string chartText = builder.ToString();

            File.WriteAllText(outputFilePath, chartText);
        }

        private int GetNoteIndexFromColumnNameIndex(int i)
        {
            if (i == 4)
            {
                return 5;
            }

            return i;
        }

        private string Quote(string text)
        {
            return $"\"{text}\"";
        }
    }
}
