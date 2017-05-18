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
using GazeDataViewer.Classes.Saccade;
using static GazeDataViewer.Classes.Denoise.FilterButterworth;
using GazeDataViewer.Classes.Denoise;
using System.IO;

namespace GazeDataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpotGainResults CurrentResults { get; set; }

        SpotGazeFileData FileData { get; set; }

        List<SaccadePosition> SaccadePositions { get; set; }

        string SpotFilePath { get; set; }

        string EyeFilePath { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //EyeFilePath = "D:\\Development\\TheEyeTribeSDK\\GazeDataViewer\\result_out.txt";

      
        }



        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName) && DataHelper.IsDataFileExtention(dialog.FileName))
            {
                EyeFilePath = dialog.FileName;
                LoadDataPathTextBox.Text = dialog.FileName;
                LoadDataAndAnalyze(EyeFilePath);
                UnlockControll(true);
            }
            else
            {
                MessageBox.Show("File name empty or extention not allowed (only csv and txt).");
            }

        }

        private void UnlockControll(bool isEnabled)
        {
            GBGraph.IsEnabled = isEnabled;
            GBEyeMoves.IsEnabled = isEnabled;
            GBFilters.IsEnabled = isEnabled;
            GBPredictions.IsEnabled = isEnabled;
            GBResults.IsEnabled = isEnabled;
            GBTextOutput.IsEnabled = isEnabled;
        }

        private void LoadDataAndAnalyze(string eyeFilePath)
        {
            var timeColumnIndex = int.Parse(TBTimestampColumnIndex.Text);
            var eyeColumnIndex = int.Parse(TBEyeColumnIndex.Text);
            var spotColumnIndex = int.Parse(TBSpotColumnIndex.Text);

            this.FileData = DataHelper.LoadDataForSpotAndGaze(eyeFilePath, timeColumnIndex, eyeColumnIndex, spotColumnIndex);
            var calcConfig = GetCurrentCalcConfig();
            TBEndRec.Text = FileData.Time.Length.ToString();
            Analyze(this.FileData, calcConfig);
        }



        private void Analyze(SpotGazeFileData fileData, CalcConfig calcConfig)
        {

            var recStart = 0;
            var recEnd = 0;


            var startParsed = int.TryParse(TBStartRec.Text, out recStart);
            if (!startParsed)
            {
                MessageBox.Show("Wrong value of start rec");
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

                var filterConfig = GetCurrentFilterConfig();
                CurrentResults = DataAnalyzer.Analyze(fileData, calcConfig, recEnd, filterConfig);
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
            var timeAxisDataSource = new EnumerableDataSource<int>(results.PlotData.TimeStamps);
            timeAxisDataSource.SetXMapping(x => x);

            var eyeDataSource = new EnumerableDataSource<double>(results.PlotData.EyeCoords);
            eyeDataSource.SetYMapping(x => x);

            var spotDataSource = new EnumerableDataSource<double>(results.PlotData.SpotCoords);
            spotDataSource.SetYMapping(x => x);

            var eyeCompositeDataSource = new CompositeDataSource(timeAxisDataSource, eyeDataSource);
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
            //amplitudePlotter.FitToView();


            SaccadePositions = new List<SaccadePosition>();
            for (int i = 0; i < results.PlotData.EarliestEyeOverSpotIndex.Count; i++)
            {
                var currentEyeOverSpotIndex = SaccadeDataHelper.CountEyeStartIndex(results.PlotData.EarliestEyeOverSpotIndex[i], calcCnfig.EyeStartShiftPeroid);
                var spotOverMeanIndex = results.PlotData.SpotOverMeanIndex[i];
                var currentEyeShiftIndex = SaccadeDataHelper.CountEyeShiftIndex(currentEyeOverSpotIndex, results.PlotData.ShiftPeriod, calcCnfig.EyeEndShiftPeroid);
                var currentSpotShiftIndex = SaccadeDataHelper.CountSpotShiftIndex(spotOverMeanIndex, results.PlotData.ShiftPeriod);

                if (currentEyeShiftIndex < results.PlotData.TimeStamps.Count() && currentSpotShiftIndex < results.PlotData.TimeStamps.Count())
                {
                    var saccItem = SaccadeDataHelper.GetSaccadePositionItem(i, currentEyeOverSpotIndex, currentEyeShiftIndex,
                        spotOverMeanIndex, currentSpotShiftIndex, results);
                    SaccadePositions.Add(saccItem);
                }
            }

        

            ApplySpotPointMarkers(SaccadePositions);
            ApplySaccadeMarkersAndControls(SaccadePositions);

            //amplitudePlotter.FitToView();

            //LogData(results);

            //fit to view with margins
            var yMargin = ((results.PlotData.EyeCoords.Max() - results.PlotData.EyeCoords.Min()) / 10);
            var xMargin = ((results.PlotData.TimeStamps.Max() - results.PlotData.TimeStamps.Min()) / 20);

            var xMin = results.PlotData.TimeStamps.Min() - xMargin;
            var xMax = results.PlotData.TimeStamps.Max() + xMargin;
            var yMin = results.PlotData.EyeCoords.Min() - yMargin;
            var yMax = results.PlotData.EyeCoords.Max() + yMargin;

            if (CBFixView.IsChecked == false)
            {
                amplitudePlotter.FitToView();
                amplitudePlotter.Viewport.Visible = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
            }
        }


        private void ApplySpotPointMarkers(List<SaccadePosition> saccades)
        {
            var spotStartDataSource = new EnumerableDataSource<SaccadePosition>(saccades);
            spotStartDataSource.SetXMapping(x => x.SpotStartTime);
            spotStartDataSource.SetYMapping(x => x.SpotStartCoord);

            var spotEndDataSource = new EnumerableDataSource<SaccadePosition>(saccades);
            spotEndDataSource.SetXMapping(x => x.SpotEndTime);
            spotEndDataSource.SetYMapping(x => x.SpotEndCoord);

            var marker = new MarkerPointsGraph(spotStartDataSource);
            var markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Red, 1);
            markPen.Size = 6;
            marker.Name = "SpotStart";
            markPen.Fill = Brushes.Red;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

            marker = new MarkerPointsGraph(spotEndDataSource);
            markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Red, 1);
            markPen.Size = 6;
            marker.Name = "SpotEnd";
            markPen.Fill = Brushes.Red;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

        }
        private void ApplySaccadeMarkersAndControls(List<SaccadePosition> saccades)
        {
            RemoveSaccadeMarkers();
            ApplySaccadePointMarkers(saccades);
            ApplySaccadeTextMarkers(saccades.Select(x => x.SaccadeStartTime).ToList(), saccades.Select(x => x.SaccadeStartCoord).ToList(), "Start");
            ApplySaccadeTextMarkers(saccades.Select(x => x.SaccadeEndTime).ToList(), saccades.Select(x => x.SaccadeEndCoord).ToList(), "End");
            ApplySaccadeControls(saccades);
        }

        private void ApplySaccadePointMarkers(List<SaccadePosition> saccades)
        {
            var saccadeStartDataSource = new EnumerableDataSource<SaccadePosition>(saccades);
            saccadeStartDataSource.SetXMapping(x => x.SaccadeStartTime);
            saccadeStartDataSource.SetYMapping(x => x.SaccadeStartCoord);

            var saccadeEndDataSource = new EnumerableDataSource<SaccadePosition>(saccades);
            saccadeEndDataSource.SetXMapping(x => x.SaccadeEndTime);
            saccadeEndDataSource.SetYMapping(x => x.SaccadeEndCoord);

            var marker = new MarkerPointsGraph(saccadeStartDataSource);
            var markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Chartreuse, 1);
            markPen.Size = 6;
            marker.Name = "SaccStart";
            markPen.Fill = Brushes.Chartreuse;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

            marker = new MarkerPointsGraph(saccadeEndDataSource);
            markPen = new CirclePointMarker();
            markPen.Pen = new Pen(Brushes.Gold, 1);
            markPen.Size = 6;
            marker.Name = "SaccEnd";
            markPen.Fill = Brushes.Gold;
            marker.Marker = markPen;

            amplitudePlotter.Children.Add(marker);

        }


        private void ApplySaccadeTextMarkers(List<double> arrayX, List<double> arrayY, string text)
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
                textMarker.Text = $"{text} #{i}";
                marker.Marker = textMarker;
                marker.Name = $"Sacc{text}Label";
                if (CBShowLabels.IsChecked == false)
                {
                    marker.Visibility = Visibility.Hidden;
                }

                amplitudePlotter.Children.Add(marker);
            }

        }

        private void RemoveSaccadeMarkers()
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

        private void ApplySaccadeControls(List<SaccadePosition> saccades)
        {
            SaccadeControlsPanel.Children.Clear();

            foreach (var saccade in saccades)
            {
                var panel = GetSaccadeControl(saccade);
                SaccadeControlsPanel.Children.Add(panel);
            }
        }

        private StackPanel GetSaccadeControl(SaccadePosition saccade)
        {
            var itemPanel = new StackPanel();
            itemPanel.Orientation = Orientation.Horizontal;
            itemPanel.HorizontalAlignment = HorizontalAlignment.Right;
            itemPanel.Margin = new Thickness(0, 10, 0, 0);
            itemPanel.Tag = saccade.Id.ToString();
            itemPanel.Name = "SaccPanel";

            var saccLabel = new Label();
            saccLabel.Content = $"#{saccade.Id}";

            var controlPadding = new Thickness(2, 5, 2, 5);

            var saccStartTB = new TextBox();
            saccStartTB.Text = saccade.SaccadeStartIndex.ToString();
            saccStartTB.Name = "TBSaccStart";
            saccStartTB.Padding = controlPadding;
            saccStartTB.VerticalAlignment = VerticalAlignment.Center;
            saccStartTB.PreviewTextInput += NumberValidationTextBox;

            var saccEndTB = new TextBox();
            saccEndTB.Text = saccade.SaccadeEndIndex.ToString();
            saccEndTB.Name = "TBSaccEnd";
            saccEndTB.Padding = controlPadding;
            saccEndTB.VerticalAlignment = VerticalAlignment.Center;
            saccEndTB.PreviewTextInput += NumberValidationTextBox;

            var removeButton = new Button();
            removeButton.Content = "X";
            //removeButton.F = FontWeights.Bold;
            removeButton.Tag = saccade.Id;
            removeButton.VerticalAlignment = VerticalAlignment.Center;
            removeButton.Padding = controlPadding;
            removeButton.Click += BtnRemoveEyeMove_Click;

            itemPanel.Children.Add(saccLabel);
            itemPanel.Children.Add(saccStartTB);
            itemPanel.Children.Add(saccEndTB);
            itemPanel.Children.Add(removeButton);

            return itemPanel;
        }

        private void ReApplySaccades()
        {
            this.SaccadePositions = GetSaccadeParamsFromControls(this.SaccadePositions);
            ApplySaccadeMarkersAndControls(SaccadePositions);
            //Quick fix for first checkobox tick not rendering
            RefreshGraphLabels(true);
            //RefreshGraphLabels(false);
        }

        private List<SaccadePosition> GetSaccadeParamsFromControls(List<SaccadePosition> oldSaccadePositions)
        {
            var newSaccadePositions = new List<SaccadePosition>();

            foreach (var children in SaccadeControlsPanel.Children)
            {
                if (children is StackPanel)
                {
                    var panel = children as StackPanel;
                    if (panel.Name.Equals("SaccPanel"))
                    {
                        var saccId = int.Parse(panel.Tag.ToString());
                        int saccStartIndex = -1;
                        int saccEndIndex = -1;

                        foreach (var panelChildren in panel.Children)
                        {
                            if (panelChildren is TextBox)
                            {
                                var textBox = panelChildren as TextBox;
                                if (textBox.Name == "TBSaccStart")
                                {
                                    saccStartIndex = int.Parse(textBox.Text);
                                }
                                else if (textBox.Name == "TBSaccEnd")
                                {
                                    saccEndIndex = int.Parse(textBox.Text);
                                }
                            }
                        }

                        var prevoiusSaccadeItem = oldSaccadePositions.FirstOrDefault(x => x.Id == saccId);
                        var saccItem = SaccadeDataHelper.GetSaccadePositionItem(saccId, saccStartIndex, saccEndIndex, prevoiusSaccadeItem.SpotStartIndex,
                            prevoiusSaccadeItem.SpotStartTime, prevoiusSaccadeItem.SpotStartCoord, prevoiusSaccadeItem.SpotEndIndex, prevoiusSaccadeItem.SpotEndTime,
                            prevoiusSaccadeItem.SpotEndCoord, CurrentResults);
                        newSaccadePositions.Add(saccItem);
                    }
                }
            }
            return newSaccadePositions;
        }




        private void LogData(List<SaccadeCalculation> results)
        {
            var sb = new StringBuilder();
            var sep = "  ";
            var rowSep = "=========================================";

            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append($"Date/Time: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            sb.Append(Environment.NewLine);
            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);



            foreach (var result in results)
            {
                sb.Append($"Saccade Id: {result.Id}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start Index: {result.SpotStartIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End Index: {result.SpotEndIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start Index: {result.EyeStartIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End Index: {result.EyeEndIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Frame Count: {result.FrameCount}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);

                sb.Append("Y Values:");
                sb.Append(Environment.NewLine);
                for (int i = result.EyeStartIndex; i < result.EyeStartIndex + result.FrameCount; i++)
                {
                    sb.Append($"Index:{i} - Value: {CurrentResults.PlotData.EyeCoords[i]}");
                    sb.Append(Environment.NewLine);
                }

                sb.Append(Environment.NewLine);
                sb.Append("X Values:");
                sb.Append(Environment.NewLine);
                for (int i = result.EyeStartIndex; i < result.EyeStartIndex + result.FrameCount; i++)
                {
                    sb.Append($"Index:{i} - Time: {CurrentResults.PlotData.TimeStamps[i]}");
                    sb.Append(Environment.NewLine);
                }

                sb.Append(Environment.NewLine);
                sb.Append($"Eye/Spot Gain: {result.Gain} ");
                sb.Append(Environment.NewLine);
                sb.Append($"Distance: {result.Distance} cm");
                sb.Append(Environment.NewLine);
                sb.Append($"Latency: {result.Latency} sec");
                sb.Append(Environment.NewLine);
                sb.Append($"Duration: {result.Duration} sec");
                sb.Append(Environment.NewLine);
                sb.Append($"Visual Angle: {result.Amplitude} deg");
                sb.Append(Environment.NewLine);
                sb.Append($"Velocity: {result.Velocity} deg/sec");
                sb.Append(Environment.NewLine);
                sb.Append($"Average Velocity: {result.AvgVelocity} deg/sec");
                sb.Append(Environment.NewLine);
                sb.Append($"Max Velocity: {result.MaxVelocity} deg/sec");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }


            var currentText = sb.ToString() + LogTextBox.Text.ToString();
            LogTextBox.Text = currentText;
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
            double spotProp;
            int reduceEyeSpotAmpDiff;
            int eyeStartShiftPeroid;
            int eyeEndShiftPeroid;
            

            var isRecStart = int.TryParse(TBStartRec.Text, out recStart);
            if (isRecStart)
            {
                calcConfig.RecStart = recStart;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Start Rec'. Should be int.");
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

            var isSaccadeEndShiftPeroid = int.TryParse(TBSaccadeEndShiftPeroid.Text, out eyeEndShiftPeroid);
            if (isSaccadeEndShiftPeroid)
            {
                calcConfig.EyeEndShiftPeroid = eyeEndShiftPeroid;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Eye End Shift Peroid'. Should be int.");
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

            var isAmpProp = double.TryParse(TBEyeAmpProp.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out ampProp);
            if (isAmpProp)
            {
                calcConfig.EyeAmpProp = ampProp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Eye Amp Prop'. Should be double (decimal).");
            }

            var isSpotProp = double.TryParse(TBSpotAmpProp.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out spotProp);
            if (isSpotProp)
            {
                calcConfig.SpotAmpProp = spotProp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Spot Amp Prop'. Should be double (decimal).");
            }

            var isEyeStartShiftPeroid = int.TryParse(TBSaccadeStartShiftPeroid.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out eyeStartShiftPeroid);
            if (isEyeStartShiftPeroid)
            {
                calcConfig.EyeStartShiftPeroid = eyeStartShiftPeroid;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Eye Start Shift Peroid'. Should be {calcConfig.EyeStartShiftPeroid.GetType().Name}");
            }

            return calcConfig;

        }

        private FiltersConfig GetCurrentFilterConfig()
        {
            var filtersConfig = new FiltersConfig();

            double butterworthFrequency;
            int butterworthSampleRate;
            PassType butterworthPassType;
            double butterworthResonance;

            int savitzkyGolayNumberOfPoints;
            int savitzkyGolayDerivativeOrder;
            int savitzkyGolayPolynominalOrder;

            if (CBFilterButterworth.IsChecked == true)
            {
                filtersConfig.FilterByButterworth = true;
                var isbutterworthFrequency = double.TryParse(TBButterWorthFrequency.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out butterworthFrequency);
                if (isbutterworthFrequency)
                {
                    filtersConfig.ButterworthFrequency = butterworthFrequency;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Butter Worth Frequency'. Should be {filtersConfig.ButterworthFrequency.GetType().Name}.");
                }

                var isButterworthSampleRate = int.TryParse(TBButterWorthSampleRate.Text, out butterworthSampleRate);
                if (isButterworthSampleRate)
                {
                    filtersConfig.ButterworthSampleRate = butterworthSampleRate;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Butter Worth Frequency'. Should be {filtersConfig.ButterworthSampleRate.GetType().Name}.");
                }

                var isButterworthPassType = Enum.TryParse(ComboFilterTypeButterworth.SelectedValue.ToString(), out butterworthPassType);
                if (isButterworthPassType)
                {
                    filtersConfig.ButterworthPassType = butterworthPassType;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Butter Pass Type'. Should be {filtersConfig.ButterworthPassType.GetType().Name}.");
                }

                var isButterworthResonance = double.TryParse(TBButterWorthResonance.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out butterworthResonance);
                if (isButterworthResonance)
                {
                    filtersConfig.ButterworthResonance = butterworthResonance;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Butter Pass Type'. Should be {filtersConfig.ButterworthResonance.GetType().Name}.");
                }
            }
            else
            {
                filtersConfig.FilterByButterworth = false;
            }

            if (CBFilterSavGolay.IsChecked == true)
            {
                filtersConfig.FilterBySavitzkyGolay = true;
                var issavitzkyGolayNuberOfPoints = int.TryParse(TBSavGoayPointsNum.Text, out savitzkyGolayNumberOfPoints);
                if (issavitzkyGolayNuberOfPoints)
                {
                    filtersConfig.SavitzkyGolayNumberOfPoints = savitzkyGolayNumberOfPoints;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Number Of Points'. Should be {filtersConfig.SavitzkyGolayNumberOfPoints.GetType().Name}.");
                }

                var issavitzkyGolayDerivativeOrder = int.TryParse(TBSavGoayDerivOrder.Text, out savitzkyGolayDerivativeOrder);
                if (issavitzkyGolayDerivativeOrder)
                {
                    filtersConfig.SavitzkyGolayDerivativeOrder = savitzkyGolayDerivativeOrder;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Derivative Order '. Should be {filtersConfig.SavitzkyGolayDerivativeOrder.GetType().Name}.");
                }

                var issavitzkyGolayPolynominalOrder = int.TryParse(TBSavGoayPolyOrder.Text, out savitzkyGolayPolynominalOrder);
                if (issavitzkyGolayPolynominalOrder)
                {
                    filtersConfig.SavitzkyGolayPolynominalOrder = savitzkyGolayPolynominalOrder;
                }
                else
                {
                    MessageBox.Show($"Wrong value of 'Polynominal Order  '. Should be {filtersConfig.SavitzkyGolayPolynominalOrder.GetType().Name}.");
                }
            }
            else
            {
                filtersConfig.FilterBySavitzkyGolay = false;
            }
            return filtersConfig;

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

        private void CBShowLabels_Click(object sender, RoutedEventArgs e)
        {
            RefreshGraphLabels(CBShowLabels.IsChecked.GetValueOrDefault());
        }


        private void RefreshGraphLabels(bool isChecked)
        {
            if (isChecked == true)
            {
                foreach (var children in amplitudePlotter.Children)
                {
                    if (children is MarkerPointsGraph)
                    {
                        var marker = children as MarkerPointsGraph;
                        if (marker.Name.Equals("SaccStartLabel") || marker.Name.Equals("SaccEndLabel"))
                        {
                            marker.Visibility = Visibility.Visible;

                        }
                    }
                }
            }
            else
            {
                foreach (var children in amplitudePlotter.Children)
                {
                    if (children is MarkerPointsGraph)
                    {
                        var marker = children as MarkerPointsGraph;
                        if (marker.Name.Equals("SaccStartLabel") || marker.Name.Equals("SaccEndLabel"))
                        {
                            marker.Visibility = Visibility.Hidden;
                        }
                    }
                }
            }
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            ReApplySaccades();
        }

        private void BtnSaccadeCalc_Click(object sender, RoutedEventArgs e)
        {

            var distanceFromScreen = int.Parse(TBDistanceFromScreen.Text);
            var frequency = int.Parse(TBFrequency.Text);
            var saccCalculator = new SaccadeParamsCalcuator(SaccadePositions, CurrentResults.PlotData.EyeCoords,
                CurrentResults.PlotData.SpotCoords, distanceFromScreen, frequency);
            var results = saccCalculator.Calculate();
            LogData(results);
        }

        private void BtnAddEyeMove_Click(object sender, RoutedEventArgs e)
        {
            var eyeStartIndex = 0;
            var spotStartIndex = eyeStartIndex;
            var eyeShift = int.Parse(TBEyeShiftPeroid.Text);
            var saccadEndShiftPeroid = int.Parse(TBSaccadeEndShiftPeroid.Text);
            var spotShiftIndex = SaccadeDataHelper.CountSpotShiftIndex(spotStartIndex, eyeShift);
            var eyeMoveEndIndex = SaccadeDataHelper.CountEyeShiftIndex(eyeStartIndex, eyeShift, saccadEndShiftPeroid);

            var newSaccades = new List<SaccadePosition>();

            var currentId = 0;
            newSaccades.Add(new SaccadePosition
            {
                Id = currentId,
                SaccadeStartIndex = eyeStartIndex,
                SaccadeStartCoord = CurrentResults.PlotData.EyeCoords[eyeStartIndex],
                SaccadeStartTime = CurrentResults.PlotData.TimeStamps[eyeStartIndex],

                SaccadeEndIndex = eyeMoveEndIndex,
                SaccadeEndCoord = CurrentResults.PlotData.EyeCoords[eyeMoveEndIndex],
                SaccadeEndTime = CurrentResults.PlotData.TimeStamps[eyeMoveEndIndex],

                SpotStartIndex = spotStartIndex,
                SpotStartCoord = CurrentResults.PlotData.SpotCoords[spotStartIndex],
                SpotStartTime = CurrentResults.PlotData.TimeStamps[spotStartIndex],

                SpotEndIndex = spotShiftIndex,
                SpotEndCoord = CurrentResults.PlotData.SpotCoords[spotShiftIndex],
                SpotEndTime = CurrentResults.PlotData.TimeStamps[spotShiftIndex]
            });

            currentId++;
            foreach (var saccade in this.SaccadePositions)
            {
                saccade.Id = currentId;
                newSaccades.Add(saccade);
                currentId++;
            };

            this.SaccadePositions = newSaccades;
            ApplySaccadeMarkersAndControls(this.SaccadePositions);
        }

        private void BtnRemoveEyeMove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                var button = sender as Button;
                var saccadeId = Convert.ToInt32(button.Tag);
                var saccadeForRemove = this.SaccadePositions.First(x => x.Id == saccadeId);
                this.SaccadePositions.Remove(saccadeForRemove);
                ApplySaccadeMarkersAndControls(this.SaccadePositions);
            }
        }

        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            foreach(var passType in Enum.GetValues(typeof(PassType)))
            {
                data.Add(passType.ToString());
            }

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
            comboBox.SelectedIndex = 0;
        }

        private void BtnClearResults_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }

        private void BtnSaveResults_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = $"GazeAnalyzeResults{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}"; 
            dlg.DefaultExt = ".txt"; 
            dlg.Filter = "Text documents (.txt)|*.txt"; 

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filePath = dlg.FileName;
                if(!File.Exists(filePath))
                 {
                    File.WriteAllText(filePath, LogTextBox.Text);
                }
            }
        }

        private void BtnSaveScreenShot_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = $"GazeAnalyzeResults{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "jpg files (.jpg)|*.jpg";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filePath = dlg.FileName;
                if (!File.Exists(filePath))
                {
                    amplitudePlotter.SaveScreenshot(filePath);
                }
            }

                    
        }
    }
}

    
