using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Spot
{
    public class SpotMove
    {
        public int SpotStartIndex { get; set; }
        public double SpotStartTimeDelta { get; set; }
        public double SpotStartTimeStamp { get; set; }
        public double SpotStartCoord { get; set; }

        public int SpotEndIndex { get; set; }
        public double SpotEndTimeDelta { get; set; }
        public double SpotEndTimeStamp { get; set; }
        public double SpotEndCoord { get; set; }
    }
}
