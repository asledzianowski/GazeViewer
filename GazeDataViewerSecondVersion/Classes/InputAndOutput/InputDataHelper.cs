﻿using GazeDataViewer.Classes.Enums;
using GazeDataViewer.Classes.EnumsAndStats;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GazeDataViewer.Classes
{
    public static class InputDataHelper
    {

        /// <summary>
        /// Checks if file contains acceptable data file extention  
        /// </summary>
        /// <param name="path">the file path</param>
        /// <returns>Is acceptable data file extention</returns>
        public static bool IsDataFileExtention(string path)
        {
            return (path.EndsWith(".txt") || path.EndsWith(".csv"));
        }

        /// <summary>
        /// Checks if file contains acceptable data file extention  
        /// </summary>
        /// <param name="path">the file path</param>
        /// <returns>Is acceptable data file extention</returns>
        public static bool IsStateDataExtention(string path)
        {
            return path.EndsWith(".xml");
        }

        public static SpotGazeFileData CloneFileData( SpotGazeFileData originalData)
        {
            return new SpotGazeFileData
            {
                Eye = originalData.Eye,
                Spot = originalData.Spot,
                Time = originalData.Time,
                TimeDeltas = originalData.TimeDeltas
            };
        }

        public static SpotGazeFileData LoadDataForSpotAndGaze(string filePath, int timeColumnIndex, int eyeColumnIndex, int spotColumnIndex)
        {
            var fileInfo = new FileInfo(filePath);
            var fileStream = fileInfo.OpenRead();
            var fileDataStream = new StreamReader(fileStream);
            var fileText = fileDataStream.ReadToEnd();
            var lines = fileText.Split('\n');

            var outputData = new SpotGazeFileData();
            outputData.Time = new int[lines.Length - 1];
            outputData.Eye = new double[lines.Length - 1];
            outputData.Spot = new double[lines.Length - 1];

            var isFileRead = true;

            for (int i=0; i < lines.Length-1; i++)
            {
                var lineColumns = lines[i].Split(' '); //;
                if (lineColumns.Length >= 4) //4
                {
                    var isTimeConverted = int.TryParse(lineColumns[timeColumnIndex], out outputData.Time[i]);
                    //var isTimeConverted = DateTime.TryParseExact(lineColumns[0], "yyyy-MM-dd HH:mm:ss.FFF",
                      //                  CultureInfo.InvariantCulture, DateTimeStyles.None, out outputData.Time[i]);

                    //double lEye;
                    double rEye;
                    double spot;
                    //var isLEyeConverted = double.TryParse(lineColumns[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lEye);
                    var isREyeConverted = double.TryParse(lineColumns[eyeColumnIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out rEye); //3
                    var isSopotConverted = double.TryParse(lineColumns[spotColumnIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out spot); //5

                    if (!isTimeConverted || /*!isLEyeConverted ||*/ !isREyeConverted || !isSopotConverted)
                    {
                        MessageBox.Show("Invalid format in input file at line " + i);
                        isFileRead = false;
                        break;
                    }
                    else
                    {
                        //outputData.LEye[i] = Math.Round(lEye, 3);
                        outputData.Eye[i] = Math.Round(rEye, 3);
                        outputData.Spot[i] = Math.Round(spot, 3);

                    }
                }
                else
                {
                    if (i != (lines.Length - 1))
                    {
                        MessageBox.Show("Missing columns in input file at line " + i);
                        break;
                    }
                }
            }


            if (isFileRead)
            {
                outputData.TimeDeltas = GetDeltaTimespansDouble(outputData.Time);
                return outputData;
            }
            else
            {
                return null;
            }
        }


        public static double[] GetDeltaTimespansDouble(int[] timestamps)
        {
            var startTime = timestamps[0];
            var stime = new List<double>();

            for (int i = 0; i < timestamps.Length; i++)
            {
                double timeSpanD = 0;
                var timeSpan = timestamps[i] - startTime;
                if(timeSpan !=0)
                {
                    timeSpanD = Convert.ToDouble(timeSpan) / Consts.GraphXScaleFactor;
                }
                if (!stime.Contains(timeSpanD))
                {
                    stime.Add(timeSpanD);
                }
            }
            return stime.ToArray();
        }

        public static double ScaleTimeByFactor(double time, int roundBy)
        {
            var output = Math.Round(time / Consts.TimeScaleFactor, roundBy);
            return output;
        }

        public static SpotGazeFileData CutData(SpotGazeFileData resultData, int skipCount, int takeCount)
        {
            return new SpotGazeFileData
            {
                Eye = resultData.Eye.Skip(skipCount).Take(takeCount).ToArray(),
                Spot = resultData.Spot.Skip(skipCount).Take(takeCount).ToArray(),
                Time = resultData.Time.Skip(skipCount).Take(takeCount).ToArray(),
                TimeDeltas = resultData.TimeDeltas.Skip(skipCount).Take(takeCount).ToArray()
            };
        }

        public static ResultData CutData(ResultData resultData, int skipCount, int takeCount)
        {
            return new ResultData
            {
                SpotOverMeanIndex = resultData.SpotOverMeanIndex.Skip(skipCount).Take(takeCount).ToList(),
                EyeCoords = resultData.EyeCoords.Skip(skipCount).Take(takeCount).ToArray(),
                SpotCoords = resultData.SpotCoords.Skip(skipCount).Take(takeCount).ToArray(),
                TimeDeltas = resultData.TimeDeltas.Skip(skipCount).Take(takeCount).ToArray(),
                TimeStamps = resultData.TimeStamps.Skip(skipCount).Take(takeCount).ToArray(),
                MeanSpotAmplitude = resultData.MeanSpotAmplitude,
                ShiftPeriod = resultData.ShiftPeriod
            };
        }

        public static List<int> GetSpotMoveDataBlock(ResultData resultData, EyeMoveTypes eyeMoveType)
        {
            int initStartTime = -1;
            int initEndTime = -1;

            if (eyeMoveType == EyeMoveTypes.Pursuit)
            {
                initStartTime = 1;
                initEndTime = 9231000;
            }
            else if (eyeMoveType == EyeMoveTypes.Saccade)
            {
                initStartTime = 10676000; //9946000;
                initEndTime = 15095000;
            }
            else if (eyeMoveType == EyeMoveTypes.AntiSaccade)
            {
                initStartTime = 15426000;
                initEndTime = 19844000;
            }

            var blockStart = resultData.SpotMoves.FirstOrDefault(x => x.SpotStartTimeStamp >= initStartTime);
            var blockEnd = resultData.SpotMoves.LastOrDefault(x => x.SpotStartTimeStamp <= initEndTime);
            var spotBlock = resultData.SpotMoves.Where(x => x.SpotStartTimeStamp >= initStartTime && x.SpotStartTimeStamp <= initEndTime);
            return spotBlock.Select(x => x.SpotStartIndex).ToList();
        }




    }
}
