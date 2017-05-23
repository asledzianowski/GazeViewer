using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes
{
    /// <summary>
    /// Basic chart item
    /// </summary>
    class SeriesItem
    {
        public int Timepoint { get; set; }
        public double Value { get; set; }

        public SeriesItem(int timePoint, double value)
        {
            this.Timepoint = timePoint;
            this.Value = value;
        }
    }
}
