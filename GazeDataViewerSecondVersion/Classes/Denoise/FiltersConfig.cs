using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GazeDataViewer.Classes.Denoise.FilterButterworth;

namespace GazeDataViewer.Classes.Denoise
{
    public class FiltersConfig
    {
        public bool FilterByButterworth { get; set; }
        public double ButterworthFrequency { get; set; } 
        public int ButterworthSampleRate { get; set; }
        public PassType ButterworthPassType { get; set; }
        public double ButterworthResonance { get; set; }

        public bool FilterBySavitzkyGolay { get; set; }
        public int SavitzkyGolayNumberOfPoints { get; set; }
        public int SavitzkyGolayDerivativeOrder { get; set; }
        public int SavitzkyGolayPolynominalOrder { get; set; }

    }


}
