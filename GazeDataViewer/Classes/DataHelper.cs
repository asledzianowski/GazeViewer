using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
