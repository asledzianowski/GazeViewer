using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Spot
{
    class SpotMove
    {
        public int SpotStartIndex { get; set; }
        public double SpotStartTime { get; set; }
        public double SpotStartCoord { get; set; }

        public int SpotEndIndex { get; set; }
        public double SpotEndTime { get; set; }
        public double SpotEndCoord { get; set; }
    }
}
