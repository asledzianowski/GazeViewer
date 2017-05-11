using DynamicDataDisplay.Markers;
using GazeDataViewer.Classes;
using GazeDataViewer.Classes.SpotAndGain;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using GazeDataViewer.Classes.Denoise;

namespace GazeDataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpotGainResults CurrentResults { get; set; }

        SpotGazeFileData FileData { get; set; }

        SpotGazeFileDataTablet FileDataTablet { get;  set; }

        string SpotFilePath { get; set; }

        string EyeFilePath { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            EyeFilePath = "D:\\Development\\TheEyeTribeSDK\\GazeDataViewer\\result_out.txt";

            //EyeFilePath = "D:\\Development\\TheEyeTribeSDK\\GazeDataViewer\\gagol 4,5 po dbs on bmt on_sakady.csv";
            //var filePath = "D:\\Development\\TheEyeTribeSDK\\GazeDataViewer\\ET_LewickaAnna_12poDBS_sakady_BMTOff_DBSoff_20150116_075226.csv";
            //LoadDataAndAnalyze(filePath);
        }

       

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            
            LoadDataAndAnalyze(EyeFilePath);

            //var dialog = new OpenFileDialog();
            //dialog.ShowDialog();
            //if (!string.IsNullOrEmpty(dialog.FileName) && DataHelper.IsDataFileExtention(dialog.FileName))
            //{
            //    EyeFilePath = dialog.FileName;
            //    LoadDataPathTextBox.Text = dialog.FileName;
            //    LoadDataAndAnalyze(EyeFilePath);
            //}
            //else
            //{
            //    MessageBox.Show("File name empty or extention not allowed (only csv and txt).");
            //}

        }

        private void LoadDataAndAnalyze(string eyeFilePath)
        {
            this.FileData = DataHelper.LoadDataForSpotAndGaze(eyeFilePath);
            var calcConfig = GetCurrentCalcConfig();
            TBEndRec.Text = FileData.Time.Length.ToString();
            Analyze(this.FileData, calcConfig);
        }

      

        private void Analyze(SpotGazeFileData fileData, CalcConfig calcConfig)
        {
            
            var recStart = 0;
            var recEnd = 0;

            if (CBEvalStart.IsChecked == false)
            {
                var startParsed = int.TryParse(TBStartRec.Text, out recStart);
                if (!startParsed)
                {
                    MessageBox.Show("Wrong value of start rec");
                }
            }

            var endParsed = int.TryParse(TBEndRec.Text, out recEnd);
            if (!endParsed)
            {
                MessageBox.Show("Wrong value of end rec");

            }

            var numCoef = int.Parse(TBSavGoayPointsNum.Text);
            if (numCoef >= Math.Abs(recEnd - recStart))
            {
                MessageBox.Show("Number of coefficients must be <= number of records ");
                numCoef = Math.Abs(recEnd - recStart) - 1;
                TBSavGoayPointsNum.Text = numCoef.ToString();
            }


            if (recStart > 0 && recEnd <= FileData.Time.Length)
            {
               
                if (numCoef >= Math.Abs(recEnd - recStart))
                {
                    MessageBox.Show("Number of coefficients must be <= number of records ");
                    numCoef = Math.Abs(recEnd - recStart) - 1;
                    TBSavGoayPointsNum.Text = numCoef.ToString();
                }

                int derevOrder = int.Parse(TBSavGoayDerivOrder.Text);
                int polyOrder = int.Parse(TBSavGoayPolyOrder.Text);
                var shouldDenoise = CBDenoise.IsChecked.GetValueOrDefault();
                CurrentResults = DataAnalyzer.Analyze(fileData, calcConfig, shouldDenoise, recEnd, numCoef, derevOrder, polyOrder);
                ClearLines();
                PlotDataForSpotGain(CurrentResults, calcConfig);
            }
            else
            {
                MessageBox.Show("Start rec must be > 0, end rec < rec length ");
                TBStartRec.Text = "0";
                TBEndRec.Text = FileData.Time.Length.ToString();
            }
        }

      

        private void PlotDataForSpotGain(SpotGainResults results, CalcConfig calcCnfig)
        {

            //if(CBDenoise.IsChecked == true)
            //{
            //    var fix = new List<double>(); 
            //    double[,] output;
            //    double[] doubleArray = Array.ConvertAll(results.PlotData.Reye, x => (double)x);
            //    var waveletFilter = new WaveletFilter(WaveletFilterType.D4);
            //    var transform = new WaveletTransform(waveletFilter);
            //    transform.decompose(doubleArray, doubleArray.Length /2, 1, WaveletTransformType.MODWT, WaveletBoundaryCondition.Reflect, out output);

            //    for(int i = 0; i < output.Length; i++)
            //    {
            //       fix.Add(output[i, 0]);
            //    }

            //    results.PlotData.Reye = Array.ConvertAll(fix.ToArray(), x => (float)x);

            //}
            var timeAxisDataSource = new EnumerableDataSource<TimeSpan>(results.PlotData.Stime);
            timeAxisDataSource.SetXMapping(x => x.TotalSeconds);
           
            var lEyeDataSource = new EnumerableDataSource<float>(results.PlotData.Reye);
            lEyeDataSource.SetYMapping(x => x);

            var spotDataSource = new EnumerableDataSource<float>(results.PlotData.Spot);
            spotDataSource.SetYMapping(x => x);

            var eyeCompositeDataSource = new CompositeDataSource(timeAxisDataSource, lEyeDataSource);
            var spotCompositeDataSource = new CompositeDataSource(timeAxisDataSource, spotDataSource);

            LineGraph line;

            line = new LineGraph(eyeCompositeDataSource);
            line.LinePen = new Pen(Brushes.CadetBlue, 1);
            Legend.SetDescription(line, "Left Eye");
            amplitudePlotter.Children.Add(line);

            line = new LineGraph(spotCompositeDataSource);
            line.LinePen = new Pen(Brushes.Red, 1);
            Legend.SetDescription(line, "Spot");

            amplitudePlotter.Children.Add(line);
            amplitudePlotter.FitToView();


            var kreMarkerXPoints = new List<double>();
            var kreMarkerYPoints = new List<double>();

            var saccadeStartXPoints = new List<double>();
            var saccadeStartYPoints = new List<double>();

            var saccadeEndXPoints = new List<double>();
            var saccadeEndYPoints = new List<double>();

            var kspMarkerXPoints = new List<double>();
            var kspMarkerYPoints = new List<double>();

            var fillGaps = CBFillPlot.IsChecked.GetValueOrDefault();

            foreach (var kreIndex in results.PlotData.Kre)
            {
                var currentIndexForShift = kreIndex + results.PlotData.ShiftPeriod;

                if (currentIndexForShift < results.PlotData.Stime.Count())
                {

                    saccadeStartXPoints.Add(Math.Round(results.PlotData.Stime[kreIndex].TotalSeconds, 6));
                    saccadeStartYPoints.Add(results.PlotData.Reye[kreIndex]);

                    saccadeEndXPoints.Add(Math.Round(results.PlotData.Stime[currentIndexForShift].TotalSeconds, 6));
                    saccadeEndYPoints.Add(results.PlotData.Reye[currentIndexForShift]);

                    if (fillGaps)
                    {
                        for (int i = kreIndex; i <= currentIndexForShift; i++)
                        {
                            //if (!kreMarkerXPoints.Contains(Math.Round(results.PlotData.Stime[i].TotalSeconds, 2)))
                            //{
                            //    kreMarkerXPoints.Add(Math.Round(results.PlotData.Stime[i].TotalSeconds, 2));
                            //    kreMarkerYPoints.Add(results.PlotData.Leye[i]);
                            //}

                            kreMarkerXPoints.Add(Math.Round(results.PlotData.Stime[i].TotalSeconds, 6));
                            kreMarkerYPoints.Add(results.PlotData.Reye[i]);
                        }
                    }
                    else
                    {
                        kreMarkerXPoints.Add(Math.Round(results.PlotData.Stime[kreIndex].TotalSeconds, 6));
                        kreMarkerYPoints.Add(results.PlotData.Reye[kreIndex]);

                        kreMarkerXPoints.Add(Math.Round(results.PlotData.Stime[currentIndexForShift].TotalSeconds, 6));
                        kreMarkerYPoints.Add(results.PlotData.Reye[currentIndexForShift]);
                    }

                }
            }

            foreach (var kspIndex in results.PlotData.Ksp)
            {
                var shiftIndex = kspIndex + results.PlotData.ShiftPeriod;

                if ((kspIndex + results.PlotData.ShiftPeriod) < results.PlotData.Stime.Count())
                {

                    if (fillGaps)
                    {
                        for (int i = kspIndex; i <= shiftIndex; i++)
                        {
                            kspMarkerXPoints.Add(results.PlotData.Stime[i].TotalSeconds);
                            kspMarkerYPoints.Add(results.PlotData.Spot[i]);
                        }
                    }
                    else
                    {
                        kspMarkerXPoints.Add(results.PlotData.Stime[kspIndex].TotalSeconds);
                        //kspMarkerYPoints.Add(results.PlotData.Leye[kspIndex]);
                        kspMarkerYPoints.Add(results.PlotData.Spot[kspIndex]);

                        kspMarkerXPoints.Add(results.PlotData.Stime[shiftIndex].TotalSeconds);
                        kspMarkerYPoints.Add(results.PlotData.Spot[shiftIndex]);
                        //kspMarkerYPoints.Add(results.PlotData.Leye[shiftIndex]);

                    }

                }
            }

            var kreMarkerXDataSource = new EnumerableDataSource<double>(kreMarkerXPoints);
            kreMarkerXDataSource.SetXMapping(x => x);
            var kreMarkerYDataSource = new EnumerableDataSource<double>(kreMarkerYPoints);
            kreMarkerYDataSource.SetYMapping(x => x);

            var kspMarkerXDataSource = new EnumerableDataSource<double>(kspMarkerXPoints);
            kspMarkerXDataSource.SetXMapping(x => x);
            var kspMarkerYDataSource = new EnumerableDataSource<double>(kspMarkerYPoints);
            kspMarkerYDataSource.SetYMapping(x => x);


            var saccadeStartXDataSource = new EnumerableDataSource<double>(saccadeStartXPoints);
            saccadeStartXDataSource.SetXMapping(x => x);
            var saccadeStartYDataSource = new EnumerableDataSource<double>(saccadeStartYPoints);
            saccadeStartYDataSource.SetYMapping(x => x);

            var saccadeEndXDataSource = new EnumerableDataSource<double>(saccadeEndXPoints);
            saccadeEndXDataSource.SetXMapping(x => x);
            var saccadeEndYDataSource = new EnumerableDataSource<double>(saccadeEndYPoints);
            saccadeEndYDataSource.SetYMapping(x => x);



            var kreMarkerCompositeDataSource = new CompositeDataSource(kreMarkerXDataSource, kreMarkerYDataSource);
            var kspMarkerCompositeDataSource = new CompositeDataSource(kspMarkerYDataSource, kspMarkerXDataSource);

            var saccadeStartCompositeDataSource = new CompositeDataSource(saccadeStartXDataSource, saccadeStartYDataSource);
            var saccadeEndCompositeDataSource = new CompositeDataSource(saccadeEndXDataSource, saccadeEndYDataSource);

            var marker = new MarkerPointsGraph(kreMarkerCompositeDataSource);
            var markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.CornflowerBlue, 1);
            markPen.Size = 5;
            markPen.Fill = Brushes.CornflowerBlue;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

            marker = new MarkerPointsGraph(kspMarkerCompositeDataSource);
            markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Red, 1);
            markPen.Size = 5;
            markPen.Fill = Brushes.Red;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

            marker = new MarkerPointsGraph(saccadeStartCompositeDataSource);
            markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Chartreuse, 1);
            markPen.Size = 6;
            markPen.Fill = Brushes.Chartreuse;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

            //marker = new MarkerPointsGraph(saccadeStartCompositeDataSource);
            //marker.IsManipulationEnabled = true;
            //var textMarker = new CenteredTextMarker();
            //textMarker.Text = "TEST";
            //marker.Marker = textMarker;
            //amplitudePlotter.Children.Add(marker);

            ApplyTextMarkers(saccadeStartCompositeDataSource, saccadeEndCompositeDataSource);

            

            marker = new MarkerPointsGraph(saccadeEndCompositeDataSource);
            markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Gold, 1);
            markPen.Size = 6;
            markPen.Fill = Brushes.Gold;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

            amplitudePlotter.FitToView();


            LogData(results);

            //fit to view with margins
            var yMargin = ((results.PlotData.Reye.Max() - results.PlotData.Reye.Min()) / 10);
            var xMargin = ((results.PlotData.Stime.Max().TotalSeconds - results.PlotData.Stime.Min().TotalSeconds) / 20);

            var xMin = results.PlotData.Stime.Min().TotalSeconds - xMargin;
            var xMax = results.PlotData.Stime.Max().TotalSeconds + xMargin;
            var yMin = results.PlotData.Reye.Min() - yMargin; 
            var yMax = results.PlotData.Reye.Max() + yMargin;
            amplitudePlotter.Viewport.Visible = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private void ApplyTextMarkers(CompositeDataSource startPoints, CompositeDataSource endPoints)
        {
            //for (int i = 0; i < startPoints.DataParts[0].; i++)
            //{

            //}

        }

       

        private void LogData(SpotGainResults results)
        {
            var sb = new StringBuilder();
            var sep = "  ";
            sb.Append($"Date/Time: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            sb.Append(Environment.NewLine);
            sb.Append($"Mean spot amplitude: {results.MeanSpotAmplitude}");
            sb.Append(Environment.NewLine);
            sb.Append("Spot amp shifts [ksp]:  ");
            sb.Append(string.Join(" ; ", results.SpotOverAmpForSpotTimeStamps.Select(x => x.TotalSeconds.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append("Left eye amp shifts [kle]: ");
            sb.Append(string.Join(" ; ", results.LEyeEarliestOverAmpForSpotTimeStamps.Select(x => x.TotalSeconds.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append("Right eye amp shifts [kre]: ");
            sb.Append(string.Join(" ; ", results.REyeEarliestOverAmpForSpotTimeStamps.Select(x => x.TotalSeconds.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Right eye delays (sec) [tre2]: ");
            sb.Append(string.Join(" ; ", results.DelaysRe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append($"Right eye delay mean: {results.MeanDelayRe} (+/- {results.StdDelayRe} SD)");
            sb.Append(Environment.NewLine);
            sb.Append("Left eye delays (sec) [tle2]: ");
            sb.Append(string.Join(" ; ", results.DelaysLe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append($"Left eye delay mean: {results.MeanDelayLe} (+/- {results.StdDelayLe} SD)");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Right eye durations (sec) [rdur]: ");
            sb.Append(string.Join(" ; ", results.DurationsRe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append($"Right eye duration mean: {results.MeanDurationRe} (+/- {results.StdDurationRe} SD)");
            sb.Append(Environment.NewLine);
            sb.Append("Left eye durations (sec) [ldur]: ");
            sb.Append(string.Join(" ; ", results.DurationsRe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append($"Left eye duration mean: {results.MeanDurationLe} (+/- {results.StdDurationLe} SD)");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Left eye max speed time [sple_t]: ");
            sb.Append(string.Join(" ; ", results.MaxSpeedTimesLe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append("Left eye max speed amp [sple]: ");
            sb.Append(string.Join(" ; ", results.MaxSpeedAmpsLe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append("Right eye max speed time [spre_t]: ");
            sb.Append(string.Join(" ; ", results.MaxSpeedTimesRe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append("Right eye max speed amp [sple]: ");
            sb.Append(string.Join(" ; ", results.MaxSpeedAmpsRe.Select(x => x.ToString()).ToArray()));
            sb.Append(Environment.NewLine);
            sb.Append("==============================");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            LogTextBox.Text += sb.ToString();
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

            //_edsSPP = new EnumerableDataSource<SeriesItem>(dataSeries[SeriesNames.XCoordinate.ToString()]);
            //_edsSPP.SetXMapping(p => p.Timepoint);
            //_edsSPP.SetYMapping(p => p.Value);
            //_eds = _edsSPP;
            //line = new LineGraph(_eds);
            //line.LinePen = new Pen(Brushes.Red, 1);
            //Legend.SetDescription(line, "X Coordinate");
            //xyPlotter.Children.Add(line);

            //_edsSPP = new EnumerableDataSource<SeriesItem>(dataSeries[SeriesNames.YCoordinate.ToString()]);
            //_edsSPP.SetXMapping(p => p.Timepoint);
            //_edsSPP.SetYMapping(p => p.Value);
            //_eds = _edsSPP;
            //line = new LineGraph(_eds);
            //line.LinePen = new Pen(Brushes.Blue, 1);
            //Legend.SetDescription(line, "Y Coordinate");
            //xyPlotter.Children.Add(line);


            //xyPlotter.FitToView();
        }

        public void ClearLines()
        {
            
            var lgc = new Collection<IPlotterElement>();
            foreach (var x in amplitudePlotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph || x is MarkerPointsGraph)
                    lgc.Add(x);
            }

            foreach (var x in lgc)
            {
                amplitudePlotter.Children.Remove(x);

            }

           
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Equals("-"))
            {
                e.Handled = false;
            }
            else
            {
                var regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {

            if (e.Text.Equals(",") || e.Text.Equals(".") || e.Text.Equals("-"))
            {
                e.Handled = false;
            }
            else
            {
                var regex = new Regex("[^0-9]+");
                e.Handled = regex.IsMatch(e.Text);
            }
        }

       
        private void CBEvalStart_Click(object sender, RoutedEventArgs e)
        {
            if(!CBEvalStart.IsChecked.GetValueOrDefault())
            {
                TBStartRec.IsEnabled = true;
                TBStartRec.Text = "0";
            }
            else
            {
                TBStartRec.IsEnabled = false;
                TBStartRec.Text = string.Empty;
            }
        
        }

       

        private void ButtonCalculate_Click(object sender, RoutedEventArgs e)
        {
            var calcConfig = GetCurrentCalcConfig();
            Analyze(this.FileData, calcConfig);
            

        }

        private CalcConfig GetCurrentCalcConfig()
        {
            var calcConfig = new CalcConfig();
            int recStart;
            int eyeShiftPeriod;
            int spotShiftPeriod;
            double ampProp;
            double delayWindowLargerThan;
            double delayWindowSmallerThan;
            int reduceEyeSpotAmpDiff;

            if (!CBEvalStart.IsChecked.GetValueOrDefault())
            {
                var isRecStart = int.TryParse(TBStartRec.Text, out recStart);
                if (isRecStart)
                {
                    calcConfig.RecStart = recStart;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Start Rec'. Should be int.");
                }
            }

            var isEyeShiftPeriod = int.TryParse(TBEyeShiftPeroid.Text, out eyeShiftPeriod);
            if (isEyeShiftPeriod)
            {
                calcConfig.EyeShiftPeriod = eyeShiftPeriod;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Eye Shift Period'. Should be int.");
            }

            var isSpotShiftPeriod = int.TryParse(TBSpotShiftPeroid.Text, out spotShiftPeriod);
            if (isSpotShiftPeriod)
            {
                calcConfig.SpotShiftPeriod = spotShiftPeriod;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Spot Shift Period'. Should be int.");
            }

            var isReduceEyeSpotAmpDiff = int.TryParse(TBReduceEyeSpotAmpDiff.Text, out reduceEyeSpotAmpDiff);
            if (isReduceEyeSpotAmpDiff)
            {
                calcConfig.ReductMinEyeSpotAmpDiff = reduceEyeSpotAmpDiff;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Reduce Eye Spot Amp Diff'. Should be int.");
            }

            var isAmpProp = double.TryParse(TBAmpProp.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out ampProp);
            if (isAmpProp)
            {
                calcConfig.AmpProp = ampProp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Amp Prop'. Should be double (decimal).");
            }

            var isDelayWindowLargerThan = double.TryParse(TBDelayWindowLargerThan.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out delayWindowLargerThan);
            if (isDelayWindowLargerThan)
            {
                calcConfig.DelayWindowLargerThan = delayWindowLargerThan;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Delay Window Larger Than' Should be double (decimal).");
            }

            var isDelayWindowSmallerThan = double.TryParse(TBDelayWindowSmallerThan.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out delayWindowSmallerThan);
            if (isDelayWindowSmallerThan)
            {
                calcConfig.DelayWindowSmallerThan = delayWindowSmallerThan;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Delay Window Smaller Than' Should be double (decimal).");
            }

            calcConfig.CalcateRecStart = CBEvalStart.IsChecked.GetValueOrDefault();

            return calcConfig;

        }

        private void GetEyeDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName) && DataHelper.IsDataFileExtention(dialog.FileName))
            {
                EyeFilePath = dialog.FileName;
                LoadDataPathTextBox.Text = dialog.FileName;
            }
            else
            {
                MessageBox.Show("File name empty or extention not allowed (only csv and txt).");
            }
        }

        private void GetSpotDataFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName) && DataHelper.IsDataFileExtention(dialog.FileName))
            {
                SpotFilePath = dialog.FileName;
                LoadDataPathTextBox.Text = dialog.FileName;
            }
            else
            {
                MessageBox.Show("File name empty or extention not allowed (only csv and txt).");
            }
        }
    }
}
