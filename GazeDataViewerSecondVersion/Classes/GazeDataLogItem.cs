using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes
{
    /// <summary>
    /// Basic gaze data log item
    /// </summary>
    public class GazeDataLogItem
    {
        public int Timestamp { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double SizeX { get; set; }
        public double SizeY { get; set; }
    }
}
