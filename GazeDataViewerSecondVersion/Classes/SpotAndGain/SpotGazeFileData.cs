using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
    public class SpotGazeFileData
    {
        public int[] Time { get; }

        public double[] Eye { get; }

        public double[] Spot { get; }

        public int originalLenght { get; }

        public SpotGazeFileData(int lenght)
        {
            Time = new int[lenght];
            Eye = new double[lenght];
            Spot = new double[lenght];
            originalLenght = lenght;
        }
    }
}
