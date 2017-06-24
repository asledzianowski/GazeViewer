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
    public static class SaccadeGraphController
    {
        public static void ApplySaccadePointMarkers(ChartPlotter amplitudePlotter, List<EyeMove> saccades)
        {

            var saccadeStartFoundDataSource = new EnumerableDataSource<EyeMove>(saccades.Where(x => x.IsStartFound == true));
            saccadeStartFoundDataSource.SetXMapping(x => x.EyeStartTime);
            saccadeStartFoundDataSource.SetYMapping(x => x.EyeStartCoord);


            var saccadeEndFoundDataSource = new EnumerableDataSource<EyeMove>(saccades.Where(x => x.IsEndFound == true));
            saccadeEndFoundDataSource.SetXMapping(x => x.EyeEndTime);
            saccadeEndFoundDataSource.SetYMapping(x => x.EyeEndCoord);


            var marker = new MarkerPointsGraph(saccadeStartFoundDataSource);
            var markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Chartreuse, 1);
            markPen.Size = 6;
            marker.Name = "SaccStart";
            markPen.Fill = Brushes.Chartreuse;
            marker.Marker = markPen;
            amplitudePlotter.Children.Add(marker);


            marker = new MarkerPointsGraph(saccadeEndFoundDataSource);
            markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Gold, 1);
            markPen.Size = 6;
            marker.Name = "SaccEnd";
            markPen.Fill = Brushes.Gold;
            marker.Marker = markPen;
            amplitudePlotter.Children.Add(marker);

        }

        public static void ApplySaccadeTextMarkers(ChartPlotter amplitudePlotter, bool isVisible, List<double> arrayX, List<double> arrayY, List<int> IDs, string text)
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
                textMarker.Text = $"{text.First()}:S#{IDs[i]}";
                marker.Marker = textMarker;
                marker.Name = $"Sacc{text}Label";
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
                    if (marker.Name.Equals("SaccStartLabel") || marker.Name.Equals("SaccEndLabel") ||
                        marker.Name.Equals("SaccStart") || marker.Name.Equals("SaccEnd"))
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
