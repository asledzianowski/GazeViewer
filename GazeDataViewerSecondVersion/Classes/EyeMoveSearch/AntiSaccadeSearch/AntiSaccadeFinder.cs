using GazeDataViewer.Classes.EyeMoveSearch;
using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.AntiSaccade
{
    class AntiSaccadeFinder : IEyeMoveFinder
    {

        EyeMoveFinderConfig config = null;

        public AntiSaccadeFinder(EyeMoveFinderConfig config)
        {
            this.config = config;
        }

        public EyeMove TryFindEyeMove(int id, int spotStartIndex, int spotEndIndex, ResultData results)
        {
            //Moze zaczac od sakady i w ciagu 500 ms powinien zaczac antysakade 
            //(wtedy zaliczamy jako prawidlowa antysakade) czyli ruch oka w kierunku przeciwnym do plamki 
            //- jesli takiego nie wykonuje to zaliczamy jako zly wynik.

            var controlWindowCoords = results.EyeCoords.Skip(spotStartIndex - this.config.ControlWindowLength).Take(config.ControlWindowLength).ToArray();

            var controlMaxCord = controlWindowCoords.Max();
            var controlMinCord = controlWindowCoords.Min();
            var meanControlAmplitude = (controlMaxCord - controlMinCord) / config.ControlAmpDivider; //2;
            var minLenght = config.MinLength; //0.3;
            var anitSaccadeLatency = config.MinLatency; //15;

            var eyeStartIndex = spotStartIndex + anitSaccadeLatency; // 500ms
            var antiSaccadeStartFindCoords = results.EyeCoords.Skip(eyeStartIndex).Take(config.MoveSearchWindowLength).ToArray();

            var spotStartOscilationXPosition = results.SpotCoords[spotStartIndex];
            var spotEndOscilationXPosition = results.SpotCoords[spotStartIndex + 1];
            var isRising = !EyeMoveSearchToolBox.IsRising(spotStartOscilationXPosition, spotEndOscilationXPosition);

            bool isStartFound = false;
            EyeMoveSearchToolBox.FindStartByMoveDirection(ref antiSaccadeStartFindCoords, ref eyeStartIndex,
            spotStartOscilationXPosition, controlMinCord, controlMaxCord, isRising, meanControlAmplitude, minLenght, ref isStartFound);

            bool isEndFound = false;
            var endIndex = EyeMoveSearchToolBox.GetEndBySpeedChange(antiSaccadeStartFindCoords, eyeStartIndex, config, ref isEndFound);

            // hamowanie odwrotne
            var antiSaccadeStartIndex = eyeStartIndex;
            var antiSaccadeEndIndex = endIndex;

            if (antiSaccadeStartIndex > results.EyeCoords.Length)
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

                    IsStartFound = isStartFound,
                    EyeStartIndex = antiSaccadeStartIndex,
                    EyeStartTime = results.TimeDeltas[antiSaccadeStartIndex],
                    EyeStartCoord = results.EyeCoords[antiSaccadeStartIndex],

                    IsEndFound = isEndFound,
                    EyeEndIndex = antiSaccadeEndIndex,
                    EyeEndTime = results.TimeDeltas[antiSaccadeEndIndex],
                    EyeEndCoord = results.EyeCoords[antiSaccadeEndIndex],

                    EyeMoveType = Enums.EyeMoveTypes.AntiSaccade,

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
