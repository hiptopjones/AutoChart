using AutoChart.Common;
using CsvHelper;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace AutoChart.SampleVisualizer
{
    class SampleProcessor
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // TODO: Move to a shared enum
        private List<string> ColumnNames { get; } = new List<string>
        {
            "Red",
            "Yellow",
            "Blue",
            "Green"
        };

        public void CreateCharts(CommandLineOptions options)
        {
            string inputDirectoryPath = options.InputDirectoryPath;
            string outputDirectoryPath = options.OutputDirectoryPath;
            int skipFrameCount = options.SkipFramesCount;
            int takeFrameCount = options.TakeFramesCount;

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            string[] inputFilePaths = Directory.GetFiles(inputDirectoryPath);
            foreach (string inputFilePath in inputFilePaths)
            {
                // Allow subsetting the input frames
                if (skipFrameCount > 0)
                {
                    skipFrameCount--;
                    continue;
                }

                if (takeFrameCount > 0)
                {
                    takeFrameCount--;
                }
                else
                {
                    break;
                }

                Logger.Info($"Processing '{inputFilePath}'");

                using (StreamReader reader = new StreamReader(inputFilePath))
                using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    List<Sample> samples = csv.GetRecords<Sample>().ToList();

                    foreach (string columnName in ColumnNames)
                    {
                        int[] yvals = samples.Where(x => x.Column == columnName).Select(x => x.Gray).ToArray();
                        int[] xvals = samples.Where(x => x.Column == columnName).Select(x => x.Row).ToArray();

                        // create the chart
                        var chart = new Chart();
                        chart.Size = new Size(1000, 1000);

                        var chartArea = new ChartArea();
                        chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
                        chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
                        chartArea.AxisX.Minimum = 0;
                        chartArea.AxisX.Interval = 20;
                        chartArea.AxisY.Minimum = 0;
                        chartArea.AxisY.Maximum = 255;
                        chartArea.AxisY.Interval = 20;
                        chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
                        chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
                        chartArea.AxisX.IsReversed = true;
                        chart.ChartAreas.Add(chartArea);

                        var series = new Series();
                        series.Name = "Series1";
                        series.ChartType = SeriesChartType.Bar;
                        chart.Series.Add(series);

                        // bind the datapoints
                        chart.Series["Series1"].Points.DataBindXY(xvals, yvals);

                        chart.Invalidate();

                        string inputFileName = Path.GetFileName(inputFilePath);
                        string outputFileName = inputFileName.Substring(0, inputFileName.IndexOf('.')) + $"-{columnName.ToLower()}.visualization.png";
                        string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);

                        chart.SaveImage(outputFilePath, ChartImageFormat.Png);
                    }
                }
            }
        }
    }
}
