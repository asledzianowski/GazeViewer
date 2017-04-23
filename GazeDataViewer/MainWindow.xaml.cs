using GazeDataViewer.Classes;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace GazeDataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName) && DataHelper.IsDataFileExtention(dialog.FileName))
            {
                var fileData = DataHelper.LoadData(dialog.FileName);

                int trackerfrequency;
                bool isTrackerFrequencyValue = int.TryParse(FrequencyTextBox.Text, out trackerfrequency);

                int distanceFromScreen;
                bool isDistanceFromScreenValue = int.TryParse(DistanceTextBox.Text, out distanceFromScreen);

                if (isDistanceFromScreenValue && isTrackerFrequencyValue)
                {
                    var calculationHelper = new CalculationHelper(distanceFromScreen, trackerfrequency);
                    var dataSeries = DrawingHelper.GenerateChartSeries(fileData, calculationHelper);
                    ClearLines();
                    PrintCharts(dataSeries);

                    LoadDataPathTextBox.Text = dialog.FileName;
                }
                else
                {
                    MessageBox.Show("Provide numeric values for screen distance and tracker frequency.");
                }

            }
            else
            {
                MessageBox.Show("File name empty or extention not allowed (only csv and txt).");
            }
        }

        private void PrintCharts(Dictionary<string, List<SeriesItem>> dataSeries)
        {
            IPointDataSource _eds = null;
            LineGraph line;
            EnumerableDataSource<SeriesItem> _edsSPP;
            _edsSPP = new EnumerableDataSource<SeriesItem>(dataSeries[SeriesNames.Amplitude.ToString()]);
            _edsSPP.SetXMapping(p => p.Timepoint);
            _edsSPP.SetYMapping(p => p.Value);
            _eds = _edsSPP;
            line = new LineGraph(_eds);
            line.LinePen = new Pen(Brushes.Red, 1);
            

            amplitudePlotter.Children.Add(line);
            amplitudePlotter.FitToView();

            _edsSPP = new EnumerableDataSource<SeriesItem>(dataSeries[SeriesNames.XCoordinate.ToString()]);
            _edsSPP.SetXMapping(p => p.Timepoint);
            _edsSPP.SetYMapping(p => p.Value);
            _eds = _edsSPP;
            line = new LineGraph(_eds);
            line.LinePen = new Pen(Brushes.Red, 1);
            Legend.SetDescription(line, "X Coordinate");
            xyPlotter.Children.Add(line);

            _edsSPP = new EnumerableDataSource<SeriesItem>(dataSeries[SeriesNames.YCoordinate.ToString()]);
            _edsSPP.SetXMapping(p => p.Timepoint);
            _edsSPP.SetYMapping(p => p.Value);
            _eds = _edsSPP;
            line = new LineGraph(_eds);
            line.LinePen = new Pen(Brushes.Blue, 1);
            Legend.SetDescription(line, "Y Coordinate");
            xyPlotter.Children.Add(line);


            xyPlotter.FitToView();
        }

        public void ClearLines()
        {
            var lgc = new Collection<IPlotterElement>();
            foreach (var x in amplitudePlotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph)
                    lgc.Add(x);
            }

            foreach (var x in lgc)
            {
                amplitudePlotter.Children.Remove(x);
            }

            lgc = new Collection<IPlotterElement>();
            foreach (var x in xyPlotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph)
                    lgc.Add(x);
            }

            foreach (var x in lgc)
            {
                xyPlotter.Children.Remove(x);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
