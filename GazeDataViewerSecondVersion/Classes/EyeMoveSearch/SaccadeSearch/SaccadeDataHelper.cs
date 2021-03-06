﻿using GazeDataViewer.Classes.EyeMoveSearch;
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
        public static EyeMove GetSaccadePositionItem(int id, int saccadeStartIndex, int saccadeEndIndex, int spotOverMeanStartIndex, int spotOverMeanEndIndex, ResultData results)
        {
            return new EyeMove
            {
                Id = id,
                EyeStartIndex = saccadeStartIndex,
                EyeStartTime = results.TimeDeltas[saccadeStartIndex],
                EyeStartCoord = results.EyeCoords[saccadeStartIndex],

                EyeEndIndex = saccadeEndIndex,
                EyeEndTime = results.TimeDeltas[saccadeEndIndex],
                EyeEndCoord = results.EyeCoords[saccadeEndIndex],

                SpotMove = new Spot.SpotMove
                {
                    SpotStartIndex = spotOverMeanStartIndex,
                    SpotStartTimeDelta = results.TimeDeltas[spotOverMeanStartIndex],
                    SpotStartCoord = results.SpotCoords[spotOverMeanStartIndex],

                    SpotEndIndex = spotOverMeanEndIndex,
                    SpotEndTimeDelta = results.TimeDeltas[spotOverMeanEndIndex],
                    SpotEndCoord = results.SpotCoords[spotOverMeanEndIndex]
                }

            };
        }

        public static EyeMove GetSaccadePositionItem(int id, int saccadeStartIndex, int saccadeEndIndex, EyeMove prevoiusSaccadeItem,  ResultData results)
        {
            var eyeMove = new EyeMove
            {
                Id = id,
                IsStartFound = prevoiusSaccadeItem.IsStartFound,
                EyeStartIndex = saccadeStartIndex,
                EyeStartTime = results.TimeDeltas[saccadeStartIndex],
                EyeStartCoord = results.EyeCoords[saccadeStartIndex],

                IsEndFound = prevoiusSaccadeItem.IsEndFound,
                EyeEndIndex = saccadeEndIndex,
                EyeEndTime = results.TimeDeltas[saccadeEndIndex],
                EyeEndCoord = results.EyeCoords[saccadeEndIndex],

                SpotMove = prevoiusSaccadeItem.SpotMove
            };

            eyeMove = EyeMoveSearchToolBox.CountTestValuesForEyeMove(eyeMove, results);
            return eyeMove;
        }


        public static EyeMove FindSaccade(int id, int spotStartIndex, int spotEndIndex, int latency, int minDuration, ResultData results)
        {

            var distanceFromScreen = 30;
            var trackerFrequency = 60;
            var controlWindowLength = 12;
            var saccadeFindWindowLength = 12;//controlWindowLength / 2;/// 2;


            var eyeStartIndex = spotStartIndex + latency;
            var spotStartOscilationXPosition = results.SpotCoords[spotStartIndex];
            var spotEndOscilationXPosition = results.SpotCoords[spotStartIndex + 1];

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


            var controlVelocity = windowsBeforeSpotVelocities.Average();
            //var controlAmplitude = windowsBeforeSpotAmplitudes.Average();

            var fixStartIndex = eyeStartIndex; //spotStartIndex;

            var numberOfStartWindows = 3;
            var startCoords = results.EyeCoords.Skip(fixStartIndex).Take(saccadeFindWindowLength * numberOfStartWindows).ToArray();
            eyeStartIndex = GetByStartIndexByCoords(startCoords, fixStartIndex, spotStartOscilationXPosition, spotEndOscilationXPosition);

            var startIndexByVelocityWindow = GetStartIndexByVelocity(results.EyeCoords, eyeStartIndex, controlVelocity, spotStartIndex,
                saccadeFindWindowLength, numberOfStartWindows, roundCoef, distanceFromScreen, trackerFrequency, minDuration);




            var saccadeStartIndex = eyeStartIndex;//startIndexByVelocityWindow;

            var saccadeStartXPosition = results.EyeCoords[saccadeStartIndex];
                  
            var endWindowStartIndex = saccadeStartIndex + minDuration;
                    
            double[] endWindowCoords; 
            if (results.EyeCoords.Skip(endWindowStartIndex).Take(5).Count() == 0)
            {
                endWindowCoords = new double[2] { results.EyeCoords[results.EyeCoords.Count() - 1], results.EyeCoords.Last() };
            }
            else
            {
                endWindowCoords = results.EyeCoords.Skip(endWindowStartIndex).Take(10).ToArray();
            }
                    
            var endWindowFrameDistances = CalculateDistances(endWindowCoords);
            var endWindowMaxFrameDistance = endWindowFrameDistances.Max();
            var endWindowMaxFrameDistanceIndex = Array.IndexOf(endWindowFrameDistances, endWindowMaxFrameDistance );

            var saccadeEndIndex = endWindowStartIndex + (endWindowMaxFrameDistanceIndex );

            if (saccadeStartIndex >= results.EyeCoords.Length)
            {
                saccadeStartIndex = results.EyeCoords.Length - minDuration;
            }
            if (saccadeEndIndex >= results.EyeCoords.Length)
            {
                saccadeEndIndex = results.EyeCoords.Length - 1;
            }

            return new EyeMove
            {
                Id = id,
                IsFirstMove = DataAnalyzer.IsEven(id),

                EyeStartIndex = saccadeStartIndex,
                EyeStartTime = results.TimeDeltas[saccadeStartIndex],
                EyeStartCoord = results.EyeCoords[saccadeStartIndex],

                EyeEndIndex = saccadeEndIndex,
                EyeEndTime = results.TimeDeltas[saccadeEndIndex],
                EyeEndCoord = results.EyeCoords[saccadeEndIndex],

                SpotMove = new Spot.SpotMove
                {
                    SpotStartIndex = spotStartIndex,
                    SpotStartTimeDelta = results.TimeDeltas[spotStartIndex],
                    SpotStartCoord = results.SpotCoords[spotStartIndex],

                    SpotEndIndex = spotEndIndex,
                    SpotEndTimeDelta = results.TimeDeltas[spotEndIndex],
                    SpotEndCoord = results.SpotCoords[spotEndIndex]
                }

            };

        }

        private static int GetByStartIndexByCoords(double[] startCoords, int eyeStartIndex, double spotStartOscilationXPosition, double spotEndOscilationXPosition)
        {
            int saccadeStartIndex = eyeStartIndex;
            var checkCount = 0;
            var checkCountMaxValue = 3;
            double? previousValue = null; 

            if (spotStartOscilationXPosition == 0 && spotEndOscilationXPosition == 1)
            {
                for (int i = 0; i < startCoords.Count(); i++)
                {
                    if (startCoords[i] > spotStartOscilationXPosition)
                    {
                        if (previousValue != null)
                        {
                            if (checkCount < checkCountMaxValue && startCoords[i] > previousValue)
                            {
                                checkCount++;
                            }
                            else
                            {
                                saccadeStartIndex = eyeStartIndex + (i - checkCount + 1);
                                break;
                            }
                        }
                        else
                        {
                            checkCount++;
                            previousValue = startCoords[i];
                        }
                    }
                    else
                    {
                        checkCount = 0;
                    }
                }
            }
            else if (spotStartOscilationXPosition == 1 && spotEndOscilationXPosition == 0)
            {
                
                for (int i = 0; i < startCoords.Count(); i++)
                {
                    if (startCoords[i] < spotStartOscilationXPosition)
                    {
                        if (previousValue != null)
                        {
                            if (checkCount < checkCountMaxValue && startCoords[i] < previousValue)
                            {
                                checkCount++;
                            }
                            else
                            {
                                saccadeStartIndex = eyeStartIndex + (i - checkCount + 1);
                                break;
                            }
                        }
                        else
                        {
                            checkCount++;
                            previousValue = startCoords[i];
                        }
                    }
                    else
                    {
                        checkCount = 0;
                    }
                }
            }
            else if (spotStartOscilationXPosition == 0 && spotEndOscilationXPosition == -1)
            {
                var cordIndex = 0;
                foreach (var cord in startCoords)
                {
                    if (cord < spotStartOscilationXPosition)
                    {
                        if (previousValue != null)
                        {
                            if (cord < previousValue)
                            {
                                if (checkCount < checkCountMaxValue)
                                {
                                    checkCount++;
                                    previousValue = cord;
                                }
                                else
                                {
                                    saccadeStartIndex = eyeStartIndex + (cordIndex - checkCount + 1);
                                    break;
                                }
                            }
                            else
                            {
                                checkCount = 0;
                                previousValue = cord;
                            }
                        }
                        else
                        {
                            checkCount++;
                            previousValue = cord;
                        }
                    }
                    cordIndex++;
                }

                for (int k = 0; k < startCoords.Count(); k++)
                {
                    if (startCoords[k] < spotStartOscilationXPosition)
                    {
                        if (previousValue != null)
                        {
                            if (checkCount < checkCountMaxValue && startCoords[k] < previousValue)
                            {
                                checkCount++;
                            }
                            else
                            {
                                saccadeStartIndex = eyeStartIndex + (k - checkCount + 1);
                                break;
                            }
                        }
                        else
                        {
                            checkCount++;
                            previousValue = startCoords[k];
                        }
                    }
                    else
                    {
                        checkCount = 0;
                    }
                }
            }
            else if (spotStartOscilationXPosition == -1 && spotEndOscilationXPosition == 0)
            {
                for (int i = 0; i < startCoords.Count(); i++)
                {
                    if (startCoords[i] > spotStartOscilationXPosition)
                    {
                        if (checkCount < checkCountMaxValue)
                        {
                            checkCount++;
                        }
                        else
                        {
                            saccadeStartIndex = eyeStartIndex + (i - checkCount + 1);
                            break;
                        }
                    }
                }
            }

            return saccadeStartIndex;
        }


        private static int GetStartIndexByVelocity(double[] eyeCoords, int eyeStartIndex, double controlVelocity, int spotStartIndex, int saccadeFindWindowLength, 
            int numberOfStartWindows, int roundCoef, int distanceFromScreen, int trackerFrequency, int minDuration)
        {
            var saccadeStartIndexByVel = eyeStartIndex;
            var eyeStartWindows = GetWindows(eyeCoords, eyeStartIndex, saccadeFindWindowLength, numberOfStartWindows);

            var amplitudesOfWindows = GetAmplitudeCollectionFromWindows(eyeStartWindows, roundCoef, distanceFromScreen);//new Dictionary<int, double>();
            var velocitiesOfWindows = GetVelocityCollectionFromWindows(eyeStartWindows, roundCoef, distanceFromScreen, trackerFrequency); //new Dictionary<int, double>();


            if (velocitiesOfWindows.Count > 0)
            {
                var saccadeStartIndex = eyeStartIndex;
                var velocityToCompare = controlVelocity + ((controlVelocity / 100) * 10);
                var windowVelocitiesOverControl = velocitiesOfWindows.Where(x => x.Value > velocityToCompare);

                if (windowVelocitiesOverControl.Count() == 0)
                {
                    windowVelocitiesOverControl.ToList().Add(velocitiesOfWindows.FirstOrDefault());//.Where(x => x.Value > controlVelocity);
                }


                if (windowVelocitiesOverControl.Count() > 0)
                {
                    var firstVelocityWindow = windowVelocitiesOverControl.First();
                    var firstWindow = eyeStartWindows.First(x => x.Key == firstVelocityWindow.Key);
                    var firstWindowFrameDistances = CalculateDistances(firstWindow.Value);

                    var eyeFixatedCoords = eyeCoords.Skip(spotStartIndex - 12).Take(minDuration).ToArray();
                    var eyeFixatedDistances = CalculateDistances(eyeFixatedCoords);
                    var eyeFixatedDistancesAverge = eyeFixatedDistances.Average();


                    var firstWindowMaxFrameDistanceIndex = -1;
                    for (int o = 0; o < firstWindowFrameDistances.Count(); o++)
                    {
                        var currentDistance = firstWindowFrameDistances[o];
                        if (currentDistance > (eyeFixatedDistancesAverge * 3))
                        {
                            firstWindowMaxFrameDistanceIndex = o;
                            break;
                        }
                    }

                    if (firstWindowMaxFrameDistanceIndex == -1)
                    {
                        firstWindowMaxFrameDistanceIndex = firstWindowFrameDistances.ToList().IndexOf(firstWindowFrameDistances.Max());
                    }


                    var startIndexByVelocityWindow = eyeStartIndex + (10 * firstVelocityWindow.Key);
                    saccadeStartIndexByVel = startIndexByVelocityWindow + firstWindowMaxFrameDistanceIndex;
                    
                }
            }

            return saccadeStartIndexByVel;
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

        public static List<double> CountOnScreenDistance(double[] saccadeCoords)
        {
            int currentPointStart = 0;
            int currentPointEnd = 1;
            var distanceDeltas = new List<double>();

            while (currentPointEnd < saccadeCoords.Count())
            {
                var distanceDelta = Math.Abs(saccadeCoords[currentPointEnd] - saccadeCoords[currentPointStart]);
                distanceDeltas.Add(distanceDelta * 7);
                currentPointStart++;
                currentPointEnd++;
            }

            return distanceDeltas;
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


        public static int CountEyeShiftIndex(int eyeStartIndex, int eyeShiftPeriod)
        {
            return eyeStartIndex + eyeShiftPeriod;
        }

        public static int CountSpotShiftIndex(int spotStartIndex, int spotShiftPeriod)
        {
            return spotStartIndex + spotShiftPeriod;
        }
    }
}
