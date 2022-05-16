using BUDDY.Helper;
using BUDDY.MS2LibrarySearch;
using BUDDY.RawData;
using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BUDDY.RawDataHandlerCommon
{
    public sealed class MS2Clustering
    {
        public MS2Clustering() { } 

        public static double DotProduct(List<RAW_PeakElement> x, List<RAW_PeakElement> y, double Ms2mzTol, bool ppm)
        {
            double DpScoreTop = 0;
            Ms2mzTol += 1e-6; // double type comparison
            List<int> xMatchedFragIndex = new List<int>();
            List<int> yMatchedFragIndex = new List<int>();

            if (x == null || y == null || x.Count == 0 || y.Count == 0)
            {
                return 0;
            }

            // save as list of tuples
            List<RAW_PeakElement> x_MS2 = new List<RAW_PeakElement>();
            List<RAW_PeakElement> y_MS2 = new List<RAW_PeakElement>();

            // normalize
            double xMaxInt = x.Max(t => t.Intensity);
            double yMaxInt = y.Max(t => t.Intensity);

            double x_SumIntSquare = 0;
            double y_SumIntSquare = 0;

            for (int i = 0; i < x.Count; i++)
            {
                x_MS2.Add(new RAW_PeakElement() { Mz = x[i].Mz, Intensity = 100.0 * x[i].Intensity / xMaxInt });
                x_SumIntSquare += Math.Pow(x_MS2[i].Intensity, 2);
            }
            for (int i = 0; i < y.Count; i++)
            {
                y_MS2.Add(new RAW_PeakElement() { Mz = y[i].Mz, Intensity = 100.0 * y[i].Intensity / yMaxInt });
                y_SumIntSquare += Math.Pow(y_MS2[i].Intensity, 2);
            }

            List<double> x_alignedInt = new List<double>();
            List<double> y_alignedInt = new List<double>();


            List<double[]> productArray = new List<double[]>();
            for (int i = 0; i < x_MS2.Count; i++)
            {
                productArray.Add(new double[y_MS2.Count]);
            }

            double tmpMs2mzTol = Ms2mzTol;
            for (int i = 0; i < x_MS2.Count; i++)
            {
                if (ppm)
                {
                    tmpMs2mzTol = x_MS2[i].Mz * 1e-6 * Ms2mzTol;
                }
                for (int j = 0; j < y_MS2.Count; j++)
                {
                    // direct match
                    if (Math.Abs(y_MS2[j].Mz - x_MS2[i].Mz) <= tmpMs2mzTol)
                    {
                        productArray[i][j] = x_MS2[i].Intensity * y_MS2[j].Intensity;
                    }
                }
            }
            Solver solver = Solver.CreateSolver("SCIP");

            // binary[i, j] is an array of 0-1 variables, which will be 1 if assigned.
            Variable[,] binary = new Variable[x_MS2.Count, y_MS2.Count];
            for (int i = 0; i < x_MS2.Count; ++i)
            {
                for (int j = 0; j < y_MS2.Count; ++j)
                {
                    binary[i, j] = solver.MakeIntVar(0, 1, $"x_{i}_y_{j}");
                }
            }

            // Each worker is assigned to at most one task.
            for (int i = 0; i < x_MS2.Count; ++i)
            {
                Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(0, 1, "");
                for (int j = 0; j < y_MS2.Count; ++j)
                {
                    constraint.SetCoefficient(binary[i, j], 1);
                }
            }
            for (int j = 0; j < y_MS2.Count; ++j)
            {
                Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(0, 1, "");
                for (int i = 0; i < x_MS2.Count; ++i)
                {
                    constraint.SetCoefficient(binary[i, j], 1);
                }
            }
            Objective objective = solver.Objective();
            for (int i = 0; i < x_MS2.Count; ++i)
            {
                for (int j = 0; j < y_MS2.Count; ++j)
                {
                    objective.SetCoefficient(binary[i, j], productArray[i][j]);
                }
            }
            objective.SetMaximization();
            Solver.ResultStatus resultStatus = solver.Solve();

            DpScoreTop = solver.Objective().Value();

            double DpScore = DpScoreTop / Math.Sqrt(x_SumIntSquare * y_SumIntSquare);


            return DpScore;

        }

        public static double FindMzInMS1(List<RAW_PeakElement> MS1, double Mz, double MS1MzTol, bool ppm) //return the MS1 intensity of searched mz
        {
            MS1MzTol += 1e-6;

            if (MS1.Count == 0)
            {
                return 0.0;
            }

            double tmpMzDiff = MS1MzTol;
            if (ppm)
            {
                tmpMzDiff = Mz * MS1MzTol * 1e-6;
            }

            double MS1Int = 0.0;
            for (int j = 0; j < MS1.Count; j++)
            {
                if (Math.Abs(MS1[j].Mz - Mz) <= tmpMzDiff) // Precursor ion, the closest ion
                {
                    tmpMzDiff = Math.Abs(MS1[j].Mz - Mz);
                    MS1Int = MS1[j].Intensity;
                }
            }

            return MS1Int;
        }

        public static List<RAW_PeakElement> FindMS1Isotope(List<RAW_PeakElement> MS1, double PrecursorMz, double isotopeBinMzTol,
            int MaxIsotopeNo, double IsotopeCutoff, bool ppm) // IsotopeCutoff (X %)
        {

            if (MS1.Count == 0)
            {
                return new List<RAW_PeakElement>();
            }

            List<RAW_PeakElement> MS1Isotope = new List<RAW_PeakElement>();


            double tmpMzDiff = isotopeBinMzTol;
            if (ppm)
            {
                tmpMzDiff = PrecursorMz * isotopeBinMzTol * 1e-6;
            }
            int index = 0;
            bool M0 = false;
            for (int j = 0; j < MS1.Count; j++)
            {
                if (Math.Abs(MS1[j].Mz - PrecursorMz) <= tmpMzDiff) // Precursor ion, the closest ion
                {
                    tmpMzDiff = Math.Abs(MS1[j].Mz - PrecursorMz);
                    index = j;
                    M0 = true;
                }
            }
            if (M0 == false)
            {
                return new List<RAW_PeakElement>();
            }
            MS1Isotope.Add(new RAW_PeakElement() { Mz = MS1[index].Mz, Intensity = MS1[index].Intensity });
            double M0Intensity = MS1[index].Intensity;


            //// find other isotopic peaks
            //for (int i = 1; i < MaxIsotopeNo; i++)
            //{
            //    bool M = false;
            //    int Mindex = 0;
            //    double tmpInt = 0;
            //    tmpMzDiff = MS1MzTol;
            //    if (ppm)
            //    {
            //        tmpMzDiff = (PrecursorMz + 1.003355 * i) * MS1MzTol * 1e-6;
            //    }
            //    for (int j = index; j < MS1.Count; j++)
            //    {
            //        if (Math.Abs(MS1[j].Mz - (PrecursorMz + 1.003355 * i)) <= tmpMzDiff &&
            //            (100.0 * MS1[j].Intensity / M0Intensity) >= IsotopeCutoff &&
            //            MS1[j].Intensity > tmpInt) // Precursor ion, within the mz window, choose the highest ion, isotope cutoff
            //        {
            //            M = true;
            //            Mindex = j;
            //            tmpInt = MS1[j].Intensity;
            //            break;
            //        }
            //    }
            //    if (M == false)
            //    {
            //        break;
            //    }
            //    else
            //    {
            //        MS1Isotope.Add(new RAW_PeakElement() { Mz = MS1[Mindex].Mz, Intensity = MS1[Mindex].Intensity });
            //    }
            //}

            double tmpMz = PrecursorMz;
            tmpMzDiff = isotopeBinMzTol;
            for (int i = 0; i < MaxIsotopeNo; i++)
            {
                bool M = false;

                List<double> MzList = new List<double>();
                List<double> IntList = new List<double>();


                if (ppm)
                {
                    tmpMzDiff = (tmpMz + 1.003355) * isotopeBinMzTol * 1e-6;
                }
                for (int j = index; j < MS1.Count; j++)
                {
                    if (Math.Abs(MS1[j].Mz - (tmpMz + 1.003355)) <= tmpMzDiff) // include all the ions within the mz range
                    {
                        MzList.Add(MS1[j].Mz);
                        IntList.Add(MS1[j].Intensity);
                        M = true;
                    }
                }
                if (M == false)
                {
                    break;
                }
                else
                {    // for all the ions within the mz range, calculate weighted average m/z, sum intensity
                    double weightedSumMz = 0;
                    double sumInt = 0;
                    for (int m = 0; m < MzList.Count; m++)
                    {
                        weightedSumMz += MzList[m] * IntList[m];
                        sumInt += IntList[m];
                    }
                    tmpMz = weightedSumMz / sumInt;

                    if ((100.0 * sumInt / M0Intensity) >= IsotopeCutoff)
                    {
                        MS1Isotope.Add(new RAW_PeakElement() { Mz = tmpMz, Intensity = sumInt });
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return MS1Isotope;
        }

        public static List<RAW_PeakElement> MS2Merge(List<RAW_PeakElement> MS2_x, List<RAW_PeakElement> MS2_y, double x_fold,
            double y_fold, double MS2MzTol, bool ppm)
        {

            // normalize MS2 int.
            double maxMS2Xint = MS2_x.Max(o => o.Intensity);
            for (int i = 0; i < MS2_x.Count; i++)
            {
                RAW_PeakElement str = MS2_x[i];
                str.Intensity = 100.0 * MS2_x[i].Intensity / maxMS2Xint;
                MS2_x[i] = str;
            }

            double maxMS2Yint = MS2_y.Max(o => o.Intensity);
            for (int i = 0; i < MS2_y.Count; i++)
            {
                RAW_PeakElement str = MS2_y[i];
                str.Intensity = 100.0 * MS2_y[i].Intensity / maxMS2Yint;
                MS2_y[i] = str;
            }


            List<RAW_PeakElement> Merged_MS2 = new List<RAW_PeakElement>();
            for (int i = 0; i < MS2_x.Count; i++)
            {
                Merged_MS2.Add(new RAW_PeakElement() { Mz = MS2_x[i].Mz, Intensity = MS2_x[i].Intensity });
            }

            for (int i = 0; i < MS2_y.Count; i++)
            {
                double tmpMzDiff = MS2MzTol;
                if (ppm)
                {
                    tmpMzDiff = MS2MzTol * MS2_y[i].Mz * 1e-6;
                }
                int index = 0;
                bool match = false;

                for (int j = 0; j < Merged_MS2.Count; j++) // align to the merged MS2
                {
                    if (Math.Abs(Merged_MS2[j].Mz - MS2_y[i].Mz) <= tmpMzDiff) // the closest ion
                    {
                        tmpMzDiff = Math.Abs(Merged_MS2[j].Mz - MS2_y[i].Mz);
                        index = j;
                        match = true;
                    }
                }
                if (match == true)
                {
                    double new_mz = (Merged_MS2[index].Mz * x_fold + MS2_y[i].Mz * y_fold) / (x_fold + y_fold);
                    double new_int = (Merged_MS2[index].Intensity * x_fold + MS2_y[i].Intensity * y_fold) / (x_fold + y_fold);
                    Merged_MS2[index] = new RAW_PeakElement() { Mz = new_mz, Intensity = new_int };
                }
                else
                {
                    Merged_MS2.Add(new RAW_PeakElement() { Mz = MS2_y[i].Mz, Intensity = MS2_y[i].Intensity * y_fold / (x_fold + y_fold) });
                }
            }

            Merged_MS2.OrderBy(o => o.Mz);

            double maxInt = Merged_MS2.Max(o => o.Intensity);
            for (int i = 0; i < Merged_MS2.Count; i++)
            {
                Merged_MS2[i] = new RAW_PeakElement() { Mz = Merged_MS2[i].Mz, Intensity = 100.0 * Merged_MS2[i].Intensity / maxInt };
            }

            //RAW_PeakElement[] outMS2 = new RAW_PeakElement[Merged_MS2.Count];
            //for (int i = 0; i < outMS2.Length; i++)
            //{
            //    outMS2[i].Mz = Merged_MS2[i].Item1;
            //    outMS2[i].Intensity = Merged_MS2[i].Item2;
            //}

            return Merged_MS2;
        }

        public static List<MS2Group> MS2Cluster(RAW_Measurement file, double MS1_mztol, double MS2_mztol, bool ppm, bool MS2Merge_bool, double dpScoreThreshold,
             double maxPeakWidth, int maxIsotopeNo, double isotopeRelativeCutoff, double isotopeBinMzTol)
        {
            List<MS2Group> MS2GroupList = new List<MS2Group>();
            int tmpMS1ScanIndex = 0;
            for (int i = 0; i < file.SpectrumList.Count; i++)
            {
                if (file.SpectrumList[i].MsLevel == 1)
                {
                    tmpMS1ScanIndex = i; // record the latest MS1 scan index for isotope retrieval
                }

                if (file.SpectrumList[i].MsLevel == 2)
                {
                    RAW_Spectrum tmpMSscan = file.SpectrumList[i]; // this MS2 scan

                    List<RAW_PeakElement> tmpMS2 = new List<RAW_PeakElement>(tmpMSscan.Spectrum.ToList()); //MS2 spectrum list

                    if (tmpMS2.Count == 0)
                    {
                        continue;
                    }

                    double tmpScanTime = tmpMSscan.ScanStartTime; //RT
                    if (tmpMSscan.ScanStartTimeUnit.ToString() == "Minute")
                    {
                        tmpScanTime = tmpMSscan.ScanStartTime * 60;
                    }

                    double tmpPrecursorMz = tmpMSscan.Precursor.SelectedIonMz; //Premz
                    if (ppm == true)
                    {
                        MS1_mztol = MS1_mztol * tmpPrecursorMz * 1e-6;
                    }

                    if (MS2GroupList.Count > 0)
                    {
                        bool MS2inList = false;
                        for (int j = 0; j < MS2GroupList.Count; j++)
                        {
                            if (Math.Abs(MS2GroupList[j].PrecursorMz - tmpPrecursorMz) <= MS1_mztol &&
                                Math.Abs(tmpScanTime - MS2GroupList[j].RTmin) <= maxPeakWidth) // first check Precursor Mz, then max peak width (RT range)
                            {
                                List<RAW_PeakElement> MS2ForThisMS2Group = new List<RAW_PeakElement>();
                                if (MS2Merge_bool)
                                {
                                    MS2ForThisMS2Group = MS2GroupList[j].MergedMS2;
                                }
                                else
                                {
                                    MS2ForThisMS2Group = MS2GroupList[j].MostAbundantMS2;
                                }

                                if (DotProduct(MS2ForThisMS2Group, tmpMS2, MS2_mztol, ppm) >= dpScoreThreshold)
                                {
                                    MS2inList = true;
                                    MS2GroupList[j].RTmax = tmpScanTime;
                                    MS2GroupList[j].MS2ScanIndex.Add(i);

                                    double tmpMS1Intensity = FindMzInMS1(file.SpectrumList[tmpMS1ScanIndex].Spectrum.ToList(), tmpPrecursorMz, MS1_mztol, ppm);
                                    if (tmpMS1Intensity > MS2GroupList[j].HighestPrecursorIntensity) // precursor intensity comparison
                                    {
                                        MS2GroupList[j].MS1ScanIndex = tmpMS1ScanIndex;
                                        MS2GroupList[j].MostAbundantMS2 = tmpMS2;
                                        MS2GroupList[j].HighestPrecursorIntensity = tmpMS1Intensity;
                                        MS2GroupList[j].HighestPreIntMS2ScanIndex = i;
                                        MS2GroupList[j].MS1Isotope = FindMS1Isotope(file.SpectrumList[tmpMS1ScanIndex].Spectrum.ToList(), tmpPrecursorMz, isotopeBinMzTol, maxIsotopeNo, isotopeRelativeCutoff, false);
                                    }
                                    if (MS2Merge_bool)
                                    {
                                        MS2GroupList[j].MergedMS2 = MS2Merge(MS2GroupList[j].MergedMS2, tmpMS2, MS2GroupList[j].MS2ScanIndex.Count, 1, MS2_mztol, ppm);
                                    }
                                    break;
                                }
                            }
                        }
                        if (MS2inList == false)
                        {
                            // add a new one to MS2GroupList
                            MS2GroupList.Add(new MS2Group()
                            {
                                RTmin = tmpScanTime,
                                RTmax = tmpScanTime,
                                MS1ScanIndex = tmpMS1ScanIndex,
                                MS2ScanIndex = new List<int>() { i },
                                PrecursorMz = tmpPrecursorMz,
                                MostAbundantMS2 = tmpMS2,
                                MergedMS2 = tmpMS2,
                                HighestPreIntMS2ScanIndex = i,
                                MS1Isotope = FindMS1Isotope(file.SpectrumList[tmpMS1ScanIndex].Spectrum.ToList(), tmpPrecursorMz, isotopeBinMzTol, maxIsotopeNo, isotopeRelativeCutoff, false),
                                HighestPrecursorIntensity = FindMzInMS1(file.SpectrumList[tmpMS1ScanIndex].Spectrum.ToList(), tmpPrecursorMz, MS1_mztol, ppm)
                            });
                        }
                    }
                    else
                    {
                        MS2GroupList.Add(new MS2Group()
                        {
                            RTmin = tmpScanTime,
                            RTmax = tmpScanTime,
                            MS1ScanIndex = tmpMS1ScanIndex,
                            MS2ScanIndex = new List<int>() { i },
                            PrecursorMz = tmpPrecursorMz,
                            MostAbundantMS2 = tmpMS2,
                            MergedMS2 = tmpMS2,
                            HighestPreIntMS2ScanIndex = i,
                            MS1Isotope = FindMS1Isotope(file.SpectrumList[tmpMS1ScanIndex].Spectrum.ToList(), tmpPrecursorMz, isotopeBinMzTol, maxIsotopeNo, isotopeRelativeCutoff, false),
                            HighestPrecursorIntensity = FindMzInMS1(file.SpectrumList[tmpMS1ScanIndex].Spectrum.ToList(), tmpPrecursorMz, MS1_mztol, ppm)
                        });
                    }
                }
            }

            return MS2GroupList;
        } // maxPeakWidth (seconds)

        public static MS2CompareResult DotProduct_MS2Compare(List<RAW_PeakElement> x, List<RAW_PeakElement> y, double Ms2mzTol, bool ppm)
        {
            double DpScoreTop = 0;
            Ms2mzTol += 1e-6; // double type comparison
            List<int> xMatchedFragIndex = new List<int>();
            List<int> yMatchedFragIndex = new List<int>();

            if (x == null || y == null || x.Count == 0 || y.Count == 0)
            {
                return null;
            }

            // save as list of tuples
            List<RAW_PeakElement> x_MS2 = new List<RAW_PeakElement>();
            List<RAW_PeakElement> y_MS2 = new List<RAW_PeakElement>();

            // normalize
            double xMaxInt = x.Max(t => t.Intensity);
            double yMaxInt = y.Max(t => t.Intensity);

            double x_SumIntSquare = 0;
            double y_SumIntSquare = 0;

            for (int i = 0; i < x.Count; i++)
            {
                x_MS2.Add(new RAW_PeakElement() { Mz = x[i].Mz, Intensity = 100.0 * x[i].Intensity / xMaxInt });
                x_SumIntSquare += Math.Pow(x_MS2[i].Intensity, 2);
            }
            for (int i = 0; i < y.Count; i++)
            {
                y_MS2.Add(new RAW_PeakElement() { Mz = y[i].Mz, Intensity = 100.0 * y[i].Intensity / yMaxInt });
                y_SumIntSquare += Math.Pow(y_MS2[i].Intensity, 2);
            }

            List<double> x_alignedInt = new List<double>();
            List<double> y_alignedInt = new List<double>();


            List<double[]> productArray = new List<double[]>();
            for (int i = 0; i < x_MS2.Count; i++)
            {
                productArray.Add(new double[y_MS2.Count]);
            }

            double tmpMs2mzTol = Ms2mzTol;
            for (int i = 0; i < x_MS2.Count; i++)
            {
                if (ppm)
                {
                    tmpMs2mzTol = x_MS2[i].Mz * 1e-6 * Ms2mzTol;
                }
                for (int j = 0; j < y_MS2.Count; j++)
                {
                    // direct match
                    if (Math.Abs(y_MS2[j].Mz - x_MS2[i].Mz) <= tmpMs2mzTol)
                    {
                        productArray[i][j] = x_MS2[i].Intensity * y_MS2[j].Intensity;
                    }
                }
            }
            Solver solver = Solver.CreateSolver("SCIP");

            // binary[i, j] is an array of 0-1 variables, which will be 1 if assigned.
            Variable[,] binary = new Variable[x_MS2.Count, y_MS2.Count];
            for (int i = 0; i < x_MS2.Count; ++i)
            {
                for (int j = 0; j < y_MS2.Count; ++j)
                {
                    binary[i, j] = solver.MakeIntVar(0, 1, $"x_{i}_y_{j}");
                }
            }

            // Each worker is assigned to at most one task.
            for (int i = 0; i < x_MS2.Count; ++i)
            {
                Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(0, 1, "");
                for (int j = 0; j < y_MS2.Count; ++j)
                {
                    constraint.SetCoefficient(binary[i, j], 1);
                }
            }
            for (int j = 0; j < y_MS2.Count; ++j)
            {
                Google.OrTools.LinearSolver.Constraint constraint = solver.MakeConstraint(0, 1, "");
                for (int i = 0; i < x_MS2.Count; ++i)
                {
                    constraint.SetCoefficient(binary[i, j], 1);
                }
            }
            Objective objective = solver.Objective();
            for (int i = 0; i < x_MS2.Count; ++i)
            {
                for (int j = 0; j < y_MS2.Count; ++j)
                {
                    objective.SetCoefficient(binary[i, j], productArray[i][j]);
                }
            }
            objective.SetMaximization();
            Solver.ResultStatus resultStatus = solver.Solve();

            DpScoreTop = solver.Objective().Value();

            double DpScore = DpScoreTop / Math.Sqrt(x_SumIntSquare * y_SumIntSquare);

            for (int i = 0; i < x_MS2.Count; i++)
            {
                for (int j = 0; j < y_MS2.Count; j++)
                {
                    if (binary[i, j].SolutionValue() > 0.5)
                    {
                        xMatchedFragIndex.Add(i);
                        yMatchedFragIndex.Add(j);
                        x_alignedInt.Add(x_MS2[i].Intensity);
                        y_alignedInt.Add(y_MS2[j].Intensity);
                    }
                }
            }

            for (int i = 0; i < x_MS2.Count; i++)
            {
                x_MS2[i] = new RAW_PeakElement { Mz = Math.Round(x_MS2[i].Mz, 4), Intensity = Math.Round(x_MS2[i].Intensity, 1) };
            }

            for (int i = 0; i < y_MS2.Count; i++)
            {
                y_MS2[i] = new RAW_PeakElement { Mz = Math.Round(y_MS2[i].Mz, 4), Intensity = Math.Round(y_MS2[i].Intensity, 1) };
            }

            MS2CompareResult output = new MS2CompareResult
            {
                Score = DpScore,
                MatchNumber = x_alignedInt.Count,
                xMatchFragIndex = xMatchedFragIndex,
                yMatchFragIndex = yMatchedFragIndex,
                NormMS2x = x_MS2,
                NormMS2y = y_MS2
            };

            return output;

        }
    }
}
