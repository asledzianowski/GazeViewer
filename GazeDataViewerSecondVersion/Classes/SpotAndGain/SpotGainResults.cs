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
        public PlotData PlotData { get; set; }
    }

    public class PlotData
    {
        public int[] TimeStamps { get; set; }
        public List<int> EarliestEyeOverSpotIndex { get; set; }
        public List<int> SpotOverMeanIndex { get; set; }
        public double[] EyeCoords { get; set; }
        public double[] SpotCoords { get; set; }
        public int ShiftPeriod { get; set; }
    }
}
