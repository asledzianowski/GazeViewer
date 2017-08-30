using GazeDataViewer.Classes.EnumsAndStats;
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

        public double[] TimeDeltas { get; set; }

        public double[] Eye { get; set; }

        public double[] Spot { get; set; }

        public FileType FileType { get; set; }


    }
}
