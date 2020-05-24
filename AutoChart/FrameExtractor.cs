using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart
{
    class FrameExtractor
    {
        public void ConvertVideoToStills(string[] args)
        {
            // ConvertVideoToStills "C:\Users\hipto\Desktop\NewKidInSchool\newkidinschool.mp4" "C:\Users\hipto\Desktop\NewKidInSchool\frames" 0.20
            string mp4FilePath = args[0];
            string outputDirectoryPath = args[1];
            double frameDelayInSeconds = Convert.ToDouble(args[2]);

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            using (Engine engine = new Engine())
            {
                MediaFile mp4 = new MediaFile { Filename = mp4FilePath };

                engine.GetMetadata(mp4);

                double i = 0;
                while (i < mp4.Metadata.Duration.TotalSeconds)
                {
                    ConversionOptions options = new ConversionOptions { Seek = TimeSpan.FromSeconds(i) };
                    long sequenceId = (long)(i * 1000);
                    MediaFile outputFile = new MediaFile { Filename = $"{outputDirectoryPath}\\frame-{sequenceId:0000000}.jpg" };
                    engine.GetThumbnail(mp4, outputFile, options);
                    i += frameDelayInSeconds;
                }
            }
        }
    }
}
