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
    public static class DataHelper
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


        /// <summary>
        /// Reads and converts data to domain class collection. 
        /// Should be replaced by CSV reader component to avoid mathching columns by index 
        /// </summary>
        /// <param name="filePath">Data file path</param>
        /// <returns>List of data log items</returns>
        public static List<GazeDataLogItem> LoadData(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileStream = fileInfo.OpenRead();
            var fileDataStream = new StreamReader(fileStream);
            var fileText = fileDataStream.ReadToEnd();
            var lines = fileText.Split('\n');

            var gazeDataItems = new List<GazeDataLogItem>();
            foreach (string[] columns in lines.Select(line => line.Split(new char[0])))
            {
                if (columns.Length >= 13)
                {
                    int timeStamp;
                    double pointX;
                    double pointY;
                    double pupilSizeX;
                    double pupilSizeY;

                    var indexOfTimeStamp = Array.IndexOf(columns, "time:");
                    if (indexOfTimeStamp != -1)
                    {
                       indexOfTimeStamp++;
                    }

                    var indexOfPupilSizeXValue = Array.IndexOf(columns, "size_x:");
                    if (indexOfPupilSizeXValue != -1)
                    {
                        indexOfPupilSizeXValue++;
                    }

                    var indexOfPupilSizeYValue = Array.IndexOf(columns, "size_y:");
                    if (indexOfPupilSizeYValue != -1)
                    {
                        indexOfPupilSizeYValue++;
                    }

                    var indexOfXValue = Array.IndexOf(columns, "x:");
                    if (indexOfXValue != -1)
                    {
                        indexOfXValue++;
                    }

                    var indexOfYValue = Array.IndexOf(columns, "y:");
                    if (indexOfYValue != -1)
                    {
                        indexOfYValue++;
                    }

                    if (indexOfTimeStamp != -1 && indexOfXValue != -1 && indexOfYValue != -1 && indexOfPupilSizeXValue != -1 && indexOfPupilSizeYValue != -1)
                    {

                        bool isTimestampValue = int.TryParse(columns[indexOfTimeStamp], out timeStamp);
                        bool isPupilSizeXValue = double.TryParse(columns[indexOfPupilSizeXValue], NumberStyles.Any, CultureInfo.InvariantCulture, out pupilSizeX);
                        bool isPupilSizeYValue = double.TryParse(columns[indexOfPupilSizeYValue], NumberStyles.Any, CultureInfo.InvariantCulture, out pupilSizeY);
                        bool isXValue = double.TryParse(columns[indexOfXValue], NumberStyles.Any, CultureInfo.InvariantCulture, out pointX);
                        bool isYValue = double.TryParse(columns[indexOfYValue], NumberStyles.Any, CultureInfo.InvariantCulture, out pointY);

                        if (isTimestampValue && isXValue && isYValue && isPupilSizeXValue && isPupilSizeYValue)
                        {
                            gazeDataItems.Add(new GazeDataLogItem
                            {
                                Timestamp = timeStamp,
                                X = pointX,
                                Y = pointY,
                                SizeX = pupilSizeX,
                                SizeY = pupilSizeY
                            });
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Data file must contains 'time:', 'size_x:', 'size_y:', 'x:', 'y:' ");
                    }
                }
            }
            return gazeDataItems;
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

            return outputData;
        }


        public static SpotGazeFileData CutData(SpotGazeFileData resultData, int skipCount, int takeCount)
        {
            return new SpotGazeFileData
            {
                Eye = resultData.Eye.Skip(skipCount).Take(takeCount).ToArray(),
                Spot = resultData.Spot.Skip(skipCount).Take(takeCount).ToArray(),
                Time = resultData.Time.Skip(skipCount).Take(takeCount).ToArray()
            };
        }

        public static ResultData CutData(ResultData resultData, int skipCount, int takeCount)
        {
            return new ResultData
            {
                EarliestEyeOverSpotIndex = resultData.EarliestEyeOverSpotIndex.Skip(skipCount).Take(takeCount).ToList(),
                SpotOverMeanIndex = resultData.SpotOverMeanIndex.Skip(skipCount).Take(takeCount).ToList(),
                EyeCoords = resultData.EyeCoords.Skip(skipCount).Take(takeCount).ToArray(),
                SpotCoords = resultData.SpotCoords.Skip(skipCount).Take(takeCount).ToArray(),
                TimeDeltas = resultData.TimeDeltas.Skip(skipCount).Take(takeCount).ToArray(),
                TimeStamps = resultData.TimeStamps.Skip(skipCount).Take(takeCount).ToArray(),
                MeanSpotAmplitude = resultData.MeanSpotAmplitude,
                ShiftPeriod = resultData.ShiftPeriod
            };
        }



    }
}
