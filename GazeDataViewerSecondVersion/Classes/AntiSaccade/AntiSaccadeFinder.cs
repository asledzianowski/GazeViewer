using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.SpotAndGain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.AntiSaccade
{
    class AntiSaccadeFinder
    {

        SaccadeFinder saccadeFinder;

        public AntiSaccadeFinder(SaccadeFinder saccadeFinder)
        {
            this.saccadeFinder = saccadeFinder;
        }

        public void FindAntiSaccade(int id, int spotStartIndex, int spotEndIndex, int latency, int minDuration, ResultData results)
        {
            //Moze zaczac od sakady i w ciagu 500 ms powinien zaczac antysakade 
            //(wtedy zaliczamy jako prawidlowa antysakade) czyli ruch oka w kierunku przeciwnym do plamki 
            //- jesli takiego nie wykonuje to zaliczamy jako zly wynik.

            var eyeStartIndex = spotStartIndex + 3; // 500ms

        }
    }
}
