using Altaxo.Calc.Regression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
    public class DataAnalyzer
    {


        public static SpotGainResults Analyze(SpotGazeFileData fileData, CalcConfig calcConfig, bool shouldDenoise, int recEnd, int numCoef, int derevOrder, int polyOrder)
        {
            var timeSpans = GetDeltaTimespans(fileData.Time);
            //wyznaczamy środek amplitudy 
            var mncal = fileData.Spot.Min();
            var mxcal = fileData.Spot.Max();
            float[] spotMinMaxDelta = { Math.Abs(mncal), mxcal };
            var meanSpotAmplitude = spotMinMaxDelta.Average();

            var recStart = 0;
            //var eyeShiftPeriod = 2;
            //var spotShiftPeriod = -1;
            //var ampProp = 0.1;

            //var delayWindowLargerThan = 0.2;
            //var delayWindowSmallerThan = 0.6;


            if (calcConfig.CalcateRecStart)
            {
                //if (isEyeTribe)
                //{
                // początek (-2) od pierwszego przekroczenia środka amplitudy
                var spotOverAmp = fileData.Spot.Where(x => x > meanSpotAmplitude);
                if (spotOverAmp.Count() > 0)
                {
                    recStart = Array.IndexOf(fileData.Spot, spotOverAmp.Min()) - 2;
                }
                //}
                //else
                //{
                //    recStart = 1000;
                //    eyeShiftPeriod = 100;
                //    ampProp = 0.5;
                //}
            }
            else
            {
                recStart = calcConfig.RecStart;
            }
            // długość od recStart

            //var recLenght = (fileData.originalLenght - recStart);
            var eyeShiftPeriod = calcConfig.EyeShiftPeriod;
            var spotShiftPeriod = calcConfig.SpotShiftPeriod;
            var ampProp = calcConfig.AmpProp;
            var delayWindowLargerThan = calcConfig.DelayWindowLargerThan;
            var delayWindowSmallerThan = calcConfig.DelayWindowSmallerThan;

            // dane od recStart
            var recLenght = (recEnd - recStart);
            var timeStamps = timeSpans.Skip(recStart).Take(recLenght).ToArray();
            var leftEyeAmplitudes = fileData.LEye.Skip(recStart).Take(recLenght).ToArray();
            var rightEyeAmplitudes = fileData.REye.Skip(recStart).Take(recLenght).ToArray();
            var spotAmplitudes = fileData.Spot.Skip(recStart).Take(recLenght).ToArray();

            if (shouldDenoise)
            {
                //left
                double[] smoothedResult = new double[leftEyeAmplitudes.Length];
                var input = Array.ConvertAll(leftEyeAmplitudes, x => (double)x);
                var savGo = new SavitzkyGolay(numCoef, derevOrder, polyOrder);
                savGo.Apply(input, smoothedResult);
                // wyrzuc float!!!
                leftEyeAmplitudes = Array.ConvertAll(smoothedResult, x => (float)x);

                //right
                smoothedResult = new double[rightEyeAmplitudes.Length];
                input = Array.ConvertAll(rightEyeAmplitudes, x => (double)x);
                savGo.Apply(input, smoothedResult);
                rightEyeAmplitudes = Array.ConvertAll(smoothedResult, x => (float)x);
            }
        

            // tworzymy kopie danych z przesniętym reye, leye na shiftPeriod do tyłu (przed poczatkiem), 
            //spot do przodu początek na (+ shifPeroid) ? 
            var shiftedRightEyeAmplitudes = BackRollArray(rightEyeAmplitudes, eyeShiftPeriod);
            var shiftedLeftEyeAmplitudes = BackRollArray(leftEyeAmplitudes, eyeShiftPeriod);
            var shiftedSpotAmplitudes = BackRollArray(spotAmplitudes, spotShiftPeriod);

            //# wylapujemy zmiany amplitudy sygnalu, dla oczu wieksza tolerancja z powodu mniejszej przewidywalnosci
            //czy różnica amplitud pomiędzy przesuniętymi odcinkami jest większa niż średnia amplitudy spot ('true')
            // z okna: amplituda przesuniete minus amplituda oryginalne
            var rightEyeOverMeanSpotAmplitudeIndexes = GetIndexesOverAmplitude(shiftedRightEyeAmplitudes, rightEyeAmplitudes, meanSpotAmplitude, ampProp);
            var leftEyeOverMeanSpotAmplitudeIndexes = GetIndexesOverAmplitude(shiftedLeftEyeAmplitudes, leftEyeAmplitudes, meanSpotAmplitude, ampProp);
            var spotAmplitudeOverMeanIndexes = GetIndexesOverAmplitude(shiftedSpotAmplitudes, spotAmplitudes, meanSpotAmplitude, 0.9);

            var lowestREyeOverAmpIndxesForSpotIndexes = new List<int>();
            var lowestLEyeOverAmpIndxesForSpotIndexes = new List<int>();
            var earliestREyeForSpotsOverAmpTimeDiffs = new List<TimeSpan>();
            var earliestLEyeForSpotsOverAmpTimeDiffs = new List<TimeSpan>();

            // dla każdego rekordu spot powyżej amplitudy
            for (int i = 0; i < spotAmplitudeOverMeanIndexes.Count; i++)
            {
                // dla każdego rekordu spot powyżej amplitudy, obliczamy różnicę dla rekordów amplitudy oczu
                // bieżemy tylko indeksy wartości z różnicą więszą niż 5
                var currSpotAmplitudeOverMeanIndex = spotAmplitudeOverMeanIndexes[i];
                var reducedREyeOverAmpIndexesForCurrSpot = GetReducePointIndexes(rightEyeOverMeanSpotAmplitudeIndexes, currSpotAmplitudeOverMeanIndex, calcConfig.ReductMinEyeSpotAmpDiff);
                var reducedLEyeOverAmpIndexesForCurrSpot = GetReducePointIndexes(leftEyeOverMeanSpotAmplitudeIndexes, currSpotAmplitudeOverMeanIndex, calcConfig.ReductMinEyeSpotAmpDiff);

                // dla każdego indeksu zredukowanego punktu
                if (reducedREyeOverAmpIndexesForCurrSpot.Count > 0)
                {

                    var lowestREyeIndexForCurrentSpot = reducedREyeOverAmpIndexesForCurrSpot.Min();
                    //do 'dre' dodajemy wartość najmniejszego indeksu z list zredukowanych punktów (dla aktualnego punktu spot)
                    lowestREyeOverAmpIndxesForSpotIndexes.Add(lowestREyeIndexForCurrentSpot);

                    // różnica czasu. czas [indeks oka powyżej amplitudy[najniższy indeks dla currSpot] - czas aktualnego punktu spot   
                    //dla każdego najniższego 'dle' dodajemy różnicę czasów pomiędzy 
                    //timeDiffBetweenLowestEyeAndCurrSpotOverAmp
                    earliestREyeForSpotsOverAmpTimeDiffs.Add(timeStamps[rightEyeOverMeanSpotAmplitudeIndexes[lowestREyeIndexForCurrentSpot]] - timeStamps[currSpotAmplitudeOverMeanIndex]);

                    if (reducedLEyeOverAmpIndexesForCurrSpot.Count > 0)
                    {
                        lowestLEyeOverAmpIndxesForSpotIndexes.Add(reducedLEyeOverAmpIndexesForCurrSpot.Min());
                    }

                    if (lowestLEyeOverAmpIndxesForSpotIndexes.Count <= i)
                    {
                    }
                        var ealiestSpotIndex = leftEyeOverMeanSpotAmplitudeIndexes[lowestLEyeOverAmpIndxesForSpotIndexes[i]];
                        earliestLEyeForSpotsOverAmpTimeDiffs.Add(timeStamps[ealiestSpotIndex] - timeStamps[currSpotAmplitudeOverMeanIndex]);
                    //}
                }
            }

            // zawezamy znalezione zmiany amplitudy dla oczu tak aby odpowiadaly w miare pewnym zmianom amplitudy markera
            // tylko amplitudy oka o indeksie równym najniżeszmu indeksowi zredukowanych punktów amplitudy oka   
            var earliestREyeOverAmpForSpotOverAmpIndexes = NarrowList(rightEyeOverMeanSpotAmplitudeIndexes, lowestREyeOverAmpIndxesForSpotIndexes);
            var earliestLEyeOverAmpForSpotOverAmpIndexes = NarrowList(leftEyeOverMeanSpotAmplitudeIndexes, lowestLEyeOverAmpIndxesForSpotIndexes);

            var earliestREyeOverAmpForSpotOverAmpTimeStamps = NarrowList(timeStamps.ToList(), earliestREyeOverAmpForSpotOverAmpIndexes);

            var results = new SpotGainResults
            {
                REyeEarliestOverAmpForSpotTimeStamps = NarrowList(timeStamps.ToList(), earliestREyeOverAmpForSpotOverAmpIndexes),
                LEyeEarliestOverAmpForSpotTimeStamps = NarrowList(timeStamps.ToList(), earliestLEyeOverAmpForSpotOverAmpIndexes),
                SpotOverAmpForSpotTimeStamps = NarrowList(timeStamps.ToList(), spotAmplitudeOverMeanIndexes)
            };

            //############### DELAY # delay with seconds
            //# filter > 0.6 as saccades with issues eg. end of saccade before marker

            var rEyeDelays = earliestREyeForSpotsOverAmpTimeDiffs.Select(x => x.TotalSeconds);
            var lEyeDelays = earliestLEyeForSpotsOverAmpTimeDiffs.Select(x => x.TotalSeconds);

            //var rEyeDelays = earliestREyeForSpotsOverAmpTimeDiffs.Select(x => x.TotalSeconds).Where(x => x < delayWindowSmallerThan && x > delayWindowLargerThan);
            //var lEyeDelays = earliestLEyeForSpotsOverAmpTimeDiffs.Select(x => x.TotalSeconds).Where(x => x < delayWindowSmallerThan && x > delayWindowLargerThan);

            var meanDelayREye = rEyeDelays.Count() > 0 ? rEyeDelays.Average() : 0;
            var meanDelayLEye = lEyeDelays.Count() > 0 ? lEyeDelays.Average() : 0;
            var sdDelayREye = rEyeDelays.Count() > 0 ? CountStd(rEyeDelays) : 0;
            var sdDelayLEye = lEyeDelays.Count() > 0 ? CountStd(lEyeDelays) : 0;

            //############### MAX SPEED
            //różnica pomiędzy czasem i czasem - przesunięcie
            var lEyeMaxSpeedTimes = GetMaxSpeedTimes(timeStamps, earliestLEyeOverAmpForSpotOverAmpIndexes, eyeShiftPeriod);
            var rEyeMaxSpeedTimes = GetMaxSpeedTimes(timeStamps, earliestREyeOverAmpForSpotOverAmpIndexes, eyeShiftPeriod);

            var meanLEyeMaxSpeedTime = lEyeMaxSpeedTimes.Count() > 0 ? lEyeMaxSpeedTimes.Average() : 0;
            var meanREyeMaxSpeedTime = rEyeMaxSpeedTimes.Count() > 0 ? rEyeMaxSpeedTimes.Average() : 0;

            // (amplituda minus amplituda shifted) / mean spot Amp  
            var lEyetMaxSpeedAmplitude = GetMaxSpeedAmplitudes(leftEyeAmplitudes, earliestLEyeOverAmpForSpotOverAmpIndexes, eyeShiftPeriod, meanSpotAmplitude);
            var rEyetMaxSpeedAmplitude = GetMaxSpeedAmplitudes(rightEyeAmplitudes, earliestREyeOverAmpForSpotOverAmpIndexes, eyeShiftPeriod, meanSpotAmplitude);

            //not used
            //var msple = lEyetMaxSpeedAmplitude.Average() / meanLEyeMaxSpeedTime;
            //var mspre = rEyetMaxSpeedAmplitude.Average() / meanREyeMaxSpeedTime;

            //############### duration

            var odd_cnt = GetEvenOrOddIndexeValues(earliestLEyeOverAmpForSpotOverAmpIndexes, false).Count();
            var even_cnt = GetEvenOrOddIndexeValues(earliestLEyeOverAmpForSpotOverAmpIndexes, true).Count();

            var elem_cnt = odd_cnt;
            if( elem_cnt > even_cnt)
            {
                elem_cnt = even_cnt;
            }

            //??? do opisania
            var ldur = GetAmplitudeSeconds(timeStamps, earliestLEyeOverAmpForSpotOverAmpIndexes, elem_cnt);
            var rdur = GetAmplitudeSeconds(timeStamps, earliestREyeOverAmpForSpotOverAmpIndexes, elem_cnt);

            var mredur = rdur.Count > 0 ? rdur.Average(): 0;
            var stdredur = rdur.Count > 0 ? CountStd(rdur): 0;
            var mledur = ldur.Count > 0 ? ldur.Average() : 0;
            var stdledur = ldur.Count > 0 ? CountStd(ldur) : 0;

            results.PlotData = new PlotData
            {
                Kre = earliestREyeOverAmpForSpotOverAmpIndexes,
                Ksp = spotAmplitudeOverMeanIndexes,
                Leye = leftEyeAmplitudes,
                Reye = rightEyeAmplitudes,
                ShiftPeriod = eyeShiftPeriod,
                Spot = spotAmplitudes,
                Stime = timeStamps

            };

            results.MeanSpotAmplitude = meanSpotAmplitude;

            results.MeanDelayRe = Math.Round(meanDelayREye, 4);
            results.MeanDelayLe = Math.Round(meanDelayLEye, 4);
            results.StdDelayRe = Math.Round(sdDelayREye, 4);
            results.StdDelayLe = Math.Round(sdDelayLEye, 4);

            results.DelaysRe = rEyeDelays.ToArray();
            results.DelaysLe = lEyeDelays.ToArray();

            results.MaxSpeedTimesRe = rEyeMaxSpeedTimes.ToArray();
            results.MaxSpeedTimesLe = lEyeMaxSpeedTimes.ToArray();
            results.MaxSpeedAmpsRe = rEyetMaxSpeedAmplitude.ToArray();
            results.MaxSpeedAmpsLe = lEyetMaxSpeedAmplitude.ToArray();

            results.MeanDurationRe = Math.Round(mredur, 4);
            results.MeanDurationLe = Math.Round(mledur, 4);
            results.StdDurationRe = Math.Round(stdredur, 4);
            results.StdDurationLe = Math.Round(stdledur, 4);

            results.DurationsRe = rdur.ToArray();
            results.DurationsLe = ldur.ToArray();

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
        private static List<double> GetMaxSpeedAmplitudes(float[] eyeAmplitudes, List<int> indexes, int shift, float meanSpotAmp)
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

        private static List<float> SubstractValuesInArray(List<float> array, float subtrahend)
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
        private static List<int> GetIndexesOverAmplitude(float[]shiftedAmplitudes, float[] amplitudes, float meanSpotAmplitude, double ampProp)
        {
            var output = new List<int>();
            for (int i = 0; i < amplitudes.Length; i++)
            {
                var subval = Math.Abs(shiftedAmplitudes[i] - amplitudes[i]);
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
