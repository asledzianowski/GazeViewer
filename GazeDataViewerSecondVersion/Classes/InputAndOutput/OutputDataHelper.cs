using GazeDataViewer.Classes.Denoise;
using GazeDataViewer.Classes.Enums;
using GazeDataViewer.Classes.EyeMoveSearch;
using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data;

namespace GazeDataViewer.Classes.DataAndLog
{
    public class OutputHelper
    {

        public static string GetPursuitTextLog(EyeMoveCalculation calc)
        {
            var sb = new StringBuilder();
            var sep = "  ";
            var rowSep = "=========================================";

            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append($"Date/Time: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            sb.Append(Environment.NewLine);
            sb.Append($"Results for: {EyeMoveTypes.Pursuit} Move");
            sb.Append(Environment.NewLine);
            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append($"Eye/Spot gain: {Math.Round(calc.Gain, 2)} ");
            sb.Append(Environment.NewLine);
            sb.Append($"Eye/Approx.Eye gain: {Math.Round(calc.ApproxGain,2)} ");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        public static string GetSaccAntiSaccTextLog(List<EyeMoveCalculation> results, ResultData currentSpotEyePointsForSaccades,
            EyeMoveTypes eyeMoveType)
        {
            var sb = new StringBuilder();
            var sep = "  ";
            var rowSep = "=========================================";

            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append($"Date/Time: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            sb.Append(Environment.NewLine);
            sb.Append($"Results for: {eyeMoveType}s. Item Count: {results.Count}");
            sb.Append(Environment.NewLine);
            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            foreach (var result in results)
            {
                sb.Append($"{result.EyeMove.EyeMoveType} Id: {result.EyeMove.Id}");
                sb.Append(Environment.NewLine);
                sb.Append($"First in Spot Sequence: {result.EyeMove.IsFirstMove}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start Index: {result.EyeMove.SpotMove.SpotStartIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start X: {currentSpotEyePointsForSaccades.SpotCoords[result.EyeMove.SpotMove.SpotStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start Time: {currentSpotEyePointsForSaccades.TimeStamps[result.EyeMove.SpotMove.SpotStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End Index: {result.EyeMove.SpotMove.SpotEndIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End X: {currentSpotEyePointsForSaccades.SpotCoords[result.EyeMove.SpotMove.SpotEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End Time: {currentSpotEyePointsForSaccades.TimeStamps[result.EyeMove.SpotMove.SpotEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start Index: {result.EyeMove.EyeStartIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start X: {currentSpotEyePointsForSaccades.EyeCoords[result.EyeMove.EyeStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start Time: {currentSpotEyePointsForSaccades.TimeStamps[result.EyeMove.EyeStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End Index: {result.EyeMove.EyeEndIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End X: {currentSpotEyePointsForSaccades.EyeCoords[result.EyeMove.EyeEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End Time: {currentSpotEyePointsForSaccades.TimeStamps[result.EyeMove.EyeEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append($"Control Amp/ Test Value: {result.EyeMove.ControlAmpTestValue}");
                sb.Append(Environment.NewLine);
                sb.Append($"Min.Length Test Value: {result.EyeMove.MinLengthTestValue}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append($"Latency Frame Count: {result.LatencyFrameCount}");
                sb.Append(Environment.NewLine);
                sb.Append($"Duration Frame Count: {result.DurationFrameCount}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);

                sb.Append($"Eye/Spot Gain: {result.Gain} ");
                sb.Append(Environment.NewLine);
                sb.Append($"Distance: {result.Distance}");
                sb.Append(Environment.NewLine);
                sb.Append($"Latency: {result.Latency} sec");
                sb.Append(Environment.NewLine);
                sb.Append($"Duration: {result.Duration} sec");
                sb.Append(Environment.NewLine);
                sb.Append($"Visual Angle: {result.Amplitude} deg");
                sb.Append(Environment.NewLine);
                sb.Append($"Average Velocity: {result.AvgVelocity} deg/sec"); ;
                sb.Append(Environment.NewLine);
                sb.Append($"Max Velocity: {result.MaxVelocity} deg/sec"); ;
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);

                sb.Append("Y Values:");
                sb.Append(Environment.NewLine);
                for (int i = result.EyeMove.EyeStartIndex; i < result.EyeMove.EyeStartIndex + result.DurationFrameCount; i++)
                {
                    sb.Append($"Index:{i} - Value: {currentSpotEyePointsForSaccades.EyeCoords[i]}");
                    sb.Append(Environment.NewLine);
                }

                sb.Append(Environment.NewLine);
                sb.Append("X Values:");
                sb.Append(Environment.NewLine);
                for (int i = result.EyeMove.EyeStartIndex; i < result.EyeMove.EyeStartIndex + result.DurationFrameCount; i++)
                {
                    sb.Append($"Index:{i} - Time: {currentSpotEyePointsForSaccades.TimeDeltas[i]}");
                    sb.Append(Environment.NewLine);
                }


                sb.Append(Environment.NewLine);
            }


            return sb.ToString();
        }


        public static string GetCsvOutput(bool addHeader, List<EyeMoveCalculation> saccadeCalculations, 
            List<EyeMoveCalculation> antiSaccadeCalculations, CalcConfig config, FiltersConfig filtersConfig)
        {
            string csvDelimiter = " "; //"\t";
            var sb = new StringBuilder();

            if (addHeader)
            {
                sb.Append("ID" + csvDelimiter);
                sb.Append("FromFixationPoint" + csvDelimiter);
                sb.Append("EyeMoveType" + csvDelimiter);
                sb.Append("Latency" + csvDelimiter);
                sb.Append("Duration" + csvDelimiter);
                sb.Append("Distance" + csvDelimiter);
                sb.Append("Amplitude" + csvDelimiter);
                sb.Append("AvgVelocity" + csvDelimiter);
                sb.Append("MaxVelocity" + csvDelimiter);
                sb.Append("Gain" + Environment.NewLine);
            }

            var allOutputItems = new List<EyeMoveCalculation>();
            allOutputItems.AddRange(saccadeCalculations);
            allOutputItems.AddRange(antiSaccadeCalculations);

            foreach (var outputItem in allOutputItems)
            {
                sb.Append(outputItem.EyeMove.Id + csvDelimiter);
                sb.Append(outputItem.EyeMove.IsFirstMove + csvDelimiter);
                sb.Append(outputItem.EyeMove.EyeMoveType + csvDelimiter);
                sb.Append(outputItem.Latency + csvDelimiter);
                sb.Append(outputItem.Duration + csvDelimiter);
                sb.Append(outputItem.Distance + csvDelimiter);
                sb.Append(outputItem.Amplitude + csvDelimiter);
                sb.Append(outputItem.AvgVelocity + csvDelimiter);
                sb.Append(outputItem.MaxVelocity + csvDelimiter);
                sb.Append(outputItem.Gain + csvDelimiter);
                sb.Append(Environment.NewLine);
            }


            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Found");
            sb.Append(Environment.NewLine);
            sb.Append($"Saccades: {saccadeCalculations.Count}");
            sb.Append(Environment.NewLine);
            sb.Append($"AntiSaccades: {antiSaccadeCalculations.Count}");

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Settings");
            sb.Append(Environment.NewLine);
            sb.Append($"Distance From Screen: {csvDelimiter} {config.DistanceFromScreen}");
            sb.Append(Environment.NewLine);
            sb.Append($"Tracker Frequency: {csvDelimiter} {config.TrackerFrequency}");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("Sacade Search Configuration");
            sb.Append(Environment.NewLine);
            sb.Append($"Min.Latency: {csvDelimiter} {config.SaccadeMoveFinderConfig.MinLatency}");
            sb.Append(Environment.NewLine);
            sb.Append($"Min.Duration: {csvDelimiter}  {config.SaccadeMoveFinderConfig.MinDuration}");
            sb.Append(Environment.NewLine);
            sb.Append($"Control Window Length: {csvDelimiter} {config.SaccadeMoveFinderConfig.ControlWindowLength}");
            sb.Append(Environment.NewLine);
            sb.Append($"Control Amplitude Divider: {csvDelimiter} {config.SaccadeMoveFinderConfig.ControlAmpDivider}");
            sb.Append(Environment.NewLine);
            sb.Append($"Move Search Window Length: {csvDelimiter} {config.SaccadeMoveFinderConfig.MoveSearchWindowLength}");
            sb.Append(Environment.NewLine);
            sb.Append($"Move Min.Length: {csvDelimiter} {config.SaccadeMoveFinderConfig.MinLength}");
            sb.Append(Environment.NewLine);
            sb.Append($"Min.Inhibition: {csvDelimiter} {config.SaccadeMoveFinderConfig.MinInhibition}");
            sb.Append(Environment.NewLine);
            sb.Append($"Max.Amplitude: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.MaxAmp}");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("AntiSacade Search Configuration");
            sb.Append(Environment.NewLine);
            sb.Append($"Min.Latency: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.MinLatency}");
            sb.Append(Environment.NewLine);
            sb.Append($"Min.Duration: {csvDelimiter}  {config.AntiSaccadeMoveFinderConfig.MinDuration}");
            sb.Append(Environment.NewLine);
            sb.Append($"Control Window Length: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.ControlWindowLength}");
            sb.Append(Environment.NewLine);
            sb.Append($"Control Amplitude Divider: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.ControlAmpDivider}");
            sb.Append(Environment.NewLine);
            sb.Append($"Move Search Window Length: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.MoveSearchWindowLength}");
            sb.Append(Environment.NewLine);
            sb.Append($"Move Min.Length: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.MinLength}");
            sb.Append(Environment.NewLine);
            sb.Append($"Min.Inhibition: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.MinInhibition}");
            sb.Append(Environment.NewLine);
            sb.Append($"Max.Amplitude: {csvDelimiter} {config.AntiSaccadeMoveFinderConfig.MaxAmp}");
            sb.Append(Environment.NewLine);

            if (filtersConfig.FilterByButterworth)
            {
                sb.Append(Environment.NewLine);
                sb.Append($"Filter Butterworth Settings:");
                sb.Append(Environment.NewLine);
                sb.Append($"Pass Type: {csvDelimiter} {filtersConfig.ButterworthPassType}");
                sb.Append(Environment.NewLine);
                sb.Append($"Frequency: {csvDelimiter} {filtersConfig.ButterworthFrequency}");
                sb.Append(Environment.NewLine);
                sb.Append($"Resonance: {csvDelimiter} {filtersConfig.ButterworthResonance}");
                sb.Append(Environment.NewLine);
                sb.Append($"SampleRate: {csvDelimiter} {filtersConfig.ButterworthSampleRate}");
                sb.Append(Environment.NewLine);
            }

            if (filtersConfig.FilterBySavitzkyGolay)
            {
                sb.Append(Environment.NewLine);
                sb.Append($"Filter Savitzky-Golay Settings:");
                sb.Append(Environment.NewLine);
                sb.Append($"Number Of Points: {csvDelimiter} {filtersConfig.SavitzkyGolayNumberOfPoints}");
                sb.Append(Environment.NewLine);
                sb.Append($"Derivative Order: {csvDelimiter} {filtersConfig.SavitzkyGolayDerivativeOrder}");
                sb.Append(Environment.NewLine);
                sb.Append($"Polynominal Order: {csvDelimiter} {filtersConfig.SavitzkyGolayPolynominalOrder}");
                sb.Append(Environment.NewLine);
            }

            if (saccadeCalculations?.Count > 0)
            {
                var saccStats = GetStatsForCollection(saccadeCalculations);
                sb.Append(Environment.NewLine);
                sb.Append("Saccade Statistics");
                sb.Append(Environment.NewLine);
                sb.Append(saccStats);
            }

            if (antiSaccadeCalculations?.Count > 0)
            {
                var antiSaccStats = GetStatsForCollection(antiSaccadeCalculations);
                sb.Append(Environment.NewLine);
                sb.Append("AntiSaccade Statistics");
                sb.Append(Environment.NewLine);
                sb.Append(antiSaccStats);
            }
            return sb.ToString();
        }


        public static string GetStatsForCollection(List<EyeMoveCalculation> results)
        {
            var sb = new StringBuilder();
            sb.Append("Latency" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.Latency).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.Latency).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.Latency).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.Latency)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Duration" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.Duration).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.Duration).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.Duration).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.Duration)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Distance" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.Distance).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.Distance).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.Distance).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.Distance)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Amplitude" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.Amplitude).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.Amplitude).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.Amplitude).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.Amplitude)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Averge Velocity" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.AvgVelocity).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.AvgVelocity).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.AvgVelocity).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.AvgVelocity)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Maximum Velocity" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.MaxVelocity).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.MaxVelocity).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.MaxVelocity).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.MaxVelocity)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Gain" + Environment.NewLine);
            sb.Append("Minimum: " + results.Select(x => x.Gain).Min() + Environment.NewLine);
            sb.Append("Maximum: " + results.Select(x => x.Gain).Max() + Environment.NewLine);
            sb.Append("Mean: " + Math.Round(results.Select(x => x.Gain).Average(), 3) + Environment.NewLine);
            sb.Append("StdDev: " + Math.Round(StandardDeviation(results.Select(x => x.Gain)), 3) + Environment.NewLine);
            sb.Append(Environment.NewLine);


            return sb.ToString();
        }

        public static void SaveText(string filePath, string csvContent)
        {
            File.WriteAllText(filePath, csvContent);
            MessageBox.Show($"File Saved at {filePath}");
        }

        public static void GetSummaryForDirectory(string foldersPath, int timeColumnIndex, int eyeColumnIndex, int spotColumnIndex,
            CalcConfig calcConfig, FiltersConfig filtersConfig)
        {
            var fileDataProcessor = new FileDataProcessor();
            string[] directories = Directory.GetDirectories(foldersPath);
            var sb = new StringBuilder();
            foreach (var directoryPath in directories)
            {
                foreach (var filePath in Directory.GetFiles(directoryPath))
                {
                    if(filePath.EndsWith("result_out.txt"))
                    {
                       var csvData = fileDataProcessor.CalculateFileData(filePath, timeColumnIndex, eyeColumnIndex, spotColumnIndex, calcConfig, filtersConfig);
                       sb.Append(directoryPath + Environment.NewLine);
                       sb.Append(csvData);
                    }
                }
            }

            SaveText(@"D:\BulkResult.txt", sb.ToString());


        }


        public static double StandardDeviation(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }

    }
    
}
