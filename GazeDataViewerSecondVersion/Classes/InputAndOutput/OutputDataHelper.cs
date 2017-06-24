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

namespace GazeDataViewer.Classes.DataAndLog
{
    public class OutputHelper
    {
        public static string GetTextLog(List<SaccadeCalculation> results, ResultData currentSpotEyePointsForSaccades,
            EyeMoveTypes eyeMoveType)
        {
            var sb = new StringBuilder();
            var sep = "  ";
            var rowSep = "=========================================";

            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append($"Date/Time: {DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}");
            sb.Append(Environment.NewLine);
            sb.Append($"Results for: {eyeMoveType}s");
            sb.Append(Environment.NewLine);
            sb.Append(rowSep);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            foreach (var result in results)
            {
                sb.Append($"{result.EyeMoveType} Id: {result.Id}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start Index: {result.SpotStartIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start X: {currentSpotEyePointsForSaccades.SpotCoords[result.SpotStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot Start Time: {currentSpotEyePointsForSaccades.TimeStamps[result.SpotStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End Index: {result.SpotEndIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End X: {currentSpotEyePointsForSaccades.SpotCoords[result.SpotEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Spot End Time: {currentSpotEyePointsForSaccades.TimeStamps[result.SpotEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start Index: {result.EyeStartIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start X: {currentSpotEyePointsForSaccades.EyeCoords[result.EyeStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye Start Time: {currentSpotEyePointsForSaccades.TimeStamps[result.EyeStartIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End Index: {result.EyeEndIndex}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End X: {currentSpotEyePointsForSaccades.EyeCoords[result.EyeEndIndex]}");
                sb.Append(Environment.NewLine);
                sb.Append($"Eye End Time: {currentSpotEyePointsForSaccades.TimeStamps[result.EyeEndIndex]}");
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
                for (int i = result.EyeStartIndex; i < result.EyeStartIndex + result.DurationFrameCount; i++)
                {
                    sb.Append($"Index:{i} - Value: {currentSpotEyePointsForSaccades.EyeCoords[i]}");
                    sb.Append(Environment.NewLine);
                }

                sb.Append(Environment.NewLine);
                sb.Append("X Values:");
                sb.Append(Environment.NewLine);
                for (int i = result.EyeStartIndex; i < result.EyeStartIndex + result.DurationFrameCount; i++)
                {
                    sb.Append($"Index:{i} - Time: {currentSpotEyePointsForSaccades.TimeDeltas[i]}");
                    sb.Append(Environment.NewLine);
                }


                sb.Append(Environment.NewLine);
            }


            return sb.ToString();
        }


        public static string GetCsvOutput(bool addHeader, List<SaccadeCalculation> saccadeCalculations, 
            List<SaccadeCalculation> antiSaccadeCalculations, CalcConfig config, FiltersConfig filtersConfig)
        {
            string csvDelimiter = " "; //"\t";
            var sb = new StringBuilder();

            if (addHeader)
            {
                sb.Append("ID" + csvDelimiter);
                sb.Append("EyeMoveType" + csvDelimiter);
                sb.Append("Latency" + csvDelimiter);
                sb.Append("Duration" + csvDelimiter);
                sb.Append("Distance" + csvDelimiter);
                sb.Append("Amplitude" + csvDelimiter);
                sb.Append("AvgVelocity" + csvDelimiter);
                sb.Append("MaxVelocity" + csvDelimiter);
                sb.Append("Gain" + Environment.NewLine);
            }

            var outputItems = saccadeCalculations;
            outputItems.AddRange(antiSaccadeCalculations);

            foreach (var outputItem in outputItems)
            {
                sb.Append(outputItem.Id + csvDelimiter);
                sb.Append(outputItem.EyeMoveType + csvDelimiter);
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

            if(filtersConfig.FilterByButterworth)
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
    
    }
    
}
