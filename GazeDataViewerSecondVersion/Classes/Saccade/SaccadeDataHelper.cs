using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public static class SaccadeDataHelper
    {
        public static SaccadePosition GetSaccadePositionItem(int id, int saccadeStartIndex, int saccadeEndIndex, int spotOverMeanStartIndex, int spotOverMeanEndIndex, SpotGainResults results)
        {
            return new SaccadePosition
            {
                Id = id,
                SaccadeStartIndex = saccadeStartIndex,
                SaccadeStartTime = results.PlotData.TimeStamps[saccadeStartIndex],
                SaccadeStartCoord = results.PlotData.EyeCoords[saccadeStartIndex],

                SaccadeEndIndex = saccadeEndIndex,
                SaccadeEndTime = results.PlotData.TimeStamps[saccadeEndIndex],
                SaccadeEndCoord = results.PlotData.EyeCoords[saccadeEndIndex],

                SpotStartIndex = spotOverMeanStartIndex,
                SpotStartTime = results.PlotData.TimeStamps[spotOverMeanStartIndex],
                SpotStartCoord = results.PlotData.SpotCoords[spotOverMeanStartIndex],

                SpotEndIndex = spotOverMeanEndIndex,
                SpotEndTime = results.PlotData.TimeStamps[spotOverMeanEndIndex],
                SpotEndCoord = results.PlotData.SpotCoords[spotOverMeanEndIndex],

            };
        }

        public static SaccadePosition GetSaccadePositionItem(int id, int saccadeStartIndex, int saccadeEndIndex, int spotOverMeanStartIndex,
            double spotStartTime, double spotStartCoord, int spotOverMeanEndIndex,
            double spotEndTime, double spotEndCoord, SpotGainResults results)
        {
            return new SaccadePosition
            {
                Id = id,
                SaccadeStartIndex = saccadeStartIndex,
                SaccadeStartTime = results.PlotData.TimeStamps[saccadeStartIndex],
                SaccadeStartCoord = results.PlotData.EyeCoords[saccadeStartIndex],

                SaccadeEndIndex = saccadeEndIndex,
                SaccadeEndTime = results.PlotData.TimeStamps[saccadeEndIndex],
                SaccadeEndCoord = results.PlotData.EyeCoords[saccadeEndIndex],

                SpotStartIndex = spotOverMeanStartIndex,
                SpotStartTime = spotStartTime,
                SpotStartCoord = spotStartCoord,

                SpotEndIndex = spotOverMeanEndIndex,
                SpotEndTime = spotEndTime,
                SpotEndCoord = spotEndCoord,

            };
        }

        public static int CountEyeStartIndex(int eyeStartIndex, int eyeStartShiftPeriod)
        {
            return eyeStartIndex + eyeStartShiftPeriod;
        }


        public static int CountEyeShiftIndex(int eyeStartIndex, int eyeShiftPeriod,  int saccadeEndShiftPeroid)
        {
            return eyeStartIndex + eyeShiftPeriod + saccadeEndShiftPeroid;
        }

        public static int CountSpotShiftIndex(int spotStartIndex, int spotShiftPeriod)
        {
            return spotStartIndex + spotShiftPeriod;
        }
    }
}
