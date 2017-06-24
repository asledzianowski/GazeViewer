using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{

    public class SpotGazeFileData
    {
        public int[] Time { get; set; }

        public int[] TimeDeltas { get; set; }

        public double[] Eye { get; set; }

        public double[] Spot { get; set; }


    }
}
