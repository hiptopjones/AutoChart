using AutoChart.Common;
using CsvHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.KataWriter
{
    class TableProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public void GenerateKataChart(CommandLineOptions options)
        {
            string inputFilePath = options.InputFilePath;
            string outputFilePath = options.OutputFilePath;

            Logger.Info($"Processing '{inputFilePath}'");

            List<dynamic> timelineRecords;
            using (StreamReader reader = new StreamReader(inputFilePath))
            using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                timelineRecords = csv.GetRecords<dynamic>().ToList();
            }

            // TODO: These should be command-line parameters
            int beatsPerMinute = 139;
            int timelineDivisionsPerBeat = 4;
            int chartDivisionsPerBeat = 192;
            int chartStartingDivisionIndex = 768;
            string songName = "newkidinschool.mp3";

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
            chart.Song.Add(new KeyValuePair<string, string>("MusicStream", Quote(songName)));

            chart.SyncTrack.Add(new KeyValuePair<string, string>("0", "TS 4"));
            chart.SyncTrack.Add(new KeyValuePair<string, string>("0", $"B {beatsPerMinute * 1000}"));

            int timelineDivisionIndex = 0;

            foreach (dynamic timelineRecord in timelineRecords)
            {
                IDictionary<string, object> timelineEntry = timelineRecord as IDictionary<string, object>;

                int chartDivisionIndex = (int)(chartStartingDivisionIndex + (timelineDivisionIndex / (double)timelineDivisionsPerBeat) * chartDivisionsPerBeat);

                foreach (string columnName in Enum.GetNames(typeof(DrumType)))
                {
                    if ((string)timelineEntry[columnName] == "X")
                    {
                        int midiNoteIndex = GetMidiNoteFromColumnName(columnName);
                        chart.ExpertDrums.Add(new KeyValuePair<string, string>(Convert.ToString(chartDivisionIndex), $"N {midiNoteIndex} 0"));
                    }
                }

                string sectionName = (string)timelineEntry["SectionName"];
                if (!string.IsNullOrEmpty(sectionName))
                {
                    chart.Events.Add(new KeyValuePair<string, string>(Convert.ToString(chartDivisionIndex), $"E \"section {sectionName}\""));
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

        private int GetMidiNoteFromColumnName(string columnName)
        {
            DrumType drumType = (DrumType)Enum.Parse(typeof(DrumType), columnName);
            return (int)drumType;
        }

        private string Quote(string text)
        {
            return $"\"{text}\"";
        }
    }
}
