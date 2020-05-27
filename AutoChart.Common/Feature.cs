using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoChart.Common
{
    public class Feature
    {
        public FeatureType FeatureType { get; set; }
        public int EffectiveLocation { get; set; }
        public List<Sample> Samples { get; set; } = new List<Sample>();
    }
}
