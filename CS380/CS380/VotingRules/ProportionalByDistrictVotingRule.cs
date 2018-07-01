using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380.VotingRules
{
    public enum ApportionmentMethod
    {
        StLague,
        Hill,
        Jefferson,
        Hamilton,
    }

    public class ProportionalByDistrictVotingRule : VotingRule
    {
        public ApportionmentMethod Apportionment { get; set; }

        public ProportionalByDistrictVotingRule()
        {
            Apportionment = ApportionmentMethod.StLague;
        }

        public override List<int> CalculateResults(VotingSituation situation)
        {
            List<int> results = new int[situation.PartyCount].ToList();

            foreach (var electorate in situation.Electorates)
            {
                int[] voteCounts = new int[situation.PartyCount];     // Number of votes for each party.

                foreach (var index in electorate.VoteCounts)
                {
                    var preferenceOrder = situation.GetPreferenceOrderByIndex(index);

                    voteCounts[preferenceOrder[0]] += electorate.VoteCounts[index];
                }

                List<int> localResults = new int[situation.PartyCount].ToList();

                var indicies = Enumerable.Range(0, situation.PartyCount).ToArray();

                double[] voteCounts2 = (from x in voteCounts select Math.Abs((double)x + ((Rand.NextDouble() - 0.5) / (double)voteCounts.Sum()))).ToArray();

                double standardDivisor = (double)voteCounts2.Sum() / (double)electorate.Magnitude;
                var standardQuota = (from x in voteCounts2 select (double)x / standardDivisor).ToArray();
                var lowerQuota = (from x in standardQuota select Math.Floor(x)).ToList();
                var upperQuota = (from x in standardQuota select Math.Ceiling(x)).ToList();
                var fractionalQuota = (from x in indicies select standardQuota[x] - lowerQuota[x]).ToList();

                double MD = standardDivisor;

                switch (Apportionment)
                {
                    case ApportionmentMethod.StLague:
                        for (int i = 0; i < electorate.Magnitude; i++)
                        {
                            double maxQuot = 0.0;
                            int maxIndex = 0;

                            for (int j = 0; j < situation.PartyCount; j++)
                            {
                                var quot = ((double)voteCounts[j]) / (2.0 * ((double)localResults[j]) + 1.0);

                                if (quot > maxQuot)
                                {
                                    maxQuot = quot;
                                    maxIndex = j;
                                }
                            }

                            localResults[maxIndex]++;
                        }

                        break;
                    case ApportionmentMethod.Hill:
                        //while (true)
                        //{
                        //    var candidateQuota = (from x in voteCounts2 select (double)x / MD).ToArray();
                        //    var candidateQuotaLo = (from x in candidateQuota select (int)Math.Floor(x)).ToArray();
                        //    var candidateQuotaHi = (from x in candidateQuota select (int)Math.Ceiling(x)).ToArray();
                        //    var candidateQuotaGeo = (from i in indicies select Math.Sqrt((double)candidateQuotaLo[i] * (double)candidateQuotaHi[i])).ToArray();
                        //    //var candidate = (from i in indicies select candidateQuota[i] > candidateQuotaGeo[i] ? (int)upperQuota[i] : (int)lowerQuota[i]).ToArray();
                        //    var candidate = (from i in indicies select standardQuota[i] > candidateQuotaGeo[i] ? (int)upperQuota[i] : (int)lowerQuota[i]).ToArray();

                        //    if (candidate.Sum() == electorate.Magnitude)
                        //    {
                        //        for (int i = 0; i < situation.PartyCount; i++)
                        //        {
                        //            localResults[i] += candidate[i];
                        //        }
                        //    }

                        //    if (localResults.Sum() > 0)
                        //    {
                        //        break;
                        //    }

                        //    MD = standardDivisor * Rand.NextDouble();
                        //}
                        for (int i = 0; i < electorate.Magnitude; i++)
                        {
                            double maxQuot = 0.0;
                            int maxIndex = 0;

                            for (int j = 0; j < situation.PartyCount; j++)
                            {
                                var geoMean = Math.Sqrt(Math.Pow((double)localResults[j],2.0) + Math.Pow((double)(localResults[j] + 1), 2.0));
                                var quot = ((double)voteCounts[j]) / geoMean;

                                if (quot > maxQuot)
                                {
                                    maxQuot = quot;
                                    maxIndex = j;
                                }
                            }

                            localResults[maxIndex]++;
                        }

                        break;
                    case ApportionmentMethod.Jefferson:
                        //while (localResults.Sum() == 0)
                        //{
                        //    var candidateQuota = (from x in voteCounts2 select (int)Math.Floor((double)x / MD)).ToArray();

                        //    if (candidateQuota.Sum() == electorate.Magnitude)
                        //    {
                        //        for (int i = 0; i < situation.PartyCount; i++)
                        //        {
                        //            localResults[i] += candidateQuota[i];
                        //        }
                        //    }

                        //    MD = standardDivisor * Rand.NextDouble();
                        //}

                        var matrix = new double[situation.PartyCount, electorate.Magnitude];
                        List<double> mValues = new List<double>();

                        for (int i = 0; i < situation.PartyCount; i++)
                        {
                            var pcVotes = voteCounts2[i];

                            for (int j = 0; j < electorate.Magnitude; j++)
                            {
                                var value = ((double)pcVotes) / ((double)(j + 1));

                                matrix[i, j] = value;
                                mValues.Add(value);
                            }
                        }

                        //List<int> localResults2 = new List<int>();

                        var cutoff = (from x in mValues orderby x descending select x).Take(electorate.Magnitude).Min();
                        var cCount = (from x in mValues where x >= cutoff select x).Count();

                        for (int i = 0; i < situation.PartyCount; i++)
                        {
                            int seatCount = 0;

                            for (int j = 0; j < electorate.Magnitude; j++)
                            {
                                if (matrix[i,j] >= cutoff)
                                {
                                    seatCount++;
                                }
                            }
                            localResults[i] = (seatCount);
                        }


                        break;
                    case ApportionmentMethod.Hamilton:
                        for (int i = 0; i < situation.PartyCount; i++)
                            localResults[i] = (int)lowerQuota[i];

                        foreach (var i in (from x in indicies orderby fractionalQuota[x] descending select x))
                        {
                            if (electorate.Magnitude > localResults.Sum())
                            {
                                localResults[i]++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        break;
                    default:
                        throw new Exception();
                }

                // Copy district results to global results.
                for (int i = 0; i < situation.PartyCount; i++)
                {
                    results[i] += localResults[i];
                }
            }

            return results;
        }
    }
}
