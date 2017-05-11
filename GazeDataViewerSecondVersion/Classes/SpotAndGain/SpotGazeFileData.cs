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

        public float[] LEye { get; }

        public float[] REye { get; }

        public float[] Spot { get; }

        public int originalLenght { get; }

        public SpotGazeFileData(int lenght)
        {
            Time = new int[lenght];
            LEye = new float[lenght];
            REye = new float[lenght];
            Spot = new float[lenght];
            originalLenght = lenght;
        }
    }
}
