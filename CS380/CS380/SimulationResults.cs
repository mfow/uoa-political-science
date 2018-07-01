using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380
{
    public class SimulationRun
    {
        public List<SimulationResults> Results { get; set; }

        public SimulationRun()
        {
            Results = new List<SimulationResults>();
        }
    }

    public class SimulationResults
    {
        public double Weight { get; set; }
        public List<int> Result { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public List<double> ResultProportions
        {
            get
            {
                var resultSum = (double)Result.Sum();
                return (from x in Result select (double)x / resultSum).ToList();
            }
        }

        public double GallagherIndex { get; set; }
        public double RaesIndex { get; set; }
        public double LijphartIndex { get; set; }
        public double LoosemoreHanbyIndex { get; set; }
        public double TrueDispropIndex { get; set; }

        public double EntropyIndex
        {
            get
            {
                var values = (from x in ShapleyShubikPowerIndex let y = Math.Log(x) select x * y);
                var values2 = (from x in values where !double.IsNaN(x) select x);

                return values2.Sum();
            }
        }

        public double EntropySeatPropIndex
        {
            get
            {
                var values = (from x in ResultProportions let y = Math.Log(x) select x * y);
                var values2 = (from x in values where !double.IsNaN(x) select x);

                return values2.Sum();
            }
        }

        public double MaxDisprop { get { return ShapleyShubikPowerIndex.Max(); } }

        public double EffectiveNumberOfParties { get; set; }
        public double Governability { get { return (from x in ShapleyShubikPowerIndex select Math.Pow(x, 2.0)).Sum(); } }

        public double[] ShapleyShubikPowerIndex { get; set; }
        public double[] BanzhafPowerIndex { get; set; }

        public double[] VarExplainedByComponentOfFirstPrefScores { get; set; }
        public List<int> SeatResults { get; set; }

        public SimulationResults()
        {
            Properties = new Dictionary<string, string>();
        }

        public double EffectiveNumberOfPCAVars
        {
            get
            {
                return 1.0 / (from x in VarExplainedByComponentOfFirstPrefScores select x * x).Sum();
            }
        }

        public static SimulationResults ComputeSimulation(VotingRules.VotingSituation situation, VotingRules.VotingRule rule)
        {
            var seatResults = rule.CalculateResults(situation);

            // Calculate number of votes for each party.
            int[] voteCounts = new int[seatResults.Count];     // Number of votes for each party.

            return ComputeSimulationScores(seatResults, situation.GetTotalVotesByParty().ToArray(), true, false, situation);
        }

        public static SimulationResults ComputeSimulationScores(List<int> results, int[] voteCounts, bool computeShapleyShubikPowerIndex, bool computeBanzhafPowerIndex, VotingRules.VotingSituation situation)
        {
            // Number of parties = results.Count

            var simResult = new SimulationResults();

            simResult.SeatResults = results;
            simResult.Result = results;

            // Index calculation

            simResult.GallagherIndex = 0.0;
            //simResult.EntropyIndex = 0.0;
            //simResult.MaxDisprop = 0.0;
            simResult.RaesIndex = 0.0;
            simResult.LijphartIndex = 0.0;
            simResult.LoosemoreHanbyIndex = 0.0;
            simResult.EffectiveNumberOfParties = 0.0;

            List<double> seatsProp = new List<double>();
            List<double> proportionFirst = new List<double>();
            List<double> deltas = new List<double>();

            for (int i = 0; i < results.Count; i++)
            {
                double proportionSeats = (double)simResult.Result[i] / (double)simResult.Result.Sum();
                double proportionFirstVotes = (double)voteCounts[i] / (double)voteCounts.Sum();
                double delta = proportionSeats - proportionFirstVotes;

                deltas.Add(delta);
                proportionFirst.Add(proportionFirstVotes);
                seatsProp.Add(proportionSeats);

                simResult.GallagherIndex += Math.Pow(delta, 2.0);

                var logDelta = Math.Log(delta);

                if (double.IsNaN(logDelta))
                {
                    logDelta = 0.0;
                }

                //simResult.EntropyIndex += delta * logDelta;
                //simResult.MaxDisprop = Math.Max(simResult.MaxDisprop, delta);

                simResult.RaesIndex += Math.Abs(delta);
                simResult.LoosemoreHanbyIndex += Math.Abs(delta);
                simResult.TrueDispropIndex += Math.Abs(delta);
                simResult.LijphartIndex = Math.Max(simResult.LijphartIndex, delta);
                simResult.EffectiveNumberOfParties += Math.Pow(proportionSeats, 2.0);
            }

            simResult.TrueDispropIndex = simResult.TrueDispropIndex / (double)simResult.Result.Sum();
            simResult.GallagherIndex = Math.Sqrt(simResult.GallagherIndex / 2.0);
            simResult.RaesIndex /= (double)results.Count;
            simResult.LoosemoreHanbyIndex /= 2;
            simResult.EffectiveNumberOfParties = 1.0 / simResult.EffectiveNumberOfParties;


            // For debugging.
            var seatsPropSum = seatsProp.Sum();
            var propFirstSum = proportionFirst.Sum();
            var deltaSum = deltas.Sum();

            // Shapley Shubik power index

            if (computeShapleyShubikPowerIndex)
            {
                simResult.ShapleyShubikPowerIndex = new double[results.Count];

                // Naive method of computing Shapley Shubik.
                /*
                double permutationCount = Program.Factorial(partyList.Count);

                foreach (var x in Program.Permute(partyList))
                {
                    double powerTotal = 0.0;

                    foreach (var y in x)
                    {
                        int index = partyList.IndexOf(y);

                        powerTotal += seatsProp[index];

                        if (powerTotal > 0.5)
                        {
                            simResult.ShapleyShubikPowerIndex[index] += (1.0 / permutationCount);
                            break;
                        }
                    }
                }
                */

                // Shapley Shubik power index (fast method).
                // http://hercules.us.es/~mbilbao/pdffiles/generat.pdf

                int n = results.Count;

                for (int i = 0; i < n; i++)
                {
                    double score = 0.0;
                    double multiplierSum = 0.0; // Should sum to one.

                    // Returns whether or not the given set of parties wins.
                    Func<List<int>, bool> v = (List<int> set) =>
                    {
                        return (from x in set select seatsProp[x]).Sum() > 0.5;
                    };

                    var combinations = DiscreteMath.Combinations(DiscreteMath.GetIntList(results.Count)).ToList();

                    combinations.Add(new List<int>()); // Our combinations function does not include the empty set.

                    foreach (var S in combinations)
                    {
                        if (S.Contains(i))
                        {
                            // S == i
                            continue;
                        }

                        var unionSet = S.ToList();
                        unionSet.Add(i);

                        int s = S.Count;

                        double multiplier;

                        //multiplier = Program.Factorial(s) * Program.Factorial(n - s - 1) / Program.Factorial(n);

                        double a = DiscreteMath.Factorial(s);
                        double b = DiscreteMath.Factorial(n - (s + 1));
                        double c = DiscreteMath.Factorial(n);

                        multiplier = (a * b) / c;

                        multiplierSum += multiplier;

                        if (v(unionSet) && (!v(S)))
                        {
                            score += multiplier;
                        }
                    }

                    // TODO: Assert that multiplierSum = 1

                    simResult.ShapleyShubikPowerIndex[i] = score;
                }
            }

            // Banzhaf power index

            if (computeBanzhafPowerIndex)
            {
                simResult.BanzhafPowerIndex = new double[results.Count];

                int winningCombinationsCount = 0;

                foreach (var x in DiscreteMath.Combinations(DiscreteMath.GetIntList(results.Count)))
                {
                    double powerTotal = 0.0;

                    foreach (var y in x)
                    {
                        int index = y;

                        powerTotal += seatsProp[index];
                    }

                    if (powerTotal > 0.5)
                    {
                        winningCombinationsCount++;
                        foreach (var y in x)
                        {
                            int index = y;

                            double thisPartyPower = seatsProp[index];

                            if ((powerTotal - thisPartyPower) < 0.5)
                            {
                                // This is a critical voter.
                                simResult.BanzhafPowerIndex[index] += 1.0;
                            }
                        }
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    simResult.BanzhafPowerIndex[i] /= (double)winningCombinationsCount;
                }
            }

            if (situation != null)
            {
                simResult.ComputePCA(situation);
            }

            return simResult;
        }

        private void ComputePCA(VotingRules.VotingSituation situation)
        {
            var channels = new double[situation.PartyCount][];

            for (int i = 0; i < situation.PartyCount; i++)
            {
                channels[i] = new double[situation.Electorates.Count];
            }

            for (int i = 0; i < situation.Electorates.Count; i++)
            {
                var electorate = situation.Electorates[i];

                int[] voteCounts = new int[situation.PartyCount];     // Number of votes for each party.

                foreach (var index in electorate.VoteCounts)
                {
                    var preferenceOrder = situation.GetPreferenceOrderByIndex(index);

                    voteCounts[preferenceOrder[0]] += electorate.VoteCounts[index];
                }

                for (int j = 0; j < situation.PartyCount; j++)
                {
                    channels[j][i] = voteCounts[j];
                }
            }

            var m = PrincipalComponentAnalysis.CalculateMatrix(channels, situation.Electorates.Count);

            VarExplainedByComponentOfFirstPrefScores = m.VarianceExplainedByComponent;
        }
    }
}
