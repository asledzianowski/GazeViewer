using GazeDataViewer.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class SaccadeCalculation
    {
        public int Id { get; set; }
        public EyeMoveTypes EyeMoveType { get; set; }
        public int SpotStartIndex { get; set; }
        public int SpotEndIndex { get; set; }
        public int EyeStartIndex { get; set; }
        public int EyeEndIndex { get; set; }
        public int DurationFrameCount { get; set; }
        public int LatencyFrameCount { get; set; }

        public double Duration { get; set; }
        public double Latency { get; set; }
        public double Distance { get; set; }
        public double Amplitude { get; set; }
        public double MaxVelocity { get; set; }
        public double AvgVelocity { get; set; }
        public double Gain { get; set; }

        
    }
}