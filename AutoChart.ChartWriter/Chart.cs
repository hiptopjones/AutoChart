using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.ChartWriter
{
    class Chart
    {
        public List<KeyValuePair<string, string>> Song { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> SyncTrack { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> Events { get; set; } = new List<KeyValuePair<string, string>>();
        public List<KeyValuePair<string, string>> ExpertDrums { get; set; } = new List<KeyValuePair<string, string>>();
    }
}
