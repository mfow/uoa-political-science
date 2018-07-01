using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380.VotingRules
{
    public class STVVotingRule : VotingRule
    {
        public override List<int> CalculateResults(VotingSituation situation)
        {
            List<int> results = new int[situation.PartyCount].ToList();

            foreach (var electorate in situation.Electorates)
            {
                int electedRepresentitives = 0;
                bool[] isExcluded = new bool[situation.PartyCount];

                double quota = ((double)electorate.VoteCounts.Weight) / (1.0 + (double)electorate.Magnitude);
                double[] preferenceWeights = new double[situation.PreferenceOrdersCount];

                for (int i = 0; i < preferenceWeights.Length; i++)
                {
                    preferenceWeights[i] = 1.0;
                }

                List<List<int>> preferenceOrders = new List<List<int>>();

                foreach (var index in electorate.VoteCounts)
                {
                    var preferenceOrder = situation.GetPreferenceOrderByIndex(index);

                    preferenceOrders.Add(preferenceOrder);
                }

                for (int round = 0; round < situation.PartyCount; round++)
                {
                    int winnersThisRound = 0;

                    double[] votesByParty = new double[situation.PartyCount];
                    //int[] votedForIndex = new int[situation.PreferenceOrdersCount];
                    //Dictionary<Int64, int> votesForIndexLookup = new Dictionary<long, int>(situation.PreferenceOrdersCount);
                    List<int> votesForIndex = new List<int>();
                    
                    //for (int i = 0; i < votedForIndex.Length; i++)
                    //{
                    //    votedForIndex[i] = -1;
                    //}

                    int index2 = 0;
                    foreach (var index in electorate.VoteCounts)
                    {
                        var preferenceOrder = preferenceOrders[index2];
                        
                        var firstNonExcludedPreference = (from x in preferenceOrder where !isExcluded[x] select x).First();

                        votesByParty[firstNonExcludedPreference] += preferenceWeights[index] * (double)electorate.VoteCounts[index];
                        //votedForIndex[index] = firstNonExcludedPreference;
                        //votesForIndexLookup.Add(index, firstNonExcludedPreference);
                        votesForIndex.Add(firstNonExcludedPreference);

                        index2++;
                    }


                    for (int i = 0; i < situation.PartyCount; i++)
                    {
                        if ((!isExcluded[i]) && votesByParty[i] > quota)
                        {
                            double multiplier = (votesByParty[i] - quota) / votesByParty[i];
                            double removedVotes = 0.0;

                            int prefOrderIndex = 0;
                            foreach (var prefOrder in electorate.VoteCounts)
                            {
                                if (votesForIndex[prefOrderIndex] == i)
                                {
                                    removedVotes += preferenceWeights[prefOrder] * (double)electorate.VoteCounts[prefOrder];
                                    preferenceWeights[prefOrder] *= multiplier;
                                }

                                prefOrderIndex++;
                            }

                            var removedVotes2 = removedVotes * (1.0 - multiplier);

                            //foreach (var x in votesForIndexLookup)
                            //{
                            //    if (x.Value == i)
                            //    {
                            //        preferenceWeights[x.Key] *= multiplier;
                            //    }
                            //}

                            //for (int j = 0; j < votedForIndex.Length; j++)
                            //{
                            //    if (votedForIndex[j] == i)
                            //    {
                            //        preferenceWeights[i] *= multiplier;
                            //    }
                            //}

                            // By commenting out this line, we allow for multiple seats per party per district.
                            //isExcluded[i] = true;
                            results[i]++;

                            winnersThisRound++;
                            electedRepresentitives++;

                            if (electedRepresentitives >= electorate.Magnitude)
                            {
                                goto endElectorate;
                            }
                        }
                    }

                    if (winnersThisRound == 0)
                    {
                        if ((from x in isExcluded where x == false select x).Count() + electedRepresentitives == electorate.Magnitude)
                        {
                            // The number of remaining candidates
                            // is equal to the number we want to elect
                            // we shouldn't ever get here
                            // but it seems like there is a problem with
                            // floating point math...
                            // TODO: Look into this.

                            throw new Exception();

                            for (int i = 0; i < results.Count; i++)
                            {
                                if (!isExcluded[i])
                                {
                                    results[i]++;
                                }
                            }

                            goto endElectorate;
                        }

                        int lowestIndex = 0;
                        double lowestVotes = double.MaxValue;

                        for (int i = 0; i < situation.PartyCount; i++)
                        {
                            if (!isExcluded[i])
                            {
                                if (votesByParty[i] < lowestVotes)
                                {
                                    lowestVotes = votesByParty[i];
                                    lowestIndex = i;
                                }
                            }
                        }

                        isExcluded[lowestIndex] = true;
                    }
                }
            endElectorate:
                continue;
            }

            return results;
        }

        //public override List<int> CalculateResults(VotingSituation situation)
        //{
        //    List<int> results = new int[situation.PartyCount].ToList();

        //    foreach (var electorate in situation.Electorates)
        //    {
        //        int electedRepresentitives = 0;
        //        bool[] isExcluded = new bool[situation.PartyCount];

        //        double quota = ((double)electorate.VoteCounts.Weight) / (1.0 + (double)electorate.Magnitude);
        //        double[] preferenceWeights = new double[situation.PreferenceOrdersCount];

        //        for (int i = 0; i < preferenceWeights.Length; i++)
        //        {
        //            preferenceWeights[i] = 1.0;
        //        }

        //        List<List<int>> preferenceOrders = new List<List<int>>();

        //        foreach (var index in electorate.VoteCounts)
        //        {
        //            var preferenceOrder = situation.GetPreferenceOrderByIndex(index);

        //            preferenceOrders.Add(preferenceOrder);
        //        }

        //        for (int round = 0; round < situation.PartyCount; round++)
        //        {
        //            int winnersThisRound = 0;

        //            double[] votesByParty = new double[situation.PartyCount];
        //            //int[] votedForIndex = new int[situation.PreferenceOrdersCount];
        //            //Dictionary<Int64, int> votesForIndexLookup = new Dictionary<long, int>(situation.PreferenceOrdersCount);
        //            List<int> votesForIndex = new List<int>();

        //            //for (int i = 0; i < votedForIndex.Length; i++)
        //            //{
        //            //    votedForIndex[i] = -1;
        //            //}

        //            int index2 = 0;
        //            foreach (var index in electorate.VoteCounts)
        //            {
        //                var preferenceOrder = preferenceOrders[index2];

        //                var firstNonExcludedPreference = (from x in preferenceOrder where !isExcluded[x] select x).First();

        //                votesByParty[firstNonExcludedPreference] += preferenceWeights[index] * (double)electorate.VoteCounts[index];
        //                //votedForIndex[index] = firstNonExcludedPreference;
        //                //votesForIndexLookup.Add(index, firstNonExcludedPreference);
        //                votesForIndex.Add(firstNonExcludedPreference);

        //                index2++;
        //            }


        //            for (int i = 0; i < situation.PartyCount; i++)
        //            {
        //                if ((!isExcluded[i]) && votesByParty[i] > quota)
        //                {
        //                    double multiplier = (votesByParty[i] - quota) / votesByParty[i];
        //                    double removedVotes = 0.0;

        //                    int prefOrderIndex = 0;
        //                    foreach (var prefOrder in electorate.VoteCounts)
        //                    {
        //                        if (votesForIndex[prefOrderIndex] == i)
        //                        {
        //                            removedVotes += preferenceWeights[prefOrder] * (double)electorate.VoteCounts[prefOrder];
        //                            preferenceWeights[prefOrder] *= multiplier;
        //                        }

        //                        prefOrderIndex++;
        //                    }

        //                    var removedVotes2 = removedVotes * (1.0 - multiplier);

        //                    //foreach (var x in votesForIndexLookup)
        //                    //{
        //                    //    if (x.Value == i)
        //                    //    {
        //                    //        preferenceWeights[x.Key] *= multiplier;
        //                    //    }
        //                    //}

        //                    //for (int j = 0; j < votedForIndex.Length; j++)
        //                    //{
        //                    //    if (votedForIndex[j] == i)
        //                    //    {
        //                    //        preferenceWeights[i] *= multiplier;
        //                    //    }
        //                    //}

        //                    isExcluded[i] = true;
        //                    results[i]++;

        //                    winnersThisRound++;
        //                    electedRepresentitives++;

        //                    if (electedRepresentitives >= electorate.Magnitude)
        //                    {
        //                        goto endElectorate;
        //                    }
        //                }
        //            }

        //            if (winnersThisRound == 0)
        //            {
        //                if ((from x in isExcluded where x == false select x).Count() + electedRepresentitives == electorate.Magnitude)
        //                {
        //                    // The number of remaining candidates
        //                    // is equal to the number we want to elect
        //                    // we shouldn't ever get here
        //                    // but it seems like there is a problem with
        //                    // floating point math...
        //                    // TODO: Look into this.

        //                    throw new Exception();

        //                    for (int i = 0; i < results.Count; i++)
        //                    {
        //                        if (!isExcluded[i])
        //                        {
        //                            results[i]++;
        //                        }
        //                    }

        //                    goto endElectorate;
        //                }

        //                int lowestIndex = 0;
        //                double lowestVotes = double.MaxValue;

        //                for (int i = 0; i < situation.PartyCount; i++)
        //                {
        //                    if (!isExcluded[i])
        //                    {
        //                        if (votesByParty[i] < lowestVotes)
        //                        {
        //                            lowestVotes = votesByParty[i];
        //                            lowestIndex = i;
        //                        }
        //                    }
        //                }

        //                isExcluded[lowestIndex] = true;
        //            }
        //        }
        //    endElectorate:
        //        continue;
        //    }

        //    return results;
        //}
    }
}
