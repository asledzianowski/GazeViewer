using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.ParamCalculations
{
    public class PursuitGainCalculations
    {
        public Dictionary<string, double?> Gains { get; set; }
        public Dictionary<string, double?> Accuracies { get; set; }
        public List<Dictionary<double, double>> FilteredControlWindows { get; set; }
    }
}
