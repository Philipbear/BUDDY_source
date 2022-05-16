using BUDDY.RawData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BUDDY.RawDataHandlerCommon
{
    public sealed class SpectrumParser
    {
        private SpectrumParser()
        {
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          Dictionary<int, double[]> accumulatedMassBin)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            foreach (KeyValuePair<int, double[]> keyValuePair in accumulatedMassBin)
            {
                int key = keyValuePair.Key;
                double num7 = keyValuePair.Value[0];
                double num8 = keyValuePair.Value[1];
                double num9 = keyValuePair.Value[2];
                num3 += num8;
                if (num8 > num1)
                {
                    num1 = num8;
                    num2 = num7;
                }
                if (num4 > num7)
                    num4 = num7;
                if (num5 < num7)
                    num5 = num7;
                if (num6 > num8)
                    num6 = num8;
                RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                {
                    Mz = Math.Round(num7, 5),
                    Intensity = Math.Round(num8, 0)
                };
                source.Add(rawPeakElement);
            }
            List<RAW_PeakElement> list = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToList<RAW_PeakElement>();
            spectrum.Spectrum = list.ToArray();
            spectrum.DefaultArrayLength = list.Count<RAW_PeakElement>();
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          double[] accumulatedMassIntensityArray)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < accumulatedMassIntensityArray.Length; ++index)
            {
                if (accumulatedMassIntensityArray[index] >= 1.0)
                {
                    double num7 = (double)index * 1E-05;
                    double accumulatedMassIntensity = accumulatedMassIntensityArray[index];
                    num3 += accumulatedMassIntensity;
                    if (accumulatedMassIntensity > num1)
                    {
                        num1 = accumulatedMassIntensity;
                        num2 = num7;
                    }
                    if (num4 > num7)
                        num4 = num7;
                    if (num5 < num7)
                        num5 = num7;
                    if (num6 > accumulatedMassIntensity)
                        num6 = accumulatedMassIntensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round(num7, 5),
                        Intensity = Math.Round(accumulatedMassIntensity, 0)
                    };
                    source.Add(rawPeakElement);
                }
            }
            spectrum.Spectrum = source.ToArray();
            spectrum.DefaultArrayLength = source.Count<RAW_PeakElement>();
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          double[] masses,
          double[] intensities,
          double peakCutOff,
          ref double[] accumulatedMassIntensityArray)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Length; ++index)
            {
                double mass = masses[index];
                double intensity = intensities[index];
                if (intensity >= peakCutOff)
                {
                    num3 += intensity;
                    if (intensity > num1)
                    {
                        num1 = intensity;
                        num2 = mass;
                    }
                    if (num4 > mass)
                        num4 = mass;
                    if (num5 < mass)
                        num5 = mass;
                    if (num6 > intensity)
                        num6 = intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round(mass, 5),
                        Intensity = Math.Round(intensity, 0)
                    };
                    source.Add(rawPeakElement);
                    if (spectrum.MsLevel == 1)
                        accumulatedMassIntensityArray[(int)(mass * 100000.0)] += intensity;
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          double[] masses,
          double[] intensities,
          double peakCutOff,
          Dictionary<int, double[]> accumulatedMassBin)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Length; ++index)
            {
                double mass = masses[index];
                double intensity = intensities[index];
                if (intensity >= peakCutOff)
                {
                    num3 += intensity;
                    if (intensity > num1)
                    {
                        num1 = intensity;
                        num2 = mass;
                    }
                    if (num4 > mass)
                        num4 = mass;
                    if (num5 < mass)
                        num5 = mass;
                    if (num6 > intensity)
                        num6 = intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round(mass, 5),
                        Intensity = Math.Round(intensity, 0)
                    };
                    source.Add(rawPeakElement);
                    if (spectrum.MsLevel == 1)
                        SpectrumParser.AddToMassBinDictionary(accumulatedMassBin, mass, intensity);
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void AddToMassBinDictionary(
          Dictionary<int, double[]> accumulatedMassBin,
          double mass,
          double intensity)
        {
            int key = (int)(mass * 1000.0);
            if (!accumulatedMassBin.ContainsKey(key))
            {
                accumulatedMassBin[key] = new double[3]
                {
          mass,
          intensity,
          intensity
                };
            }
            else
            {
                accumulatedMassBin[key][1] += intensity;
                if (accumulatedMassBin[key][2] >= intensity)
                    return;
                accumulatedMassBin[key][0] = mass;
                accumulatedMassBin[key][2] = intensity;
            }
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          List<double> masses,
          List<double> intensities,
          double peakCutOff,
          ref double[] accumulatedMassIntensityArray)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Count; ++index)
            {
                double mass = masses[index];
                double intensity = intensities[index];
                if (intensity >= peakCutOff)
                {
                    num3 += intensity;
                    if (intensity > num1)
                    {
                        num1 = intensity;
                        num2 = mass;
                    }
                    if (num4 > mass)
                        num4 = mass;
                    if (num5 < mass)
                        num5 = mass;
                    if (num6 > intensity)
                        num6 = intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round(mass, 5),
                        Intensity = Math.Round(intensity, 0)
                    };
                    source.Add(rawPeakElement);
                    if (spectrum.MsLevel == 1)
                        accumulatedMassIntensityArray[(int)(mass * 100000.0)] += intensity;
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          List<double> masses,
          List<double> intensities,
          double peakCutOff,
          Dictionary<int, double[]> accumulatedMassBin)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Count; ++index)
            {
                double mass = masses[index];
                double intensity = intensities[index];
                if (intensity >= peakCutOff)
                {
                    num3 += intensity;
                    if (intensity > num1)
                    {
                        num1 = intensity;
                        num2 = mass;
                    }
                    if (num4 > mass)
                        num4 = mass;
                    if (num5 < mass)
                        num5 = mass;
                    if (num6 > intensity)
                        num6 = intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round(mass, 5),
                        Intensity = Math.Round(intensity, 0)
                    };
                    source.Add(rawPeakElement);
                    if (spectrum.MsLevel == 1)
                        SpectrumParser.AddToMassBinDictionary(accumulatedMassBin, mass, intensity);
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          float[] masses,
          float[] intensities,
          double peakCutOff,
          ref double[] accumulatedMassIntensityArray)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Length; ++index)
            {
                float mass = masses[index];
                float intensity = intensities[index];
                if ((double)intensity >= peakCutOff)
                {
                    num3 += (double)intensity;
                    if ((double)intensity > num1)
                    {
                        num1 = (double)intensity;
                        num2 = (double)mass;
                    }
                    if (num4 > (double)mass)
                        num4 = (double)mass;
                    if (num5 < (double)mass)
                        num5 = (double)mass;
                    if (num6 > (double)intensity)
                        num6 = (double)intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round((double)mass, 5),
                        Intensity = Math.Round((double)intensity, 0)
                    };
                    source.Add(rawPeakElement);
                    if (spectrum.MsLevel == 1)
                        accumulatedMassIntensityArray[(int)((double)mass * 100000.0)] += (double)intensity;
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          float[] masses,
          float[] intensities,
          double peakCutOff,
          Dictionary<int, double[]> accumulatedMassBin)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Length; ++index)
            {
                float mass = masses[index];
                float intensity = intensities[index];
                if ((double)intensity >= peakCutOff)
                {
                    num3 += (double)intensity;
                    if ((double)intensity > num1)
                    {
                        num1 = (double)intensity;
                        num2 = (double)mass;
                    }
                    if (num4 > (double)mass)
                        num4 = (double)mass;
                    if (num5 < (double)mass)
                        num5 = (double)mass;
                    if (num6 > (double)intensity)
                        num6 = (double)intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round((double)mass, 5),
                        Intensity = Math.Round((double)intensity, 0)
                    };
                    source.Add(rawPeakElement);
                    if (spectrum.MsLevel == 1)
                        SpectrumParser.AddToMassBinDictionary(accumulatedMassBin, (double)mass, (double)intensity);
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          float[] masses,
          float[] intensities,
          double peakCutOff)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Length; ++index)
            {
                float mass = masses[index];
                float intensity = intensities[index];
                if ((double)intensity >= peakCutOff)
                {
                    num3 += (double)intensity;
                    if ((double)intensity > num1)
                    {
                        num1 = (double)intensity;
                        num2 = (double)mass;
                    }
                    if (num4 > (double)mass)
                        num4 = (double)mass;
                    if (num5 < (double)mass)
                        num5 = (double)mass;
                    if (num6 > (double)intensity)
                        num6 = (double)intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round((double)mass, 5),
                        Intensity = Math.Round((double)intensity, 0)
                    };
                    source.Add(rawPeakElement);
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }

        public static void setSpectrumProperties(
          RAW_Spectrum spectrum,
          List<double> masses,
          List<double> intensities,
          double peakCutOff)
        {
            double num1 = 0.0;
            double num2 = 0.0;
            double num3 = 0.0;
            double num4 = double.MaxValue;
            double num5 = double.MinValue;
            double num6 = double.MaxValue;
            List<RAW_PeakElement> source = new List<RAW_PeakElement>();
            for (int index = 0; index < masses.Count; ++index)
            {
                double mass = masses[index];
                double intensity = intensities[index];
                if (intensity >= peakCutOff)
                {
                    num3 += intensity;
                    if (intensity > num1)
                    {
                        num1 = intensity;
                        num2 = mass;
                    }
                    if (num4 > mass)
                        num4 = mass;
                    if (num5 < mass)
                        num5 = mass;
                    if (num6 > intensity)
                        num6 = intensity;
                    RAW_PeakElement rawPeakElement = new RAW_PeakElement()
                    {
                        Mz = Math.Round(mass, 5),
                        Intensity = Math.Round(intensity, 0)
                    };
                    source.Add(rawPeakElement);
                }
            }
            spectrum.Spectrum = source.OrderBy<RAW_PeakElement, double>((Func<RAW_PeakElement, double>)(n => n.Mz)).ToArray<RAW_PeakElement>();
            spectrum.DefaultArrayLength = source.Count;
            spectrum.BasePeakIntensity = num1;
            spectrum.BasePeakMz = num2;
            spectrum.TotalIonCurrent = num3;
            spectrum.LowestObservedMz = num4;
            spectrum.HighestObservedMz = num5;
            spectrum.MinIntensity = num6;
        }
    }
}
