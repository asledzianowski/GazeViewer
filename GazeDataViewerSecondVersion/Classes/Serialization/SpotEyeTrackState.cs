using GazeDataViewer.Classes.Denoise;
using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.Spot;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Serialization
{
    [Serializable]
    public class SpotEyeTrackState
    {
        public List<EyeMove> SaccadePositions { get; set; }
        public List<EyeMove> AntiSaccadePositions { get; set; }
        public List<SpotMove> SpotPositions { get; set; }
        public List<EyeMoveCalculation> SaccadeCalculations { get; set; }
        public List<EyeMoveCalculation> AntiSaccadeCalculations { get; set; }

        public SpotGazeFileData FileData { get; set; }
        public ResultData CurrentResults { get; set; }
        public CalcConfig CalcConfig { get; set; }
        public FiltersConfig FiltersConfig { get; set; }
    }
}
