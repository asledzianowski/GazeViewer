using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
   
    public class ResultData
    {
        public int[] TimeStamps { get; set; }
        public int[] TimeDeltas { get; set; }
        public List<Spot.SpotMove> SpotMoves { get; set; }
        public List<int> SpotOverMeanIndex { get; set; }
        public double[] EyeCoords { get; set; }
        public double[] SpotCoords { get; set; }
        public int ShiftPeriod { get; set; }
        public double MeanSpotAmplitude { get; set; }
    }
}
