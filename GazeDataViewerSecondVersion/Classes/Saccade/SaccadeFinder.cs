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
            var isRising = IsRising(spotStartOscilationXPosition, spotEndOscilationXPosition);

            //SetEyeStartAfterSpotPosition(ref saccadeStartFindCoords, ref eyeStartIndex, 
            //    spotStartOscilationXPosition, controlMinCord, controlMaxCord, isRising, meanControlAmplitude);

            FindStartByMoveDirection(ref saccadeStartFindCoords, ref eyeStartIndex,
                spotStartOscilationXPosition, controlMinCord, controlMaxCord, isRising, meanControlAmplitude);

            //var saccadeStartIndex = startCoordBySpotPosition;

            var endIndex = GetEndByDirectionChange(saccadeStartFindCoords, eyeStartIndex, spotStartOscilationXPosition,
                spotEndOscilationXPosition, isRising);

            //var endWindowStartIndex = saccadeStartIndex + minDuration;
            //var endWindowCoords = results.EyeCoords.Skip(endWindowStartIndex).Take(30).ToArray();

            // hamowanie odwrotne
            var saccadeStartIndex = eyeStartIndex;
            var saccadeEndIndex = endIndex;

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


        private bool IsRising(double spotStartOscilationXPosition, double spotEndOscilationXPosition)
        {
            if (spotStartOscilationXPosition == 0 && spotEndOscilationXPosition == 1
               || spotStartOscilationXPosition == -1 && spotEndOscilationXPosition == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int GetEndByDirectionChange(double[] startCoords, int eyeStartIndex, double spotStartOscilationXPosition,
           double spotEndOscilationXPosition, bool isRising)
        {
            var minDuration = 3;
            int endIndex = eyeStartIndex + minDuration;
            bool isEndCorrection = false; 

            if (startCoords.Length > minDuration + 1)
            {
                var previousCord = startCoords[minDuration];
                startCoords = startCoords.Skip(minDuration + 1).ToArray();

                for (int i = 0; i < startCoords.Length; i++)
                {
                    if (startCoords.Length > i + 1)
                    {
                        var currentCord = startCoords[i];
                        var nextCord = startCoords[i + 1];
                        var isEndIndex = false;


                        if (isRising)
                        {
                            isEndIndex = currentCord < previousCord; // && nextCord < currentCord;
                            if(isEndIndex && currentCord > nextCord)
                            {
                                isEndCorrection = true;
                            }
                        }
                        else
                        {
                            isEndIndex = currentCord > previousCord; //&& nextCord > currentCord;
                        }

                        if (isEndIndex)
                        {
                            if (i > 0)
                            {
                                if (isEndCorrection)
                                {
                                    endIndex = (endIndex + i) - 2;
                                }
                                else
                                {
                                    endIndex = (endIndex + i) - 1;
                                }
                            }
                            break;
                        }
                        else
                        {
                            previousCord = currentCord;
                        }
                    }
                }
            }
            return endIndex;
        }

        private void SetEyeStartAfterSpotPosition(ref double[] startCoords, ref int eyeStartIndex, double spotStartX, 
            double controlMin, double controlMax,  bool isRising, double controlAmp)
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
                            FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
                                isRising, controlAmp);
                            break;
                        }
                    }
                }
                else
                {
                    FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
                        isRising, controlAmp);
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
                            FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
                                isRising, controlAmp);
                            break;
                        }
                    }
                }
                else
                {
                    FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
                        isRising, controlAmp);
                }
            }
        }

        private void FindStartByMoveDirection(ref double[] startCoords, ref int eyeStartIndex, double spotStartX,
             double controlMin, double controlMax, bool isRising, double controlAmp)
        {
            var firstSaccadeFrameIndx = 0;
            var secondSaccadeFrameIndx = 1;
            var thirdSaccadeFrameIndx = 2;
            int? lastProperMoveStartIndex = null; 
            bool isStartFound = false;

            for (int saccNum = 0; saccNum < startCoords.Length; saccNum++)
            {
                if (thirdSaccadeFrameIndx < startCoords.Length)
                {
                    var isCordsDirection = false;
                    var isCordsAmp = false;
                    var isCordsLength = false;

                    if (isRising)
                    {
                        isCordsDirection = startCoords[firstSaccadeFrameIndx] < startCoords[secondSaccadeFrameIndx]
                        && startCoords[secondSaccadeFrameIndx] < startCoords[thirdSaccadeFrameIndx];
                        isCordsAmp = (startCoords[secondSaccadeFrameIndx] - startCoords[firstSaccadeFrameIndx]) > (controlAmp);
                        isCordsLength = (startCoords[thirdSaccadeFrameIndx] - startCoords[firstSaccadeFrameIndx]) > 0.4;
                    }
                    else
                    {
                        isCordsDirection = startCoords[firstSaccadeFrameIndx] > startCoords[secondSaccadeFrameIndx]
                        && startCoords[secondSaccadeFrameIndx] > startCoords[thirdSaccadeFrameIndx];
                        isCordsAmp = (startCoords[firstSaccadeFrameIndx] - startCoords[secondSaccadeFrameIndx]) > (controlAmp);
                        isCordsLength =  (startCoords[firstSaccadeFrameIndx] - startCoords[thirdSaccadeFrameIndx]) > 0.4;
                    }

                    if (isCordsDirection /*&& lastProperMoveStartIndex == null*/)
                    {
                        lastProperMoveStartIndex = firstSaccadeFrameIndx;
                    }

                    var isLongMoveOneDirection = isCordsDirection && isCordsAmp && isCordsLength;

                    if (isLongMoveOneDirection)
                    {
                        isStartFound = true;
                        var spotFirstFrameLength = Math.Abs(startCoords[firstSaccadeFrameIndx] - spotStartX);
                        var firstSecondFrameLength = Math.Abs(startCoords[secondSaccadeFrameIndx] - startCoords[firstSaccadeFrameIndx]);
                        var secondThirdFrameLength = Math.Abs(startCoords[thirdSaccadeFrameIndx] - startCoords[secondSaccadeFrameIndx]);

                        

                        //if (firstSecondFrameLength < controlAmp)
                        //{
                        //    //&& secondThirdFrameLength > firstSecondFrameLength
                        //    //firstSaccadeFrame++;
                        //}
                        //else
                        //{
                        //    if (secondThirdFrameLength > firstSecondFrameLength)
                        //    {
                        //        //firstSaccadeFrame++;
                        //    }
                        //    else
                        //    {
                        //        var diffPercent = (controlAmp / firstSecondFrameLength) * 100;
                        //        if (diffPercent < 50)
                        //        {
                        //            //firstSaccadeFrame++;
                        //        }
                        //    }
                        //}

                       

                        if (isRising && controlMax > startCoords[0] )
                        {
                            //double diffPercent2 = (startCoords[0] / controlMax) * 100;
                            //if (diffPercent2 < 50)
                            //{
                            //    firstSaccadeFrame++;
                            //}
                            firstSaccadeFrameIndx++;
                        }

                        eyeStartIndex = eyeStartIndex + firstSaccadeFrameIndx;
                        startCoords = startCoords.Skip(firstSaccadeFrameIndx).ToArray();
                        break;
                    }
                    else
                    {
                        firstSaccadeFrameIndx++;
                        secondSaccadeFrameIndx++;
                        thirdSaccadeFrameIndx++;
                    }
                }
            }

            if (!isStartFound && lastProperMoveStartIndex != null)
            {
                eyeStartIndex = eyeStartIndex + lastProperMoveStartIndex.GetValueOrDefault();
                startCoords = startCoords.Skip(lastProperMoveStartIndex.GetValueOrDefault()).ToArray();
            }
        }

       
    }

}
