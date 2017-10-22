using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.EyeMoveSearch
{
    public class EyeMoveFinderConfig
    {
        public int MinLatency { get; set; }
        public int MinDuration { get; set; }
        public int ControlWindowLength { get; set; }
        public int MoveSearchWindowLength { get; set; }
        public double MinLength { get; set; }
        public int MinInhibition { get; set; }
        public int MinAmp { get; set; }
        public int MaxAmp { get; set; }
        public double ControlAmpDivider { get; set; }
        public double Multiplication { get; set; }
    }
}
