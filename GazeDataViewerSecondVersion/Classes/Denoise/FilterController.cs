using Altaxo.Calc.Regression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Denoise
{
    public static class FilterController
    {
        public static double[] FilterByButterworth(FiltersConfig filterConfig, double[] eyeCoords)
        {
            var filterButterworth = new FilterButterworth(filterConfig.ButterworthFrequency, filterConfig.ButterworthSampleRate,
                   filterConfig.ButterworthPassType, filterConfig.ButterworthResonance);
            eyeCoords = filterButterworth.FilterArray(eyeCoords);
            return eyeCoords;
        }

        public static double[] FilterBySavitzkyGolay(FiltersConfig filterConfig, double[] eyeCoords)
        {
            double[] smoothedResult = new double[eyeCoords.Length];
            var filterSavitzkyGolay = new SavitzkyGolay(filterConfig.SavitzkyGolayNumberOfPoints, filterConfig.SavitzkyGolayDerivativeOrder,
                filterConfig.SavitzkyGolayPolynominalOrder);
            filterSavitzkyGolay.Apply(eyeCoords, smoothedResult);
            eyeCoords = smoothedResult;
            return eyeCoords;
        }
    }
}
