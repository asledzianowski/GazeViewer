using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
    public class CalcConfig
    {
        public int RecStart { get; set; } = 0;
        public int EyeShiftPeriod { get; set; } = 2;
        public int SpotShiftPeriod { get; set; } = -1;
        public int EyeStartShiftPeroid { get; set; } = 0;
        public int EyeEndShiftPeroid { get; set; } = 0;
        public double EyeAmpProp { get; set; } = 0.0;
        public double SpotAmpProp { get; set; } = 0.0;
        public int ReductMinEyeSpotAmpDiff { get; set; } = 5;
        
    }
}
