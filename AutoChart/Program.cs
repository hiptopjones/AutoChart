using CsvHelper;
using ImageMagick;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart
{
    class Program
    {
        static void Main(string[] args)
        {
            string mode = args[0];
            args = args.Skip(1).ToArray();

            switch (mode)
            {
                case "ConvertVideoToStills":
                    FrameExtractor frameExtractor = new FrameExtractor();
                    frameExtractor.ConvertVideoToStills(args);
                    break;

                case "CorrectStillsPerspectiveAndCrop":
                    ImageCorrector imageCorrector = new ImageCorrector();
                    imageCorrector.CorrectStillsPerspectiveAndCrop(args);
                    break;

                case "SampleNoteColumnsInStills":
                    ColumnSampler columnSampler = new ColumnSampler();
                    columnSampler.SampleNoteColumnsInStills(args);
                    break;

                case "DetectFeaturesInSamples":
                    FeatureDetector featureDetector = new FeatureDetector();
                    featureDetector.DetectFeaturesInSamples(args);
                    break;

                default:
                    Console.WriteLine($"Unhandled mode: '{mode}'");
                    break;
            }

            Console.WriteLine("Hit enter to continue...");
            Console.ReadLine();
        }
    }
}