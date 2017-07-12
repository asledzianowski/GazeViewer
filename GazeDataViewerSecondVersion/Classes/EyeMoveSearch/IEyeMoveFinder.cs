using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.EyeMoveSearch
{
    interface IEyeMoveFinder
    {
        EyeMove TryFindEyeMove(int id, int spotStartIndex, int spotEndIndex, ResultData results);
    }
}
