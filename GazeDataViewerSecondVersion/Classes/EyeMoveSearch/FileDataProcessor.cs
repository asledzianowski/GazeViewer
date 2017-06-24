using GazeDataViewer.Classes.AntiSaccade;
using GazeDataViewer.Classes.DataAndLog;
using GazeDataViewer.Classes.Denoise;
using GazeDataViewer.Classes.Enums;
using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.Spot;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.EyeMoveSearch
{
    public class FileDataProcessor
    {
        public string CalculateFileData(string eyeFilePath, int timeColumnIndex, int eyeColumnIndex, int spotColumnIndex, 
            CalcConfig calcConfig, FiltersConfig filtersConfig)
        {
            var fileData = InputDataHelper.LoadDataForSpotAndGaze(eyeFilePath, timeColumnIndex, eyeColumnIndex, spotColumnIndex);
            var spotEyePoints = DataAnalyzer.FindSpotEyePointsForSaccadeAntiSaccadeSearch(fileData, calcConfig);

            var saccadeFinder = new SaccadeFinder(calcConfig.SaccadeMoveFinderConfig);
            var antiSaccadeFinder = new AntiSaccadeFinder(calcConfig.AntiSaccadeMoveFinderConfig);

            List<EyeMove> saccadePositions = new List<EyeMove>();
            List<EyeMove> antiSaccadePositions = new List<EyeMove>();

            for (int i = 0; i < spotEyePoints.SpotOverMeanIndex.Count; i++)
            {
                var spotOverMeanIndex = spotEyePoints.SpotOverMeanIndex[i];
                var currentSpotShiftIndex = SaccadeDataHelper.CountSpotShiftIndex(spotOverMeanIndex, spotEyePoints.ShiftPeriod);

                if (currentSpotShiftIndex < spotEyePoints.TimeDeltas.Count())
                {
                    var saccItem = saccadeFinder.TryFindEyeMove(i, spotOverMeanIndex, currentSpotShiftIndex, spotEyePoints);
                    var antiSaccItem = antiSaccadeFinder.TryFindEyeMove(i, spotOverMeanIndex, currentSpotShiftIndex, spotEyePoints);

                    if (antiSaccItem != null)
                    {
                        antiSaccadePositions.Add(antiSaccItem);
                    }
                    else if (saccItem != null)
                    {
                        saccadePositions.Add(saccItem);
                    }
                }
            }

            var saccCalculator = new SaccadeParamsCalcuator(spotEyePoints.EyeCoords,
                spotEyePoints.SpotCoords, calcConfig.DistanceFromScreen, calcConfig.TrackerFrequency);

            var saccadeCalculations = saccCalculator.Calculate(saccadePositions, EyeMoveTypes.Saccade);
            var antiSaccadeCalculations = saccCalculator.Calculate(antiSaccadePositions, EyeMoveTypes.AntiSaccade);

            var csvOutput = OutputHelper.GetCsvOutput(true, saccadeCalculations, antiSaccadeCalculations, 
                calcConfig, filtersConfig);
            return csvOutput;
        }
    }

}
