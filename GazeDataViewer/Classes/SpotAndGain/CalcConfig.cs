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
        public double AmpProp { get; set; } = 0.1;
        public double DelayWindowLargerThan { get; set; } = 0.2;
        public double DelayWindowSmallerThan { get; set; } = 0.6;
        public int ReductMinEyeSpotAmpDiff { get; set; } = 5;

        public bool CalcateRecStart { get; set; } = true;
    }
}
