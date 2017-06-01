using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class SaccadeParamsCalcuator
    {
        private readonly List<SaccadePosition> saccadePositions = null;

        private readonly double[] eyeCoords = null;

        private readonly double[] spotCoords = null;

        private readonly double distanceFromScreen;

        private readonly double trackerFrequency;

        public SaccadeParamsCalcuator(List<SaccadePosition> saccadePositions, double[] eyeCoords, double[] spotCoords, int distanceFromScreen, int trackerFrequency)
        {
            this.saccadePositions = saccadePositions;
            this.eyeCoords = eyeCoords;
            this.spotCoords = spotCoords;
            this.distanceFromScreen = distanceFromScreen;
            this.trackerFrequency = trackerFrequency;
        }

        public List<SaccadeCalculation> Calculate()
        {
            var saccCalculations = new List<SaccadeCalculation>();
            foreach(var saccadePosition in saccadePositions)
            {
                var saccParams = CalculateSaccadeParams(saccadePosition, this.eyeCoords, this.spotCoords);
                saccCalculations.Add(saccParams);
            }

            return saccCalculations;
        }


        private double CountOnScreenDistance(double[] saccadeCoords)
        {
            double distance = 0;
            int currentPointStart = 0;
            int currentPointEnd = 1;

            while (currentPointEnd < saccadeCoords.Count())
            {
                var distanceDelta = Math.Abs(saccadeCoords[currentPointEnd] - saccadeCoords[currentPointStart]);
                distance = distance + distanceDelta;
                currentPointStart++;
                currentPointEnd++;
            }
            return distance;
        }

       

        private SaccadeCalculation CalculateSaccadeParams(SaccadePosition saccadePosition, double[] eyeCoords, double[] spotCoords)
        {

            //var arctang = Math.Atan2(30, 7);
            //var testAmp = Math.Round(arctang * 10f, 0);
            //double saccadeTestTime = this.trackerFrequency / 3;
            //var velocityTest = testAmp * saccadeTestTime;

            //Zalozmy ze jest to 7 cm na ekranie przy odleglosci oka od ekranu 30 cm
            //To przesuniecie katowe bedzie artag(7 / 30) = 13deg
            //Jesli zalozmy czas sakady 3 ramki tj  3 / 60 = 1 / 20 s to szybkosc 13 * 20 = 260deg / s

            //var test = Math.Atan2(this.distanceFromScreen, 7);
            //var visualAngle3 = Math.Round((Math.Atan2(this.distanceFromScreen, 0.1) * 10), 0);
            //var visualAngle1 = Math.Round((Math.Atan2(this.distanceFromScreen, 7) * 10), 0);
            //var visualAngle2 = Math.Round((Math.Atan2(this.distanceFromScreen, 70) * 10), 0);

            var frameCount = Math.Abs(saccadePosition.SaccadeEndIndex - saccadePosition.SaccadeStartIndex); //+ 1;


            double[] saccadeCoords;
            
            if(eyeCoords.Length > 1)
            {
                saccadeCoords = eyeCoords.Skip(saccadePosition.SaccadeStartIndex).Take(Math.Abs(saccadePosition.SaccadeEndIndex - saccadePosition.SaccadeStartIndex) + 1).ToArray();
            }    
            else
            {
                saccadeCoords = eyeCoords;
            }
                
            var spotLenght = (saccadePosition.SpotEndIndex - saccadePosition.SpotStartIndex) ;
            var spotMove = spotCoords.Skip(saccadePosition.SpotStartIndex).Take(spotLenght).ToArray();

            var eyeMoveDistanceOnScreen = CountOnScreenDistance(saccadeCoords)  ;
            var spotMoveDistanceOnScreen = CountOnScreenDistance(spotMove);

            var eyeMoveVisualAngle = Math.Round((Math.Atan2(this.distanceFromScreen, eyeMoveDistanceOnScreen) * 10) ,0);
            var spotMoveVisualAngle = Math.Round((Math.Atan2(this.distanceFromScreen, spotMoveDistanceOnScreen) * 10), 0);
            //var gain = eyeMoveVisualAngle / spotMoveVisualAngle;

            var gain = eyeMoveDistanceOnScreen / spotMoveDistanceOnScreen ;


            //var saccadeTime = this.trackerFrequency / frameCount;
            //var saccadeTime2 = (1 / this.trackerFrequency) * frameCount;
            //var velocityOrginal = eyeMoveVisualAngle /* * */ / saccadeTime;
            //var velocity2 = eyeMoveVisualAngle /* * */ / saccadeTime2;
            // var velocity3 = eyeMoveVisualAngle /* * */ / trackerFrequency;

            var duration = frameCount / this.trackerFrequency; ;
            var velocity = eyeMoveVisualAngle / duration;

            //double duration = frameCount / this.trackerFrequency;
            double duration2 = (1 / this.trackerFrequency) * frameCount;

            var spotEyeIndexDiff = Math.Abs(saccadePosition.SaccadeStartIndex - saccadePosition.SpotStartIndex);
            var latency = (spotEyeIndexDiff / this.trackerFrequency);
            var latencyFrameCount = spotEyeIndexDiff;
            //var latency = 1f / latencySpan;

            var velocities = GetFrameVelocityCollection(saccadeCoords);

            return new SaccadeCalculation
            {
                Id = saccadePosition.Id,
                SpotStartIndex = saccadePosition.SpotStartIndex,
                SpotEndIndex = saccadePosition.SpotEndIndex,
                EyeStartIndex = saccadePosition.SaccadeStartIndex,
                EyeEndIndex = saccadePosition.SaccadeEndIndex,

                LatencyFrameCount = Convert.ToInt32(spotEyeIndexDiff),
                DurationFrameCount = Convert.ToInt32(frameCount),
                Latency = Math.Round(latency,3),
                Duration = Math.Round(duration, 3),
                Distance = Math.Round(eyeMoveDistanceOnScreen, 3) * 10,
                Amplitude = Math.Round(eyeMoveVisualAngle, 3),
                Velocity = Math.Round(velocity, 0),
                //MaxVelocity = Math.Round(velocities.Max(),3),
                //AvgVelocity = Math.Round(velocities.Average(), 3),
                Gain = Math.Round(gain, 2)
            };

           
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
