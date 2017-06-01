using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class SaccadeFinder
    {
        int distanceFromScreen = 30;
        int trackerFrequency = 60;
        int controlWindowLength = 50;
        int saccadeFindWindowLength = 50;

        public SaccadeFinder(int distanceFromScreen, int trackerFrequency, int controlWindowLength, int saccadeFindWindowLength)
        {
            this.distanceFromScreen = distanceFromScreen;
            this.trackerFrequency = trackerFrequency;
            this.controlWindowLength = controlWindowLength;
            this.saccadeFindWindowLength = saccadeFindWindowLength;
        }

        public SaccadePosition FindSaccade(int id, int spotStartIndex, int spotEndIndex, int latency, int minDuration, PlotData results)
        {
            var spotStartOscilationXPosition = results.SpotCoords[spotStartIndex];
            var spotEndOscilationXPosition = results.SpotCoords[spotStartIndex + 1];

            var controlWindowStartIndex = spotStartIndex;
            var roundCoef = 5;
            int numberOfControlWindows = 6;

            var controlWindowCoords = results.EyeCoords.Skip(spotStartIndex - controlWindowLength).Take(controlWindowLength).ToArray();

            var controlMaxCord = controlWindowCoords.Max();
            var controlMinCord = controlWindowCoords.Min();
            var meanControlAmplitude = (controlMaxCord - controlMinCord) / 2;


            var eyeStartIndex = spotStartIndex + latency;
            var saccadeStartFindCoords = results.EyeCoords.Skip(eyeStartIndex).Take(saccadeFindWindowLength).ToArray();


            var startCoordBySpotPosition = GetByStartIndexByCoords(saccadeStartFindCoords, eyeStartIndex, 
                spotStartOscilationXPosition, spotEndOscilationXPosition, meanControlAmplitude);

            var saccadeStartIndex = startCoordBySpotPosition;
            var endWindowStartIndex = saccadeStartIndex + minDuration;

            var endWindowCoords = results.EyeCoords.Skip(endWindowStartIndex).Take(30).ToArray();

            // hamowanie odwrotne

            var saccadeEndIndex = endWindowStartIndex;

            if (saccadeStartIndex > results.EyeCoords.Length)
            {
                return null;
            }
            else
            {
                return new SaccadePosition
                {
                    Id = id,

                    SaccadeStartIndex = saccadeStartIndex,
                    SaccadeStartTime = results.TimeDeltas[saccadeStartIndex],
                    SaccadeStartCoord = results.EyeCoords[saccadeStartIndex],

                    SaccadeEndIndex = saccadeEndIndex,
                    SaccadeEndTime = results.TimeDeltas[saccadeEndIndex],
                    SaccadeEndCoord = results.EyeCoords[saccadeEndIndex],

                    SpotStartIndex = spotStartIndex,
                    SpotStartTime = results.TimeDeltas[spotStartIndex],
                    SpotStartCoord = results.SpotCoords[spotStartIndex],

                    SpotEndIndex = spotEndIndex,
                    SpotEndTime = results.TimeDeltas[spotEndIndex],
                    SpotEndCoord = results.SpotCoords[spotEndIndex],

                };
            }

        }

        private int GetByStartIndexByCoords(double[] startCoords, int eyeStartIndex, double spotStartOscilationXPosition, 
            double spotEndOscilationXPosition, double controlAmp)
        {
            int saccadeStartIndex = eyeStartIndex;
            var checkCount = 0;
            var checkCountMaxValue = 2;
            double? previousValue = null;

            if (spotStartOscilationXPosition == 0 && spotEndOscilationXPosition == 1
                || spotStartOscilationXPosition == -1 && spotEndOscilationXPosition == 0)
            {
                SetEyeStartAfterSpotPosition(ref startCoords, ref eyeStartIndex, spotStartOscilationXPosition, true, controlAmp);

            }
            else 
            {
                SetEyeStartAfterSpotPosition(ref startCoords, ref eyeStartIndex, spotStartOscilationXPosition, false, controlAmp);
            }

            saccadeStartIndex = eyeStartIndex;
            return saccadeStartIndex;
        }


        private void GetEndByDirectionChange(double[] startCoords, int eyeStartIndex, double spotStartOscilationXPosition,
           double spotEndOscilationXPosition, double controlAmp)
        {
            var endWindowStartIndex = eyeStartIndex + 3;
        }

        private void SetEyeStartAfterSpotPosition(ref double[] startCoords, ref int eyeStartIndex, double spotStartX, bool isRising, double controlAmp)
        {
            if (isRising)
            {
                if (startCoords.First() < spotStartX)
                {
                    for (int i = 0; i < startCoords.Length; i++)
                    {
                        if (startCoords[i] > spotStartX)
                        {
                            eyeStartIndex = eyeStartIndex + i;
                            startCoords = startCoords.Skip(i).ToArray();
                            FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, isRising, controlAmp);
                            break;
                        }
                    }
                }
                else
                {
                    FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, isRising, controlAmp);
                }
            }
            else
            {
                if (startCoords.First() > spotStartX)
                {
                    for (int j = 0; j < startCoords.Length; j++)
                    {
                        if (startCoords[j] < spotStartX)
                        {
                            eyeStartIndex = eyeStartIndex + j;
                            startCoords = startCoords.Skip(j).ToArray();
                            FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, isRising, controlAmp);
                            break;
                        }
                    }
                }
                else
                {
                    FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, isRising, controlAmp);
                }
            }
        }

        private void FindStartByMoveDirection(ref double[] startCoords, ref int eyeStartIndex, bool isRising, double controlAmp)
        {
            var firstSaccadeFrame = 0;
            var secondSaccadeFrame = 1;
            var thirdSaccadeFrame = 2;

            for (int saccNum = 0; saccNum < startCoords.Length; saccNum++)
            {
                bool isLongMoveOneDirection = false;

                if(isRising)
                {
                    isLongMoveOneDirection = startCoords[firstSaccadeFrame] < startCoords[secondSaccadeFrame]
                    && startCoords[secondSaccadeFrame] < startCoords[thirdSaccadeFrame];
                }
                else
                {
                    isLongMoveOneDirection = startCoords[firstSaccadeFrame] > startCoords[secondSaccadeFrame]
                    && startCoords[secondSaccadeFrame] > startCoords[thirdSaccadeFrame];
                }

                if (isLongMoveOneDirection)
                {
                    var firstSecondFrameLength = Math.Abs(startCoords[secondSaccadeFrame] - startCoords[firstSaccadeFrame]);
                    var secondThirdFrameLength = Math.Abs(startCoords[thirdSaccadeFrame] - startCoords[secondSaccadeFrame]);
                    if (firstSecondFrameLength < controlAmp && secondThirdFrameLength > firstSecondFrameLength)
                    {
                        firstSaccadeFrame++;
                    }

                    eyeStartIndex = eyeStartIndex + firstSaccadeFrame;
                    startCoords = startCoords.Skip(firstSaccadeFrame).ToArray();
                    break;
                }
                else
                {
                    firstSaccadeFrame++;
                    secondSaccadeFrame++;
                    thirdSaccadeFrame++;
                }
            }
        }

        private int FindIndex(double[] startCoords, int eyeStartIndex, double spotStartOscilationXPosition,
            double spotEndOscilationXPosition, double fluctuationAmp, bool isRising)
        {
            var saccadeStartIndex = eyeStartIndex;
           
            double currentDisplacement = 0;
            var checkCount = 0;
            var checkCountMaxValue = 2;
            double? previousValue = null;

            if (isRising)
            {
                var cordIndexRise = 0;
                foreach (var currCordRise in startCoords)
                {
                    if (currCordRise > spotStartOscilationXPosition)
                    {
                        if (previousValue != null)
                        {
                            if (currCordRise > previousValue)
                            {
                                if (checkCount < checkCountMaxValue)
                                {
                                    checkCount++;
                                    currentDisplacement = currentDisplacement + Math.Abs(currCordRise - previousValue.GetValueOrDefault());
                                    previousValue = currCordRise;
                                }
                                else
                                {
                                    saccadeStartIndex = eyeStartIndex + (cordIndexRise - checkCount);
                                    break;
                                }
                            }
                            else
                            {
                                checkCount = 0;
                                currentDisplacement = 0;
                                previousValue = currCordRise;
                            }
                        }
                        else
                        {
                            checkCount++;
                            previousValue = currCordRise;
                        }
                    }

                    cordIndexRise = cordIndexRise++;
                }
            }
            else
            {
                int cordIndexDown = 0;
                foreach (var currCord in startCoords)
                {
                    if (currCord < spotStartOscilationXPosition)
                    {
                        if (previousValue != null)
                        {
                            if (currCord < previousValue)
                            {
                                //if (checkCount <= checkCountMaxValue)
                                //{
                                    //checkCount++;
                                    currentDisplacement = currentDisplacement + Math.Abs(currCord - previousValue.GetValueOrDefault());
                                    if(currentDisplacement > fluctuationAmp)
                                    {
                                        //cordIndexDown++;
                                        saccadeStartIndex = eyeStartIndex + (cordIndexDown - 1);
                                        break;
                                    }
                                    else
                                    {
                                        previousValue = currCord;
                                    }
                                //}
                                //else
                                //{
                                //    //cordIndexDown++;
                                //    saccadeStartIndex = eyeStartIndex + (cordIndexDown - checkCount);
                                //    break;
                                //}
                            }
                            else
                            {
                                checkCount = 0;
                                currentDisplacement = 0;
                                previousValue = currCord;
                            }
                        }
                        else
                        {
                            checkCount++;
                            previousValue = currCord;
                        }
                    }
                    cordIndexDown ++;
                }

            }

            return saccadeStartIndex;
        }
    }

}
