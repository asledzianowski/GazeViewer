using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.EyeMoveSearch
{
    public static class PursuitMoveHelper
    {
        /// <summary>
        /// http://lab.polygonal.de/2007/07/18/fast-and-accurate-sinecosine-approximation/
        /// </summary>
        public static double? GetSinusoideApproximation(double x)
        {
            double? sin = null;
            double? cos = null;

            //always wrap input angle to -PI..PI
            if (x < -3.14159265)
                x += 6.28318531;
            else
            if (x > 3.14159265)
                x -= 6.28318531;

            //compute sine
            if (x < 0)
            {
                sin = 1.27323954 * x + .405284735 * x * x;

                if (sin < 0)
                    sin = .225 * (sin * -sin - sin) + sin;
                else
                    sin = .225 * (sin * sin - sin) + sin;
            }
            else
            {
                sin = 1.27323954 * x - 0.405284735 * x * x;

                if (sin < 0)
                    sin = .225 * (sin * -sin - sin) + sin;
                else
                    sin = .225 * (sin * sin - sin) + sin;
            }

            //compute cosine: sin(x + PI/2) = cos(x)
            x += 1.57079632;
            if (x > 3.14159265)
                x -= 6.28318531;

            if (x < 0)
            {
                cos = 1.27323954 * x + 0.405284735 * x * x;

                if (cos < 0)
                    cos = .225 * (cos * -cos - cos) + cos;
                else
                    cos = .225 * (cos * cos - cos) + cos;
            }
            else
            {
                cos = 1.27323954 * x - 0.405284735 * x * x;

                if (cos < 0)
                    cos = .225 * (cos * -cos - cos) + cos;
                else
                    cos = .225 * (cos * cos - cos) + cos;
            }

           
                return sin;
           
        }
    }
}
