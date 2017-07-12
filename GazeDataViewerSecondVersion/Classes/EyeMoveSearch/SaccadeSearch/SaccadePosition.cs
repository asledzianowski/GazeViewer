using GazeDataViewer.Classes.Enums;
using GazeDataViewer.Classes.Spot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    [Serializable]
    public class EyeMove
    {
        public int Id { get; set; }

        public bool IsStartFound { get; set; }
        public int EyeStartIndex { get; set; }
        public double EyeStartTime { get; set; }
        public double EyeStartCoord { get; set; }

        public bool IsEndFound { get; set; }
        public int EyeEndIndex { get; set; }
        public double EyeEndTime { get; set; }
        public double EyeEndCoord { get; set; }

        public EyeMoveTypes EyeMoveType { get; set; }
        public SpotMove SpotMove { get; set; }
        public bool IsFirstMove { get; set; }

}
}
