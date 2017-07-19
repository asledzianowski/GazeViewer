using GazeDataViewer.Classes.Saccade;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GazeDataViewer.Classes.GraphControl
{
    public static class AntiSaccadeGraphController
    {
        public static void ApplyAntiSaccadePointMarkers(ChartPlotter amplitudePlotter, List<EyeMove> antiSaccades)
        {
            var saccadeStartFoundDataSource = new EnumerableDataSource<EyeMove>(antiSaccades.Where(x => x.IsStartFound == true));
            saccadeStartFoundDataSource.SetXMapping(x => x.EyeStartTime);
            saccadeStartFoundDataSource.SetYMapping(x => x.EyeStartCoord);

            var saccadeEndFoundDataSource = new EnumerableDataSource<EyeMove>(antiSaccades.Where(x => x.IsEndFound == true));
            saccadeEndFoundDataSource.SetXMapping(x => x.EyeEndTime);
            saccadeEndFoundDataSource.SetYMapping(x => x.EyeEndCoord);


            var marker = new MarkerPointsGraph(saccadeStartFoundDataSource);
            var markPen = new TrianglePointMarker();
            markPen.Pen = new Pen(Brushes.Chartreuse, 1);
            markPen.Size = 7;
            marker.Name = "AntiSaccStart";
            markPen.Fill = Brushes.Chartreuse;
            marker.Marker = markPen;
            amplitudePlotter.Children.Add(marker);


            marker = new MarkerPointsGraph(saccadeEndFoundDataSource);
            markPen = new TrianglePointMarker();
            markPen.Pen = new Pen(Brushes.Gold, 1);
            markPen.Size = 7;
            marker.Name = "SaccEnd";
            markPen.Fill = Brushes.Gold;
            marker.Marker = markPen;
            amplitudePlotter.Children.Add(marker);


        }

        public static void ApplyAntiSaccadeTextMarkers(ChartPlotter amplitudePlotter, bool isVisible, List<double> arrayX, List<double> arrayY, List<int> IDs, string text)
        {

            for (int i = 0; i < arrayX.Count; i++)
            {
                var xDataSource = new EnumerableDataSource<double>(new double[1] { arrayX[i] });
                xDataSource.SetXMapping(x => x);
                var yDataSource = new EnumerableDataSource<double>(new double[1] { arrayY[i] });
                yDataSource.SetYMapping(x => x);

                var saccadeStartCompositeDataSource = new CompositeDataSource(xDataSource, yDataSource);

                var marker = new MarkerPointsGraph(saccadeStartCompositeDataSource);
                var textMarker = new CenteredTextMarker();
                textMarker.Text = $"{text}:A#{IDs[i]}";
                marker.Marker = textMarker;
                marker.Name = $"AntiSacc{text}Label";
                if (!isVisible)
                {
                    marker.Visibility = Visibility.Hidden;
                }

                amplitudePlotter.Children.Add(marker);
            }

        }

        public static void RemoveSaccadeMarkers(ChartPlotter amplitudePlotter)
        {
            var removeItems = new List<MarkerPointsGraph>();

            foreach (var children in amplitudePlotter.Children)
            {
                if (children is MarkerPointsGraph)
                {
                    var marker = children as MarkerPointsGraph;
                    if (marker.Name.Equals("AntiSaccStartLabel") || marker.Name.Equals("AntiSaccEndLabel") ||
                        marker.Name.Equals("AntiSaccStart") || marker.Name.Equals("AntiSaccEnd"))
                    {
                        removeItems.Add(marker);
                    }
                }
            }

            foreach (var itemForRemoval in removeItems)
            {
                amplitudePlotter.Children.Remove(itemForRemoval);
            }

        }
    }
}
