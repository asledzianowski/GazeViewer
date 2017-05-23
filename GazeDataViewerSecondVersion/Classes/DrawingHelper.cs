using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;



namespace GazeDataViewer.Classes
{
    /// <summary>
    /// Drawing methods
    /// </summary>
    static class DrawingHelper
    {
        #region Private Variables

        /// <summary>
        /// Full gaze data path stroke 
        /// </summary>
        private static Stroke gazeFullPathStroke = null;

        /// <summary>
        /// Current position stroke
        /// </summary>
        private static Stroke timestampTrackStroke = null;

        # endregion

        #region Public Methods
        /// <summary>
        /// Generates chart series collection
        /// </summary>
        /// <param name="sortedData">Sorted gaze data log</param>
        /// <returns>Chart series collection</returns>
        public static Dictionary<string, List<SeriesItem>> GenerateChartSeries(List<GazeDataLogItem> sortedData, CalculationHelper calculationHelper)
        {
            var velocityPerFrame = new List<SeriesItem>();
            var distancePerFrame = new List<SeriesItem>();
            var amplitudePerFrame = new List<SeriesItem>();
            var xPositionPerFrame = new List<SeriesItem>();
            var yPositionPerFrame = new List<SeriesItem>();
            var pupilSizeLeft = new List<SeriesItem>();
            var pupilSizeRight = new List<SeriesItem>();
 
            int currentPointStart = 0;
            int currentPointEnd = 1;
            int currentTimePoint = sortedData[0].Timestamp;

            velocityPerFrame.Add(new SeriesItem(currentTimePoint, 0));
            xPositionPerFrame.Add(new SeriesItem(currentTimePoint, sortedData[0].X));
            yPositionPerFrame.Add(new SeriesItem(currentTimePoint, sortedData[0].Y));
            amplitudePerFrame.Add(new SeriesItem(currentTimePoint, 0));
            distancePerFrame.Add(new SeriesItem(currentTimePoint, 0));
            pupilSizeLeft.Add(new SeriesItem(currentTimePoint, sortedData[0].SizeX));
            pupilSizeRight.Add(new SeriesItem(currentTimePoint, sortedData[0].SizeY));
          
            while (currentPointEnd < sortedData.Count())
            {
                var distanceAb = calculationHelper.CountEuclideanDistance(sortedData[currentPointStart], sortedData[currentPointEnd]);
                distanceAb = calculationHelper.ConvertPixelsToCentimeters(distanceAb);
                var amplitude = calculationHelper.CountVisualAngle(distanceAb);
                var velocityPerSec = calculationHelper.CountAngularVelocityPerSecond(amplitude, 2);

                var timeDelta = (sortedData[currentPointEnd].Timestamp - sortedData[currentPointStart].Timestamp);
                currentTimePoint = currentTimePoint + timeDelta;

                velocityPerFrame.Add(new SeriesItem(currentTimePoint, Math.Round(velocityPerSec, 2)));
                xPositionPerFrame.Add(new SeriesItem(currentTimePoint, sortedData[currentPointEnd].X));
                yPositionPerFrame.Add(new SeriesItem(currentTimePoint, sortedData[currentPointEnd].Y));
                amplitudePerFrame.Add(new SeriesItem(currentTimePoint, amplitude));
                distancePerFrame.Add(new SeriesItem(currentTimePoint, distanceAb));
                pupilSizeLeft.Add(new SeriesItem(currentTimePoint, sortedData[currentPointEnd].SizeX));
                pupilSizeRight.Add(new SeriesItem(currentTimePoint, sortedData[currentPointEnd].SizeY));

                currentPointStart++;
                currentPointEnd++;
            }

            var output = new Dictionary<string, List<SeriesItem>>
            {
                {SeriesNames.Velocity.ToString(), velocityPerFrame},
                {SeriesNames.XCoordinate.ToString(), xPositionPerFrame},
                {SeriesNames.YCoordinate.ToString(), yPositionPerFrame},
                {SeriesNames.Amplitude.ToString(), amplitudePerFrame},
                {SeriesNames.Distance.ToString(), distancePerFrame},
                {SeriesNames.PupilSizeLeft.ToString(), pupilSizeLeft},
                {SeriesNames.PupilSizeRight.ToString(), pupilSizeRight}
            };

            return output;
        }

        /// <summary>
        /// Draws continous bar chart on given canvas
        /// </summary>
        /// <param name="series">chart data</param>
        /// <param name="canvas">chart canvas</param>
        /// <param name="seriesName">Series name</param>
        /// <param name="color">Bar color</param>
        public static void DrawChart(List<SeriesItem> series, Canvas canvas, SeriesNames seriesName, Color color)
        {          
            const int yStart = 0;
            const double xScale = 1;
            
            var flipTransform = new ScaleTransform{ScaleY = -1};

            for (int i = 0; i < series.Count; i++)
            {
                var line = new Line
                {
                    X1 = i * xScale,
                    Y1 = yStart,
                    X2 = i * xScale,
                    Y2 = NormalizeWithFactor(series[i].Value, seriesName),
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(color),
                    RenderTransform = flipTransform 
                };
                Canvas.SetLeft(line, i);
                Canvas.SetTop(line, 0);
                canvas.Children.Add(line);
            }
        }

        /// <summary>
        /// Draws gaze tracking path stroke from entire data 
        /// </summary>
        /// <param name="canvas">The canvas</param>
        /// <param name="gazeLogData">The data</param>
        public static void CanvasDrawGazeFullPathStroke(InkCanvas canvas, List<GazeDataLogItem> gazeLogData)
        {
            if (gazeFullPathStroke != null)
            {
                canvas.Strokes.Remove(gazeFullPathStroke);
            }

            var strokesPoints = DrawingHelper.GetStrokes(gazeLogData, 0.5F);
            gazeFullPathStroke = new Stroke(strokesPoints) { DrawingAttributes = { Color = Colors.Orange } };
            canvas.Strokes.Add(gazeFullPathStroke);
        }

        /// <summary>
        /// Draws gaze tracking path stroke till desired data position 
        /// </summary>
        /// <param name="canvas">The canvas</param>
        /// <param name="gazeLogData">The data</param>
        /// <param name="currentGazeDataIndex">The last data frame index</param>
        public static void CanvasDrawTimestampTrackPathStroke(InkCanvas canvas, List<GazeDataLogItem> gazeLogData, int currentGazeDataIndex)
        {
            var currentGazeDataLogItem = gazeLogData[currentGazeDataIndex];

            if (timestampTrackStroke != null)
            {
                canvas.Strokes.Remove(timestampTrackStroke);
            }

            var currentTrackingPath = gazeLogData.Where(x => x.Timestamp <= currentGazeDataLogItem.Timestamp);
            var strokesPoints = DrawingHelper.GetStrokes(currentTrackingPath, 1);
            timestampTrackStroke = new Stroke(strokesPoints) { DrawingAttributes = { Color = Colors.LawnGreen } };
            canvas.Strokes.Add(timestampTrackStroke);
        }

        /// <summary>
        /// Creates stylus point collection
        /// </summary>
        /// <param name="gazeLogItems">Data log items</param>
        /// <param name="preassureFactor">Line thickness</param>
        /// <returns>Stylus point collection </returns>
        public static StylusPointCollection GetStrokes(IEnumerable<GazeDataLogItem> gazeLogItems, float preassureFactor)
        {
            var strokePoints = new StylusPointCollection();

            foreach (var gazeLogItem in gazeLogItems)
            {
                strokePoints.Add(new StylusPoint(gazeLogItem.X, gazeLogItem.Y, preassureFactor));
            }
            return strokePoints;
        }

        /// <summary>
        /// Moves canvas element
        /// </summary>
        /// <param name="newPosition">The target position</param>
        /// <param name="element">The target element</param>
        public static void MoveCanvasElement(Point newPosition, UIElement element)
        {
            var oldPosition = new Point { X = Canvas.GetLeft(element), Y = Canvas.GetTop(element) };
            var animX = new DoubleAnimation(oldPosition.X, newPosition.X, TimeSpan.FromSeconds(0));
            var animY = new DoubleAnimation(oldPosition.Y, newPosition.Y, TimeSpan.FromSeconds(0));

            element.BeginAnimation(Canvas.LeftProperty, animX);
            element.BeginAnimation(Canvas.TopProperty, animY);
        }

        # endregion

        # region Private Methods
        /// <summary>
        /// For presentation reasons normalizes line height according to series type value range
        /// </summary>
        /// <param name="value">the value</param>
        /// <param name="seriesName">the series name</param>
        /// <returns>Normalized line height</returns>
        private static double NormalizeWithFactor(double value, SeriesNames seriesName)
        {
            double output = value;
            switch (seriesName)
            {
                case SeriesNames.Velocity:
                    output = value / 8;
                    break;
                case SeriesNames.Distance:
                    output = value * 4;
                    break;
                case SeriesNames.XCoordinate:
                case SeriesNames.YCoordinate:
                    output = value / 20;
                    break;
                case SeriesNames.PupilSizeLeft:
                case SeriesNames.PupilSizeRight:
                    output = value * 8;
                    break;
            }
            return output;
        }

        # endregion
    }
}
