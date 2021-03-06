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
        public static bool IsAllowedDataFileExtention(string path)
        {
            return (path.EndsWith(Consts.FileExtentionTxt) || path.EndsWith(Consts.FileExtentionCsv) 
                || path.EndsWith(Consts.FileExtentionMaruniec));
        }

        /// <summary>
        /// Checks if file contains acceptable data file extention  
        /// </summary>
        /// <param name="path">the file path</param>
        /// <returns>Is acceptable data file extention</returns>
        public static bool IsStateDataExtention(string path)
        {
            return path.EndsWith(Consts.FileExtentionXml);
        }

        public static FileType GetFileType(string path)
        {
            if(path.EndsWith(Consts.FileExtentionET))
            {
                return FileType.ET;
            }
            else if (path.EndsWith(Consts.FileExtentionMaruniec))
            {
                return FileType.Maruniec;
            }
            else
            {
                return FileType.Brudno;
            }
        }



        public static SpotGazeFileData CloneFileData( SpotGazeFileData originalData)
        {
            return new SpotGazeFileData
            {
                Eye = originalData.Eye,
                Spot = originalData.Spot,
                Time = originalData.Time,
                TimeDeltas = originalData.TimeDeltas,
                FileType = originalData.FileType
            };
        }

        public static SpotGazeFileData LoadDataForSpotAndGaze(string filePath, int timeColumnIndex, int eyeColumnIndex, 
            int spotColumnIndex)
        {
            var fileInfo = new FileInfo(filePath);
            var fileStream = fileInfo.OpenRead();
            var fileDataStream = new StreamReader(fileStream);
            var fileText = fileDataStream.ReadToEnd();
            var lines = fileText.Split('\n');

            var outputData = new SpotGazeFileData();
            outputData.FileType = GetFileType(filePath);

            // fix for JazzNovo
            DateTime testOut;
            var isDate = DateTime.TryParse(lines[0].Split(' ')[0], out testOut);
            if (isDate)
            {
                lines = FixJazzNovoFormat(lines);
                outputData.FileType = FileType.JazzNovo;
            }

            outputData.Time = new int[lines.Length];
            outputData.Eye = new double[lines.Length];
            outputData.Spot = new double[lines.Length];

            var isFileRead = true;



            for (int i=0; i < lines.Length; i++)
            {
                var lineColumns = lines[i].Split(' '); //;

                if (lineColumns.Length >= 4) //4
                {
                    
                    var isTimeConverted = int.TryParse(lineColumns[timeColumnIndex], out outputData.Time[i]);
                  
                    double rEye;
                    double spot;
                    var isREyeConverted = double.TryParse(lineColumns[eyeColumnIndex], NumberStyles.Any, CultureInfo.InvariantCulture, out rEye); //3
                    var isSopotConverted = double.TryParse(lineColumns[spotColumnIndex].Replace(@"\r", string.Empty), NumberStyles.Any, CultureInfo.InvariantCulture, out spot); //5

                    if (!isTimeConverted || !isREyeConverted || !isSopotConverted)
                    {
                        MessageBox.Show("Invalid format in input file at line " + i);
                        isFileRead = false;
                        break;
                    }
                    else
                    {
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
                outputData.TimeDeltas = GetUnifiedTimeDeltas(outputData.Time, outputData.FileType);
                return outputData;
            }
            else
            {
                return null;
            }
        }

        public static string[] FixJazzNovoFormat(string[] lines)
        {
            var output = new List<string>();

            var firstLine = lines[0].Split(';');
            var firstTime = DateTime.Parse(firstLine[0]).TimeOfDay;

            var freqCounter = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == freqCounter)
                {
                    var currentLine = lines[i].Split(';');
                    if (currentLine.Length > 1)
                    {
                        var currentTime = DateTime.Parse(currentLine[0]).TimeOfDay;

                        var span = currentTime.Subtract(firstTime);
                        currentLine[0] = span.TotalMilliseconds.ToString();
                        var lineString = string.Join(";", currentLine);
                        lineString = lineString.Replace(';', ' ');
                        output.Add(lineString);
                    }

                    freqCounter = freqCounter + 17;
                }

            }
            

            return output.ToArray();
        }


        public static double[] GetUnifiedTimeDeltas(int[] timeStamps, FileType fileType)
        {
            double xScale;
          
            if (fileType == FileType.Maruniec)
            {
                xScale = Consts.DeltaScaleFactorMaruniec;
            }
            else if (fileType == FileType.JazzNovo)
            {
                xScale = 1D;
            }
            else
            {
                xScale = Consts.DeltaScaleFactorStandard;
            }

            var unifiedDeltas = GetDeltaTimespansDouble(timeStamps, xScale);
            return unifiedDeltas;

        }

        public static int[] GetTimeStampsScaled(int[] timestamps, FileType fileType)
        {
            int xScale;

            if (fileType == FileType.Maruniec)
            {
                xScale = Convert.ToInt32(Consts.DeltaScaleFactorMaruniec);
            }
            else
            {
                xScale = Convert.ToInt32(Consts.DeltaScaleFactorStandard);
            }

            var unifiedTimestamps = new List<int>();
            foreach (var timestamp in timestamps)
            {
                unifiedTimestamps.Add(timestamp / xScale);
            }

            return unifiedTimestamps.ToArray();
        }

        public static double[] GetDeltaTimespansDouble(int[] timestamps, double xScale)
        {
            var startTime = timestamps[0];
            var stime = new List<double>();

            for (int i = 0; i < timestamps.Length; i++)
            {
                if(i == 998)
                {

                }
                double timeSpanD = 0;
                var timeSpan = timestamps[i] - startTime;
                if(timeSpan !=0)
                {
                    timeSpanD = Convert.ToDouble(timeSpan) / xScale;
                }
                if (!stime.Contains(timeSpanD))
                {
                    stime.Add(timeSpanD);
                }
            }
            return stime.ToArray();
        }


        public static double ScaleByTimeFactor(double time, int roundBy, bool scaleDown, FileType fileType)
        {
            double output;

            double scaleFactor;
            if (fileType == FileType.JazzNovo)
            {
                scaleFactor = Consts.TimeScaleFactorJazzNovo;
            }
            else
            {
                scaleFactor = Consts.TimeScaleFactorStandard;
            }

            if (scaleDown)
            {
                output = time / scaleFactor;
            }
            else
            {
                output = time * scaleFactor;
            }
            return output;
        }

        public static double? GetScaledTimeFromIndex(SpotGazeFileData resultData, int index)
        {
            if (index < resultData.TimeDeltas.Count())
            {
                var delta = resultData.TimeDeltas[index];
                var scaledDelta = ScaleByTimeFactor(delta, 2, true, resultData.FileType);
                return scaledDelta;
            }
            else
            {
                return null;
            }
        }

        public static int? GetIndexFromScaledTime(SpotGazeFileData resultData, double delta)
        {
            var scaledDelta = ScaleByTimeFactor(delta, 2, false, resultData.FileType);
            try
            {
                var indexItem = resultData.TimeDeltas.Where(x => x >= scaledDelta).OrderBy(x => x).FirstOrDefault();
                var index = Array.IndexOf(resultData.TimeDeltas, indexItem);
                return index;
            }
            catch
            {
                return null;
            }
        }

        public static SpotGazeFileData CutData(SpotGazeFileData resultData, int skipCount, int takeCount)
        {
            return new SpotGazeFileData
            {
                Eye = resultData.Eye.Skip(skipCount).Take(takeCount).ToArray(),
                Spot = resultData.Spot.Skip(skipCount).Take(takeCount).ToArray(),
                Time = resultData.Time.Skip(skipCount).Take(takeCount).ToArray(),
                TimeDeltas = resultData.TimeDeltas.Skip(skipCount).Take(takeCount).ToArray(),
                FileType = resultData.FileType
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

        public static List<int> GetSpotMoveDataBlock(ResultData resultData, EyeMoveTypes eyeMoveType, FileType fileType)
        {
            int initStartTime = -1;
            int initEndTime = -1;


            if (eyeMoveType == EyeMoveTypes.Saccade)
            {
                if(fileType == FileType.Brudno)
                {
                    initStartTime = Consts.SaccadeStartTimeStandard;
                    initEndTime = Consts.AntiSaccadeStartTimeStandard;
                }
                else if (fileType == FileType.Maruniec)
                {
                    initStartTime = Consts.SaccadeStartTimeMaruniec;
                    initEndTime = Consts.AntiSaccadeStartTimeMaruniec;
                }
                else if (fileType == FileType.JazzNovo)
                {
                    initStartTime = 2006;
                    initEndTime = 57001;
                }
                //initStartTime = 10676000; //9946000;
                //initEndTime = 15095000;
            }
            else if (eyeMoveType == EyeMoveTypes.AntiSaccade)
            {
                if (fileType == FileType.Brudno)
                {
                    initStartTime = Consts.AntiSaccadeStartTimeStandard;
                }
                else if(fileType == FileType.Maruniec)
                {
                    initStartTime = Consts.AntiSaccadeStartTimeMaruniec;
                }

                initEndTime = resultData.TimeStamps.Last();
                //initStartTime = 15426000;
                //initEndTime = 19844000;
            }

            var blockStart = resultData.SpotMoves.FirstOrDefault(x => x.SpotStartTimeStamp >= initStartTime);
            var blockEnd = resultData.SpotMoves.LastOrDefault(x => x.SpotStartTimeStamp <= initEndTime);
            var spotBlock = resultData.SpotMoves.Where(x => x.SpotStartTimeStamp >= initStartTime && x.SpotStartTimeStamp <= initEndTime);
            return spotBlock.Select(x => x.SpotStartIndex).ToList();
        }




    }
}
