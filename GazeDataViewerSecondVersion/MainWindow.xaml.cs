﻿using DynamicDataDisplay.Markers;
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
using GazeDataViewer.Classes.Serialization;
using GazeDataViewer.Classes.Spot;
using GazeDataViewer.Classes.AntiSaccade;
using GazeDataViewer.Classes.GraphControl;
using GazeDataViewer.Classes.Enums;
using GazeDataViewer.Classes.DataAndLog;
using GazeDataViewer.Classes.EyeMoveSearch;
using GazeDataViewer.Classes.EnumsAndStats;
using GazeDataViewer.Classes.Additionals;

namespace GazeDataViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SpotGazeFileData FileData { get; set; }
        ResultData SpotEyePoints { get; set; }
        List<EyeMove> SaccadePositions { get; set; } = new List<EyeMove>();
        List<EyeMove> AntiSaccadePositions { get; set; } = new List<EyeMove>();
        List<SpotMove> SpotMovePositions { get; set; } = new List<SpotMove>();
        List<EyeMoveCalculation> SaccadeCalculations { get; set; } = new List<EyeMoveCalculation>();
        List<EyeMoveCalculation> AntiSaccadeCalculations { get; set; } = new List<EyeMoveCalculation>();
        EyeMoveCalculation PursuitMoveCalculations { get; set; } = new EyeMoveCalculation();

        public MainWindow()
        {
            InitializeComponent();
            //ACIIDSHelper.CopyFiles();
            BulkCalcJazzNovoPom();
        }

        private void BulkCalcJazzNovoPom()
        {
            var timeColumnIndex = int.Parse(TBTimestampColumnIndex.Text);
            var eyeColumnIndex = int.Parse(TBEyeColumnIndex.Text);
            var spotColumnIndex = int.Parse(TBSpotColumnIndex.Text);
            var pursuitWindowsFilterConfig = GetCurrentPursuitFilterConfig();

            var targetPath = @"D:\Data Scientific\ACIIDS\Output\";

            var fileInfo = new FileInfo(@"D:\Data Scientific\ACIIDS\BD_2017-10-25.csv");
            var fileStream = fileInfo.OpenRead();
            var fileDataStream = new StreamReader(fileStream);
            var fileText = fileDataStream.ReadToEnd();
            var neuroDataLines = fileText.Split('\n');
            var headers = neuroDataLines[0].Split(';');
            var headersVals = neuroDataLines[1].Split(';');

            var sessionTypes = new string[] { "POP", "DBS" };
            var moveTypes = new string[] { "125", "250", "500" };

            foreach (var sessionType in sessionTypes)
            {
                foreach (var moveType in moveTypes)
                {
                    //var sessionType = "POP";
                    //var moveType = "POM125";
                var directoryPath = @"D:\Data Scientific\ACIIDS\Sources\" + sessionType + @"\POM" + moveType + @"Sources\";
                var files = Directory.GetFiles(directoryPath).ToList();
                var csvFiles = files.Where(x => x.EndsWith(".csv"));

                var sb = new StringBuilder();
                sb.Append("@relation " + moveType + sessionType + Environment.NewLine);
                sb.Append("@attribute PatID numeric" + Environment.NewLine);
                sb.Append("@attribute CzasChoroby numeric" + Environment.NewLine);
                //sb.Append("@attribute Year numeric" + Environment.NewLine);
                //sb.Append("@attribute VisitID numeric" + Environment.NewLine);
                sb.Append($"@attribute {sessionType}-{moveType}-Gain numeric" + Environment.NewLine);
                sb.Append($"@attribute {sessionType}-{moveType}-Accuracy numeric" + Environment.NewLine);
                sb.Append("@attribute UPDRSIII numeric" + Environment.NewLine);
                sb.Append("@attribute DBS {0,1}" + Environment.NewLine);
                sb.Append("@attribute BMT {0,1}" + Environment.NewLine + Environment.NewLine);
                sb.Append("@data" + Environment.NewLine);

                    foreach (var eyeFilePath in csvFiles)
                    {
                        var fileNameParams = Path.GetFileName(eyeFilePath).Substring(0, 21).Split('-');

                        var dbs = fileNameParams[0];
                        var bmt = fileNameParams[1];
                        var patId = fileNameParams[2];
                        var year = fileNameParams[5];
                        var visit = fileNameParams[6];

                        var testTrue = Convert.ToInt32(bool.Parse("False"));
                        var testFalse = Convert.ToInt32(bool.Parse("True"));

                        for (int i = 1; i < neuroDataLines.Length; i++)
                        {
                            var neuroDataLine = neuroDataLines[i];
                            if (neuroDataLine != string.Empty)
                            {
                                var neuroItems = neuroDataLine.Split(';');
                                var neuroSessionID = neuroItems[0].Split('/');

                                var neuroPatId = neuroSessionID[0];
                                var neuroSessionType = neuroSessionID[2];
                                var neuroSessionYear = neuroSessionID[3];

                                if (patId.Equals(neuroPatId) && sessionType.Equals(neuroSessionType) && year.Equals(neuroSessionYear))
                                {
                                    var neuroDBS = neuroItems[129];
                                    var neuroBMT = neuroItems[130];
                                    var neuroVisit = neuroItems[7];
                                    if (neuroBMT != string.Empty)
                                    {
                                        neuroBMT = Convert.ToInt32(bool.Parse(neuroBMT)).ToString();
                                    }


                                    if (dbs == neuroDBS && bmt == neuroBMT && visit == neuroVisit)
                                    {
                                        var updrsIII = neuroItems[160];
                                        var czasChoroby = neuroItems[15];


                                        var fileData = InputDataHelper.LoadDataForSpotAndGaze(eyeFilePath, timeColumnIndex, eyeColumnIndex, spotColumnIndex);
                                        var pursuitCalculations = DataAnalyzer.CountSingleTypeSignalPursuitParameters(fileData, pursuitWindowsFilterConfig);

                                        var sinGain = Math.Round(pursuitCalculations.Gains.FirstOrDefault().Value.GetValueOrDefault(), 2);
                                        var sinAccuracy = Math.Round(pursuitCalculations.Accuracies.FirstOrDefault().Value.GetValueOrDefault(), 2);

                                        //sb.Append(sessionType + " ");
                                        sb.Append(patId + " ");
                                        sb.Append(czasChoroby.Replace(',', '.') + " ");
                                        //sb.Append(year + " ");
                                        //sb.Append(visit + " ");
                                        sb.Append(sinGain + " ");
                                        sb.Append(sinAccuracy + " ");
                                        sb.Append(updrsIII + " ");
                                        sb.Append(dbs + " ");
                                        sb.Append(bmt);
                                        sb.Append(Environment.NewLine);
                                    }

                                }

                            }
                        }
                    }

                    File.WriteAllText(targetPath + sessionType + moveType + "out.arff", sb.ToString());
                }
            }

            //var targetPath = @"D:\Data Scientific\ACIIDS\Output\";
            //File.WriteAllText(targetPath + moveType + "out.arff", sb.ToString());
            //File.WriteAllText(directoryPath + moveType + "outForJoin.arff", sb.ToString());
        }

        private void LoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.FileName) && InputDataHelper.IsAllowedDataFileExtention(dialog.FileName))
            {
                LoadDataPathTextBox.Text = dialog.FileName;
                LoadDataAndAnalyze(dialog.FileName);
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
            GBSaccades.IsEnabled = isEnabled;
            GBAntiSaccades.IsEnabled = isEnabled;
            GBPursuit.IsEnabled = isEnabled;
            GBPredictions.IsEnabled = isEnabled;
            GBResults.IsEnabled = isEnabled;
            GBTextOutput.IsEnabled = isEnabled;
        }

        private void LoadDataAndAnalyze(string eyeFilePath)
        {
            var timeColumnIndex = int.Parse(TBTimestampColumnIndex.Text);
            var eyeColumnIndex = int.Parse(TBEyeColumnIndex.Text);
            var spotColumnIndex = int.Parse(TBSpotColumnIndex.Text);

            var fileData = InputDataHelper.LoadDataForSpotAndGaze(eyeFilePath, timeColumnIndex, eyeColumnIndex, spotColumnIndex);
 

            if (fileData != null)
            {
                this.FileData = fileData;

                double? startTimeDelta = null;
                double? endTimeDelta = null;
               
                //Signal start/end to scaled time
                startTimeDelta = InputDataHelper.GetScaledTimeFromIndex(FileData, 1);
                endTimeDelta = InputDataHelper.GetScaledTimeFromIndex(FileData, FileData.TimeDeltas.Count() - 1);
                TBStartRec.Text = startTimeDelta.GetValueOrDefault().ToString();
                TBEndRec.Text = endTimeDelta.GetValueOrDefault().ToString();

                if (fileData.FileType != FileType.JazzNovo)
                {
                    CalculateAndSetParametersFromFileData(this.FileData);
                }

                var calcConfig = GetCurrentCalcConfig();
                var dataClone = InputDataHelper.CloneFileData(this.FileData);
                Analyze(dataClone, calcConfig);
                UnlockControll(true);
            }
            else
            {
                MessageBox.Show("Unable to process the file. Check column order or file text.");
            }
        }


        //Frequency, min latency and duration calculation
        private void CalculateAndSetParametersFromFileData(SpotGazeFileData fileData)
        {
            var frequencyCheckStart = InputDataHelper.GetIndexFromScaledTime(fileData, 8);
            var frequencyCheckEnd = InputDataHelper.GetIndexFromScaledTime(fileData, 36);
            if (frequencyCheckStart != null && frequencyCheckEnd != null)
            {
                var frequencyCheckFrameCount = frequencyCheckEnd - frequencyCheckStart;
                var calculatedFrequency = frequencyCheckFrameCount.GetValueOrDefault() / 28;
                TBFrequency.Text = calculatedFrequency.ToString();

                var saccMinLatency = calculatedFrequency / 10m; //100 ms
                var saccMinDuration = calculatedFrequency / 20m; //50 ms
                var antisaccMinLatency = calculatedFrequency / 4m; //250 ms
                var antisaccMinDuration = calculatedFrequency / 6.666666666666667m; //150 ms

                var saccMinLatencyRoundUp = Math.Ceiling(saccMinLatency);
                var saccMinDurationRoundUp = Math.Ceiling(saccMinDuration);
                var antisaccMinLatencyRoundUp = Math.Ceiling(antisaccMinLatency);
                var antisaccMinDurationRoundUp = Math.Ceiling(antisaccMinDuration);

                TBSaccMinLatency.Text = saccMinLatencyRoundUp.ToString();
                TBSaccMinDuration.Text = saccMinDurationRoundUp.ToString();
                TBAntiSaccMinLatency.Text = antisaccMinLatencyRoundUp.ToString();
                TBAntiSaccMinDuration.Text = antisaccMinDurationRoundUp.ToString();
            }
        }

        //private void BulkProcessFiles()
        //{
        //    var timeColumnIndex = int.Parse(TBTimestampColumnIndex.Text);
        //    var eyeColumnIndex = int.Parse(TBEyeColumnIndex.Text);
        //    var spotColumnIndex = int.Parse(TBSpotColumnIndex.Text);

        //    var calcConfig = GetCurrentCalcConfig();
        //    var filterConfig = GetCurrentFilterConfig();
        //    OutputHelper.GetSummaryForDirectory(@"D:\WynikiBrudno\Wszystkie", timeColumnIndex, eyeColumnIndex, spotColumnIndex, 
        //        calcConfig, filterConfig);
        //}


        private void Analyze(SpotGazeFileData fileData, CalcConfig calcConfig)
        {
            var filterConfig = GetCurrentFilterConfig();
            if (filterConfig.SavitzkyGolayNumberOfPoints >= Math.Abs(calcConfig.RecEnd - calcConfig.RecStart))
            {
                MessageBox.Show("Number of coefficients must be <= number of records ");
                filterConfig.SavitzkyGolayNumberOfPoints = Math.Abs(calcConfig.RecEnd - calcConfig.RecStart) - 1;
                TBSavGoayPointsNum.Text = filterConfig.SavitzkyGolayNumberOfPoints.ToString();
            }

            if (calcConfig.RecStart > 0 && calcConfig.RecEnd > 0 && calcConfig.RecEnd <= fileData.TimeDeltas.Count())
            {
                if (filterConfig.SavitzkyGolayNumberOfPoints >= Math.Abs(calcConfig.RecEnd - calcConfig.RecStart))
                {
                    MessageBox.Show("Number of coefficients must be <= number of records ");
                    filterConfig.SavitzkyGolayNumberOfPoints = Math.Abs(calcConfig.RecEnd - calcConfig.RecStart) - 1;
                    TBSavGoayPointsNum.Text = filterConfig.SavitzkyGolayNumberOfPoints.ToString();
                }
  
                if (filterConfig.FilterByButterworth)
                {
                    fileData.Eye = FilterController.FilterByButterworth(filterConfig, fileData.Eye);
                }

                if (filterConfig.FilterBySavitzkyGolay)
                {
                    fileData.Eye = FilterController.FilterBySavitzkyGolay(filterConfig, fileData.Eye);
                }

                var recLenght = (calcConfig.RecEnd - calcConfig.RecStart);
                fileData = InputDataHelper.CutData(fileData, calcConfig.RecStart, recLenght);

                this.SaccadeCalculations.Clear();
                this.AntiSaccadeCalculations.Clear();
                this.SpotMovePositions.Clear();
                this.SaccadePositions.Clear();
                this.AntiSaccadePositions.Clear();
                GraphClearElements();
                ApplyEyeSpotSinusoids(fileData.TimeDeltas, fileData.Eye, fileData.Spot, fileData.FileType);

               
                if(fileData.FileType == FileType.Brudno || fileData.FileType == FileType.Maruniec)
                {
                    ProcessCompoundSignal(fileData, calcConfig);
                }
                else
                {
                    ProcessSingleTypeSignal(fileData, calcConfig);
                }
              
               
                if (CBFixView.IsChecked == false && fileData?.TimeDeltas?.Length > 0 && fileData?.Eye?.Length > 0)
                {
                    FitGraphView(fileData);
                }

            }
            else
            {
                MessageBox.Show("Start rec must be > 0, end rec < rec length ");

                var startTimeDelta = InputDataHelper.GetScaledTimeFromIndex(fileData, 1);
                TBStartRec.Text = startTimeDelta.GetValueOrDefault().ToString();

                var endTimeDelta = InputDataHelper.GetScaledTimeFromIndex(fileData, fileData.TimeDeltas.Count() - 1);
                TBEndRec.Text = endTimeDelta.GetValueOrDefault().ToString();
            }
        }

        private void ProcessSingleTypeSignal(SpotGazeFileData fileData, CalcConfig calcConfig)
        {
            var pomResult = MessageBox.Show("Process signal for POM?", "Eye Move Choice", MessageBoxButton.YesNo);
            if (pomResult  == MessageBoxResult.Yes)
            {
                var pursuitWindowsFilterConfig = GetCurrentPursuitFilterConfig();
                var pursuitCalculations = DataAnalyzer.CountSingleTypeSignalPursuitParameters(fileData, pursuitWindowsFilterConfig);

                if (CBPOMShowWindows.IsChecked == true)
                {
                    ApplyPursuitFilteredWindows(pursuitCalculations.FilteredControlWindows);
                }

                this.PursuitMoveCalculations = new EyeMoveCalculation
                {
                    PursuitLongSinGain = pursuitCalculations.Gains.FirstOrDefault().Value,
                    PursuitLongSinAccuracy = pursuitCalculations.Accuracies.FirstOrDefault().Value
                };

            }
            else if (pomResult == MessageBoxResult.No)
            {
                var saccResult = MessageBox.Show("Process signal for Saccades?", "Eye Move Choice", MessageBoxButton.YesNo);

                SpotEyePoints = DataAnalyzer.FindSpotEyePointsForSaccadeAntiSaccadeSearch(fileData, calcConfig);
                this.ApplySpotPointMarkers(this.SpotEyePoints);
                List<int> spotBlock;

                if (saccResult  == MessageBoxResult.Yes)
                {
                    spotBlock = InputDataHelper.GetSpotMoveDataBlock(SpotEyePoints, EyeMoveTypes.Saccade, fileData.FileType);   
                    FindAndPlotSaccAntiSacc(spotBlock, SpotEyePoints, calcConfig, EyeMoveTypes.Saccade);
                }
                else 
                {
                    MessageBox.Show("Signal will be processed for AntiSaccades.", "Eye Move Choice");
                    spotBlock = InputDataHelper.GetSpotMoveDataBlock(SpotEyePoints, EyeMoveTypes.AntiSaccade, fileData.FileType);
                    FindAndPlotSaccAntiSacc(spotBlock, SpotEyePoints, calcConfig, EyeMoveTypes.AntiSaccade);
                }

                if (spotBlock.Count > 0)
                {
                    GBEyeMoves.IsEnabled = true;
                }
                else
                {
                    GBEyeMoves.IsEnabled = false;
                }
            }
        }



        private void ProcessCompoundSignal(SpotGazeFileData fileData, CalcConfig calcConfig)
        {
            //pursuit end timestamp 9231000 / initial index 5850
            // moruniec end 92310

            int pursuitEndStaticTag;
            int saccadeStartStaticTag;

            if (fileData.FileType == FileType.Maruniec)
            {
                pursuitEndStaticTag = Consts.PursuitEndTimeMaruniec;
                saccadeStartStaticTag = Consts.SaccadeStartTimeMaruniec;
            }
            else
            {
                pursuitEndStaticTag = Consts.PursuitEndTimeStandard;
                saccadeStartStaticTag = Consts.SaccadeStartTimeStandard;
            }

            var pursuitEndTime = fileData.Time.FirstOrDefault(x => x >= pursuitEndStaticTag);
            if (pursuitEndTime == 0)
            {
                pursuitEndTime = fileData.Time.LastOrDefault();
            }

            //Calibration end timestamp 10149000
            var saccadesStartTime = fileData.Time.FirstOrDefault(x => x >= saccadeStartStaticTag);
            if (saccadesStartTime == 0)
            {
                saccadesStartTime = fileData.Time.LastOrDefault();
            }

            var pursuitEndIndex = Array.IndexOf(fileData.Time, pursuitEndTime);
            var saccadesStartIndex = Array.IndexOf(fileData.Time, saccadesStartTime); //1

            var pursuitMovesBlock = InputDataHelper.CutData(fileData, 0, pursuitEndIndex - 1);
            var saccadesMovesBlock = InputDataHelper.CutData(fileData, saccadesStartIndex, (fileData.Spot.Length) - saccadesStartIndex);

            if (pursuitMovesBlock.Spot.Length > 0)
            {
                var pursuitWindowsFilterConfig = GetCurrentPursuitFilterConfig();
                var pursuitCalculations = DataAnalyzer.CountCompoundSignalPursuitParameters(pursuitMovesBlock, pursuitWindowsFilterConfig);
                if (CBPOMShowWindows.IsChecked == true)
                {
                    ApplyPursuitFilteredWindows(pursuitCalculations.FilteredControlWindows);
                }

                var longGain = new double?();
                if (pursuitCalculations.Gains.FirstOrDefault(x => x.Key == "Long").Value.HasValue)
                {
                    longGain = pursuitCalculations.Gains.FirstOrDefault(x => x.Key == "Long").Value;
                }

                var midGain = new double?();
                if (pursuitCalculations.Gains.FirstOrDefault(x => x.Key == "Mid").Value.HasValue)
                {
                    midGain = pursuitCalculations.Gains.FirstOrDefault(x => x.Key == "Mid").Value.GetValueOrDefault();
                }

                var shortGain = new double?();
                if (pursuitCalculations.Gains.FirstOrDefault(x => x.Key == "Short").Value.HasValue)
                {
                    shortGain = pursuitCalculations.Gains.FirstOrDefault(x => x.Key == "Short").Value.GetValueOrDefault();
                }

                var longAccuracy = new double?();
                if (pursuitCalculations.Accuracies.FirstOrDefault(x => x.Key == "Long").Value.HasValue)
                {
                    longAccuracy = pursuitCalculations.Accuracies.FirstOrDefault(x => x.Key == "Long").Value;
                }

                var midAccuracy = new double?();
                if (pursuitCalculations.Accuracies.FirstOrDefault(x => x.Key == "Mid").Value.HasValue)
                {
                    midAccuracy = pursuitCalculations.Accuracies.FirstOrDefault(x => x.Key == "Mid").Value.GetValueOrDefault();
                }

                var shortAccuracy = new double?();
                if (pursuitCalculations.Accuracies.FirstOrDefault(x => x.Key == "Short").Value.HasValue)
                {
                    shortAccuracy = pursuitCalculations.Accuracies.FirstOrDefault(x => x.Key == "Short").Value.GetValueOrDefault();
                }


                this.PursuitMoveCalculations = new EyeMoveCalculation
                {
                    PursuitLongSinGain = Math.Round(longGain.GetValueOrDefault(), 2),
                    PursuitMidSinGain = Math.Round(midGain.GetValueOrDefault(), 2),
                    PursuitShortSinGain = Math.Round(shortGain.GetValueOrDefault(), 2),


                    PursuitLongSinAccuracy = Math.Round(longAccuracy.GetValueOrDefault(), 2),
                    PursuitMidSinAccuracy = Math.Round(midAccuracy.GetValueOrDefault(), 2),
                    PursuitShortSinAccuracy = Math.Round(shortAccuracy.GetValueOrDefault(), 2)
                };
            }

            if (saccadesMovesBlock.Spot.Length > 0)
            {

                SpotEyePoints = DataAnalyzer.FindSpotEyePointsForSaccadeAntiSaccadeSearch(saccadesMovesBlock, calcConfig);


                var saccadeSpotBlock = InputDataHelper.GetSpotMoveDataBlock(SpotEyePoints, EyeMoveTypes.Saccade, fileData.FileType);
                var antiSaccadeSpotBlock = InputDataHelper.GetSpotMoveDataBlock(SpotEyePoints, EyeMoveTypes.AntiSaccade, fileData.FileType);

                this.ApplySpotPointMarkers(this.SpotEyePoints);

                //var fakeSacc = SpotEyePoints.SpotMoves.Select(x => x.SpotStartIndex).ToList();
                FindAndPlotSaccAntiSacc(saccadeSpotBlock, SpotEyePoints, calcConfig, EyeMoveTypes.Saccade);
                FindAndPlotSaccAntiSacc(antiSaccadeSpotBlock, SpotEyePoints, calcConfig, EyeMoveTypes.AntiSaccade);

                if (saccadeSpotBlock.Count > 0 || antiSaccadeSpotBlock.Count > 0)
                {
                    GBEyeMoves.IsEnabled = true;
                }
                else
                {
                    GBEyeMoves.IsEnabled = false;
                }

            }
        }


        private void FindAndPlotSaccAntiSacc(List<int> spotOverMeanPoints, ResultData fullData, CalcConfig calcConfig, EyeMoveTypes moveType)
        {
            IEyeMoveFinder finder = null;
            if (moveType == EyeMoveTypes.Saccade)
            {
                finder = new SaccadeFinder(calcConfig.SaccadeMoveFinderConfig);
            }
            else
            {
                finder = new AntiSaccadeFinder(calcConfig.AntiSaccadeMoveFinderConfig);
            }


            var saccadeParamsCalcuator = new SaccadeParamsCalcuator(fullData.EyeCoords, fullData.SpotCoords, calcConfig.DistanceFromScreen, calcConfig.TrackerFrequency);

            for (int i = 0; i < spotOverMeanPoints.Count; i++)
            {
                var spotOverMeanIndex = spotOverMeanPoints[i];
                var currentSpotShiftIndex = SaccadeDataHelper.CountSpotShiftIndex(spotOverMeanIndex, fullData.ShiftPeriod);
                var lastIndex = Array.IndexOf(fullData.SpotCoords, fullData.SpotCoords.Last());
                if (currentSpotShiftIndex < fullData.SpotCoords.Count())
                {
                    EyeMove eyeMove = null;

                    if (moveType == EyeMoveTypes.Saccade)
                    {
                        eyeMove = finder.TryFindEyeMove(i + 1, spotOverMeanIndex, currentSpotShiftIndex, fullData);
                        if (eyeMove != null)
                        {
                            var saccParams = saccadeParamsCalcuator.CalculateSaccadeParams(eyeMove, eyeMove.EyeMoveType);
                            if (saccParams.Amplitude >= calcConfig.SaccadeMoveFinderConfig.MinAmp &&
                                saccParams.Amplitude <= calcConfig.SaccadeMoveFinderConfig.MaxAmp)
                            {
                                SaccadePositions.Add(eyeMove);
                            }
                        }
                    }
                    else
                    {
                        eyeMove = finder.TryFindEyeMove(i + 1, spotOverMeanIndex, currentSpotShiftIndex, fullData);
                        if (eyeMove != null)
                        {
                            var antiSaccParams = saccadeParamsCalcuator.CalculateSaccadeParams(eyeMove, eyeMove.EyeMoveType);
                            if (antiSaccParams.Amplitude >= calcConfig.AntiSaccadeMoveFinderConfig.MinAmp &&
                                antiSaccParams.Amplitude <= calcConfig.AntiSaccadeMoveFinderConfig.MaxAmp)
                            {
                                AntiSaccadePositions.Add(eyeMove);
                            }
                        }
                    }
                }
                
            }

            ApplySaccadeAndAntiSaccadeElements(this.SaccadePositions, this.AntiSaccadePositions);

        }

        private void ApplySaccadeAndAntiSaccadeElements(List<EyeMove> saccades, List<EyeMove> antiSaccades)
        {
            ApplySaccadeMarkersAndControls(SaccadePositions);
            ApplyAntiSaccadeMarkersAndControls(AntiSaccadePositions);
            ApplySaccadeControls(SaccadePositions, AntiSaccadePositions);
        }

        private void FitGraphView(SpotGazeFileData results)
        {
            //fit to view with margins
            var yMargin = ((results.Eye.Max() - results.Eye.Min()) / 10);
            var xMargin = ((results.TimeDeltas.Max() - results.TimeDeltas.Min()) / 20);

            var xMin = results.TimeDeltas.Min() - xMargin;
            var xMax = results.TimeDeltas.Max() + xMargin;
            var yMin = results.Eye.Min() - yMargin;
            var yMax = results.Eye.Max() + yMargin;

            amplitudePlotter.FitToView();
            amplitudePlotter.Viewport.Visible = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        private void ApplyEyeSpotSinusoids(double[] timeDeltas, double[] eyeCoords, double[] spotCoords, FileType fileType)
        {
            var timeAxisDataSource = new EnumerableDataSource<double>(timeDeltas);
            timeAxisDataSource.SetXMapping(x => x);

            timeAxis.LabelProvider.CustomFormatter = (tickInfo) =>
            {
                double scale;
                if(fileType == FileType.JazzNovo)
                {
                    scale = Consts.TimeScaleFactorJazzNovo;
                }
                else
                {
                    scale = Consts.TimeScaleFactorStandard;
                }
                var output = (tickInfo.Tick / scale).ToString();
                return output;
            };

            var eyeDataSource = new EnumerableDataSource<double>(eyeCoords);
            eyeDataSource.SetYMapping(x => x);

            var spotDataSource = new EnumerableDataSource<double>(spotCoords);
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
        }


        private void ApplyPursuitFilteredWindows(List<Dictionary<double, double>> filteredControlWindows)
        {
            foreach(var window in filteredControlWindows)
            {
                var timeAxisDataSource = new EnumerableDataSource<double>(window.Keys);
                timeAxisDataSource.SetXMapping(x => x);

                var eyeDataSource = new EnumerableDataSource<double>(window.Values);
                eyeDataSource.SetYMapping(x => x);

                var eyeCompositeDataSource = new CompositeDataSource(timeAxisDataSource, eyeDataSource);

                var marker = new MarkerPointsGraph(eyeCompositeDataSource);
                var markPen = new CirclePointMarker();
                markPen.Pen = new Pen(Brushes.Green, 1);
                markPen.Size = 2;
                marker.Name = "FilteredWindowMarker";
                markPen.Fill = Brushes.Green;
                marker.Marker = markPen;

                amplitudePlotter.Children.Add(marker);
            }
        }


        private void ApplyPursutiApproximationSinusoid(double[] timeDeltas, double[] eyeCoords)
        {
            var timeAxisDataSource = new EnumerableDataSource<double>(timeDeltas);
            timeAxisDataSource.SetXMapping(x => x);

            timeAxis.LabelProvider.CustomFormatter = (tickInfo) =>
            {
                var output = (tickInfo.Tick /100).ToString();
                return output;
            };
           
            var eyeDataSource = new EnumerableDataSource<double>(eyeCoords);
            eyeDataSource.SetYMapping(x => x);

            var eyeCompositeDataSource = new CompositeDataSource(timeAxisDataSource, eyeDataSource);
          

            LineGraph line;

            line = new LineGraph(eyeCompositeDataSource);
            line.LinePen = new Pen(Brushes.YellowGreen, 2);
            line.LinePen.DashStyle = DashStyles.Dash;
            Legend.SetDescription(line, "Approx. Eye");
            amplitudePlotter.Children.Add(line);

            
        }
        private void ApplySpotPointMarkers(ResultData spotEyePoints)
        {
            SpotMovePositions = spotEyePoints.SpotMoves;
            LoadComboAddEMSpot();

            var spotStartDataSource = new EnumerableDataSource<SpotMove>(SpotMovePositions);
            spotStartDataSource.SetXMapping(x => x.SpotStartTimeDelta);
            spotStartDataSource.SetYMapping(x => x.SpotStartCoord);

            var spotEndDataSource = new EnumerableDataSource<SpotMove>(SpotMovePositions);
            spotEndDataSource.SetXMapping(x => x.SpotEndTimeDelta);
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
        private void ApplySaccadeMarkersAndControls(List<EyeMove> saccades)
        {
            SaccadeGraphController.RemoveSaccadeMarkers(amplitudePlotter);
            SaccadeGraphController.ApplySaccadePointMarkers(amplitudePlotter, saccades);
            SaccadeGraphController.ApplySaccadeTextMarkers(amplitudePlotter, CBShowLabels.IsChecked.GetValueOrDefault(), 
                saccades.Select(x => x.EyeStartTime).ToList(), saccades.Select(x => x.EyeStartCoord).ToList(),
                saccades.Select(x => x.Id).ToList(), "Start");
            //SaccadeGraphController.ApplySaccadeTextMarkers(amplitudePlotter, CBShowLabels.IsChecked.GetValueOrDefault(), saccades.Select(x => x.EyeEndTime).ToList(), saccades.Select(x => x.EyeEndCoord).ToList(),
            //    saccades.Select(x => x.Id).ToList(), "End");
        }

        private void ApplyAntiSaccadeMarkersAndControls(List<EyeMove> antiSaccades)
        {
            AntiSaccadeGraphController.RemoveSaccadeMarkers(amplitudePlotter);
            AntiSaccadeGraphController.ApplyAntiSaccadePointMarkers(amplitudePlotter, antiSaccades);
            AntiSaccadeGraphController.ApplyAntiSaccadeTextMarkers(amplitudePlotter, CBShowLabels.IsChecked.GetValueOrDefault(),
                antiSaccades.Select(x => x.EyeStartTime).ToList(), antiSaccades.Select(x => x.EyeStartCoord).ToList(),
                antiSaccades.Select(x => x.Id).ToList(), "Start");
            //AntiSaccadeGraphController.ApplyAntiSaccadeTextMarkers(amplitudePlotter, CBShowLabels.IsChecked.GetValueOrDefault(), antiSaccades.Select(x => x.EyeEndTime).ToList(), antiSaccades.Select(x => x.EyeEndCoord).ToList(),
            //    antiSaccades.Select(x => x.Id).ToList(), "End");

        }

        private void ReApplySaccades()
        {
            var calcConfig = GetCurrentCalcConfig();
            this.SaccadePositions = GetEyeMoveParamsFromControls(calcConfig, this.SaccadePositions, EyeMoveTypes.Saccade);
            this.AntiSaccadePositions = GetEyeMoveParamsFromControls(calcConfig, this.AntiSaccadePositions, EyeMoveTypes.AntiSaccade);

            ApplySaccadeAndAntiSaccadeElements(this.SaccadePositions, this.AntiSaccadePositions);
            //Quick fix for first checkobox tick not rendering
            //RefreshGraphLabels(true);
            RefreshGraphLabels(false);
        }

        public void GraphClearElements()
        {
            var lgc = new Collection<IPlotterElement>();
            foreach (var x in amplitudePlotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph || x is MarkerPointsGraph
                 || x is PointMarker || x is CirclePointMarker || x is TrianglePointMarker)
                {
                    lgc.Add(x);
                }
            }

            foreach (var x in lgc)
            {
                amplitudePlotter.Children.Remove(x);

            }
        }

        public void CalculateSaccParameters()
        {
            var calcConfig = GetCurrentCalcConfig();
            var saccCalculator = new SaccadeParamsCalcuator(SpotEyePoints.EyeCoords,
                SpotEyePoints.SpotCoords, calcConfig.DistanceFromScreen, calcConfig.TrackerFrequency);

            this.SaccadeCalculations = saccCalculator.Calculate(this.SaccadePositions, EyeMoveTypes.Saccade);
            this.AntiSaccadeCalculations = saccCalculator.Calculate(this.AntiSaccadePositions, EyeMoveTypes.AntiSaccade);
        }

        #region SaccadeControlPanels
        private void ApplySaccadeControls(List<EyeMove> saccades, List<EyeMove> antiSaccades)
        {
            SaccadeControlsPanel.Children.Clear();

            SaccadeControlsPanel.Children.Add(new Label
            {
                Content = "Saccades:",
                Margin = new Thickness(0,10,0,0),
                //Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            foreach (var saccade in saccades)
            {
                var timePanel = GetTimeLabelPanel(saccade);
                var editPanel = GetSaccadeControl(saccade, EyeMoveTypes.Saccade);
                SaccadeControlsPanel.Children.Add(timePanel);
                SaccadeControlsPanel.Children.Add(editPanel);
            }

            SaccadeControlsPanel.Children.Add(new Label
            {
                Content = "AntiSaccades:",
                Margin = new Thickness(0,10,0,0),
                HorizontalAlignment = HorizontalAlignment.Left
            });
            foreach (var antiSaccade in antiSaccades)
            {
                var timePanel = GetTimeLabelPanel(antiSaccade);
                var editPanel = GetSaccadeControl(antiSaccade, EyeMoveTypes.AntiSaccade);
                SaccadeControlsPanel.Children.Add(timePanel);
                SaccadeControlsPanel.Children.Add(editPanel);
            }
        }


        private StackPanel GetTimeLabelPanel(EyeMove saccade)
        {
            var timeLabelPanel = new StackPanel();
            timeLabelPanel.Orientation = Orientation.Horizontal;
           
            timeLabelPanel.Margin = new Thickness(0);
            timeLabelPanel.Height = 43;
            timeLabelPanel.HorizontalAlignment = HorizontalAlignment.Right;

            var timeLabel = new Label();
            timeLabel.FontSize = 11;
            timeLabel.Padding = new Thickness(0);
            timeLabel.Margin = new Thickness(0);
            timeLabel.Content = $"ID: {saccade.Id}{Environment.NewLine}Time Start: {InputDataHelper.ScaleByTimeFactor(saccade.EyeStartTime, 3, true, FileData.FileType)} {Environment.NewLine}End: {InputDataHelper.ScaleByTimeFactor(saccade.EyeEndTime, 4, true, FileData.FileType)}";
            
            timeLabelPanel.Children.Add(timeLabel);

            return timeLabelPanel;
        }

        private StackPanel GetSaccadeControl(EyeMove saccade, EyeMoveTypes eyeMoveTypeTag)
        {
            var itemPanel = new StackPanel();
            itemPanel.Orientation = Orientation.Horizontal;
            itemPanel.HorizontalAlignment = HorizontalAlignment.Right;
            itemPanel.Margin = new Thickness(0, 0, 0, 0);
            itemPanel.Tag = saccade.Id.ToString();
            itemPanel.Name = eyeMoveTypeTag.ToString();

            var saccLabel = new Label();
            saccLabel.Padding = new Thickness(0, 15, 5, 0);
            saccLabel.Content = $"Frame";
            saccLabel.FontSize = 11;

            var controlPadding = new Thickness(2, 5, 2, 5);

            var saccStartTB = new TextBox();
            saccStartTB.Text = saccade.EyeStartIndex.ToString();
            saccStartTB.Name = "TBEyeMoveStart";
            saccStartTB.Padding = controlPadding;
            saccStartTB.VerticalAlignment = VerticalAlignment.Center;
            saccStartTB.PreviewTextInput += NumberValidationTextBox;

            var saccEndTB = new TextBox();
            saccEndTB.Text = saccade.EyeEndIndex.ToString();
            saccEndTB.Name = "TBEyeMoveEnd";
            saccEndTB.Padding = controlPadding;
            saccEndTB.VerticalAlignment = VerticalAlignment.Center;
            saccEndTB.PreviewTextInput += NumberValidationTextBox;

            var removeButton = new Button();
            removeButton.Content = "X";
            //removeButton.F = FontWeights.Bold;
            removeButton.Tag = saccade.Id;
            removeButton.Name = eyeMoveTypeTag.ToString();
            removeButton.VerticalAlignment = VerticalAlignment.Center;
            removeButton.Padding = controlPadding;
            removeButton.Click += BtnRemoveEyeMove_Click;

            itemPanel.Children.Add(saccLabel);
            itemPanel.Children.Add(saccStartTB);
            itemPanel.Children.Add(saccEndTB);
            itemPanel.Children.Add(removeButton);

            return itemPanel;
        }


        private List<EyeMove> GetEyeMoveParamsFromControls(CalcConfig calcConfig, List<EyeMove> oldSaccadePositions, EyeMoveTypes eyeMoveTypeTag)
        {
            var newSaccadePositions = new List<EyeMove>();

            foreach (var children in SaccadeControlsPanel.Children)
            {
                if (children is StackPanel)
                {
                    var panel = children as StackPanel;
                    if (panel.Name.Equals(eyeMoveTypeTag.ToString()))
                    {
                        var saccId = int.Parse(panel.Tag.ToString());
                        int saccStartIndex = -1;
                        int saccEndIndex = -1;

                        foreach (var panelChildren in panel.Children)
                        {
                            if (panelChildren is TextBox)
                            {
                                var textBox = panelChildren as TextBox;
                                if (textBox.Name == "TBEyeMoveStart")
                                {
                                    saccStartIndex = int.Parse(textBox.Text);
                                }
                                else if (textBox.Name == "TBEyeMoveEnd")
                                {
                                    saccEndIndex = int.Parse(textBox.Text);
                                }
                            }
                        }

                        saccStartIndex = saccStartIndex + calcConfig.EyeStartShiftPeroid;
                        saccEndIndex = saccEndIndex + calcConfig.EyeEndShiftPeroid;

                       var prevoiusSaccadeItem = oldSaccadePositions.FirstOrDefault(x => x.Id == saccId);
                        var saccItem = SaccadeDataHelper.GetSaccadePositionItem(saccId, saccStartIndex, saccEndIndex, prevoiusSaccadeItem, SpotEyePoints);
                        newSaccadePositions.Add(saccItem);
                    }
                }
            }
            return newSaccadePositions;
        }
        #endregion

        #region Configs

        private void SetCalcConfigGUI(CalcConfig calcConfig)
        {
            //TBEyeAmpProp.Text = calcConfig.EyeAmpProp.ToString();
            TBSaccadeEndShiftPeroid.Text = calcConfig.EyeEndShiftPeroid.ToString();
            TBSaccadeStartShiftPeroid.Text = calcConfig.EyeStartShiftPeroid.ToString();
            TBEyeShiftPeroid.Text = calcConfig.EyeShiftPeriod.ToString();
            TBSpotAmpProp.Text = calcConfig.SpotAmpProp.ToString();
            TBSpotShiftPeroid.Text = calcConfig.SpotShiftPeriod.ToString();

            var startTimeDelta = InputDataHelper.GetScaledTimeFromIndex(FileData, calcConfig.RecStart);
            TBStartRec.Text = startTimeDelta.ToString();

            var endTimeDelta = InputDataHelper.GetScaledTimeFromIndex(FileData, calcConfig.RecEnd);
            TBEndRec.Text = endTimeDelta.ToString();
        }

        private void SetFilterConfigGUI(FiltersConfig filtersConfig)
        {
            CBFilterButterworth.IsChecked = filtersConfig.FilterByButterworth;
            CBFilterSavGolay.IsChecked = filtersConfig.FilterBySavitzkyGolay;
            TBButterWorthFrequency.Text = filtersConfig.ButterworthFrequency.ToString();
            TBButterWorthResonance.Text = filtersConfig.ButterworthResonance.ToString();
            TBButterWorthSampleRate.Text = filtersConfig.ButterworthSampleRate.ToString();

            TBSavGoayPointsNum.Text = filtersConfig.SavitzkyGolayNumberOfPoints.ToString();
            TBSavGoayPolyOrder.Text = filtersConfig.SavitzkyGolayPolynominalOrder.ToString();
            TBSavGoayDerivOrder.Text = filtersConfig.SavitzkyGolayDerivativeOrder.ToString();

        }

        private void SetSaccadeFinderConfigGUI(EyeMoveFinderConfig config)
        {
            TBSaccMinDuration.Text = config.MinDuration.ToString();
            TBSaccMinLatency.Text = config.MinLatency.ToString();
            TBSaccMinInhibition.Text = config.MinInhibition.ToString();
            TBSaccMinLength.Text = config.MinLength.ToString();
            TBSaccSearchLength.Text = config.MoveSearchWindowLength.ToString();
            TBSaccControlLength.Text = config.ControlWindowLength.ToString();
            TBSaccControlAmpDivider.Text = config.ControlAmpDivider.ToString();
            TBSaccMinAmp.Text = config.MinAmp.ToString();
            TBSaccMaxAmp.Text = config.MaxAmp.ToString();
        }

        private void SetAntiSaccadeFinderConfigGUI(EyeMoveFinderConfig config)
        {
            TBAntiSaccMinDuration.Text = config.MinDuration.ToString();
            TBAntiSaccMinLatency.Text = config.MinLatency.ToString();
            TBAntiSaccMinInhibition.Text = config.MinInhibition.ToString();
            TBAntiSaccMinLength.Text = config.MinLength.ToString();
            TBAntiSaccSearchLength.Text = config.MoveSearchWindowLength.ToString();
            TBAntiSaccControlLength.Text = config.ControlWindowLength.ToString();
            TBAntiSaccControlAmpDivider.Text = config.ControlAmpDivider.ToString();
            TBAntiSaccMinAmp.Text = config.MinAmp.ToString(); ;
            TBAntiSaccMaxAmp.Text = config.MaxAmp.ToString();
        }


        private void SetPursuitFinderConfigGUI(FiltersConfig config)
        {
            CBPOMFilterButterworth.IsChecked = config.FilterByButterworth;
            TBPOMButterWorthFrequency.Text = config.ButterworthFrequency.ToString();
            TBPOMButterWorthResonance.Text = config.ButterworthResonance.ToString();
            TBPOMButterWorthSampleRate.Text = config.ButterworthSampleRate.ToString();
        }

        private CalcConfig GetCurrentCalcConfig()
        {
            var saccadeConfig = GetCurrentSaccadeFinderConfig();
            var antiSaccadeConfig = GetCurrentAntiSaccadeFinderConfig();
            var pursuitConfig = GetCurrentPursuitFilterConfig();
            var calcConfig = new CalcConfig();
            calcConfig.SaccadeMoveFinderConfig = saccadeConfig;
            calcConfig.AntiSaccadeMoveFinderConfig = antiSaccadeConfig;
            calcConfig.PursuitMoveFinderConfig = pursuitConfig;
            double recStart;
            double recEnd;
            int eyeShiftPeriod;
            int spotShiftPeriod;
            double ampProp;
            double spotProp;
      
            int eyeStartShiftPeroid;
            int eyeEndShiftPeroid;

            int trackerFrequency;
            int distanceFromScreen;


            var isRecStart = double.TryParse(TBStartRec.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, 
                out recStart);
            if (isRecStart)
            {

                int? recStartIndex = null;
                //if (this.FileType == FileType.Maruniec)
                //{
                //    var indexItem = FileData.TimeDeltas.Where(x => x >= recStart).OrderBy(x => x).FirstOrDefault();
                //    recStartIndex = Array.IndexOf(FileData.TimeDeltas, indexItem);
                //}
                //else
                //{

                recStartIndex = InputDataHelper.GetIndexFromScaledTime(FileData, recStart);
                

                if (recStartIndex != null)
                {

                    calcConfig.RecStart= recStartIndex.GetValueOrDefault();
                }
                else
                {
                    MessageBox.Show($"Start Rec: Unable to convert from time to index.");
                    calcConfig.RecStart = 1;
                }
                
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Start Rec'. Should be int.");
            }

          
            var isRecEnd = double.TryParse(TBEndRec.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out recEnd);
            if (isRecEnd)
            {
                int? recEndIndex = null;
                //if (this.FileType == FileType.Maruniec)
                //{
                //    var indexItem = FileData.TimeDeltas.Where(x => x >= recEnd).OrderBy(x => x).FirstOrDefault();
                //    recEndIndex = Array.IndexOf(FileData.TimeDeltas, indexItem);
                //}
               
                recEndIndex = InputDataHelper.GetIndexFromScaledTime(FileData, recEnd);
                

                if (recEndIndex != null)
                {

                    calcConfig.RecEnd = recEndIndex.GetValueOrDefault();
                }
                else
                {
                    MessageBox.Show($"EndRec: Unable to convert from time to index.");
                    calcConfig.RecEnd = FileData.TimeDeltas.Count();
                }
            }
            else
            {
                MessageBox.Show($"Wrong value of 'End Rec'. Should be int.");
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

          

            var isSpotProp = double.TryParse(TBSpotAmpProp.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out spotProp);
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

            var isFrequency = int.TryParse(TBFrequency.Text, out trackerFrequency);
            if (isFrequency)
            {
                calcConfig.TrackerFrequency = trackerFrequency;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Frequency'. Should be int.");
            }

            var isDistanceFromScreen = int.TryParse(TBDistanceFromScreen.Text, out distanceFromScreen);
            if (isDistanceFromScreen)
            {
                calcConfig.DistanceFromScreen = distanceFromScreen;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Distance From Screen'. Should be int.");
            }

            return calcConfig;

        }

        private FiltersConfig GetCurrentFilterConfig()
        {
            var filtersConfig = new FiltersConfig();

            double butterworthFrequency;
            int butterworthSampleRate;
            double butterworthResonance;

            int savitzkyGolayNumberOfPoints;
            int savitzkyGolayDerivativeOrder;
            int savitzkyGolayPolynominalOrder;

          
            filtersConfig.FilterByButterworth = CBFilterButterworth.IsChecked.GetValueOrDefault();
            var isbutterworthFrequency = double.TryParse(TBButterWorthFrequency.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out butterworthFrequency);
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

           
            var isButterworthResonance = double.TryParse(TBButterWorthResonance.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out butterworthResonance);
            if (isButterworthResonance)
            {
                filtersConfig.ButterworthResonance = butterworthResonance;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Butter Pass Type'. Should be {filtersConfig.ButterworthResonance.GetType().Name}.");
            }
           
            filtersConfig.FilterBySavitzkyGolay =  CBFilterSavGolay.IsChecked.GetValueOrDefault();
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


            // The only option allowed
            filtersConfig.ButterworthPassType = PassType.Lowpass;

            return filtersConfig;
        }

        private EyeMoveFinderConfig GetCurrentSaccadeFinderConfig()
        {
            var eyeMoveFinderConfig = new EyeMoveFinderConfig();
            int minLatency;
            int minDuration;
            int controlWindowLength;
            int moveSearchWindowLength;
            double minLength;
            double controlAmpDivider;
            int minInhibition;
            int maxAmp;
            int minAmp;

            var isMinLatency = int.TryParse(TBSaccMinLatency.Text, out minLatency);
            if (isMinLatency)
            {
                eyeMoveFinderConfig.MinLatency = minLatency;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Min Latency'. Should be { eyeMoveFinderConfig.MinLatency.GetType().Name}.");
            }

            var isminDuration = int.TryParse(TBSaccMinDuration.Text, out minDuration);
            if (isminDuration)
            {
                eyeMoveFinderConfig.MinDuration= minDuration;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Min Duration  '. Should be { eyeMoveFinderConfig.MinDuration.GetType().Name}.");
            }

            var iscontrolWindowLength = int.TryParse(TBSaccControlLength.Text, out controlWindowLength);
            if (iscontrolWindowLength)
            {
                eyeMoveFinderConfig.ControlWindowLength = controlWindowLength;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Control Window Length'. Should be { eyeMoveFinderConfig.ControlWindowLength.GetType().Name}.");
            }

            var ismoveSearchWindowLength = int.TryParse(TBSaccSearchLength.Text, out moveSearchWindowLength);
            if (ismoveSearchWindowLength)
            {
                eyeMoveFinderConfig.MoveSearchWindowLength = moveSearchWindowLength;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Move Search Window Length'. Should be { eyeMoveFinderConfig.MoveSearchWindowLength.GetType().Name}.");
            }

            var isminLength = double.TryParse(TBSaccMinLength.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out minLength);
            if (ismoveSearchWindowLength)
            {
                eyeMoveFinderConfig.MinLength = minLength;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Move Min Length'. Should be { eyeMoveFinderConfig.MinLength.GetType().Name}.");
            }

            var iscontrolAmpDivider = double.TryParse(TBSaccControlAmpDivider.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out controlAmpDivider);
            if (iscontrolAmpDivider)
            {
                eyeMoveFinderConfig.ControlAmpDivider = controlAmpDivider;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Control Amplitude Divider (max-min/x)'. Should be { eyeMoveFinderConfig.ControlAmpDivider.GetType().Name}.");
            }

            var ismeanInhibition = int.TryParse(TBSaccMinInhibition.Text, out minInhibition);
            if (ismeanInhibition)
            {
                eyeMoveFinderConfig.MinInhibition = minInhibition;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Min. Inhibition'. Should be { eyeMoveFinderConfig.MinInhibition.GetType().Name}.");
            }


            var isMinAmp = int.TryParse(TBSaccMinAmp.Text, out minAmp);
            if (isMinAmp)
            {
                eyeMoveFinderConfig.MinAmp = minAmp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Min.Amplitude'. Should be { eyeMoveFinderConfig.MinAmp.GetType().Name}.");
            }

            var isMaxAmp = int.TryParse(TBSaccMaxAmp.Text, out maxAmp);
            if (isMaxAmp)
            {
                eyeMoveFinderConfig.MaxAmp = maxAmp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Saccade Max.Amplitude'. Should be { eyeMoveFinderConfig.MaxAmp.GetType().Name}.");
            }

            return eyeMoveFinderConfig;
        }

        private EyeMoveFinderConfig GetCurrentAntiSaccadeFinderConfig()
        {
            var eyeMoveFinderConfig = new EyeMoveFinderConfig();
            int minLatency;
            int minDuration;
            int controlWindowLength;
            int moveSearchWindowLength;
            double minLength;
            double controlAmpDivider;
            int minInhibition;
            int maxAmp;
            int minAmp;

            var isMinLatency = int.TryParse(TBAntiSaccMinLatency.Text, out minLatency);
            if (isMinLatency)
            {
                eyeMoveFinderConfig.MinLatency = minLatency;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Min Latency'. Should be { eyeMoveFinderConfig.MinLatency.GetType().Name}.");
            }

            var isminDuration = int.TryParse(TBAntiSaccMinDuration.Text, out minDuration);
            if (isminDuration)
            {
                eyeMoveFinderConfig.MinDuration = minDuration;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Min Duration  '. Should be { eyeMoveFinderConfig.MinDuration.GetType().Name}.");
            }

            var iscontrolWindowLength = int.TryParse(TBAntiSaccControlLength.Text, out controlWindowLength);
            if (iscontrolWindowLength)
            {
                eyeMoveFinderConfig.ControlWindowLength = controlWindowLength;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Control Window Length'. Should be { eyeMoveFinderConfig.ControlWindowLength.GetType().Name}.");
            }

            var ismoveSearchWindowLength = int.TryParse(TBAntiSaccSearchLength.Text, out moveSearchWindowLength);
            if (ismoveSearchWindowLength)
            {
                eyeMoveFinderConfig.MoveSearchWindowLength = moveSearchWindowLength;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Move Search Window Length'. Should be { eyeMoveFinderConfig.MoveSearchWindowLength.GetType().Name}.");
            }

            var isminLength = double.TryParse(TBAntiSaccMinLength.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out minLength);
            if (ismoveSearchWindowLength)
            {
                eyeMoveFinderConfig.MinLength = minLength;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Move Min Length'. Should be { eyeMoveFinderConfig.MinLength.GetType().Name}.");
            }

            var iscontrolAmpDivider = double.TryParse(TBAntiSaccControlAmpDivider.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out controlAmpDivider);
            if (iscontrolAmpDivider)
            {
                eyeMoveFinderConfig.ControlAmpDivider = controlAmpDivider;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Control Amplitude Divider (max-min/x)'. Should be { eyeMoveFinderConfig.ControlAmpDivider.GetType().Name}.");
            }

            var ismeanInhibition  = int.TryParse(TBAntiSaccMinInhibition.Text, out minInhibition);
            if (ismeanInhibition)
            {
                eyeMoveFinderConfig.MinInhibition = minInhibition;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Min. Inhibition'. Should be { eyeMoveFinderConfig.MinInhibition.GetType().Name}.");
            }

            var isMinAmp = int.TryParse(TBAntiSaccMinAmp.Text, out minAmp);
            if (isMinAmp)
            {
                eyeMoveFinderConfig.MinAmp = minAmp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Min.Amplitude'. Should be { eyeMoveFinderConfig.MinAmp.GetType().Name}.");
            }


            var isMaxAmp = int.TryParse(TBAntiSaccMaxAmp.Text, out maxAmp);
            if (isMaxAmp)
            {
                eyeMoveFinderConfig.MaxAmp = maxAmp;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'AntiSaccade Max.Amplitude'. Should be { eyeMoveFinderConfig.MaxAmp.GetType().Name}.");
            }

            return eyeMoveFinderConfig;
        }

        private FiltersConfig GetCurrentPursuitFilterConfig()
        {
            var pursuitWindowsFilter = new FiltersConfig();
            double pomButterWorthFrequency;
            double pomButterWorthResonance;
            int pomButterWorthSampleRate;

            pursuitWindowsFilter.FilterByButterworth = CBPOMFilterButterworth.IsChecked.GetValueOrDefault();

            var isPomButterWorthFrequency = double.TryParse(TBPOMButterWorthFrequency.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out pomButterWorthFrequency);
            if (isPomButterWorthFrequency)
            {
                pursuitWindowsFilter.ButterworthFrequency = pomButterWorthFrequency;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Pursuit Move Filter Frequency'. Should be { pursuitWindowsFilter.ButterworthFrequency.GetType().Name}.");
            }

            var isPomButterWorthResonance = double.TryParse(TBPOMButterWorthResonance.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out pomButterWorthResonance);
            if (isPomButterWorthResonance)
            {
                pursuitWindowsFilter.ButterworthResonance = pomButterWorthResonance;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Pursuit Move Filter Resonance'. Should be { pursuitWindowsFilter.ButterworthResonance.GetType().Name}.");
            }

            var isPomButterWorthSampleRate = int.TryParse(TBPOMButterWorthSampleRate.Text, out pomButterWorthSampleRate);
            if (isPomButterWorthSampleRate)
            {
                pursuitWindowsFilter.ButterworthSampleRate = pomButterWorthSampleRate;
            }
            else
            {
                MessageBox.Show($"Wrong value of 'Pursuit Move Filter Sample Rate'. Should be { pursuitWindowsFilter.ButterworthSampleRate.GetType().Name}.");
            }


            return pursuitWindowsFilter;
        }


        //private bool IsFileTypeMaruniec()
        //{
        //    return InputDataHelper.IsMaruniecExtention(LoadDataPathTextBox.Text);
        //}
        #endregion

            #region UI Events

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
            var dataClone = InputDataHelper.CloneFileData(this.FileData);
            Analyze(dataClone, calcConfig);
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
                        if (marker.Name.Equals("SaccStartLabel") || marker.Name.Equals("SaccEndLabel") 
                            || marker.Name.Equals("AntiSaccStartLabel") || marker.Name.Equals("AntiSaccEndLabel"))
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
                        if (marker.Name.Equals("SaccStartLabel") || marker.Name.Equals("SaccEndLabel")
                            || marker.Name.Equals("AntiSaccStartLabel") || marker.Name.Equals("AntiSaccEndLabel"))
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
            if (this.SpotEyePoints != null)
            {
                CalculateSaccParameters();
            }

            var pursuitTextResults = OutputHelper.GetPursuitTextLog(this.PursuitMoveCalculations);
            var saccadeTextResults = OutputHelper.GetSaccAntiSaccTextLog(this.SaccadeCalculations, this.SpotEyePoints, EyeMoveTypes.Saccade);
            var antiSaccadeTextResults = OutputHelper.GetSaccAntiSaccTextLog(this.AntiSaccadeCalculations, this.SpotEyePoints, EyeMoveTypes.AntiSaccade);

            var currentText = pursuitTextResults + saccadeTextResults + antiSaccadeTextResults + LogTextBox.Text.ToString();
            LogTextBox.Text = currentText;
        }

        private void BtnAddEyeMove_Click(object sender, RoutedEventArgs e)
        {
            var eyeMoveType = Enum.Parse(typeof(EyeMoveTypes), ComboAddEMType.SelectedValue.ToString());
            var spotStartTimeStamp = (double)ComboAddEMSpot.SelectedValue;
            var spot = SpotMovePositions.FirstOrDefault(x => x.SpotStartTimeStamp == spotStartTimeStamp);
            var spotIndex = SpotMovePositions.IndexOf(spot);
            var spotStartIndex = spot.SpotStartIndex;
            var eyeStartIndex = spotStartIndex;
            var eyeShift = int.Parse(TBEyeShiftPeroid.Text);
            var saccadEndShiftPeroid = int.Parse(TBSaccadeEndShiftPeroid.Text);
            var spotShiftIndex = SaccadeDataHelper.CountSpotShiftIndex(spotStartIndex, eyeShift);
            var eyeMoveEndIndex = SaccadeDataHelper.CountEyeShiftIndex(eyeStartIndex, eyeShift);

            int newId;

            if (this?.SaccadePositions.Count == 0 && this?.AntiSaccadePositions.Count == 0)
            {
                newId = 0;
            }

            else if(this.SaccadePositions == null || this.SaccadePositions.Count == 0)
            {
                newId = this.AntiSaccadePositions.Last().Id + 1;
            }
            else if(this.AntiSaccadePositions == null || this.AntiSaccadePositions.Count == 0)
            {
                newId = this.SaccadePositions.Last().Id + 1;
            }
            else if (this.SaccadePositions.Last().Id > this.AntiSaccadePositions.Last().Id)
            {
                newId = this.SaccadePositions.Last().Id + 1;
            }
            else
            {
                newId = this.AntiSaccadePositions.Last().Id + 1;
            }

            var newEyeMove = new EyeMove
            {
                Id = newId,
                IsFirstMove = DataAnalyzer.IsEven(spotIndex),
                EyeMoveType = (EyeMoveTypes)eyeMoveType,
                IsStartFound = true,
                EyeStartIndex = eyeStartIndex,
                EyeStartCoord = SpotEyePoints.EyeCoords[eyeStartIndex],
                EyeStartTime = SpotEyePoints.TimeDeltas[eyeStartIndex],

                IsEndFound = true,
                EyeEndIndex = eyeMoveEndIndex,
                EyeEndCoord = SpotEyePoints.EyeCoords[eyeMoveEndIndex],
                EyeEndTime = SpotEyePoints.TimeDeltas[eyeMoveEndIndex],

                SpotMove = new SpotMove
                {
                    SpotStartIndex = spotStartIndex,
                    SpotStartCoord = SpotEyePoints.SpotCoords[spotStartIndex],
                    SpotStartTimeDelta = SpotEyePoints.TimeDeltas[spotStartIndex],

                    SpotEndIndex = spotShiftIndex,
                    SpotEndCoord = SpotEyePoints.SpotCoords[spotShiftIndex],
                    SpotEndTimeDelta = SpotEyePoints.TimeDeltas[spotShiftIndex]
                }
            };

            newEyeMove = EyeMoveSearchToolBox.CountTestValuesForEyeMove(newEyeMove, this.SpotEyePoints);

           

            if (eyeMoveType.Equals(EyeMoveTypes.Saccade))
            {
                this.SaccadePositions.Add(newEyeMove);
            }
            else if (eyeMoveType.Equals(EyeMoveTypes.AntiSaccade))
            {
                this.AntiSaccadePositions.Add(newEyeMove);
            }

            ApplySaccadeAndAntiSaccadeElements(this.SaccadePositions, this.AntiSaccadePositions);

            MessageBox.Show($"Added to {eyeMoveType.ToString()} with ID: {newEyeMove.Id}");
        }

        private void BtnRemoveEyeMove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button)
            {
                var button = sender as Button;
                var eyeMoveId = Convert.ToInt32(button.Tag);
                var eyeMoveType = (EyeMoveTypes)Enum.Parse(typeof(EyeMoveTypes),  button.Name);

                if (eyeMoveType == EyeMoveTypes.Saccade)
                {
                    var saccadeForRemove = this.SaccadePositions.First(x => x.Id == eyeMoveId);
                    this.SaccadePositions.Remove(saccadeForRemove);
                }
                else
                {
                    var antiSaccadeForRemove = this.AntiSaccadePositions.First(x => x.Id == eyeMoveId);
                    this.AntiSaccadePositions.Remove(antiSaccadeForRemove);
                }
                
                ApplySaccadeAndAntiSaccadeElements(this.SaccadePositions, this.AntiSaccadePositions);
            }
        }

        private void ComboBoxFilterType_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();

            foreach(var passType in Enum.GetValues(typeof(PassType)))
            {
                data.Add(passType.ToString());
            }

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
            comboBox.SelectedIndex = 1;
        }

        private void ComboBoxAddMoveType_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add(EyeMoveTypes.Saccade.ToString());
            data.Add(EyeMoveTypes.AntiSaccade.ToString());
            //foreach (var passType in Enum.GetValues(typeof(EyeMoveTypes)))
            //{
            //    data.Add(passType.ToString());
            //}

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
            comboBox.SelectedIndex = 0;
        }

       
        private void LoadComboAddEMSpot()
        {
            var data = new List<KeyValuePair<double, double>>();

            foreach (var spotMove in this.SpotMovePositions)
            {
                data.Add( new KeyValuePair<double, double>(spotMove.SpotStartTimeStamp, InputDataHelper.ScaleByTimeFactor(spotMove.SpotStartTimeDelta, 2, true, FileData.FileType)));
            }

            ComboAddEMSpot.SelectedValuePath = "Key";
            ComboAddEMSpot.DisplayMemberPath = "Value";
            ComboAddEMSpot.ItemsSource = data;
            ComboAddEMSpot.SelectedIndex = 0;
        }

        private void BtnClearResults_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Text = string.Empty;
        }

        private void BtnSaveResults_Click(object sender, RoutedEventArgs e)
        {
            if(this.SaccadeCalculations.Count == 0 && this.AntiSaccadeCalculations.Count == 0)
            {
                CalculateSaccParameters();
            }
            var defaultFileName = LoadDataPathTextBox.Text.Replace("\result_out.txt", string.Empty);
            var dlg = new SaveFileDialog();
            //var fileName = Path.GetFileNameWithoutExtension(LoadDataPathTextBox.Text);
            dlg.FileName = Path.GetFileNameWithoutExtension(defaultFileName);
            dlg.DefaultExt = ".csv"; 
            dlg.Filter = "Text documents (.csv)|*.csv"; 

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filePath = dlg.FileName;
                
                    var calcConfig = GetCurrentCalcConfig();
                    var filtersConfig = GetCurrentFilterConfig();
                    var csvOutput = OutputHelper.GetCsvOutput(true, this.SaccadeCalculations, this.AntiSaccadeCalculations, this.PursuitMoveCalculations,
                        calcConfig, filtersConfig);
                    OutputHelper.SaveText(filePath, csvOutput);
                
            }
        }

        private void BtnSaveScreenShot_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            var defaultFileName = LoadDataPathTextBox.Text.Replace("\result_out.txt", string.Empty);
            dlg.FileName = Path.GetFileNameWithoutExtension(defaultFileName);
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "jpg files (.jpg)|*.jpg";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filePath = dlg.FileName;

                amplitudePlotter.SaveScreenshot(filePath);
                MessageBox.Show($"File saved at {filePath}");
                //}
            }

                    
        }

        private void LoadStateButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.ShowDialog();
            dialog.DefaultExt = ".xml";
            dialog.Filter = "xml files (.xml)|*.xml";

            if (!string.IsNullOrEmpty(dialog.FileName) && InputDataHelper.IsStateDataExtention(dialog.FileName))
            {
                LoadDataPathTextBox.Text = dialog.FileName;

                var spotGazeTrackState = SerializationHelper.DeserializeFromFile(dialog.FileName);
                if(spotGazeTrackState != null)
                {
                    this.GraphClearElements();
                    this.FileData = spotGazeTrackState.FileData;
                    this.SetCalcConfigGUI(spotGazeTrackState.CalcConfig);
                    this.SetFilterConfigGUI(spotGazeTrackState.FiltersConfig);
                    this.SetSaccadeFinderConfigGUI(spotGazeTrackState.CalcConfig.SaccadeMoveFinderConfig);
                    this.SetAntiSaccadeFinderConfigGUI(spotGazeTrackState.CalcConfig.AntiSaccadeMoveFinderConfig);
                    this.SetPursuitFinderConfigGUI(spotGazeTrackState.CalcConfig.PursuitMoveFinderConfig);
                    this.SaccadePositions = spotGazeTrackState.SaccadePositions;
                    this.AntiSaccadePositions = spotGazeTrackState.AntiSaccadePositions;
                    this.SpotEyePoints = spotGazeTrackState.CurrentResults;
                    this.SpotMovePositions = spotGazeTrackState.SpotPositions;
                    this.SaccadeCalculations = spotGazeTrackState.SaccadeCalculations;
                    this.AntiSaccadeCalculations = spotGazeTrackState.AntiSaccadeCalculations;

                    CalculateAndSetParametersFromFileData(this.FileData);
                    var fileItemsCount = spotGazeTrackState.CalcConfig.RecEnd - spotGazeTrackState.CalcConfig.RecStart;
                    var cutTimeDeltas = this.FileData.TimeDeltas.Skip(spotGazeTrackState.CalcConfig.RecStart).Take(fileItemsCount).ToArray();
                    var cutEyeCoords = this.FileData.Eye.Skip(spotGazeTrackState.CalcConfig.RecStart).Take(fileItemsCount).ToArray();
                    var cutSpotCoords = this.FileData.Spot.Skip(spotGazeTrackState.CalcConfig.RecStart).Take(fileItemsCount).ToArray();


                    this.ApplyEyeSpotSinusoids(cutTimeDeltas, cutEyeCoords, cutSpotCoords, this.FileData.FileType);
                    this.ApplySpotPointMarkers(this.SpotEyePoints);
                    this.ApplySaccadeControls(this.SaccadePositions, this.AntiSaccadePositions);
                    this.ReApplySaccades();
                    this.UnlockControll(true);
                    if (CBFixView.IsChecked == false)
                    {
                        FitGraphView(this.FileData);
                    }


                }
                else
                {
                    MessageBox.Show($"Unable to deserialize state from {dialog.FileName}");
                }
            }
            else
            {
                MessageBox.Show("File name empty or extention not allowed (only xml).");
            }

        }

        private void BtnSaveState_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = $"SpotEyeTrackState{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "xml files (.xml)|*.xml";

            var result = dlg.ShowDialog();
            if (result == true)
            {
                string filePath = dlg.FileName;
                //if (File.Exists(filePath))
                //{
                    var spotEyeTrackState = new SpotEyeTrackState
                    {
                        CalcConfig = GetCurrentCalcConfig(),
                        FileData = this.FileData,
                        SaccadePositions = this.SaccadePositions,
                        AntiSaccadePositions = this.AntiSaccadePositions,
                        FiltersConfig = GetCurrentFilterConfig(),
                        CurrentResults = this.SpotEyePoints,
                        SpotPositions = this.SpotMovePositions,
                        SaccadeCalculations = this.SaccadeCalculations,
                        AntiSaccadeCalculations = this.AntiSaccadeCalculations
                    };

                    SerializationHelper.SerializeToFile(filePath, spotEyeTrackState);
                }
            //}
        }

        //private void BulkExportButton_Click(object sender, RoutedEventArgs e)
        //{
        //    BulkProcessFiles();
        //}

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var expander = sender as Expander;
            if(expander != null)
            {
                switch (expander.Name)
                {
                    case "ExpanderFilters":
                        GBSaccades.Visibility = Visibility.Collapsed;
                        GBAntiSaccades.Visibility = Visibility.Collapsed;
                        GBPursuit.Visibility = Visibility.Collapsed;
                        break;
                    case "ExpanderSaccades":
                        GBAntiSaccades.Visibility = Visibility.Collapsed;
                        GBFilters.Visibility = Visibility.Collapsed;
                        GBPursuit.Visibility = Visibility.Collapsed;
                        break;
                    case "ExpanderAntiSaccades":
                        GBSaccades.Visibility = Visibility.Collapsed;
                        GBFilters.Visibility = Visibility.Collapsed;
                        GBPursuit.Visibility = Visibility.Collapsed;
                        break;
                    case "ExpanderPursuit":
                        GBAntiSaccades.Visibility = Visibility.Collapsed;
                        GBSaccades.Visibility = Visibility.Collapsed;
                        GBFilters.Visibility = Visibility.Collapsed;
                        break;

                }
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            GBFilters.Visibility = Visibility.Visible;
            GBSaccades.Visibility = Visibility.Visible;
            GBAntiSaccades.Visibility = Visibility.Visible;
            GBPursuit.Visibility = Visibility.Visible;

        }


        #endregion UI Events

       
    }
}

    
