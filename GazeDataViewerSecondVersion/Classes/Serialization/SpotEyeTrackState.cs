using GazeDataViewer.Classes.Denoise;
using GazeDataViewer.Classes.Saccade;
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
        public List<SaccadePosition> SaccadePositions { get; set; }
        public SpotGazeFileData FileData { get; set; }
        public SpotGainResults CurrentResults { get; set; }
        public CalcConfig CalcConfig { get; set; }
        public FiltersConfig FiltersConfig { get; set; }
    }
}
