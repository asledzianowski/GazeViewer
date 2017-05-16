using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class SaccadePosition
    {
        public int Id { get; set; }

        public int SaccadeStartIndex { get; set; }
        public double SaccadeStartTime { get; set; }
        public double SaccadeStartCoord { get; set; }

        public int SaccadeEndIndex { get; set; }
        public double SaccadeEndTime { get; set; }
        public double SaccadeEndCoord { get; set; }

        public int SpotStartIndex { get; set; }
        public double SpotStartTime { get; set; }
        public double SpotStartCoord { get; set; }

        public int SpotEndIndex { get; set; }
        public double SpotEndTime { get; set; }
        public double SpotEndCoord { get; set; }

    }
}
