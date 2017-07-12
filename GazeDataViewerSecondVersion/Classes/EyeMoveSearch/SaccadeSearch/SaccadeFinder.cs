using GazeDataViewer.Classes.EyeMoveSearch;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.Saccade
{
    public class SaccadeFinder : IEyeMoveFinder
    {
 
        EyeMoveFinderConfig config = null;

        public SaccadeFinder(EyeMoveFinderConfig config)
        {
            this.config = config;
        }

        public EyeMove TryFindEyeMove(int id, int spotStartIndex, int spotEndIndex, ResultData results)
        {
            var spotStartOscilationXPosition = results.SpotCoords[spotStartIndex];
            var spotEndOscilationXPosition = results.SpotCoords[spotStartIndex + 1];

            var controlWindowStartIndex = spotStartIndex;
            var controlWindowCoords = results.EyeCoords.Skip(spotStartIndex - config.ControlWindowLength).Take(config.ControlWindowLength).ToArray();

            var controlMaxCord = controlWindowCoords.Max();
            var controlMinCord = controlWindowCoords.Min();
            var meanControlAmplitude = (controlMaxCord - controlMinCord) / config.ControlAmpDivider; /*2*/
            var minLenght = config.MinLength; /*0.4*/


            var eyeStartIndex = spotStartIndex + config.MinLatency;
            var saccadeStartFindCoords = results.EyeCoords.Skip(eyeStartIndex).Take(config.MoveSearchWindowLength).ToArray();
            var isRising = EyeMoveSearchToolBox.IsRising(spotStartOscilationXPosition, spotEndOscilationXPosition);

           
            bool isStartFound = false;
            EyeMoveSearchToolBox.FindStartByMoveDirection(ref saccadeStartFindCoords, ref eyeStartIndex,
                spotStartOscilationXPosition, controlMinCord, controlMaxCord, isRising, meanControlAmplitude, minLenght, ref isStartFound);

            bool isEndFound = false;
            //var endIndex = GetEndByDirectionChange(saccadeStartFindCoords, eyeStartIndex, isRising, ref isEndFound);
            var endIndex = EyeMoveSearchToolBox.GetEndBySpeedChange(saccadeStartFindCoords, eyeStartIndex, config, ref isEndFound);



            // hamowanie odwrotne
            var saccadeStartIndex = eyeStartIndex;
            var saccadeEndIndex = endIndex;

            if (saccadeStartIndex > results.EyeCoords.Length)
            {
                return null;
            }
            else if (isStartFound == false || isEndFound == false)
            {
                return null;
            }
            else
            {
                return new EyeMove
                {
                    Id = id,
                    IsFirstMove = DataAnalyzer.IsEven(id),
                    IsStartFound = isStartFound,
                    EyeStartIndex = saccadeStartIndex,
                    EyeStartTime = results.TimeDeltas[saccadeStartIndex],
                    EyeStartCoord = results.EyeCoords[saccadeStartIndex],

                    IsEndFound = isEndFound,
                    EyeEndIndex = saccadeEndIndex,
                    EyeEndTime = results.TimeDeltas[saccadeEndIndex],
                    EyeEndCoord = results.EyeCoords[saccadeEndIndex],

                    EyeMoveType = Enums.EyeMoveTypes.Saccade,

                    SpotMove = new Spot.SpotMove
                    {
                        SpotStartIndex = spotStartIndex,
                        SpotStartTimeDelta = results.TimeDeltas[spotStartIndex],
                        SpotStartCoord = results.SpotCoords[spotStartIndex],

                        SpotEndIndex = spotEndIndex,
                        SpotEndTimeDelta = results.TimeDeltas[spotEndIndex],
                        SpotEndCoord = results.SpotCoords[spotEndIndex]
                    }

                };
            }

        }

    }
}
