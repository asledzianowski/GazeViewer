﻿using GazeDataViewer.Classes.EyeMoveSearch;
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

            var controlWindowCoords = EyeMoveSearchToolBox.GetControlWindow(results, spotStartIndex, config.ControlWindowLength);

            var meanControlAmplitude = EyeMoveSearchToolBox.CalculateControlAmplitude(controlWindowCoords, config.ControlAmpDivider);
            var minLenght = config.MinLength; //0.3;
            var anitSaccadeLatency = config.MinLatency; //15;

            var eyeStartIndex = spotStartIndex + anitSaccadeLatency; // 500ms
            var antiSaccadeStartFindCoords = results.EyeCoords.Skip(eyeStartIndex).Take(config.MoveSearchWindowLength).ToArray();

            var spotStartOscilationXPosition = results.SpotCoords[spotStartIndex];
            var spotEndOscilationXPosition = results.SpotCoords[spotStartIndex + 1];
            var isRising = !EyeMoveSearchToolBox.IsRising(spotStartOscilationXPosition, spotEndOscilationXPosition);

            bool isStartFound = false;
            double controlAmpTestValue = -1;
            double minLengthTestValue = -1;
            EyeMoveSearchToolBox.FindStartByMoveDirection(ref antiSaccadeStartFindCoords, ref eyeStartIndex,
            spotStartOscilationXPosition, isRising, meanControlAmplitude, minLenght, 
            ref isStartFound, ref controlAmpTestValue, ref minLengthTestValue);

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
                    IsFirstMove = DataAnalyzer.IsEven(id),

                    IsStartFound = isStartFound,
                    EyeStartIndex = antiSaccadeStartIndex,
                    EyeStartTime = results.TimeDeltas[antiSaccadeStartIndex],
                    EyeStartCoord = results.EyeCoords[antiSaccadeStartIndex],
                    ControlAmpTestValue = controlAmpTestValue,
                    MinLengthTestValue = minLengthTestValue,

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
