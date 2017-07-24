using GazeDataViewer.Classes.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class SaccadeParamsCalcuator
    {

        private readonly double[] eyeCoords = null;

        private readonly double[] spotCoords = null;

        private readonly double distanceFromScreen;

        private readonly double trackerFrequency;

        public SaccadeParamsCalcuator( double[] eyeCoords, double[] spotCoords, int distanceFromScreen, int trackerFrequency)
        {
            this.eyeCoords = eyeCoords;
            this.spotCoords = spotCoords;
            this.distanceFromScreen = distanceFromScreen;
            this.trackerFrequency = trackerFrequency;
        }

        public List<EyeMoveCalculation> Calculate(List<EyeMove> eyeMovePositions, EyeMoveTypes eyeMoveType)
        {
            var saccCalculations = new List<EyeMoveCalculation>();
            foreach(var saccadePosition in eyeMovePositions)
            {
                var saccParams = CalculateSaccadeParams(saccadePosition, eyeMoveType);
                saccCalculations.Add(saccParams);
            }

            return saccCalculations;
        }


        private List<double> CountOnScreenDistance(double[] saccadeCoords)
        {
            return SaccadeDataHelper.CountOnScreenDistance(saccadeCoords);
        }

       

        public EyeMoveCalculation CalculateSaccadeParams(EyeMove saccadePosition,  EyeMoveTypes eyeMoveType)
        {
            var frameCount = Math.Abs(saccadePosition.EyeEndIndex - saccadePosition.EyeStartIndex); //+ 1;

            var duration = frameCount / this.trackerFrequency;
            var spotEyeIndexDiff = Math.Abs(saccadePosition.EyeStartIndex - saccadePosition.SpotMove.SpotStartIndex);
            var latency = (spotEyeIndexDiff / this.trackerFrequency);

            double[] saccadeCoords;
            
            if(eyeCoords.Length > 1)
            {
                saccadeCoords = eyeCoords.Skip(saccadePosition.EyeStartIndex).Take(Math.Abs(saccadePosition.EyeEndIndex - saccadePosition.EyeStartIndex) + 1).ToArray();
            }    
            else
            {
                saccadeCoords = eyeCoords;
            }
                
            var spotLenght = (saccadePosition.SpotMove.SpotEndIndex - saccadePosition.SpotMove.SpotStartIndex) ;
            var spotMove = spotCoords.Skip(saccadePosition.SpotMove.SpotStartIndex).Take(spotLenght).ToArray();

            var eyeMoveDistancesOnScreen = CountOnScreenDistance(saccadeCoords)  ;
            var multipiedDistanceOnScreen = eyeMoveDistancesOnScreen.Sum();
            var spotMoveDistanceOnScreen = CountOnScreenDistance(spotMove).Sum();

            var tangens = multipiedDistanceOnScreen / this.distanceFromScreen;
            var ampInRadians = Math.Atan(tangens);
            var amplitude = Math.Round(RadianToDegree(ampInRadians));

            var maxEyeDistance = eyeMoveDistancesOnScreen.Max();
            var movePrecentage = (maxEyeDistance / multipiedDistanceOnScreen) * 100;
            var maxAmpPercentage = (amplitude / 100) * movePrecentage;
            var maxVelocity = maxAmpPercentage / 0.016; /* 0.016 = one frame*/

            var velocity = amplitude / duration;
            var gain = multipiedDistanceOnScreen / spotMoveDistanceOnScreen ;

            return new EyeMoveCalculation
            {
                EyeMove = saccadePosition,
                LatencyFrameCount = Convert.ToInt32(spotEyeIndexDiff),
                DurationFrameCount = Convert.ToInt32(frameCount),
                Latency = Math.Round(latency,3),
                Duration = Math.Round(duration, 3),
                Distance = Math.Round(multipiedDistanceOnScreen, 3), 
                Amplitude = amplitude,
                AvgVelocity = Math.Round(velocity, 0),
                MaxVelocity = Math.Round(maxVelocity, 0),
                Gain = Math.Round(gain, 2)
            };

           
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private List<double> GetFrameVelocityCollection(double[] saccadeCoords)
        {
            int currentPointStart = 0;
            int currentPointEnd = 1;
            var distances = new List<double>();

            while (currentPointEnd < saccadeCoords.Count())
            {
                var distanceDelta = Math.Abs(saccadeCoords[currentPointEnd] - saccadeCoords[currentPointStart]);
                distances.Add(distanceDelta);
                currentPointStart++;
                currentPointEnd++;
            }

            var velocities = new List<double>();

            foreach(var distanceOnScreen in distances)
            {
                var visualAngle = Math.Round((Math.Atan2(this.distanceFromScreen, distanceOnScreen)), 0);
                var saccadeTime = 2 /this.trackerFrequency;
                var velocity = visualAngle * saccadeTime;
                velocities.Add(velocity);

                //var tangent = distanceOnScreen / this.distanceFromScreen;
                //var arctangent = 2 * Math.Atan(tangent);
                //var output = arctangent * (180 / Math.PI);
            }

            return velocities;
        }

        private double ConvertPixelsToCentimeters(double value)
        {
            return value * 2.54 / 96;
        }
    }
}
