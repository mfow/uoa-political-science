using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS380
{
    public class PreferenceFile
    {
        private class PreferenceInformation
        {
            public int FirstIndex { get; set; }
            public int SecondIndex { get; set; }
            public int ThirdIndex { get; set; }
            public double Weight { get; set; }
        }

        private List<string> partyNames;
        private List<PreferenceInformation> preferenceInformation;

        private PreferenceFile()
        {
            partyNames = new List<string>();
            preferenceInformation = new List<PreferenceInformation>();
        }

        public static PreferenceFile FromStream(string filename)
        {
            using (var strm = File.OpenRead(filename))
            {
                return FromStream(strm);
            }
        }

        public static PreferenceFile FromStream(Stream strm)
        {
            var result = new PreferenceFile();
            var csv = CSVReader.ReadFromStream(strm).ToList();

            var headerRow = csv[0];
            var current = headerRow.IndexOf("CURRENT");
            var prev = headerRow.IndexOf("PREVIOUS");
            var weight = headerRow.IndexOf("WEIGHT");

            var parties = headerRow.ToList();
            parties.Remove("CURRENT");
            parties.Remove("PREVIOUS");
            parties.Remove("WEIGHT");

            result.partyNames = parties;

            var partiesIndex = (from x in parties select headerRow.IndexOf(x)).ToList();

            for (int i = 1; i < csv.Count; i++)
            {
                var row = csv[i];

                List<double> rawScores = new List<double>();

                for (int j = 0; j < parties.Count; j++)
                {
                    rawScores.Add(double.Parse(row[partiesIndex[j]]));
                }

                var actualVote = parties.IndexOf(row[current]);
                var prevVote = parties.IndexOf(row[prev]);

                if (row[weight].Trim() == string.Empty)
                {
                    continue;
                }

                var currentWeight = double.Parse(row[weight]);

                var indicies = DiscreteMath.GetIntList(parties.Count);

                var preferenceOrder =       (from x in indicies
                                            orderby (actualVote == x ? 1 : 0) descending, rawScores[x] descending, Rand.NextDouble() descending
                                            select x).ToList();

                var secondPreference = preferenceOrder[1];
                var thirdPreference = preferenceOrder[2];

                var info = new PreferenceInformation();
                
                info.FirstIndex = actualVote;
                info.SecondIndex = secondPreference;
                info.ThirdIndex = thirdPreference;
                info.Weight = currentWeight;

                result.preferenceInformation.Add(info);
            }

            return result;
        }

        public class PreferenceMatrix
        {
            public double[,,] VotesByParty { get; private set; }
            public int PartyCount { get; private set; }

            public PreferenceMatrix(int partyCount)
            {
                VotesByParty = new double[partyCount, partyCount, partyCount];
                this.PartyCount = partyCount;
            }

            public PreferenceMatrix Evolve(double switch2nd, double switch3rd)
            {
                var result = new PreferenceMatrix(PartyCount);

                double dontSwitch = 1.0 - (switch2nd + switch3rd);

                for (int i = 0; i < PartyCount; i++)
                    for (int j = 0; j < PartyCount; j++)
                        for (int k = 0; k < PartyCount; k++)
                            result.VotesByParty[i, j, k] =
                                this.VotesByParty[i, j, k] * dontSwitch +
                                this.VotesByParty[j, i, k] * switch2nd +
                                this.VotesByParty[k, i, j] * switch3rd;

                return result;
            }

            /// <summary>
            /// Returns a random permutation with the given first second and third preferences.
            /// </summary>
            /// <param name="first"></param>
            /// <param name="second"></param>
            /// <param name="third"></param>
            /// <returns></returns>
            private List<int> GetRandPermutation(int first, int second, int third)
            {
                var indicies = DiscreteMath.GetIntList(PartyCount);

                var result = (from x in indicies
                              orderby
                                  (((x == first) ? 3 : 0) +
                                  ((x == second) ? 2 : 0) +
                                  ((x == third) ? 1 : 0)) descending, Rand.NextDouble()
                              select x).ToList();

                return result;
            }

            public HashMultiset<Int64> ToMultiset(int totalVotes)
            {
                var result = new HashMultiset<Int64>();

                for (int i = 0; i < PartyCount; i++)
                    for (int j = 0; j < PartyCount; j++)
                        for (int k = 0; k < PartyCount; k++)
                            if (i != j && j != k && i != k)
                                result[DiscreteMath.GetIndexByPermutation(GetRandPermutation(i,j,k))] += (int)(VotesByParty[i, j, k] * (double)totalVotes);

                return result;
            }
        }

        public PreferenceMatrix InferPreferences(SpatialFile.District district)
        {
            var result = new PreferenceMatrix(this.partyNames.Count);

            for (int i = 0; i < district.ResultsByParty.Count; i++)
            {
                var voteShare = district.ResultsByParty[i];

                var relevantPrefData = (from x in this.preferenceInformation where x.FirstIndex == i select x).ToList();
                var totalWeight = (from x in relevantPrefData select x.Weight).Sum();

                foreach (var prefOrder in relevantPrefData)
                {
                    result.VotesByParty[prefOrder.FirstIndex, prefOrder.SecondIndex, prefOrder.ThirdIndex] += prefOrder.Weight * voteShare / totalWeight;
                }
            }

            double total = 0.0;
            for (int i = 0; i < this.partyNames.Count; i++)
                for (int j = 0; j < this.partyNames.Count; j++)
                    for (int k = 0; k < this.partyNames.Count; k++)
                        total += result.VotesByParty[i, j, k];

            for (int i = 0; i < this.partyNames.Count; i++)
                for (int j = 0; j < this.partyNames.Count; j++)
                    for (int k = 0; k < this.partyNames.Count; k++)
                        result.VotesByParty[i, j, k] /= total;

            return result;
        }
    }
}
