using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart
{
    class ColumnSampler
    {
        public void SampleNoteColumnsInStills(string[] args)
        {
            // SampleNoteColumnsInStills "C:\Users\hipto\Desktop\NewKidInSchool\corrected" "C:\Users\hipto\Desktop\NewKidInSchool\samples"
            string inputDirectoryPath = args[0];
            string outputDirectoryPath = args[1];

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            const int redLeft = 40;
            const int redRight = 90;
            const int yellowLeft = 145;
            const int yellowRight = 210;
            const int blueLeft = 260;
            const int blueRight = 325;
            const int greenLeft = 380;
            const int greenRight = 430;

            List<int> sampleColumns = new List<int>
            {
                (redLeft + redRight) / 2,
                (yellowLeft + yellowRight) / 2,
                (blueLeft + blueRight) / 2,
                (greenLeft + greenRight) / 2,
            };

            List<string> sampleNames = new List<string>
            {
                "Red",
                "Yellow",
                "Blue",
                "Green"
            };

            int skipFiles = 100;
            int takeFiles = Int32.MaxValue;

            string[] inputFilePaths = Directory.GetFiles(inputDirectoryPath);
            foreach (string inputFilePath in inputFilePaths)
            {
                if (skipFiles > 0)
                {
                    skipFiles--;
                    continue;
                }

                if (takeFiles > 0)
                {
                    takeFiles--;
                }
                else
                {
                    break;
                }

                Console.WriteLine($"Processing '{inputFilePath}'");

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Column,Row,Gray,Red,Green,Blue,Hue,Saturation,Brightness");

                Bitmap bitmap = new Bitmap(inputFilePath);

                for (int sampleIndex = 0; sampleIndex < sampleColumns.Count; sampleIndex++)
                {
                    int x = sampleColumns[sampleIndex];

                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color pixel = bitmap.GetPixel(x, y);
                        int grayLevel = (pixel.R + pixel.G + pixel.B) / 3;
                        builder.AppendLine($"{sampleNames[sampleIndex]},{y},{grayLevel},{pixel.R},{pixel.G},{pixel.B},{pixel.GetHue()},{pixel.GetSaturation()},{pixel.GetBrightness()}");
                    }
                }

                string inputFileName = Path.GetFileName(inputFilePath);
                string outputFileName = inputFileName.Substring(0, inputFileName.IndexOf('.')) + ".samples.csv";
                string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);

                File.WriteAllText(outputFilePath, builder.ToString());
            }
        }
    }
}
