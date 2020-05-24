using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.Common
{
    public class Feature
    {
        public string FeatureType { get; set; }
        public int FeatureLocation { get; set; }
        public List<Sample> Samples { get; set; } = new List<Sample>();
    }
}
