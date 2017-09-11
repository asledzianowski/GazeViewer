using Altaxo.Calc.Regression;
using GazeDataViewer.Classes.Denoise;
using GazeDataViewer.Classes.EnumsAndStats;
using GazeDataViewer.Classes.EyeMoveSearch;
using GazeDataViewer.Classes.ParamCalculations;
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


        public static PursuitGainCalculations CountPursuitParameters(SpotGazeFileData fileData, FiltersConfig filterConfig)
        {
            var mncal = fileData.Spot.Min();
            var mxcal = fileData.Spot.Max();
            var amp = (mxcal - mncal) / 2;
            var srod = (mxcal + mncal) / 2;

            var start = fileData.Eye.FirstOrDefault(x => x == srod);
          

            var testValues = new List<double>();

            var kspIndexes = new List<KeyValuePair<double, int>>();
            var accuracyKspSpotValues = new List<KeyValuePair<double, double>>();
            var accuracyKspDiffValues = new List<KeyValuePair<double, double>>();


            double sinMidStart;
            double sinShortStart;

            if (fileData.FileType == FileType.Maruniec)
            {
                sinMidStart = 37 * Consts.TimeScaleFactorStandard;
                sinShortStart = 79 * Consts.TimeScaleFactorStandard;
            }
            else 
            {
                sinMidStart = 40 * Consts.TimeScaleFactorStandard;
                sinShortStart = 74 * Consts.TimeScaleFactorStandard;
            }

            var unifiedTimeStamps = InputDataHelper.GetTimeStampsScaled(fileData.Time, fileData.FileType);

            for (int i = 0; i < fileData.Spot.Length; i++)
            {
                var xSpot = fileData.Spot[i];

                var sinLenght = 240D;

                if(unifiedTimeStamps[i] >= sinMidStart && unifiedTimeStamps[i] <= sinShortStart)
                {
                    sinLenght = 120D;
                }
                else if (unifiedTimeStamps[i] >= sinShortStart)
                {
                    sinLenght = 30D;
                }

                //if (i > 1000 && i < 2400)
                //{
                //    sinLenght = 120D;
                //}
                //else if (i > 2400)
                //{
                //    sinLenght = 30D;
                //}

                
                if(Math.Abs(xSpot - srod) > amp * 0.995D)
                {
                    kspIndexes.Add(new KeyValuePair<double, int>(sinLenght, i));

                    //Accuracy
                    var xEye = fileData.Eye[i];
                    var diffVal = Math.Abs(xEye - xSpot);
                    accuracyKspDiffValues.Add(new KeyValuePair<double, double>(sinLenght, diffVal));
                    accuracyKspSpotValues.Add(new KeyValuePair<double, double>(sinLenght, Math.Abs(xSpot)));
                }
            }

            var kspEyeValues= new List<double>();
            var kspSpotValues = new List<double>();
            
            var results = new List<KeyValuePair<double, double>>();
            var isPositive = true;
            var times = new List<double>();
            var filteredControlWindows = new List<Dictionary<double, double>>();
            var kspDiffValues = new List<Dictionary<double, double>>();

            for (int j = 0; j < kspIndexes.Count; j++)
            {
                var sinLenght = kspIndexes[j].Key;
                var index = kspIndexes[j].Value;

                var time = InputDataHelper.GetScaledTimeFromIndex(fileData, index).GetValueOrDefault();
                times.Add(time);

                var controlWindow = new List<double>();
                var controlTimeDeltas = new List<double>();
                var controlWindowLength = Convert.ToInt32(Math.Round(sinLenght / 5, 0));

                if (j > 0)
                {
                    controlWindow = fileData.Eye.Skip(index - (controlWindowLength / 2)).Take(controlWindowLength).ToList();
                    controlTimeDeltas = fileData.TimeDeltas.Skip(index - (controlWindowLength / 2)).Take(controlWindowLength).ToList();
                }
                else
                {
                    controlWindow = fileData.Eye.Skip(index).Take(controlWindowLength).ToList();
                    controlTimeDeltas = fileData.TimeDeltas.Skip(index).Take(controlWindowLength).ToList();
                }


                //var filterConfig = new FiltersConfig
                //{
                //    FilterByButterworth = true,
                //    ButterworthPassType = FilterButterworth.PassType.Lowpass,
                //    ButterworthFrequency = 3,
                //    ButterworthResonance = 1,
                //    ButterworthSampleRate = 40
                //};

                if (filterConfig.FilterByButterworth)
                {
                    controlWindow = FilterController.FilterByButterworth(filterConfig, controlWindow.ToArray()).ToList();
                }

                var filteredWindowItems = new Dictionary<double, double>();
                for(int g = 0; g < controlWindow.Count(); g++)
                {
                    filteredWindowItems.Add(controlTimeDeltas[g], controlWindow[g]);
                }
                filteredControlWindows.Add(filteredWindowItems);

                double eyeValue;
                if (isPositive)
                {
                    eyeValue = controlWindow.Average(); //Max();
                    isPositive = false;
                }
                else
                {
                    eyeValue = controlWindow.Average(); //Min();
                    isPositive = true;
                }

                if(sinLenght == 30D)
                {
                    testValues.Add(eyeValue);
                }

                //var eyeValue = fileData.Eye[index];
                var spotValue = fileData.Spot[index];
                var result = eyeValue / spotValue;
                results.Add(new KeyValuePair<double,double>(sinLenght, result));

            }

            var longSinGain = new double?();
            if (results.Where(x => x.Key == 240D).Count() > 0)
            {
                longSinGain = results.Where(x => x.Key == 240D).Select(x => x.Value).Average();
            }
            
            var midSinGain = new double?();
            if (results.Where(x => x.Key == 120D).Select(x => x.Value).Count() > 0)
            {
                midSinGain = results.Where(x => x.Key == 120D).Select(x => x.Value).Average();
            }
           
            var shortSinGain = new double?();
            if (results.Where(x => x.Key == 30D).Select(x => x.Value).Count() > 0)
            {
                shortSinGain = results.Where(x => x.Key == 30D).Select(x => x.Value).Average();
            }
          
            var gainCalculations = new Dictionary<string, double?>();
            if(longSinGain.HasValue)
            {
                gainCalculations.Add("Long", longSinGain.GetValueOrDefault());
            }
            else
            {
                gainCalculations.Add("Long", null);
            }
            
            if(midSinGain.HasValue)
            {
                gainCalculations.Add("Mid", midSinGain.GetValueOrDefault());
            }  
            else
            {
                gainCalculations.Add("Mid", null);
            }

            if (shortSinGain.HasValue)
            {
                gainCalculations.Add("Short", shortSinGain.GetValueOrDefault());
            }
            else
            {
                gainCalculations.Add("Short", null);
            }

            var longSinAcc = new double?();
            if (accuracyKspDiffValues.Where(x => x.Key == 240D).Count() > 0 && accuracyKspSpotValues.Where(x => x.Key == 240D).Count() > 0)
            {
                var longSinW1 = accuracyKspDiffValues.Where(x => x.Key == 240D).Select(x => x.Value).Sum();
                var longSinW2 = accuracyKspSpotValues.Where(x => x.Key == 240D).Select(x => x.Value).Sum();
                longSinAcc = 1D - longSinW1 / longSinW2;
            }

            var midSinAcc = new double?();
            if (accuracyKspDiffValues.Where(x => x.Key == 120D).Count() > 0 && accuracyKspDiffValues.Where(x => x.Key == 120D).Count() > 0)
            {
                var midSinW1 = accuracyKspDiffValues.Where(x => x.Key == 120D).Select(x => x.Value).Sum();
                var midSinW2 = accuracyKspSpotValues.Where(x => x.Key == 120D).Select(x => x.Value).Sum();
                midSinAcc = 1D - midSinW1 / midSinW2;
            }


            var shortSinAcc = new double?();
            if (accuracyKspDiffValues.Where(x => x.Key == 30D).Count() > 0 && accuracyKspDiffValues.Where(x => x.Key == 30D).Count() > 0)
            {
                var shortSinW1 = accuracyKspDiffValues.Where(x => x.Key == 30D).Select(x => x.Value).Sum();
                var shortSinW2 = accuracyKspSpotValues.Where(x => x.Key == 30D).Select(x => x.Value).Sum();
                shortSinAcc = 1D - shortSinW1 / shortSinW2;
            }
            else
            {
                shortSinAcc = null;
            }

            var accuracyCalculations = new Dictionary<string, double?>();
            accuracyCalculations.Add("Long", longSinAcc);
            accuracyCalculations.Add("Mid", midSinAcc);
            accuracyCalculations.Add("Short", shortSinAcc);

            return new PursuitGainCalculations {Gains = gainCalculations, Accuracies= accuracyCalculations,
                FilteredControlWindows = filteredControlWindows };

        }


        public static double CountAccuracy(SpotGazeFileData fileData)
        {
            //accuracy
            //start from second max/ min to one before last max / min
            // kspc=ksp(2:(length(ksp)-1));
            //spotc = spot - srod;
            //leyec = leye - srod;
            //reyec = reye - srod;
            //le = length(kspc);
            // w1L = sum(abs(leyec(kspc(1):kspc(le))'- spotc(kspc(1):kspc(le))'));
            //% integral of diff
            //w2 = sum(abs(spotc(kspc(1):kspc(le)))); % integral of abs sin
            //   accL = 1 - w1L / w2;

            var mncal = fileData.Spot.Min();
            var mxcal = fileData.Spot.Max();
            var amp = (mxcal - mncal) / 2;
            var srod = (mxcal + mncal) / 2;

            var oneIndexes = new List<int>();
            var kspIndexes = new List<int>();
            //var kspEyeValues = new List<double>();
            var kspSpotValues = new List<double>();
            var kspDiffValues = new List<double>();

           
            // kspc=ksp(2:(length(ksp)-1));
            for (int i = 1; i < fileData.Spot.Length-1; i++)
            {
                var xSpot = fileData.Spot[i];
                if (Math.Abs(xSpot - srod) > amp * 0.995D)
                {
                    var xEye = fileData.Eye[i];
                    var val = Math.Abs(xEye - xSpot);
                    kspDiffValues.Add(val);
                    kspSpotValues.Add(Math.Abs(xSpot));
                    //kspEyeValues.Add(Math.Abs(fileData.Eye[i]));
                }
            }

            var w1 = kspDiffValues.Sum();
            var w2 = kspSpotValues.Sum();
            var acc = 1 - w1 / w2;
            return acc;

        }

        public static double CountPursoitGain(double[] eye, double[] spot)
        {
            var spotOnScreenDistance = SaccadeDataHelper.CountOnScreenDistance(spot.ToArray()).Sum();
            var eyeOnScreenDistance = SaccadeDataHelper.CountOnScreenDistance(eye.ToArray()).Sum();
            return eyeOnScreenDistance / spotOnScreenDistance;
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
            //var eyeAmpProp = calcConfig.EyeAmpProp;
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
            //var rightEyeOverMeanSpotAmplitudeIndexes = GetIndexesOverAmplitude(shiftedEyeCoords, eyeCoords, meanSpotAmplitude, eyeAmpProp);
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
            //var earliestEyeOverAmpForSpotOverAmpIndexes = NarrowList(rightEyeOverMeanSpotAmplitudeIndexes, lowestEyeOverAmpIndxesForSpotIndexes);
           
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
