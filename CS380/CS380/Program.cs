using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CS380.VotingRules;
using System.Diagnostics;
using System.Threading;

namespace CS380
{
    public class Program
    {
        static void Main(string[] args)
        {
            /*
            List<string> values = new List<string>();

            values.Add("A");
            values.Add("B");
            values.Add("C");
            values.Add("D");
            values.Add("E");

            Multiset<string> mset = new HashMultiset<string>();
            
            while (true)
            {
                bool moreResults = mset.NextMultiset(values, 3);

                Console.WriteLine(mset.ToString());

                if (!moreResults)
                {
                    break;
                }
            }

            foreach (var x in Permute(values))
            {
                foreach (var y in x)
                {
                    Console.Write(y);
                    Console.Write(" ");
                }

                Console.WriteLine();
            }
            */

            /*
            var mc = Multiset<int>.Multichoose(3, 4);

            var lst = new List<int>();

            for (int i = 0; i < 10; i++)
            {
                lst.Add(i);
            }

            for (int i = 0; i < 100; i++)
            {
                var mSet1 = Multiset<int>.SampleMultisetUrn(lst, 100, 1.0);

                Console.WriteLine(mSet1);
            }
             */

            //ComputeNewZealandWithPreferenceVariance3("nzsim_3", false, 0.18, 0.12, 5, 0);
            //ComputeNewZealandWithPreferenceVariance();
            //ComputeNewZealand();

            //while (true)
            //{
            //    Stopwatch watch = new Stopwatch();
            //    watch.Start();
            //    ComputeStatistical();
            //    watch.Stop();
            //    Console.WriteLine(watch.ElapsedMilliseconds + " ms");
            //}

            //ComputeSTVCompare();

            //ComputeCompareSTV_PropLocal();

            //ValidateSpatialModelPCAHypothesis2();

            TestApportionment();

            //var nzPCA = CalculateNZPCAValues();
            //EstimateNZSpatialParams(nzPCA);

            Console.WriteLine("Complete");
            Console.ReadLine();
        }

        private static void TestApportionment()
        {
            while (true)
            {
                var situation = new VotingSituation();
                situation.PartyCount = 8;

                var society = new SpatialArtificialSociety();
                society.PartyCount = situation.PartyCount;
                society.Dimensions = new double[] { 1.0, 1.0 };
                society.DistrictMagnitude = 1;
                society.ElectorateCount = 1;

                society.SetupElection();
                for (int electorateIndex = 0; electorateIndex < society.ElectorateCount; electorateIndex++)
                {
                    var electorate = new ElectorateVotes();

                    electorate.Magnitude = 100;

                    electorate.VoteCounts = society.SampleElectorate(1000);

                    situation.Electorates.Add(electorate);
                }

                var rule1 = new ProportionalByDistrictVotingRule() { Apportionment = ApportionmentMethod.StLague };
                var rule2 = new ProportionalByDistrictVotingRule() { Apportionment = ApportionmentMethod.Hamilton };
                var rule3 = new ProportionalByDistrictVotingRule() { Apportionment = ApportionmentMethod.Jefferson };
                var rule4 = new ProportionalByDistrictVotingRule() { Apportionment = ApportionmentMethod.Hill };

                List<List<int>> results = new List<List<int>>();

                results.Add(rule1.CalculateResults(situation));
                results.Add(rule2.CalculateResults(situation));
                results.Add(rule3.CalculateResults(situation));
                results.Add(rule4.CalculateResults(situation));

                bool isSame = true;

                for (int j = 0; j < results[0].Count; j++)
                {
                    var compareTo = results[0][j];

                    for (int i = 1; i < results.Count; i++)
                    {
                        if (results[i][j] != compareTo)
                        {
                            isSame = false;
                        }
                    }
                }
                Console.WriteLine(".");
            }
        }

        #region "Spatial Model Validation"

        private class SpatialModelHypothesisTestResult
        {
            public List<double> ComponentVars { get; set; }
            public List<double> PCAResults { get; set; }
        }

        private static void ValidateSpatialModelPCAHypothesis2()
        {
            var numberOfComponents = 8;

            var strmReport = System.IO.File.OpenWrite("out/" + "pcavalidation2" + ".csv");
            strmReport.SetLength(0);

            var report = new CSVReportWriter<SpatialModelHypothesisTestResult>(new CSVWriter(strmReport));

            for (int i = 0; i < numberOfComponents; i++)
            {
                int i2 = i;
                report.AddColumn("spatialvar" + (i2 + 1), (SpatialModelHypothesisTestResult simR) => { return simR.ComponentVars[i2].ToString(); });
                report.AddColumn("pcavar" + (i2 + 1), (SpatialModelHypothesisTestResult simR) => { return simR.PCAResults[i2].ToString(); });
            }

            for (int i = 0; i < 1000; i++)
            {
                var componentVarList = new List<double>();

                for (int j = 0; j < numberOfComponents; j++)
                {
                    componentVarList.Add(0.0);
                }

                componentVarList[0] = 1.0;
                componentVarList[1] = Rand.NextDouble();

                var componentTotal = componentVarList.Sum();

                componentVarList = (from x in componentVarList orderby x descending select x / componentTotal).ToList();

                var simResults = ComputeSpatialSimulation(componentVarList);

                var result = new SpatialModelHypothesisTestResult();

                result.ComponentVars = componentVarList;
                result.PCAResults = simResults.VarExplainedByComponentOfFirstPrefScores.ToList();

                report.WriteLine(result);

                Console.WriteLine("Simulation " + i + " complete");
            }
        }


        private static void ValidateSpatialModelPCAHypothesis()
        {
            var numberOfComponents = 8;

            var strmReport = System.IO.File.OpenWrite("out/" + "pcavalidation" + ".csv");
            strmReport.SetLength(0);

            var report = new CSVReportWriter<SpatialModelHypothesisTestResult>(new CSVWriter(strmReport));

            for (int i = 0; i < numberOfComponents; i++)
            {
                int i2 = i;
                report.AddColumn("spatialvar" + (i2 + 1), (SpatialModelHypothesisTestResult simR) => { return simR.ComponentVars[i2].ToString(); });
                report.AddColumn("pcavar" + (i2 + 1), (SpatialModelHypothesisTestResult simR) => { return simR.PCAResults[i2].ToString(); });
            }

            for (int i = 0; i < 1000; i++)
            {
                var componentVarList = new List<double>();

                for (int j = 0; j < numberOfComponents; j++)
                {
                    componentVarList.Add((1.0 - componentVarList.Sum()) * Rand.NextDouble());
                }

                componentVarList = (from x in componentVarList orderby x descending select x).ToList();

                var simResults = ComputeSpatialSimulation(componentVarList);

                var result = new SpatialModelHypothesisTestResult();

                result.ComponentVars = componentVarList;
                result.PCAResults = simResults.VarExplainedByComponentOfFirstPrefScores.ToList();

                report.WriteLine(result);

                Console.WriteLine("Simulation " + i + " complete");
            }
        }

        private static double[] CalculateNZPCAValues()
        {
            List<List<string>> rows;

            using (var strm = System.IO.File.OpenRead("nz/2011electorate.csv"))
            {
                rows = CSVReader.ReadFromStream(strm).ToList();
            }

            var partyCount = rows.First().Count - 1;
            var electorateCount = rows.Count;

            double[][] data = new double[partyCount][];

            for (int i = 0; i < partyCount; i++)
            {
                data[i] = new double[electorateCount];

                for (int j = 0; j < electorateCount; j++)
                {
                    data[i][j] = double.Parse(rows[j][i + 1]);
                }
            }

            var m = PrincipalComponentAnalysis.CalculateMatrix(data, electorateCount);

            return m.VarianceExplainedByComponent;
        }

        private class SpatialModelNZParamEstimationResult
        {
            public List<SimulationResults> SimResult { get; set; }
            public List<double> Dislikeness { get; set; }
            public List<double> Components { get; set; }
        }

        private static void EstimateNZSpatialParams(double[] nzPCA)
        {
            List<SpatialModelNZParamEstimationResult> hypResults = new List<SpatialModelNZParamEstimationResult>();

            for (int i = 0; i < 100; i++)
            {
                var componentVarList = new List<double>();

                for (int j = 0; j < 5; j++)
                {
                    componentVarList.Add((1.0 - componentVarList.Sum()) * Rand.NextDouble());
                }

                componentVarList = (from x in componentVarList orderby x descending select x).ToList();

                var hypResult = new SpatialModelNZParamEstimationResult();
                hypResult.Components = componentVarList;
                hypResult.Dislikeness = new List<double>();
                hypResult.SimResult = new List<SimulationResults>();

                for (int k = 0; k < 5; k++)
                {
                    var simScores = ComputeSpatialSimulation(componentVarList);

                    double unlikenessToNZ = 0.0;

                    for (int j = 0; j < simScores.VarExplainedByComponentOfFirstPrefScores.Length; j++)
                    {
                        unlikenessToNZ += Math.Pow(simScores.VarExplainedByComponentOfFirstPrefScores[j] - nzPCA[j], 2.0);
                    }

                    hypResult.Dislikeness.Add(unlikenessToNZ);
                    hypResult.SimResult.Add(simScores);
                }

                hypResults.Add(hypResult);
                Console.WriteLine("Simulation " + i + " complete ");
            }

            var mostSimilarResults = (from x in hypResults orderby x.Dislikeness.Average() ascending select x).Take(hypResults.Count / 10);

            foreach (var x in mostSimilarResults)
	        {
                for (int i = 0; i < 5; i++)
			    {
			        Console.Write(Math.Round(x.Components[i], 3) + " ");
			    }

                Console.WriteLine();
	        }
        }

        private static SimulationResults ComputeSpatialSimulation(List<double> componentVarList)
        {
            return ComputeSpatialSimulation(componentVarList, new FPPVotingRule(), 1);
        }

        private static SimulationResults ComputeSpatialSimulation(List<double> componentVarList, VotingRule votingRule, int districtMagnitude)
        {
            var society = new SpatialArtificialSociety();

            society.PartyCount = 8;
            society.Dimensions = componentVarList.ToArray();
            society.DistrictMagnitude = districtMagnitude;

            var situation = new VotingSituation();

            var electorateCount = 120 / districtMagnitude;
            var voteCount = 500;
            situation.PartyCount = society.PartyCount;

            situation.GetPreferenceOrderByIndex = (Int64 pIndex) =>
            {
                return DiscreteMath.GetPermutationByIndex(pIndex, situation.PartyCount);
            };

            society.SetupElection();
            for (int electorateIndex = 0; electorateIndex < electorateCount; electorateIndex++)
            {
                var electorate = new ElectorateVotes();

                electorate.Magnitude = districtMagnitude;

                electorate.VoteCounts = society.SampleElectorate(voteCount);

                situation.Electorates.Add(electorate);
            }

            var results = votingRule.CalculateResults(situation);

            var prefOrdersTotals = new HashMultiset<Int64>();

            foreach (var electorate in situation.Electorates)
            {
                foreach (var index in electorate.VoteCounts)
                {
                    prefOrdersTotals[index] += electorate.VoteCounts[index];
                }
            }

            var simScores = ComputeSimulationScores(results, ((Multiset<Int64> mset) => { return 1.0; }), prefOrdersTotals, situation);
            return simScores;
        }

        #endregion

        #region "STV Simulation"
        private class STVSimulationResult
        {
            public int WinnersPerElectorate { get; set; }
            public SimulationResults SimulationResults { get; set; }
        }

        private static void ComputeCompareSTV_PropLocal()
        {
            var society = new SpatialArtificialSociety();
            society.Dimensions = new double[] { 1.0, 1.0 };

            ComputeCompareArtificalSocieties("plsim_spatial2_l", () => { return new ProportionalByDistrictVotingRule(); }, society, 7, 5000);
            ComputeCompareArtificalSocieties("stvsim_spatial2_l", () => { return new STVVotingRule(); }, society, 7, 5000);
        }

        private static void ComputeSTVCompare()
        {
            for (int i = 10; i < 100; i+=10)
            {
                var society = new UrnArtificialSociety();

                society.AlphaGenerator = () =>
                {
                    //var beta = Rand.NextDouble();
                    var alpha = i;

                    return alpha;
                };

                ComputeCompareArtificalSocieties("stvsim_largefixed_" + i, society, 8, 50000);
            }

            //var society = new UrnArtificialSociety();

            //society.AlphaGenerator = () =>
            //{
            //    var beta = Rand.NextDouble();
            //    var alpha = beta / (1.0 - beta);

            //    return alpha;
            //};

            //ComputeCompareArtificalSocieties("stvsim_med2", society, 8, 10000);

            //var society = new SpatialArtificialSociety();

            //society.Dimensions = new double[] { 1.0 };

            //ComputeCompareArtificalSocieties("stvsim_spatial1", society, 7, 5000);

            //var society = new SpatialArtificialSociety();

            //society.Dimensions = new double[] { 1.0, 1.0 };

            //ComputeCompareArtificalSocieties("stvsim_spatial2", society, 7, 5000);

            //var society = new SpatialArtificialSociety();

            //society.Dimensions = new double[] { 1.0, 0.5 };

            //ComputeCompareArtificalSocieties("stvsim_spatial2e", society, 7, 500);
        }

        private static void ComputeCompareArtificalSocieties(string outputName, ArtificialSocietyGenerator society, int partyCount, int voteCount)
        {
            ComputeCompareArtificalSocieties(outputName, () => { return new STVVotingRule(); }, society, partyCount, voteCount);
        }

        private static void ComputeCompareArtificalSocieties(string outputName, Func<VotingRule> votingRuleGenerator, ArtificialSocietyGenerator society, int partyCount, int voteCount)
        {
            Console.WriteLine("Artificial Society Compare");

            var winnersPerElectorateList = new List<int>();
            var totalSeats = 120;
            //totalSeats = 48;

            winnersPerElectorateList.Add(1);
            winnersPerElectorateList.Add(2);
            winnersPerElectorateList.Add(3);
            winnersPerElectorateList.Add(4);
            //winnersPerElectorateList.Add(5);

            var strmReport = System.IO.File.OpenWrite("out/" + outputName + ".csv");
            strmReport.SetLength(0);

            var report = new CSVReportWriter<STVSimulationResult>(new CSVWriter(strmReport));

            report.AddColumn("wpe", (STVSimulationResult simR) => { return simR.WinnersPerElectorate.ToString(); });

            report.AddColumn("lijphart", (STVSimulationResult simR) => { return simR.SimulationResults.LijphartIndex.ToString(); });
            report.AddColumn("loosemorehanby", (STVSimulationResult simR) => { return simR.SimulationResults.LoosemoreHanbyIndex.ToString(); });
            report.AddColumn("raes", (STVSimulationResult simR) => { return simR.SimulationResults.RaesIndex.ToString(); });
            report.AddColumn("gallagher", (STVSimulationResult simR) => { return simR.SimulationResults.GallagherIndex.ToString(); });
            report.AddColumn("enp", (STVSimulationResult simR) => { return simR.SimulationResults.EffectiveNumberOfParties.ToString(); });
            report.AddColumn("governability", (STVSimulationResult simR) => { return simR.SimulationResults.Governability.ToString(); });
            report.AddColumn("entropy", (STVSimulationResult simR) => { return simR.SimulationResults.EntropyIndex.ToString(); });

            foreach (var winnerPerElectorate in winnersPerElectorateList)
            {
                var rule = votingRuleGenerator();
                
                for (int i = 0; i < 150; i++)
                {
                    Console.WriteLine(winnerPerElectorate + " " + i);

                    var situation = new VotingSituation();
                    
                    var electorateCount = totalSeats / winnerPerElectorate;

                    situation.PartyCount = partyCount;

                    var parties = new List<int>();

                    for (int j = 0; j < situation.PartyCount; j++)
                    {
                        parties.Add(j);
                    }

                    society.PartyCount = situation.PartyCount;
                    society.ElectorateCount = electorateCount;
                    society.DistrictMagnitude = winnerPerElectorate;
                    society.SetupElection();

                    situation.GetPreferenceOrderByIndex = (Int64 pIndex) =>
                    {
                        return DiscreteMath.GetPermutationByIndex(pIndex, situation.PartyCount);
                    };

                    //situation.LegacyPreferenceOrders = DiscreteMath.PermuteIntegers(situation.PartyCount);
                    //var prefOrdersIndicies = DiscreteMath.GetIntList(situation.LegacyPreferenceOrders.Count);

                    for (int electorateIndex = 0; electorateIndex < electorateCount; electorateIndex++)
                    {
                        var electorate = new ElectorateVotes();

                        electorate.Magnitude = winnerPerElectorate;

                        electorate.VoteCounts = society.SampleElectorate(voteCount);

                        situation.Electorates.Add(electorate);
                    }

                    var prefOrdersTotals = new HashMultiset<Int64>();

                    foreach (var electorate in situation.Electorates)
                    {
                        foreach (var index in electorate.VoteCounts)
                        {
                            prefOrdersTotals[index] += electorate.VoteCounts[index];
                        }
                    }

                    var resultsByRule = new SimulationRun();

                    var results = rule.CalculateResults(situation);

                    var stvResult = new STVSimulationResult();

                    stvResult.WinnersPerElectorate = winnerPerElectorate;
                    stvResult.SimulationResults = ComputeSimulationScores(results, ((Multiset<Int64> mset) => { return 1.0; }), prefOrdersTotals);

                    report.WriteLine(stvResult);
                }
            }
        }
        #endregion

        private static void ComputeNewZealandWithPreferenceVariance()
        {
            ComputeNewZealandWithPreferenceVariance("nzsim2", true, 0.5, 1, 1);
            ComputeNewZealandWithPreferenceVariance("nzsim2pure", false, 0.5, 1, 1);

            ComputeNewZealandWithPreferenceVariance3("nzsim_3", false, 0.18, 0.12, 5, 1);

            //ComputeNewZealandWithPreferenceVariance("nzsim2pure_pred5", false, 0.39, 5, 1);
            ComputeNewZealandWithPreferenceVariance("nzsim2pure_pred20", false, 0.6, 20, 15);
        }

        private static void ComputeNewZealandWithPreferenceVariance3(string name, bool oneSeatThreshold, double p_cap, double p2_cap, int p_timesCap, int p_timesMin)
        {
            Console.WriteLine(name);

            var strmReport = System.IO.File.OpenWrite("out/" + name + ".csv");
            strmReport.SetLength(0);

            var report = new CSVReportWriter<NewZealandSimulationRun>(new CSVWriter(strmReport));

            report.AddColumn("year", (NewZealandSimulationRun simR) => { return simR.year.ToString(); });
            report.AddColumn("threshold", (NewZealandSimulationRun simR) => { return simR.Threshold.ToString(); });
            report.AddColumn("p", (NewZealandSimulationRun simR) => { return simR.p.ToString(); });
            report.AddColumn("p2", (NewZealandSimulationRun simR) => { return simR.p2.ToString(); });

            report.AddColumn("lijphart", (NewZealandSimulationRun simR) => { return simR.SimulationResults.LijphartIndex.ToString(); });
            report.AddColumn("loosemorehanby", (NewZealandSimulationRun simR) => { return simR.SimulationResults.LoosemoreHanbyIndex.ToString(); });
            report.AddColumn("entropy", (NewZealandSimulationRun simR) => { return simR.SimulationResults.EntropyIndex.ToString(); });
            report.AddColumn("entropy2", (NewZealandSimulationRun simR) => { return simR.SimulationResults.EntropySeatPropIndex.ToString(); });
            report.AddColumn("maxdelta", (NewZealandSimulationRun simR) => { return simR.SimulationResults.MaxDisprop.ToString(); });

            report.AddColumn("raes", (NewZealandSimulationRun simR) => { return simR.SimulationResults.RaesIndex.ToString(); });
            report.AddColumn("gallagher", (NewZealandSimulationRun simR) => { return simR.SimulationResults.GallagherIndex.ToString(); });
            report.AddColumn("enp", (NewZealandSimulationRun simR) => { return simR.SimulationResults.EffectiveNumberOfParties.ToString(); });
            report.AddColumn("governability", (NewZealandSimulationRun simR) => { return simR.SimulationResults.Governability.ToString(); });

            List<string> parties = new List<string>();
            parties.Add("National");
            parties.Add("Labour");
            parties.Add("NZ First");
            parties.Add("UF");
            parties.Add("Act");
            parties.Add("Green");
            parties.Add("Maori");
            parties.Add("Progressive");
            parties.Add("Other");

            double[,] matrix = new double[parties.Count, parties.Count];

            double totalWeight = 0.0;
            double totalWeightChangedVote = 0.0;
            double totalWeightChangedFrom2nd = 0.0;
            double totalWeightChangedFrom3rd = 0.0;
            //double totalWeightChangedFrom2ndBackwards = 0.0;

            // Base matrix for weight of voters with preferences [1st, 2nd, 3rd]
            // Normalized to 1 by party.
            double [,,] basePreferenceScores = new double[parties.Count, parties.Count, parties.Count];

            Console.WriteLine("Computing preferences.");

            using (var strm = System.IO.File.OpenRead("C:/Users/Michael/Documents/Visual Studio 11/Projects/CS380/Data/NZ/nzpref.csv"))
            {
                var csv = CSVReader.ReadFromStream(strm).ToList();

                var headerRow = csv.First();
                var vote2008 = headerRow.IndexOf("E3: Party Vote in 2008");
                var vote2005 = headerRow.IndexOf("E14: Party Vote in 2005");
                var weight = headerRow.IndexOf("Absolutely Best Weight");
                var nat = headerRow.IndexOf("NAT");
                var lab = headerRow.IndexOf("LAB");
                var nzf = headerRow.IndexOf("NZF");
                var uf = headerRow.IndexOf("UF");
                var act = headerRow.IndexOf("ACT");
                var grn = headerRow.IndexOf("GRN");
                var mao = headerRow.IndexOf("MAO");
                var pro = headerRow.IndexOf("PRO");

                for (int i = 1; i < csv.Count; i++)
                {
                    var row = csv[i];

                    var actual2008 = TransformNZPartyName(row[vote2008]);
                    var actual2005 = TransformNZPartyName(row[vote2005]);

                    double w;

                    try
                    {
                        w = double.Parse(row[weight]);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    List<int> scores = new List<int>();
                    scores.Add(TransformIntScore(row[nat]));
                    scores.Add(TransformIntScore(row[lab]));
                    scores.Add(TransformIntScore(row[nzf]));
                    scores.Add(TransformIntScore(row[uf]));
                    scores.Add(TransformIntScore(row[act]));
                    scores.Add(TransformIntScore(row[grn]));
                    scores.Add(TransformIntScore(row[mao]));
                    scores.Add(TransformIntScore(row[pro]));
                    scores.Add(0); // "Other" score.

                    var actual2008Index = parties.IndexOf(actual2008);

                    for (int randIteration = 0; randIteration < 1000; randIteration++ )
                    {
                        var indicies = DiscreteMath.GetIntList(parties.Count);

                        var randVector = (from x in indicies select Rand.NextDouble()).ToArray();

                        var preferenceOrderIndicies = (from x in indicies
                                                       orderby (actual2008Index == x ? 1 : 0) descending, scores[x] descending, randVector[x] descending
                                                       select x).ToList();

                        var voted2008Index = parties.IndexOf(actual2008);
                        var secondPreference = preferenceOrderIndicies[1];
                        var thirdPreference = preferenceOrderIndicies[2];

                        matrix[voted2008Index, secondPreference] += w;

                        totalWeight += w;

                        basePreferenceScores[voted2008Index, secondPreference, thirdPreference] += w;

                        if (actual2005 != actual2008)
                        {
                            totalWeightChangedVote += w;

                            if (scores[parties.IndexOf(actual2005)] == scores[secondPreference])
                            {
                                totalWeightChangedFrom2nd += w;
                            }

                            if (scores[parties.IndexOf(actual2005)] == scores[thirdPreference])
                            {
                                totalWeightChangedFrom3rd += w;
                            }
                        }
                    }
                }
            }

            Console.WriteLine("Done");
            Console.WriteLine(totalWeightChangedVote / totalWeight);
            Console.WriteLine(totalWeightChangedFrom2nd / totalWeight);
            Console.WriteLine(totalWeightChangedFrom3rd / totalWeight);

            // Normalize.
            for (int i = 0; i < parties.Count; i++)
            {
                double sum = 0.0;

                for (int j = 0; j < parties.Count; j++)
                    sum += matrix[i, j];

                for (int j = 0; j < parties.Count; j++)
                    matrix[i, j] /= sum;

                double sum2 = 0.0;

                for (int b = 0; b < parties.Count; b++)
                    for (int c = 0; c < parties.Count; c++)
                        sum2 += basePreferenceScores[i, b, c];

                for (int b = 0; b < parties.Count; b++)
                    for (int c = 0; c < parties.Count; c++)
                        basePreferenceScores[i, b, c] /= sum2;
            }

            // Matrix for weight of voters with preferences [1st, 2nd, 3rd]
            // Not normalized to 1 by party.
            double [,,] localizedPreferenceScores;

            double[] votesByParty = new double[parties.Count];
            int[] localSeatsByParty = new int[parties.Count];

            List<int> years = new List<int>();

            years.Add(2011);
            years.Add(2002);
            years.Add(2005);
            years.Add(2008);

            foreach (var year in years)
            {
                using (var strm = System.IO.File.OpenRead("nz/" + year + ".csv"))
                {
                    var csv = CSVReader.ReadFromStream(strm).ToList();

                    foreach (var item in csv)
                    {
                        var partyName = item[0];
                        var totalVotes = double.Parse(item[1]);
                        var electorateSeats = int.Parse(item[2]);

                        if (partyName.Length > 0)
                        {
                            int partyIndex = parties.IndexOf(partyName);

                            if (partyIndex == -1)
                            {
                                partyIndex = parties.IndexOf("Other");
                            }

                            votesByParty[partyIndex] += totalVotes;

                            if (oneSeatThreshold)
                            {
                                localSeatsByParty[partyIndex] += electorateSeats;
                            }
                        }
                    }
                }

                // Normalize votesByParty to sum to one.

                double voteSum = votesByParty.Sum();

                for (int i = 0; i < votesByParty.Length; i++)
                    votesByParty[i] /= voteSum;

                // Calculate localizedPreferenceScores base.

                localizedPreferenceScores = new double[parties.Count, parties.Count, parties.Count];

                for (int a = 0; a < parties.Count; a++)
                {
                    var mul = votesByParty[a];

                    for (int b = 0; b < parties.Count; b++)
                    {
                        for (int c = 0; c < parties.Count; c++)
                        {
                            localizedPreferenceScores[a, b, c] = basePreferenceScores[a, b, c] * mul;
                        }
                    }
                }

                for (int pInt = 0; pInt < (int)(p_cap * 100.0); pInt++)
                {
                    // p is the proportion of voters who switch to their second preference.
                    double p = 0.01 * (double)pInt;

                    if (p >= 0.5)
                    {
                        Console.WriteLine();
                    }

                    for (int pInt2 = 0; pInt2 < (int)(p2_cap * 100.0); pInt2++)
                    {
                        // p2 is the proportion of voters who switch their third and first preference.
                        double p2 = 0.01 * (double)(pInt2);

                        Console.WriteLine(year + " " + p + " " + p2);

                        // Begin simulation round.


                        for (int p_times = p_timesMin; p_times < p_timesCap; p_times++)
                        {
                            // Reset prereference calculations
                            var lastPreferenceScores = new double[parties.Count, parties.Count, parties.Count];
                            double[, ,] newPreferenceScores = lastPreferenceScores;

                            for (int a = 0; a < parties.Count; a++)
                            {
                                for (int b = 0; b < parties.Count; b++)
                                {
                                    for (int c = 0; c < parties.Count; c++)
                                    {
                                        lastPreferenceScores[a, b, c] = localizedPreferenceScores[a, b, c];
                                    }
                                }
                            }

                            for (int p_index = 0; p_index < p_times; p_index++)
                            {
                                newPreferenceScores = new double[parties.Count, parties.Count, parties.Count];

                                for (int a = 0; a < parties.Count; a++)
                                    for (int b = 0; b < parties.Count; b++)
                                        for (int c = 0; c < parties.Count; c++)
                                            newPreferenceScores[a, b, c] =
                                                lastPreferenceScores[a, b, c] * (1.0 - (p + p2)) +
                                                lastPreferenceScores[b, a, c] * (p) +
                                                lastPreferenceScores[c, a, b] * (p2);

                                lastPreferenceScores = newPreferenceScores;
                            }

                            var transformedVotes = new double[parties.Count];

                            for (int a = 0; a < parties.Count; a++)
                                for (int b = 0; b < parties.Count; b++)
                                    for (int c = 0; c < parties.Count; c++)
                                        transformedVotes[a] += newPreferenceScores[a, b, c];

                            var situation = new VotingSituation();

                            situation.PartyCount = parties.Count;
                            situation.TotalVotesByPartyOverride = (from x in transformedVotes select (int)(x * 1000000.0)).ToList();
                            situation.MMPElectorateSeatsOverride = localSeatsByParty.ToList();
                            situation.TotalSeatsOverride = 120;

                            for (int thresholdSubPoints = 0; thresholdSubPoints < 100; thresholdSubPoints += 10)
                            {
                                var mmp = new MMPVotingRule() { Threshold = 0.001 * (double)thresholdSubPoints };

                                var results = mmp.CalculateResults(situation);

                                var nzResults = new NewZealandSimulationRun();

                                nzResults.year = year;
                                nzResults.SimulationResults = ComputeSimulationScores(results, situation.TotalVotesByPartyOverride.ToArray(), true, false, situation);
                                nzResults.Threshold = mmp.Threshold;
                                nzResults.p = p;
                                nzResults.p2 = p2;
                                nzResults.p_times = p_times;

                                report.WriteLine(nzResults);
                            }
                        }

                        // End simulation round.
                    }
                }
            }

            report.Close();
        }

        private static void ComputeNewZealandWithPreferenceVariance(string name, bool oneSeatThreshold, double p_cap, int p_timesCap, int p_timesMin)
        {
            Console.WriteLine(name);

            var strmReport = System.IO.File.OpenWrite("out/" + name + ".csv");
            strmReport.SetLength(0);

            var report = new CSVReportWriter<NewZealandSimulationRun>(new CSVWriter(strmReport));

            report.AddColumn("year", (NewZealandSimulationRun simR) => { return simR.year.ToString(); });
            report.AddColumn("threshold", (NewZealandSimulationRun simR) => { return simR.Threshold.ToString(); });
            report.AddColumn("p", (NewZealandSimulationRun simR) => { return simR.p.ToString(); });

            report.AddColumn("lijphart", (NewZealandSimulationRun simR) => { return simR.SimulationResults.LijphartIndex.ToString(); });
            report.AddColumn("loosemorehanby", (NewZealandSimulationRun simR) => { return simR.SimulationResults.LoosemoreHanbyIndex.ToString(); });
            report.AddColumn("entropy", (NewZealandSimulationRun simR) => { return simR.SimulationResults.EntropyIndex.ToString(); });
            report.AddColumn("maxdelta", (NewZealandSimulationRun simR) => { return simR.SimulationResults.MaxDisprop.ToString(); });

            report.AddColumn("raes", (NewZealandSimulationRun simR) => { return simR.SimulationResults.RaesIndex.ToString(); });
            report.AddColumn("gallagher", (NewZealandSimulationRun simR) => { return simR.SimulationResults.GallagherIndex.ToString(); });
            report.AddColumn("enp", (NewZealandSimulationRun simR) => { return simR.SimulationResults.EffectiveNumberOfParties.ToString(); });
            report.AddColumn("governability", (NewZealandSimulationRun simR) => { return simR.SimulationResults.Governability.ToString(); });

            List<string> parties = new List<string>();
            parties.Add("National");
            parties.Add("Labour");
            parties.Add("NZ First");
            parties.Add("UF");
            parties.Add("Act");
            parties.Add("Green");
            parties.Add("Maori");
            parties.Add("Progressive");
            parties.Add("Other");

            double[,] matrix = new double[parties.Count, parties.Count];

            double totalWeight = 0.0;
            double totalWeightChangedVote = 0.0;
            double totalWeightChangedFrom2nd = 0.0;
            double totalWeightChangedFrom3rd = 0.0;
            //double totalWeightChangedFrom2ndBackwards = 0.0;

            using (var strm = System.IO.File.OpenRead("C:/Users/Michael/Desktop/New folder/CS380/Data/NZ/nzpref.csv"))
            {
                var csv = CSVReader.ReadFromStream(strm).ToList();

                var headerRow = csv.First();
                var vote2008 = headerRow.IndexOf("E3: Party Vote in 2008");
                var vote2005 = headerRow.IndexOf("E14: Party Vote in 2005");
                var weight = headerRow.IndexOf("Absolutely Best Weight");
                var nat = headerRow.IndexOf("NAT");
                var lab = headerRow.IndexOf("LAB");
                var nzf = headerRow.IndexOf("NZF");
                var uf = headerRow.IndexOf("UF");
                var act = headerRow.IndexOf("ACT");
                var grn = headerRow.IndexOf("GRN");
                var mao = headerRow.IndexOf("MAO");
                var pro = headerRow.IndexOf("PRO");

                for (int i = 1; i < csv.Count; i++)
                {
                    var row = csv[i];

                    var actual2008 = TransformNZPartyName(row[vote2008]);
                    var actual2005 = TransformNZPartyName(row[vote2005]);

                    double w;

                    try
                    {
                        w = double.Parse(row[weight]);
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    List<int> scores = new List<int>();
                    scores.Add(TransformIntScore(row[nat]));
                    scores.Add(TransformIntScore(row[lab]));
                    scores.Add(TransformIntScore(row[nzf]));
                    scores.Add(TransformIntScore(row[uf]));
                    scores.Add(TransformIntScore(row[act]));
                    scores.Add(TransformIntScore(row[grn]));
                    scores.Add(TransformIntScore(row[mao]));
                    scores.Add(TransformIntScore(row[pro]));
                    scores.Add(0); // "Other" score.

                    var actual2008Index = parties.IndexOf(actual2008);

                    var indicies = DiscreteMath.GetIntList(parties.Count);
                    var preferenceOrderIndicies = (from x in indicies
                                                   orderby (actual2008Index == x ? 1 : 0) descending, scores[x] descending, Rand.NextDouble() descending
                                                   select x).ToList();

                    var voted2008Index = parties.IndexOf(actual2008);

                    //scores[voted2008Index] = -1;

                    var secondPreference = preferenceOrderIndicies[1];

                    //scores[secondPreference] = -2;

                    var thirdPreference = preferenceOrderIndicies[2];

                    matrix[voted2008Index, secondPreference] += w;

                    totalWeight += w;

                    if (actual2005 != actual2008)
                    {
                        totalWeightChangedVote += w;

                        if (scores[parties.IndexOf(actual2005)] == scores[secondPreference])
                        {
                            totalWeightChangedFrom2nd += w;
                        }

                        if (scores[parties.IndexOf(actual2005)] == scores[thirdPreference])
                        {
                            totalWeightChangedFrom3rd += w;
                        }
                    }
                }
            }

            Console.WriteLine(totalWeightChangedVote / totalWeight);
            Console.WriteLine(totalWeightChangedFrom2nd / totalWeight);
            Console.WriteLine(totalWeightChangedFrom3rd / totalWeight);

            // Normalize.
            for (int i = 0; i < parties.Count; i++)
            {
                double sum = 0.0;

                for (int j = 0; j < parties.Count; j++)
                    sum += matrix[i, j];

                for (int j = 0; j < parties.Count; j++)
                    matrix[i, j] /= sum;
            }

            double[] votesByParty = new double[parties.Count];
            int[] localSeatsByParty = new int[parties.Count];

            List<int> years = new List<int>();

            years.Add(2011);
            years.Add(2002);
            years.Add(2005);
            years.Add(2008);

            foreach (var year in years)
            {
                using (var strm = System.IO.File.OpenRead("nz/" + year + ".csv"))
                {
                    var csv = CSVReader.ReadFromStream(strm).ToList();

                    foreach (var item in csv)
                    {
                        var partyName = item[0];
                        var totalVotes = double.Parse(item[1]);
                        var electorateSeats = int.Parse(item[2]);

                        if (partyName.Length > 0)
                        {
                            int partyIndex = parties.IndexOf(partyName);

                            if (partyIndex == -1)
                            {
                                partyIndex = parties.IndexOf("Other");
                            }

                            votesByParty[partyIndex] += totalVotes;

                            if (oneSeatThreshold)
                            {
                                localSeatsByParty[partyIndex] += electorateSeats;
                            }
                        }
                    }
                }



                // Normalize votesByParty to sum to one.

                double voteSum = votesByParty.Sum();

                for (int i = 0; i < votesByParty.Length; i++)
                    votesByParty[i] /= voteSum;

                for (int pInt = 0; pInt < (int)(p_cap * 100.0); pInt++)
                {
                    // p is the proportion of voters who switch to their second preference.
                    double p = 0.01 * (double)pInt;

                    if (p >= 0.5)
                    {
                        Console.WriteLine();
                    }

                    double[] lastVotesCopy = votesByParty.ToArray();

                    double[] transformedVotes = new double[parties.Count];

                    for (int p_times = p_timesMin; p_times < p_timesCap; p_times++)
                    {
                        for (int p_index = 0; p_index < p_times; p_index++)
                        {
                            for (int i = 0; i < parties.Count; i++)
                            {
                                transformedVotes[i] = lastVotesCopy[i] * (1 - p);

                                for (int j = 0; j < parties.Count; j++)
                                {
                                    transformedVotes[i] += lastVotesCopy[j] * matrix[j, i] * p;
                                }
                            }
                        }

                        var situation = new VotingSituation();

                        situation.PartyCount = parties.Count;
                        situation.TotalVotesByPartyOverride = (from x in transformedVotes select (int)(x * 1000000.0)).ToList();
                        situation.MMPElectorateSeatsOverride = localSeatsByParty.ToList();
                        situation.TotalSeatsOverride = 120;

                        for (int thresholdSubPoints = 0; thresholdSubPoints < 100; thresholdSubPoints += 10)
                        {
                            var mmp = new MMPVotingRule() { Threshold = 0.001 * (double)thresholdSubPoints };

                            var results = mmp.CalculateResults(situation);

                            var nzResults = new NewZealandSimulationRun();

                            nzResults.year = year;
                            nzResults.SimulationResults = ComputeSimulationScores(results, situation.TotalVotesByPartyOverride.ToArray(), true, false, situation);
                            nzResults.Threshold = mmp.Threshold;
                            nzResults.p = p;
                            nzResults.p_times = p_times;

                            report.WriteLine(nzResults);
                        }
                    }
                }
            }

            report.Close();
        }

        private static int TransformIntScore(string score)
        {
            int x;

            if (int.TryParse(score, out x))
            {
                if (x < 0 || x > 10)
                {
                    return 0;
                }

                return x;
            }
            else
            {
                return 0;
            }
        }
        private static string TransformNZPartyName(string name)
        {
            switch (name)
            {
                case "National":
                    return "National";
                case "Labour":
                    return "Labour";
                case "NZ First":
                case "NZFirst":
                    return "NZ First";
                case "Maori":
                    return "Maori";
                case "Green":
                    return "Green";
                case "Act":
                    return "Act";
                case "UF":
                case "United F":
                    return "UF";
                case "Progressive":
                    return "Progressive";
                case "Other":
                case "DK":
                case "No Vote":
                case "Legalise Cannabis":
                case "Destiny":
                case "Another":
                case "Bill and Ben":
                case "":
                case "Direct Democracy":
                case "Kiwi":
                case "Republic":
                case "Family":
                case "Libertarianz":
                case "Christian Hetitage":
                case "NZ Pacific":
                case "Alliance":
                    return "Other";
                default:
                    throw new Exception();
            }
        }

        private class NewZealandSimulationRun
        {
            public SimulationResults SimulationResults { get; set; }
            public double Threshold { get; set; }
            public double p { get; set; }
            public double p2 { get; set; }
            public int year { get; set; }

            public int p_times { get; set; }
        }

        private static void ComputeNewZealand()
        {
            List<int> years = new List<int>();

            years.Add(1996);
            years.Add(1999);
            years.Add(2002);
            years.Add(2005);
            years.Add(2008);
            years.Add(2011);

            var strmReport = System.IO.File.OpenWrite("out/nzsim.csv");
            strmReport.SetLength(0);

            var report = new CSVReportWriter<NewZealandSimulationRun>(new CSVWriter(strmReport));

            report.AddColumn("year", (NewZealandSimulationRun simR) => { return simR.year.ToString(); });
            report.AddColumn("threshold", (NewZealandSimulationRun simR) => { return simR.Threshold.ToString(); });
            report.AddColumn("p", (NewZealandSimulationRun simR) => { return simR.p.ToString(); });
            report.AddColumn("p_times", (NewZealandSimulationRun simR) => { return simR.p_times.ToString(); });

            report.AddColumn("lijphart", (NewZealandSimulationRun simR) => { return simR.SimulationResults.LijphartIndex.ToString(); });
            report.AddColumn("loosemorehanby", (NewZealandSimulationRun simR) => { return simR.SimulationResults.LoosemoreHanbyIndex.ToString(); });
            report.AddColumn("raes", (NewZealandSimulationRun simR) => { return simR.SimulationResults.RaesIndex.ToString(); });
            report.AddColumn("gallagher", (NewZealandSimulationRun simR) => { return simR.SimulationResults.GallagherIndex.ToString(); });
            report.AddColumn("enp", (NewZealandSimulationRun simR) => { return simR.SimulationResults.EffectiveNumberOfParties.ToString(); });
            report.AddColumn("governability", (NewZealandSimulationRun simR) => { return simR.SimulationResults.Governability.ToString(); });

            foreach (var year in years)
            {
                var situation = new VotingSituation();

                situation.TotalSeatsOverride = 120;

                var partyList = new List<string>();
                var partyVoteCount = new List<int>();
                var partyLocalSeatCount = new List<int>();

                using (var strm = System.IO.File.OpenRead("nz/" + year + ".csv"))
                {
                    var csv = CSVReader.ReadFromStream(strm).ToList();

                    foreach (var item in csv)
                    {
                        var partyName = item[0];
                        var totalVotes = (int)(double.Parse(item[1]) * 100);
                        var electorateSeats = int.Parse(item[2]);

                        if (partyName.Length > 0)
                        {
                            partyList.Add(partyName);
                            partyVoteCount.Add(totalVotes);
                            partyLocalSeatCount.Add(electorateSeats);
                        }
                    }
                }

                var threshold = (double)partyVoteCount.Sum() / (double)situation.TotalSeatsOverride;

                for (int i = partyList.Count - 1; i >= 0; i--)
                {
                    if (partyVoteCount[i] < threshold)
                    {
                        partyList.RemoveAt(i);
                        partyVoteCount.RemoveAt(i);
                        partyLocalSeatCount.RemoveAt(i);
                    }
                }

                situation.PartyCount = partyList.Count;

                situation.TotalVotesByPartyOverride = partyVoteCount;
                situation.MMPElectorateSeatsOverride = partyLocalSeatCount;

                for (int thresholdSubPoints = 0; thresholdSubPoints < 100; thresholdSubPoints += 10)
                {
                    Console.WriteLine(year + " " + thresholdSubPoints);

                    var mmp = new MMPVotingRule() { Threshold = 0.001 * (double)thresholdSubPoints };

                    var results = mmp.CalculateResults(situation);

                    var nzResults = new NewZealandSimulationRun();

                    nzResults.year = year;
                    nzResults.SimulationResults = ComputeSimulationScores(results, partyVoteCount.ToArray(), true, false, null);
                    nzResults.Threshold = mmp.Threshold;
                    nzResults.p = 0.0;

                    report.WriteLine(nzResults);
                }
            }

            report.Close();
        }

        private static void ComputeStatistical()
        {
            var strm = System.IO.File.OpenWrite("out/simulations" + DateTime.UtcNow.Ticks + ".csv");
            strm.SetLength(0);

            var report = new CSVReportWriter<SimulationRun>(new CSVWriter(strm));

            ConcurrentBag<SimulationRun> simResults = new ConcurrentBag<SimulationRun>();

            List<VotingRule> rules = new List<VotingRule>();

            rules.Add(new FPPVotingRule() { Name = "FPP" });
            rules.Add(new STVVotingRule() { Name = "PV" });

            for (int i = 0; i < 70; i += 10)
            {
                rules.Add(new MMPVotingRule() { Threshold = 0.001 * (double)i, Name = "MMP_" + i });
            }

            int col = 0;
            foreach (var r in rules)
            {
                var col2 = col;
                var r2 = r;
                report.AddColumn(r2.Name + "_" + "_governability", (SimulationRun simR) => { return simR.Results[col2].Governability.ToString(); });
                report.AddColumn(r2.Name + "_" + "_loosemorehanby", (SimulationRun simR) => { return simR.Results[col2].LoosemoreHanbyIndex.ToString(); });
                report.AddColumn(r2.Name + "_" + "_entropy", (SimulationRun simR) => { return simR.Results[col2].EntropyIndex.ToString(); });
                report.AddColumn(r2.Name + "_" + "_maxdelta", (SimulationRun simR) => { return simR.Results[col2].MaxDisprop.ToString(); });

                report.AddColumn(r2.Name + "_" + "_gallagher", (SimulationRun simR) => { return simR.Results[col2].GallagherIndex.ToString(); });
                report.AddColumn(r2.Name + "_" + "_truedisp", (SimulationRun simR) => { return simR.Results[col2].TrueDispropIndex.ToString(); });

                report.AddColumn(r2.Name + "_" + "_lijphart", (SimulationRun simR) => { return simR.Results[col2].LijphartIndex.ToString(); });
                report.AddColumn(r2.Name + "_" + "_raes", (SimulationRun simR) => { return simR.Results[col2].RaesIndex.ToString(); });
                report.AddColumn(r2.Name + "_" + "_enp", (SimulationRun simR) => { return simR.Results[col2].EffectiveNumberOfParties.ToString(); });

                col++;
            }

            Parallel.For(0, 25, (int simulationIndex) =>
            {
                System.Threading.Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                //Console.WriteLine("begin " + simulationIndex);

                var situation = new VotingSituation();

                //situation.PartyCount = Rand.NextInt(2, 10);
                situation.PartyCount = 7;

                int electorateCount = 70; //Rand.NextInt(5, 5);

                var parties = new List<int>();

                for (int i = 0; i < situation.PartyCount; i++)
                {
                    parties.Add(i);
                }

                situation.LegacyPreferenceOrders = DiscreteMath.PermuteIntegers(situation.PartyCount);
                var prefOrdersIndicies = DiscreteMath.GetIntList(situation.LegacyPreferenceOrders.Count);

                for (int electorateIndex = 0; electorateIndex < electorateCount; electorateIndex++)
                {
                    var electorate = new ElectorateVotes();

                    electorate.Magnitude = 1;

                    var beta = Rand.NextDouble();
                    var alpha = beta / (1.0 - beta);


                    electorate.VoteCounts = DiscreteMath.SampleMultisetUrn(DiscreteMath.FactorialInt64(situation.PartyCount), 50000, alpha);

                    situation.Electorates.Add(electorate);
                }

                var prefOrdersTotals = new HashMultiset<Int64>();

                foreach (var electorate in situation.Electorates)
                {
                    foreach (var index in electorate.VoteCounts)
                    {
                        prefOrdersTotals[index] += electorate.VoteCounts[index];
                    }
                }

                var resultsByRule = new SimulationRun();

                foreach (var rule in rules)
                {
                    var results = rule.CalculateResults(situation);

                    var scores = ComputeSimulationScores(results, ((Multiset<Int64> mset) => { return 1.0; }), prefOrdersTotals);

                    resultsByRule.Results.Add(scores);
                }

                simResults.Add(resultsByRule);

                Console.WriteLine("end " + simulationIndex);

                System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Normal;
            });

            foreach (var item in simResults)
            {
                report.WriteLine(item);
            }

            report.Close();
        }

        /*
        private static void ComputeExact()
        {

            var partyList = new List<int>();

            for (int i = 0; i < 4; i++)
            {
                partyList.Add(i);
            }


            var preferenceOrders = DiscreteMath.Permute(partyList).ToList();

            Func<Multiset<int>, double> getProbabilityWeight = ((Multiset<int> mset2) =>
            {
                return 1.0 / ((double)partyList.Count);
            });

            VotingRule rule = new FPPVotingRule();
            ConcurrentBag<SimulationResults> simResults = new ConcurrentBag<SimulationResults>();

            Stopwatch watch = new Stopwatch();

            watch.Start();

            Parallel.ForEach(DiscreteMath.GetMultisets(partyList, 100), (Multiset<Int64> votingSituation) =>
            {
                var results = rule.CalculateResults(partyList.Count, votingSituation, preferenceOrders);
                var simResult = ComputeSimulationScores(results, getProbabilityWeight, votingSituation);

                simResults.Add(simResult);
            });

            watch.Stop();

            Console.WriteLine("Elapsed Time: " + watch.ElapsedMilliseconds + "ms");
            Console.WriteLine("Time per result: " + (watch.Elapsed.TotalMilliseconds / (double)simResults.Count).ToString() + "ms");
        }
        */

        private static SimulationResults ComputeSimulationScores(List<int> results, Func<Multiset<Int64>, double> getProbabilityWeight, Multiset<Int64> votingSituation)
        {
            return ComputeSimulationScores(results, getProbabilityWeight, votingSituation, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="results">Results in terms of seats, by party. Count = Party Count</param>
        /// <param name="getProbabilityWeight">Weight function.</param>
        /// <param name="votingSituation"></param>
        /// <returns></returns>
        private static SimulationResults ComputeSimulationScores(List<int> results, Func<Multiset<Int64>, double> getProbabilityWeight, Multiset<Int64> votingSituation, VotingSituation situation)
        {
            // Calculate number of votes for each party.
            int[] voteCounts = new int[results.Count];     // Number of votes for each party.

            foreach (var index in votingSituation)
            {
                var preferenceOrder = DiscreteMath.GetPermutationByIndex(index, results.Count);

                voteCounts[preferenceOrder[0]] += votingSituation[index];
            }

            var result = ComputeSimulationScores(results, voteCounts, true, false, situation);

            result.Weight = getProbabilityWeight(votingSituation);

            return result;
        }

        private static SimulationResults ComputeSimulationScores(List<int> results, int[] voteCounts, bool computeShapleyShubikPowerIndex, bool computeBanzhafPowerIndex, VotingSituation situation)
        {
            return SimulationResults.ComputeSimulationScores(results, voteCounts, computeShapleyShubikPowerIndex, computeBanzhafPowerIndex, situation);
        }
    }
}
