using GazeDataViewer.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class EyeMoveCalculation
    {
        public EyeMove EyeMove { get; set; }

        public double LatencyFrameCount { get; set; }
        public double DurationFrameCount { get; set; }

        public double Duration { get; set; }
        public double Latency { get; set; }
        public double Distance { get; set; }
        public double Amplitude { get; set; }
        public double MaxVelocity { get; set; }
        public double AvgVelocity { get; set; }
        public double Gain { get; set; }

        public double? PursuitLongSinGain { get; set; }
        public double? PursuitMidSinGain { get; set; }
        public double? PursuitShortSinGain { get; set; }

        public double? PursuitLongSinAccuracy { get; set; }
        public double? PursuitMidSinAccuracy { get; set; }
        public double? PursuitShortSinAccuracy { get; set; }


    }
}