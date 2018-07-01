using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380.VotingRules
{
    public class FPPVotingRule : VotingRule
    {
        public override List<int> CalculateResults(VotingSituation situation)
        {
            List<int> results = new int[situation.PartyCount].ToList();

            foreach (var electorate in situation.Electorates)
            {
                if (electorate.Magnitude != 1)
                {
                    throw new NotSupportedException();
                }

                int[] voteCounts = new int[situation.PartyCount];     // Number of votes for each party.

                foreach (var index in electorate.VoteCounts)
                {
                    var preferenceOrder = situation.GetPreferenceOrderByIndex(index);

                    voteCounts[preferenceOrder[0]] += electorate.VoteCounts[index];
                }

                int maxScore = 0;
                int maxIndex = 0;

                for (int i = 0; i < situation.PartyCount; i++)
                {
                    if (voteCounts[i] > maxScore)
                    {
                        maxScore = voteCounts[i];
                        maxIndex = i;
                    }
                }

                results[maxIndex]++;
            }

            return results;
        }
    }
}
