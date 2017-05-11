using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
    public class SpotGainResults
    {
        public double MeanSpotAmplitude { get; set; }

        public List<TimeSpan> REyeEarliestOverAmpForSpotTimeStamps { get; set; }
        public List<TimeSpan> LEyeEarliestOverAmpForSpotTimeStamps { get; set; }
        public List<TimeSpan> SpotOverAmpForSpotTimeStamps { get; set; }

        public double[] DelaysRe { get; set; }
        public double[] DelaysLe { get; set; }

        public double MeanDelayRe { get; set; }
        public double MeanDelayLe { get; set; }

        public double StdDelayRe { get; set; }
        public double StdDelayLe { get; set; }

        public double[] MaxSpeedTimesRe { get; set; }
        public double[] MaxSpeedTimesLe { get; set; }

        public double[] MaxSpeedAmpsRe { get; set; }
        public double[] MaxSpeedAmpsLe { get; set; }

        public double[] DurationsRe { get; set; }
        public double[] DurationsLe { get; set; }

        public double MeanDurationRe { get; set; }
        public double MeanDurationLe { get; set; }

        public double StdDurationRe { get; set; }
        public double StdDurationLe { get; set; }

        public PlotData PlotData { get; set; }
    }

    public class PlotData
    {
        public TimeSpan[] Stime { get; set; }
        public List<int> Kre { get; set; }
        public List<int> Ksp { get; set; }
        public float[] Leye{ get; set; }
        public float[] Reye { get; set; }
        public float[] Spot { get; set; }
        public int ShiftPeriod { get; set; }
    }
}
