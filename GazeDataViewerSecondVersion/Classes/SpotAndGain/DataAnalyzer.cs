using Altaxo.Calc.Regression;
using GazeDataViewer.Classes.Denoise;
using GazeDataViewer.Classes.EyeMoveSearch;
using GazeDataViewer.Classes.Saccade;
using GazeDataViewer.Classes.Spot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
    public class DataAnalyzer
    {

        public static Dictionary<int, double> GetApproxEyeSinusoidForPursuitSearch(SpotGazeFileData fileData, CalcConfig calcConfig)
        {
            //var spotAvg = fileData.Spot.Average();
            //var spotAtAvg = new List<SpotMove>();
            //var spotTrack = fileData.Spot.ToList();
            /////var spotMoveStartIndex = spotTrack.IndexOf(spotTrack.FirstOrDefault(x => x != 0));

            var data = fileData; // InputDataHelper.CutData(fileData, spotMoveStartIndex, fileData.Spot.Length - spotMoveStartIndex);
            var latency = calcConfig.PursuitMoveFinderConfig.MinLatency;
            var approximations = new Dictionary<int, double>();
            for(int i = latency; i < data.Eye.Count(); i++)
            {
                double? appVal = null;
                if (data.Eye[i - latency] != 0)
                {
                    var originalVal = data.Eye[i - latency];
                    appVal =  PursuitMoveHelper.GetSinusoideApproximation(originalVal);
                    appVal = appVal * calcConfig.PursuitMoveFinderConfig.Multiplication;
                }

                
                if (appVal != null)
                {
                    approximations.Add(data.TimeDeltas[i], appVal.GetValueOrDefault());
                }
                else
                { 
                    //var randomVal = Convert.ToDouble(new Random().Next(-1, 2)); //min + new Random().NextDouble() * (max - min);
                    //if(randomVal != 0)
                    //{
                    //    randomVal = randomVal / 10;
                    //}
                    approximations.Add(data.TimeDeltas[i], 0);
                }
                
            }

           

            return approximations;
        }

        public static EyeMoveCalculation CountPursoitParameters(SpotGazeFileData data, Dictionary<int, double> approximations, CalcConfig calcConfig)
        {
            var spotOnScreenDistance = SaccadeDataHelper.CountOnScreenDistance(data.Spot.ToArray()).Sum();
            var eyeOnScreenDistance = SaccadeDataHelper.CountOnScreenDistance(data.Eye.ToArray()).Sum();
            var eyeOnScreenDistanceApproximations = SaccadeDataHelper.CountOnScreenDistance(approximations.Values.ToArray()).Sum();

            var spotEyeGain = eyeOnScreenDistance / spotOnScreenDistance;
            var eyeToApproxEyeGain = eyeOnScreenDistance / eyeOnScreenDistanceApproximations;

            return new EyeMoveCalculation
            {
                Gain = spotEyeGain,
                ApproxGain = eyeToApproxEyeGain,
                Latency = calcConfig.PursuitMoveFinderConfig.MinLatency
            };
        }

        public static ResultData FindSpotEyePointsForSaccadeAntiSaccadeSearch(SpotGazeFileData fileData, CalcConfig calcConfig)
        {
            var spotMovePositions = new List<SpotMove>();
            //wyznaczamy środek amplitudy 
            var mncal = fileData.Spot.Min();
            var mxcal = fileData.Spot.Max();
            double[] spotMinMaxDelta = { Math.Abs(mncal), mxcal };
            var meanSpotAmplitude = spotMinMaxDelta.Average();

          
            var recStart = calcConfig.RecStart;
       
            var eyeShiftPeriod = calcConfig.EyeShiftPeriod;
            var spotShiftPeriod = calcConfig.SpotShiftPeriod;
            var eyeAmpProp = calcConfig.EyeAmpProp;
            var spotAmpProp = calcConfig.SpotAmpProp;

            // dane od recStart
            //var recLenght = (recEnd - recStart);
            //var timeDeltas = fileData.TimeDeltas.Skip(recStart).Take(recLenght).ToArray();
            //var eyeCoords = fileData.Eye.Skip(recStart).Take(recLenght).ToArray();
            //var spotCoords = fileData.Spot.Skip(recStart).Take(recLenght).ToArray();

            var timeDeltas = fileData.TimeDeltas;
            var eyeCoords = fileData.Eye;
            var spotCoords = fileData.Spot;
            var timeStamps = fileData.Time;

            // tworzymy kopie danych z przesniętym reye, leye na shiftPeriod do tyłu (przed poczatkiem), 
            //spot do przodu początek na (+ shifPeroid) ? 
            var shiftedEyeCoords = BackRollArray(eyeCoords, eyeShiftPeriod);
            var shiftedSpotCoords = BackRollArray(spotCoords, spotShiftPeriod);

            //# wylapujemy zmiany amplitudy sygnalu, dla oczu wieksza tolerancja z powodu mniejszej przewidywalnosci
            //czy różnica amplitud pomiędzy przesuniętymi odcinkami jest większa niż średnia amplitudy spot ('true')
            // z okna: amplituda przesuniete minus amplituda oryginalne
            var rightEyeOverMeanSpotAmplitudeIndexes = GetIndexesOverAmplitude(shiftedEyeCoords, eyeCoords, meanSpotAmplitude, eyeAmpProp);
            var spotAmplitudeOverMeanIndexes = GetIndexesOverAmplitude(shiftedSpotCoords, spotCoords, meanSpotAmplitude, spotAmpProp);

            var lowestEyeOverAmpIndxesForSpotIndexes = new List<int>();
           
            // dla każdego rekordu spot powyżej amplitudy
            for (int i = 0; i < spotAmplitudeOverMeanIndexes.Count; i++)
            {

                // dla każdego rekordu spot powyżej amplitudy, obliczamy różnicę dla rekordów amplitudy oczu
                // bieżemy tylko indeksy wartości z różnicą więszą niż 5
                var currSpotOverMeanIndex = spotAmplitudeOverMeanIndexes[i];
                var currentSpotShiftIndex = SaccadeDataHelper.CountSpotShiftIndex(currSpotOverMeanIndex, eyeShiftPeriod);

                if (currentSpotShiftIndex < timeDeltas.Count())
                {
                    spotMovePositions.Add(new SpotMove
                    {
                        SpotStartIndex = currSpotOverMeanIndex,
                        SpotStartTimeDelta = timeDeltas[currSpotOverMeanIndex],
                        SpotStartTimeStamp = timeStamps[currSpotOverMeanIndex],
                        SpotStartCoord = spotCoords[currSpotOverMeanIndex],

                        SpotEndIndex = currentSpotShiftIndex,
                        SpotEndTimeDelta = timeDeltas[currentSpotShiftIndex],
                        SpotEndTimeStamp = timeStamps[currentSpotShiftIndex],
                        SpotEndCoord = spotCoords[currentSpotShiftIndex],
                    });
                }

         
            }

            // zawezamy znalezione zmiany amplitudy dla oczu tak aby odpowiadaly w miare pewnym zmianom amplitudy markera
            // tylko amplitudy oka o indeksie równym najniżeszmu indeksowi zredukowanych punktów amplitudy oka   
            var earliestEyeOverAmpForSpotOverAmpIndexes = NarrowList(rightEyeOverMeanSpotAmplitudeIndexes, lowestEyeOverAmpIndxesForSpotIndexes);
           
            var results = new ResultData();
            results = new ResultData
            {
                SpotOverMeanIndex = spotAmplitudeOverMeanIndexes,
                SpotMoves = spotMovePositions,
                EyeCoords = eyeCoords,
                ShiftPeriod = eyeShiftPeriod,
                SpotCoords = spotCoords,
                TimeDeltas = timeDeltas,
                TimeStamps = fileData.Time,
            };

            return results;
        }

      

        private static List<double> GetAmplitudeSeconds(TimeSpan[] stime, List<int> indexes, int elemCent)
        {
            var output = new List<double>();
            var oddIndexes = GetEvenOrOddIndexeValues(indexes, false).Take(elemCent).ToList();
            var evenIndexes = GetEvenOrOddIndexeValues(indexes,true).Take(elemCent).ToList();
            for (int i = 0; i<elemCent; i++)
            {
                var span = stime[oddIndexes[i]] - stime[evenIndexes[i]];
                output.Add(span.TotalSeconds);
            } 
            return output;
        }


        private static List<int> GetEvenOrOddIndexeValues(List<int> array, bool even)
        {
            var output = new List<int>();
            for(int i = 0; i< array.Count; i++)
            {
                if(even)
                {
                    if(IsEven(i))
                    {
                        output.Add(array[i]);
                    }
                }
                else
                {
                    if (!IsEven(i))
                    {
                        output.Add(array[i]);
                    }
                }
            }
            return output;
        }

        public static bool IsEven(int value)
        {
            return value % 2 == 0;
        }


        // (amplituda minus amplituda shifted) / mean spot Amp 
        private static List<double> GetMaxSpeedAmplitudes(double[] eyeAmplitudes, List<int> indexes, int shift, double meanSpotAmp)
        {
            var output = new List<double>();
           
            foreach (var index in indexes)
            {
                var shiftedIndex = index - shift;
                if ((shiftedIndex) >= 0)
                {
                    var value = Math.Abs(eyeAmplitudes[index] - eyeAmplitudes[shiftedIndex]) / meanSpotAmp;
                    output.Add(Math.Round(value, 4));
                }
            }
            return output;
        }

        //różnica pomiędzy czasem i czasem przesunięcia
        private static List<double> GetMaxSpeedTimes(TimeSpan[] stime, List<int> indexes, int shift)
        {
            var output = new List<double>();
           
            foreach (var index in indexes)
            {
                var shiftedIndex = index - shift;
                if ((shiftedIndex) >= 0)
                {
                    var span = stime[index] - stime[shiftedIndex];
                    output.Add(span.TotalSeconds);
                }
            }
            
            return output;
        }

        private static List<double> SubstractValuesInArray(List<double> array, float subtrahend)
        {
            for(int i=0; i < array.Count; i++)
            {
                array[i] = array[i] - subtrahend;
            }

            return array;
        }

        private static List<T> NarrowList<T>(List<T> listForNarrow, List<int> indexSource)
        {
            var narrowedList = new List<T>();
            foreach (var index in indexSource)
            {
                narrowedList.Add(listForNarrow[index]);
            }
            return narrowedList;
        }

        private static double CountStd(IEnumerable<double> data)
        {
            return Math.Sqrt(data.Average(z => z * z) - Math.Pow(data.Average(), 2));
        }


        //dla każdego punktu oka powyżej amplitudy, obliczamy różnicę dla punktu spot.  
        //wybieram punkty z różnicą powyżej 5
        private static List<int> GetReducePointIndexes(List<int> eyeOverAmplitudeIndexes,  int spotOverAmplitudeIndex, int minReduceEyeSpotAmpDiff)
        {
            var reducePointsIndexes = new List<int>();
            for (int i = 0; i < eyeOverAmplitudeIndexes.Count; i++)
            {
                var substraction = eyeOverAmplitudeIndexes[i] - spotOverAmplitudeIndex;
                if (substraction > minReduceEyeSpotAmpDiff)
                {
                    reducePointsIndexes.Add(i);
                }
            }
            return reducePointsIndexes;
        }

        //czy różnica amplitud pomiędzy przesuniętymi odcinkami jest większa niż średnia amplitudy spot.  
        private static List<int> GetIndexesOverAmplitude(double[]shiftedCoords, double[] coords, double meanSpotAmplitude, double ampProp)
        {
            var output = new List<int>();
            for (int i = 0; i < coords.Length; i++)
            {
                var subval = Math.Abs(shiftedCoords[i] - coords[i]);
                if (subval > meanSpotAmplitude * ampProp)
                {
                    output.Add(i);
                }
            }
            return output;
        }
        

        

        private static TimeSpan[] GetDeltaTimespans(DateTime[] timestamps)
        {
            var startTime = timestamps[0];
            var stime = new List<TimeSpan>();

            for (int i = 0; i < timestamps.Length; i++)
            {
                var timeSpan = new TimeSpan(timestamps[i].Ticks - startTime.Ticks);
                stime.Add(timeSpan);
            }
            return stime.ToArray();
        }

        private static TimeSpan[] GetDeltaTimespans(int[] timestamps)
        {
            var startTime = timestamps[0];
            var stime = new List<TimeSpan>();

            for (int i = 0; i < timestamps.Length; i++)
            {
                var timeSpan =  TimeSpan.FromMilliseconds(timestamps[i] - startTime);
                stime.Add(timeSpan);
            }
            return stime.ToArray();
        }

       

        /// <summary>
        /// Jeżeli rollBy > 0 to back, tail na początek (było 1,2...9,10 - jest 9,10,1,2,3...)
        /// Jeżeli rollBy < 0 to forward, head na koniec (było 1,2...9,10 - jest 3..9,10,1,2)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="rollBy"></param>
        /// <returns></returns>
        private static T[] BackRollArray <T>(T[] array, int rollBy)
        {

            if(rollBy == 0)
            {
                return array;
            }

            if (rollBy > 0)
            {
                var head = array.Take(rollBy);
                var tail = array.Skip(array.Length - rollBy).Take(rollBy).ToList();
                tail.AddRange(head);
                tail.AddRange(array.Skip(rollBy).Take(array.Length - (rollBy * 2)));
                return tail.ToArray();
            }

            if(rollBy < 0)
            {
                rollBy = Math.Abs(rollBy);
                var output = array.Skip(rollBy).Take(array.Length - rollBy).ToList();
                if (rollBy == 1)
                {
                    output.Add(array.First());
                }
                else
                {
                    output.AddRange(array.Take(rollBy));
                }
                
                return output.ToArray();
            }

            return array;

        }


       
    }
}
