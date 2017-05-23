﻿using Altaxo.Calc.Regression;
using GazeDataViewer.Classes.Denoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GazeDataViewer.Classes.SpotAndGain
{
    public class DataAnalyzer
    {

        public static SpotGainResults Analyze(SpotGazeFileData fileData, CalcConfig calcConfig, int recEnd, FiltersConfig filterConfig)
        {
            var timeSpans = GetDeltaTimespansInt(fileData.Time);
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
            var recLenght = (recEnd - recStart);
            var timeStamps = timeSpans.Skip(recStart).Take(recLenght).ToArray();
            var eyeCoords = fileData.Eye.Skip(recStart).Take(recLenght).ToArray();
            var spotCoords = fileData.Spot.Skip(recStart).Take(recLenght).ToArray();

            if(filterConfig.FilterByButterworth)
            {
                var filterButterworth = new FilterButterworth(filterConfig.ButterworthFrequency, filterConfig.ButterworthSampleRate,
                    filterConfig.ButterworthPassType, filterConfig.ButterworthResonance);
                eyeCoords = filterButterworth.FilterArray(eyeCoords);
            }

            if (filterConfig.FilterBySavitzkyGolay)
            {
                //left
                double[] smoothedResult = new double[eyeCoords.Length];
                var filterSavitzkyGolay = new SavitzkyGolay(filterConfig.SavitzkyGolayNumberOfPoints, filterConfig.SavitzkyGolayDerivativeOrder, 
                    filterConfig.SavitzkyGolayPolynominalOrder);
                filterSavitzkyGolay.Apply(eyeCoords, smoothedResult);
                eyeCoords = smoothedResult;
            }
        

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
                var currSpotAmplitudeOverMeanIndex = spotAmplitudeOverMeanIndexes[i];
                var reducedREyeOverAmpIndexesForCurrSpot = GetReducePointIndexes(rightEyeOverMeanSpotAmplitudeIndexes, currSpotAmplitudeOverMeanIndex, calcConfig.ReductMinEyeSpotAmpDiff);
               
                // dla każdego indeksu zredukowanego punktu
                if (reducedREyeOverAmpIndexesForCurrSpot.Count > 0)
                {

                    var lowestREyeIndexForCurrentSpot = reducedREyeOverAmpIndexesForCurrSpot.Min();
                    //do 'dre' dodajemy wartość najmniejszego indeksu z list zredukowanych punktów (dla aktualnego punktu spot)
                    lowestEyeOverAmpIndxesForSpotIndexes.Add(lowestREyeIndexForCurrentSpot);

                }
            }

            // zawezamy znalezione zmiany amplitudy dla oczu tak aby odpowiadaly w miare pewnym zmianom amplitudy markera
            // tylko amplitudy oka o indeksie równym najniżeszmu indeksowi zredukowanych punktów amplitudy oka   
            var earliestEyeOverAmpForSpotOverAmpIndexes = NarrowList(rightEyeOverMeanSpotAmplitudeIndexes, lowestEyeOverAmpIndxesForSpotIndexes);
           
            var results = new SpotGainResults();
            results.PlotData = new PlotData
            {
                EarliestEyeOverSpotIndex = earliestEyeOverAmpForSpotOverAmpIndexes,
                SpotOverMeanIndex = spotAmplitudeOverMeanIndexes,
                EyeCoords = eyeCoords,
                ShiftPeriod = eyeShiftPeriod,
                SpotCoords = spotCoords,
                TimeStamps = timeStamps

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

        private static int[] GetDeltaTimespansInt(int[] timestamps)
        {
            var startTime = timestamps[0];
            var stime = new List<int>();

            for (int i = 0; i < timestamps.Length; i++)
            {
                var timeSpan = timestamps[i] - startTime;
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