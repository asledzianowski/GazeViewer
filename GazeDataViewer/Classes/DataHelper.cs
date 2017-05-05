﻿using GazeDataViewer.Classes.SpotAndGain;
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

        public static SpotGazeFileData LoadDataForSpotAndGaze(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var fileStream = fileInfo.OpenRead();
            var fileDataStream = new StreamReader(fileStream);
            var fileText = fileDataStream.ReadToEnd();
            var lines = fileText.Split('\n');

            var outputData = new SpotGazeFileData(lines.Length -1);

            for(int i=0; i < lines.Length-1; i++)
            {
                var lineColumns = lines[i].Split(';');
                if (lineColumns.Length > 5)
                {
                    var isTimeConverted = DateTime.TryParse(lineColumns[0], out outputData.Time[i]);
                    //var isTimeConverted = DateTime.TryParseExact(lineColumns[0], "yyyy-MM-dd HH:mm:ss.FFF",
                      //                  CultureInfo.InvariantCulture, DateTimeStyles.None, out outputData.Time[i]);

                    float lEye; 
                    float rEye;
                    float spot;
                    var isLEyeConverted = float.TryParse(lineColumns[1], NumberStyles.Any, CultureInfo.InvariantCulture, out lEye);
                    var isREyeConverted = float.TryParse(lineColumns[3], NumberStyles.Any, CultureInfo.InvariantCulture, out rEye);
                    var isSopotConverted = float.TryParse(lineColumns[5], NumberStyles.Any, CultureInfo.InvariantCulture, out spot);

                    if (!isTimeConverted || !isLEyeConverted || !isREyeConverted || !isSopotConverted)
                    {
                        MessageBox.Show("Invalid format in input file at line " + i);
                        break;
                    }
                    else
                    {
                        outputData.LEye[i] = lEye;
                        outputData.REye[i] = rEye;
                        outputData.Spot[i] = spot;

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

        public static SpotGazeFileDataTablet LoadDataForSpotAndGaze(string eyeFilePath, string spotFilePath)
        {
            var fileInfo = new FileInfo(eyeFilePath);
            var fileStream = fileInfo.OpenRead();
            var fileDataStream = new StreamReader(fileStream);
            var fileText = fileDataStream.ReadToEnd();
            var eyeLines = fileText.Split('\n');

            fileInfo = new FileInfo(spotFilePath);
            fileStream = fileInfo.OpenRead();
            fileDataStream = new StreamReader(fileStream);
            fileText = fileDataStream.ReadToEnd();
            var spotLines = fileText.Split('\n');

            if(spotLines.Length != eyeLines.Length)
            {
                MessageBox.Show("Different length of eye and spot files!");
            }

            var outputData = new SpotGazeFileDataTablet(eyeLines.Length - 1);

            for (int i = 0; i < eyeLines.Length - 1; i++)
            {
                var eyeLineColumns = eyeLines[i].Split(';');
                var spotLineColumns = spotLines[i].Split(';');

                if (eyeLineColumns.Length > 11 && spotLineColumns.Length > 2)
                {
                    var isTimeConverted = int.TryParse(eyeLineColumns[5], out outputData.Time[i]);
                    //var isTimeConverted = DateTime.TryParseExact(lineColumns[0], "yyyy-MM-dd HH:mm:ss.FFF",
                    //                  CultureInfo.InvariantCulture, DateTimeStyles.None, out outputData.Time[i]);

                    float lEye;
                    float rEye;
                    float spot;
                    var isLEyeConverted = float.TryParse(eyeLineColumns[11], NumberStyles.Any, CultureInfo.InvariantCulture, out lEye);
                    var isREyeConverted = float.TryParse(eyeLineColumns[13], NumberStyles.Any, CultureInfo.InvariantCulture, out rEye);
                    var isSopotConverted = float.TryParse(spotLineColumns[1], NumberStyles.Any, CultureInfo.InvariantCulture, out spot);

                    if (!isTimeConverted || !isLEyeConverted || !isREyeConverted || !isSopotConverted)
                    {
                        MessageBox.Show("Invalid format in input file at line " + i);
                        break;
                    }
                    else
                    {
                        outputData.LEye[i] = lEye;
                        outputData.REye[i] = rEye;
                        outputData.Spot[i] = spot;

                    }
                }
                else
                {
                    if (i != (eyeLines.Length - 1))
                    {
                        MessageBox.Show("Missing columns in eye input file at line " + i);
                        break;
                    }
                    if (i != (spotLines.Length - 1))
                    {
                        MessageBox.Show("Missing columns in spot input file at line " + i);
                        break;
                    }
                }
            }

            return outputData;
        }

    }
}
