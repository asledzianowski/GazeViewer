using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes
{
    /// <summary>
    /// Calculation methods for eye move parameters
    /// </summary>
    public class CalculationHelper
    {
        #region Public Fields

        /// <summary>
        /// Value from data log "Distance(Resp)-60cm" 
        /// </summary>
        public double distanceFromScreen;

        /// <summary>
        /// Value from data log "Freq-60Hz"
        /// </summary>
        public double trackerFrequency = 60;

        #endregion

        public CalculationHelper(int distanceFromScreen, int trackerFrequency)
        {
            this.distanceFromScreen = distanceFromScreen;
            this.trackerFrequency = trackerFrequency;
        }


        #region Public Methods

        /// <summary>
        /// Calculates euclidean distance
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>Euclidean distance between points</returns>
        public double CountEuclideanDistance(GazeDataLogItem p1, GazeDataLogItem p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }


        /// <summary>
        /// Calculates angle from arctangent
        /// </summary>
        /// <param name="abDistance">Distance between points</param>
        /// <returns>Visual angle (amplitude)</returns>
        public double CountVisualAngle(double abDistance)
        {
            if (abDistance > 0)
            {
                var tangent = abDistance / distanceFromScreen;
                var arctangent = 2 * Math.Atan(tangent);
                return arctangent * (180 / Math.PI);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculates angular velocity per second
        /// </summary>
        /// <param name="amplitude">The amplitude </param>
        /// <param name="frameCount">Number of frames</param>
        /// <returns>Angular velocity per second</returns>
        public double CountAngularVelocityPerSecond(double amplitude, double frameCount)
        {
            if (amplitude > 0 & frameCount > 0)
            {
                return (amplitude / frameCount) * trackerFrequency;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts to centimeters with exemplary DPI
        /// </summary>
        /// <param name="value">distance in pixels</param>
        /// <returns>Distance in centimeters</returns>
        public double ConvertPixelsToCentimeters(double value)
        {
            return value * 2.54 / 96;
        }

        # endregion
    }
}
