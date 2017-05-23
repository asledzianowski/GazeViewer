using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public static class SaccadeDataHelper
    {
        public static SaccadePosition GetSaccadePositionItem(int id, int saccadeStartIndex, int saccadeEndIndex, int spotOverMeanStartIndex, int spotOverMeanEndIndex, SpotGainResults results)
        {
            return new SaccadePosition
            {
                Id = id,
                SaccadeStartIndex = saccadeStartIndex,
                SaccadeStartTime = results.PlotData.TimeStamps[saccadeStartIndex],
                SaccadeStartCoord = results.PlotData.EyeCoords[saccadeStartIndex],

                SaccadeEndIndex = saccadeEndIndex,
                SaccadeEndTime = results.PlotData.TimeStamps[saccadeEndIndex],
                SaccadeEndCoord = results.PlotData.EyeCoords[saccadeEndIndex],

                SpotStartIndex = spotOverMeanStartIndex,
                SpotStartTime = results.PlotData.TimeStamps[spotOverMeanStartIndex],
                SpotStartCoord = results.PlotData.SpotCoords[spotOverMeanStartIndex],

                SpotEndIndex = spotOverMeanEndIndex,
                SpotEndTime = results.PlotData.TimeStamps[spotOverMeanEndIndex],
                SpotEndCoord = results.PlotData.SpotCoords[spotOverMeanEndIndex],

            };
        }

        public static SaccadePosition GetSaccadePositionItem(int id, int saccadeStartIndex, int saccadeEndIndex, int spotOverMeanStartIndex,
            double spotStartTime, double spotStartCoord, int spotOverMeanEndIndex,
            double spotEndTime, double spotEndCoord, SpotGainResults results)
        {
            return new SaccadePosition
            {
                Id = id,
                SaccadeStartIndex = saccadeStartIndex,
                SaccadeStartTime = results.PlotData.TimeStamps[saccadeStartIndex],
                SaccadeStartCoord = results.PlotData.EyeCoords[saccadeStartIndex],

                SaccadeEndIndex = saccadeEndIndex,
                SaccadeEndTime = results.PlotData.TimeStamps[saccadeEndIndex],
                SaccadeEndCoord = results.PlotData.EyeCoords[saccadeEndIndex],

                SpotStartIndex = spotOverMeanStartIndex,
                SpotStartTime = spotStartTime,
                SpotStartCoord = spotStartCoord,

                SpotEndIndex = spotOverMeanEndIndex,
                SpotEndTime = spotEndTime,
                SpotEndCoord = spotEndCoord,

            };
        }

        public static SaccadePosition FindSaccade(int id, int spotStartIndex, int spotEndIndex, int latency, int minDuration, PlotData results)
        {

            if (id == 4)
            {

            }
            //TODO: Latency
          
            var distanceFromScreen = 30;
            var trackerFrequency = 60;
            var controlWindowLength = 12;
            var saccadeFindWindowLength = 12;//controlWindowLength / 2;/// 2;


            var eyeStartIndex = spotStartIndex + latency;
            var controlWindowStartIndex = spotStartIndex;

            var roundCoef = 5;
            int numberOfControlWindows = 6;

            var windowsBeforeSpot = new List<double[]>();
            var windowsBeforeSpotVelocities = new List<double>();
            var windowsBeforeSpotAmplitudes = new List<double>();
            var currentStartIndex = controlWindowStartIndex - controlWindowLength;
            for (int i = 0; i < numberOfControlWindows; i++)
            {
                var currentWindowCoords = results.EyeCoords.Skip(currentStartIndex).Take(controlWindowLength).ToArray();
                var currentControlVelocity = Math.Round(GetAvergeVelocityForSection(currentWindowCoords, distanceFromScreen, trackerFrequency), roundCoef);
                var currentControlAmplitude = Math.Round(GetAmplitudeForSection(currentWindowCoords, distanceFromScreen), roundCoef);

                windowsBeforeSpot.Add(currentWindowCoords);
                windowsBeforeSpotVelocities.Add(currentControlVelocity);
                windowsBeforeSpotAmplitudes.Add(currentControlAmplitude);

                currentStartIndex = currentStartIndex - controlWindowLength;
            }

            //var controlWindowCoords = results.EyeCoords.Skip(controlWindowStartIndex - controlWindowLength).Take(controlWindowLength).ToArray();
            //var controlWindowVelocities = GetFrameVelocityCollection(controlWindowCoords, distanceFromScreen, trackerFrequency);
            //var controlVelocity = controlWindowVelocities.Max();


            //var controlVelocity = Math.Round(GetAvergeVelocityForSection(controlWindowCoords, distanceFromScreen, trackerFrequency), roundCoef);
            //var controlAmplitude = Math.Round(GetAmplitudeForSection(controlWindowCoords, distanceFromScreen), roundCoef);

            var controlVelocity = windowsBeforeSpotVelocities.Average();
            var controlAmplitude = windowsBeforeSpotAmplitudes.Average();

            var eyeStartWindows = GetWindows(results.EyeCoords, eyeStartIndex, saccadeFindWindowLength, 12);

            var amplitudesOfWindows = GetAmplitudeCollectionFromWindows(eyeStartWindows, roundCoef, distanceFromScreen);//new Dictionary<int, double>();
            var velocitiesOfWindows = GetVelocityCollectionFromWindows(eyeStartWindows, roundCoef, distanceFromScreen, trackerFrequency); //new Dictionary<int, double>();

            //for (int i = 0; i < windows.Count; i++)
            //{
            //    if(windows[i].Length > 0)
            //    {
            //        var windowVelocity = GetAvergeVelocityForSection(windows[i], distanceFromScreen, trackerFrequency);
            //        velocitiesOfWindows.Add(i, Math.Round(windowVelocity, roundCoef));

            //        var windowAmplitude = GetAmplitudeForSection(windows[i], distanceFromScreen);
            //        amplitudesOfWindows.Add(i, Math.Round(windowAmplitude, roundCoef));
            //    }
                
            //}


            if (velocitiesOfWindows.Count > 0)
            {

                var velocityToCompare = controlVelocity + ((controlVelocity / 100) * 20);
                var windowVelocitiesOverControl = velocitiesOfWindows.Where(x => x.Value > velocityToCompare);
                var numberOfStartWindows = 9;
                if(windowVelocitiesOverControl.Count() == 0)
                {
                    eyeStartWindows = GetWindows(results.EyeCoords, eyeStartIndex + saccadeFindWindowLength, saccadeFindWindowLength, numberOfStartWindows);

                    amplitudesOfWindows = GetAmplitudeCollectionFromWindows(eyeStartWindows, roundCoef, distanceFromScreen);//new Dictionary<int, double>();
                    velocitiesOfWindows = GetVelocityCollectionFromWindows(eyeStartWindows, roundCoef, distanceFromScreen, trackerFrequency); //new Dictionary<int, double>();

                    windowVelocitiesOverControl = velocitiesOfWindows.Where(x => x.Value > controlVelocity);
                   
                }
                int saccadeStartIndex = 0;

                if (windowVelocitiesOverControl.Count() > 0)
                {
                    var firstVelocityWindow = windowVelocitiesOverControl.First();
                    var firstWindow = eyeStartWindows.First(x => x.Key == firstVelocityWindow.Key);
                    var firstWindowFrameDistances = CalculateDistances(firstWindow.Value);
                    var firstWindowMaxFrameDistance = firstWindowFrameDistances.Max();
                    var firstWindowMaxFrameDistanceIndex = Array.IndexOf(firstWindowFrameDistances, firstWindowMaxFrameDistance);

                    //var indexDiff = Math.Abs(firstWindowMaxFrameDistanceIndex - eyeStartIndex);
                    var startIndexByVelocityWindow = eyeStartIndex + (10 * firstVelocityWindow.Key );
                    saccadeStartIndex = startIndexByVelocityWindow + firstWindowMaxFrameDistanceIndex ;

                    var saccadeStartXPosition = results.EyeCoords[saccadeStartIndex];
                    var spotStartOscilationXPosition = results.SpotCoords[spotStartIndex];
                    var spotEndOscilationXPosition = results.SpotCoords[spotStartIndex + 1];

                    var fixPosition = false;

                    if (spotStartOscilationXPosition < spotEndOscilationXPosition)
                    {
                        if (saccadeStartXPosition > spotEndOscilationXPosition)
                        {
                            fixPosition = true;
                        }
                    }
                    else
                    {
                        if (saccadeStartXPosition > spotEndOscilationXPosition)
                        {
                            fixPosition = true;
                        }
                    }

                    //if (saccadeStartXPosition > spotEndOscilationXPosition)
                    if (fixPosition)
                    {
                        var fixStartIndex = eyeStartIndex; //spotStartIndex;
                        var startCoords = results.EyeCoords.Skip(fixStartIndex).Take(saccadeFindWindowLength * numberOfStartWindows).ToArray();
                        double? startCoord ;


                        if (spotStartOscilationXPosition == 0 && spotEndOscilationXPosition == 1)
                        {
                            for (int i = 0; i < startCoords.Count(); i++)
                            {
                                if (startCoords[i] > spotStartOscilationXPosition)
                                {
                                    saccadeStartIndex = fixStartIndex + i;
                                    break;
                                }
                            }
                        }
                        else if (spotStartOscilationXPosition == 1 && spotEndOscilationXPosition == 0)
                        {
                            for (int i = 0; i < startCoords.Count(); i++)
                            {
                                if (startCoords[i] < spotStartOscilationXPosition)
                                {
                                    saccadeStartIndex = fixStartIndex + i;
                                    break;
                                }
                            }
                        }
                        else if (spotStartOscilationXPosition == 0 && spotEndOscilationXPosition == -1)
                        {
                            for (int k = 0; k < startCoords.Count(); k++)
                            {
                                if (!IsPositive(startCoords[k], k))
                                {
                                    saccadeStartIndex = fixStartIndex + k;
                                    break;
                                }
                            }
                        }
                        else if (spotStartOscilationXPosition == -1 && spotEndOscilationXPosition == 0)
                        {
                            for (int i = 0; i < startCoords.Count(); i++)
                            {
                                if (startCoords[i] > spotStartOscilationXPosition)
                                {
                                    saccadeStartIndex = fixStartIndex + i;
                                    break;
                                }
                            }
                        }

                        //if (spotEndOscilationXPosition < 0)
                        //{
                        //    for(int i = 0; i < startCoords.Count(); i++ )
                        //    {
                        //        if(startCoords[i] < spotStartOscilationXPosition)
                        //        {
                        //            saccadeStartIndex = spotStartIndex + i;
                        //            break;
                        //        }
                        //    }
                        //    //startCoord = startCoords.FirstOrDefault(x => x < spotStartXPosition);
                        //}
                        //else
                        //{
                        //    for (int i = 0; i < startCoords.Count(); i++)
                        //    {
                        //        if (startCoords[i] < spotStartOscilationXPosition)
                        //        {
                        //            saccadeStartIndex = spotStartIndex + i;
                        //            break;
                        //        }
                        //    }
                        //    //startCoord = startCoords.FirstOrDefault(x => x > spotStartXPosition);
                        //}

                        //if (startCoord != null)
                        //{
                        //    saccadeStartIndex = Array.IndexOf(results.EyeCoords, startCoord);
                        //}
                    }

                    //accadeStartIndex--;
                    var endWindowStartIndex = saccadeStartIndex + 3;
                    //var spotLength = Math.Abs(spotEndIndex - spotStartIndex);

                    double[] endWindowCoords; 
                    if (results.EyeCoords.Skip(endWindowStartIndex).Take(5).Count() == 0)
                    {
                        endWindowCoords = new double[2] { results.EyeCoords[results.EyeCoords.Count() - 1], results.EyeCoords.Last() };
                    }
                    else
                    {
                        ///var lenghtToSpot =  Math.Abs(spotEndIndex - endWindowStartIndex);
                        endWindowCoords = results.EyeCoords.Skip(endWindowStartIndex).Take(4).ToArray();
                    }
                    
                    var endWindowFrameDistances = CalculateDistances(endWindowCoords);
                    var endWindowMaxFrameDistance = endWindowFrameDistances.Max();
                    var endWindowMaxFrameDistanceIndex = Array.IndexOf(endWindowFrameDistances, endWindowMaxFrameDistance );

                    var saccadeEndIndex = endWindowStartIndex + (endWindowMaxFrameDistanceIndex );

                    // var saccadeEndIndex = saccadeStartIndex + Math.Abs(spotEndIndex - spotStartIndex);

                    if (saccadeStartIndex >= results.EyeCoords.Length)
                    {
                        saccadeStartIndex = results.EyeCoords.Length - minDuration;
                    }
                    if (saccadeEndIndex >= results.EyeCoords.Length)
                    {
                        saccadeEndIndex = results.EyeCoords.Length - 1;
                    }

                    return new SaccadePosition
                    {
                        Id = id,

                        SaccadeStartIndex = saccadeStartIndex,
                        SaccadeStartTime = results.TimeStamps[saccadeStartIndex],
                        SaccadeStartCoord = results.EyeCoords[saccadeStartIndex],

                        SaccadeEndIndex = saccadeEndIndex,
                        SaccadeEndTime = results.TimeStamps[saccadeEndIndex],
                        SaccadeEndCoord = results.EyeCoords[saccadeEndIndex],

                        SpotStartIndex = spotStartIndex,
                        SpotStartTime = results.TimeStamps[spotStartIndex],
                        SpotStartCoord = results.SpotCoords[spotStartIndex],

                        SpotEndIndex = spotEndIndex,
                        SpotEndTime = results.TimeStamps[spotEndIndex],
                        SpotEndCoord = results.SpotCoords[spotEndIndex],

                    };
                }
                else
                {
                    return new SaccadePosition
                    {
                        Id = id,

                        SaccadeStartIndex = eyeStartIndex,
                        SaccadeStartTime = results.TimeStamps[eyeStartIndex],
                        SaccadeStartCoord = results.EyeCoords[eyeStartIndex],

                        SaccadeEndIndex = eyeStartIndex + 3,
                        SaccadeEndTime = results.TimeStamps[eyeStartIndex + 3],
                        SaccadeEndCoord = results.EyeCoords[eyeStartIndex + 3],


                        SpotStartIndex = spotStartIndex,
                        SpotStartTime = results.TimeStamps[spotStartIndex],
                        SpotStartCoord = results.SpotCoords[spotStartIndex],

                        SpotEndIndex = spotEndIndex,
                        SpotEndTime = results.TimeStamps[spotEndIndex],
                        SpotEndCoord = results.SpotCoords[spotEndIndex],

                    };
                }
            }
            else
            {
                return new SaccadePosition
                {
                    Id = id,

                    SaccadeStartIndex = eyeStartIndex,
                    SaccadeStartTime = results.TimeStamps[eyeStartIndex],
                    SaccadeStartCoord = results.EyeCoords[eyeStartIndex],

                    SaccadeEndIndex = eyeStartIndex + 3,
                    SaccadeEndTime = results.TimeStamps[eyeStartIndex + 3],
                    SaccadeEndCoord = results.EyeCoords[eyeStartIndex + 3],

                    SpotStartIndex = spotStartIndex,
                    SpotStartTime = results.TimeStamps[spotStartIndex],
                    SpotStartCoord = results.SpotCoords[spotStartIndex],

                    SpotEndIndex = spotEndIndex,
                    SpotEndTime = results.TimeStamps[spotEndIndex],
                    SpotEndCoord = results.SpotCoords[spotEndIndex],

                };
            }

           

            

            

            //var saccadeStartWindow1 = results.EyeCoords.Skip(eyeStartIndex).Take(saccadeFindWindowLength).ToArray();

            //var saccadeStartVelocity1 = GetAvergeVelocityForSection(saccadeStartWindow1, distanceFromScreen, trackerFrequency);
            //var saccadeStar1tAmplitude1 = GetAmplitudeForSection(saccadeStartWindow1, distanceFromScreen);

            //var saccadeStartWindow2 = results.EyeCoords.Skip(eyeStartIndex + saccadeFindWindowLength).Take(saccadeFindWindowLength).ToArray();

            //var saccadeStartVelocity2 = GetAvergeVelocityForSection(saccadeStartWindow2, distanceFromScreen, trackerFrequency);
            //var saccadeStar1tAmplitude2 = GetAmplitudeForSection(saccadeStartWindow2, distanceFromScreen);

            //var saccadeStartWindowVelcoities = GetFrameVelocityCollection(saccadeStartWindow1, distanceFromScreen, trackerFrequency);

            //var velocities2 = new List<double>();
            //var angles = new List<double>();
            //var saccadeStartDistances = CalculateDistances(saccadeStartWindow1);

            //foreach (var distanceOnScreen in saccadeStartDistances)
            //{

            //    var visualAngle = Math.Atan2(distanceFromScreen, distanceOnScreen);
            //    var velocity = distanceOnScreen / trackerFrequency;
            //    angles.Add(visualAngle);
            //    velocities2.Add(Math.Round(velocity, 3));
            //}

            //var saccadeStartIndex = 0;
            //for (int i = 0; i < saccadeStartWindowVelcoities.Count; i++)
            //{
            //    if (saccadeStartWindowVelcoities[i] >= controlVelocity)
            //    {
            //        saccadeStartIndex = i;
            //        break;
            //    }
            //}

            //var saccadeEndWindow = results.EyeCoords.Skip(saccadeStartIndex + minDuration).Take(controlWindowLength).ToArray();
            //var saccadeEndWindowVelcoities = GetFrameVelocityCollection(saccadeEndWindow, distanceFromScreen, trackerFrequency);

            //var saccadeEndIndex = 0;
            //for (int i = 0; i < saccadeEndWindowVelcoities.Count; i++)
            //{
            //    if (saccadeEndWindowVelcoities[i] <= controlVelocity)
            //    {
            //        saccadeEndIndex = i;
            //        break;
            //    }
            //}

            //return new SaccadePosition
            //{
            //    Id = id,

            //    SaccadeStartIndex = saccadeStartIndex,
            //    SaccadeStartTime = results.TimeStamps[saccadeStartIndex],
            //    SaccadeStartCoord = results.EyeCoords[saccadeStartIndex],

            //    SaccadeEndIndex = saccadeEndIndex,
            //    SaccadeEndTime = results.TimeStamps[saccadeEndIndex],
            //    SaccadeEndCoord = results.EyeCoords[saccadeEndIndex],

            //    SpotStartIndex = spotStartIndex,
            //    SpotStartTime = results.TimeStamps[spotStartIndex],
            //    SpotStartCoord = results.SpotCoords[spotStartIndex],

            //    SpotEndIndex = spotEndIndex,
            //    SpotEndTime = results.TimeStamps[spotEndIndex],
            //    SpotEndCoord = results.SpotCoords[spotEndIndex],

            //};

        }

        private static bool IsPositive(double value, int i)
        {
            if (value == Math.Abs(value))
            {
                return true;
            }
            else {
                return false;
            }
        }

        private static Dictionary<int, double> GetVelocityCollectionFromWindows(Dictionary<int, double[]> windows, int roundCoef, int distanceFromScreen, int trackerFrequency)
        {
            var velocitiesOfWindows = new Dictionary<int, double>();

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].Length > 0)
                {
                    var windowVelocity = GetAvergeVelocityForSection(windows[i], distanceFromScreen, trackerFrequency);
                    velocitiesOfWindows.Add(i, Math.Round(windowVelocity, roundCoef));

                }

            }

            return velocitiesOfWindows;
        }

        private static Dictionary<int, double> GetAmplitudeCollectionFromWindows(Dictionary<int, double[]> windows, int roundCoef, int distanceFromScreen)
        {
            var amplitudesOfWindows = new Dictionary<int, double>();

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].Length > 0)
                {
                    var windowAmplitude = GetAmplitudeForSection(windows[i], distanceFromScreen);
                    amplitudesOfWindows.Add(i, Math.Round(windowAmplitude, roundCoef));

                }

            }

            return amplitudesOfWindows;
        }

        private static Dictionary<int, double[]> GetWindows(double[] eyeCoords, int startIndex, int windowLength, int numOfWindows)
        {

            var windows = new Dictionary<int, double[]>();

            for (int i =0; i < numOfWindows; i++)
            {
                double[] window;

                if (windowLength <= (eyeCoords.Length - startIndex))
                {
                    window = eyeCoords.Skip(startIndex).Take(windowLength).ToArray();
                }
                else
                {
                    window = eyeCoords.Skip(startIndex).ToArray();
                }
               
                windows.Add(i, window);
                startIndex = startIndex + windowLength;
            }
            return windows;
           
        }


        private static double GetAvergeVelocityForSection(double[] saccadeCoords, double distanceFromScreen, double trackerFrequency)
        {
            var displacement = Math.Abs(saccadeCoords[saccadeCoords.Count() - 1] - saccadeCoords[0]);
            var velocity = displacement / trackerFrequency;
            return velocity;
        }


        private static double GetAmplitudeForSection(double[] saccadeCoords, double distanceFromScreen)
        {
            //var distanceOnScreen = Math.Abs(saccadeCoords[saccadeCoords.Count() - 1] - saccadeCoords[0]);
            var distanceOnScreen = CalculateTotalDistance(saccadeCoords);
            var amplitude = Math.Atan2(distanceFromScreen, distanceOnScreen);
            return amplitude;
        }

        private static List<double> GetFrameVelocityCollection(double[] saccadeCoords, double distanceFromScreen, double trackerFrequency)
        {
            int currentPointStart = 0;
            int currentPointEnd = 1;
            var distances = new List<double>();

            while (currentPointEnd < saccadeCoords.Count())
            {
                var distanceDelta = Math.Abs(saccadeCoords[currentPointEnd] - saccadeCoords[currentPointStart]);
                distances.Add(Math.Round(distanceDelta, 3));
                currentPointStart++;
                currentPointEnd++;
            }

            var velocities = new List<double>();
            var angles = new List<double>();

            foreach (var distanceOnScreen in distances)
            {
                var visualAngle = Math.Atan2(distanceFromScreen, distanceOnScreen);
                var saccadetime = 2f / trackerFrequency;
                var velocity = visualAngle * saccadetime;
                velocities.Add(Math.Round(velocity, 3));

                angles.Add(visualAngle);
                


                //var tangent = distanceOnScreen / this.distanceFromScreen;
                //var arctangent = 2 * Math.Atan(tangent);
                //var output = arctangent * (180 / Math.PI);
            }

            return velocities;
        }

        private static double CalculateTotalDistance(double[] coords)
        {
            int currentPointStart = 0;
            int currentPointEnd = 1;
            double distance = 0;

            while (currentPointEnd < coords.Count())
            {
                var distanceDelta = Math.Abs(coords[currentPointEnd] - coords[currentPointStart]);
                distance = distanceDelta + distance;
                currentPointStart++;
                currentPointEnd++;
            }

            return distance;
        }

        private static double[] CalculateDistances(double[] coords)
        {
            int currentPointStart = 0;
            int currentPointEnd = 1;
            var distances = new List<double>();

            while (currentPointEnd < coords.Count())
            {
                var distanceDelta = Math.Abs(coords[currentPointEnd] - coords[currentPointStart]);
                distances.Add(distanceDelta);
                currentPointStart++;
                currentPointEnd++;
            }

            return distances.ToArray();
        } 

        public static int CountEyeStartIndex(int eyeStartIndex, int eyeStartShiftPeriod)
        {
            return eyeStartIndex + eyeStartShiftPeriod;
        }


        public static int CountEyeShiftIndex(int eyeStartIndex, int eyeShiftPeriod,  int saccadeEndShiftPeroid)
        {
            return eyeStartIndex + eyeShiftPeriod + saccadeEndShiftPeroid;
        }

        public static int CountSpotShiftIndex(int spotStartIndex, int spotShiftPeriod)
        {
            return spotStartIndex + spotShiftPeriod;
        }
    }
}
