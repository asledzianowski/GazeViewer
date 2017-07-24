using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.EyeMoveSearch
{
    public static class EyeMoveSearchToolBox
    {
        public static bool IsRising(double spotStartOscilationXPosition, double spotEndOscilationXPosition)
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

        public static int GetEndByDirectionChange(double[] startCoords, int eyeStartIndex, bool isRising, ref bool isEndFound)
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
                            if (isEndIndex && currentCord > nextCord)
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
                            isEndFound = true;
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

        public static int GetEndBySpeedChange(double[] startCoords, int eyeStartIndex, EyeMoveFinderConfig config, ref bool isEndFound)
        {
            int endIndex = eyeStartIndex ;

            for (int i = 0; i < startCoords.Length; i++)
            {
                if (i + 2 < startCoords.Length)
                {
                    var currentDistance = Math.Abs(startCoords[i] - startCoords[i + 1]);
                    var nextDistance = Math.Abs(startCoords[i + 1] - startCoords[i + 2]);

                    //var currentDistance = Math.Abs(startCoords[i] - startCoords[i - 1]);
                    //var nextDistance = Math.Abs(startCoords[i + 1] - startCoords[i]);

                    if (nextDistance < currentDistance)
                    {
                        var diff = (nextDistance / currentDistance) * 100;
                        if (diff < config.MinInhibition)
                        {
                            isEndFound = true;

                            if (i < config.MinDuration)
                            {
                                endIndex = endIndex + config.MinDuration;
                            }
                            else
                            {
                                endIndex = endIndex + i;
                            }
                            break;
                        }
                    }
                }
            }

            return endIndex;

        }

        public static EyeMove CountTestValuesForEyeMove(EyeMove eyeMove, ResultData results)
        {
            var isRising = IsRising(eyeMove.SpotMove.SpotStartCoord, eyeMove.SpotMove.SpotEndCoord);

            if (isRising)
            {
                eyeMove.ControlAmpTestValue = results.EyeCoords[eyeMove.EyeStartIndex + 1] - results.EyeCoords[eyeMove.EyeStartIndex];
                eyeMove.MinLengthTestValue = results.EyeCoords[eyeMove.EyeStartIndex + 2] - results.EyeCoords[eyeMove.EyeStartIndex];
            }
            else
            {
                eyeMove.ControlAmpTestValue = results.EyeCoords[eyeMove.EyeStartIndex] - results.EyeCoords[eyeMove.EyeStartIndex + 1];
                eyeMove.MinLengthTestValue = results.EyeCoords[eyeMove.EyeStartIndex] - results.EyeCoords[eyeMove.EyeStartIndex + 2];
            }

            eyeMove.ControlAmpTestValue = Math.Abs(eyeMove.ControlAmpTestValue);
            eyeMove.MinLengthTestValue = Math.Abs(eyeMove.MinLengthTestValue);
            return eyeMove;
        }



        public static void FindStartByMoveDirection(ref double[] startCoords, ref int eyeStartIndex, double spotStartX,
             double controlMin, double controlMax, bool isRising, double controlAmp, double minLength,
             ref bool isStartFound, ref double controlAmpTestValue, ref double minLengthTestValue)
        {
            var firstSaccadeFrameIndx = 0;
            var secondSaccadeFrameIndx = 1;
            var thirdSaccadeFrameIndx = 2;
            int? lastProperMoveStartIndex = null;

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
                        controlAmpTestValue = (startCoords[secondSaccadeFrameIndx] - startCoords[firstSaccadeFrameIndx]);
                        isCordsAmp = controlAmpTestValue > controlAmp;
                        minLengthTestValue = (startCoords[thirdSaccadeFrameIndx] - startCoords[firstSaccadeFrameIndx]);
                        isCordsLength = minLengthTestValue > minLength;
                    }
                    else
                    {
                        isCordsDirection = startCoords[firstSaccadeFrameIndx] > startCoords[secondSaccadeFrameIndx]
                        && startCoords[secondSaccadeFrameIndx] > startCoords[thirdSaccadeFrameIndx];
                        controlAmpTestValue = (startCoords[firstSaccadeFrameIndx] - startCoords[secondSaccadeFrameIndx]);
                        isCordsAmp = controlAmpTestValue > controlAmp;
                        minLengthTestValue = (startCoords[firstSaccadeFrameIndx] - startCoords[thirdSaccadeFrameIndx]);
                        isCordsLength = minLengthTestValue > minLength;
                    }

                    if (isCordsDirection)
                    {

                    }

                    if (isCordsDirection && lastProperMoveStartIndex == null)
                    {
                        lastProperMoveStartIndex = firstSaccadeFrameIndx;
                    }

                    var isLongMoveOneDirection = isCordsDirection && isCordsAmp && isCordsLength;

                    if (isLongMoveOneDirection)
                    {
                        isStartFound = true;

                        //if (isRising && controlMax > startCoords[0] )
                        //{

                        //    firstSaccadeFrameIndx++;
                        //}

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

        //private void SetEyeStartAfterSpotPosition(ref double[] startCoords, ref int eyeStartIndex, double spotStartX, 
        //    double controlMin, double controlMax,  bool isRising, double controlAmp)
        //{
        //    if (isRising)
        //    {
        //        if (startCoords.First() < spotStartX)
        //        {
        //            for (int i = 0; i < startCoords.Length; i++)
        //            {
        //                if (startCoords[i] > spotStartX)
        //                {
        //                    eyeStartIndex = eyeStartIndex + i;
        //                    startCoords = startCoords.Skip(i).ToArray();
        //                    FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
        //                        isRising, controlAmp);
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
        //                isRising, controlAmp);
        //        }
        //    }
        //    else
        //    {
        //        if (startCoords.First() > spotStartX)
        //        {
        //            for (int j = 0; j < startCoords.Length; j++)
        //            {
        //                if (startCoords[j] < spotStartX)
        //                {
        //                    eyeStartIndex = eyeStartIndex + j;
        //                    startCoords = startCoords.Skip(j).ToArray();
        //                    FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
        //                        isRising, controlAmp);
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            FindStartByMoveDirection(ref startCoords, ref eyeStartIndex, spotStartX, controlMin, controlMax,
        //                isRising, controlAmp);
        //        }
        //    }
        //}       
    }
}
