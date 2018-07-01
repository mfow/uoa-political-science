using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380.VotingRules
{
    public class MMPVotingRule : VotingRule
    {
        public double Threshold { get; set; }

        public override List<int> CalculateResults(VotingSituation situation)
        {
            List<int> results = new int[situation.PartyCount].ToList();

            var totalVotesByParty = situation.GetTotalVotesByParty();

            var totalVotes = totalVotesByParty.Sum();
            var thresholdLimit = ((double)totalVotes) * Threshold;

            bool[] partyNotExcluded = new bool[situation.PartyCount];

            for (int i = 0; i < situation.PartyCount; i++)
            {
                if ((situation.MMPElectorateSeatsOverride != null) && situation.MMPElectorateSeatsOverride[i] > 0)
                {
                    partyNotExcluded[i] = true;
                }
                else if (totalVotesByParty[i] > thresholdLimit)
                {
                    partyNotExcluded[i] = true;
                }
                else
                {
                    partyNotExcluded[i] = false;
                }
            }

            int totalVotesAfterRemoval = 0;

            for (int i = 0; i < situation.PartyCount; i++)
            {
                if (partyNotExcluded[i])
                {
                    totalVotesAfterRemoval += totalVotesByParty[i];
                }
            }
            
            var seatTotal = situation.GetSeatTotal();

            for (int i = 0; i < seatTotal; i++)
            {
                double maxQuot = 0.0;
                int maxIndex = 0;

                for (int j = 0; j < situation.PartyCount; j++)
                {
                    if (partyNotExcluded[j])
                    {
                        var quot = ((double)totalVotesByParty[j]) / (2.0 * ((double)results[j]) + 1.0);

                        if (quot > maxQuot)
                        {
                            maxQuot = quot;
                            maxIndex = j;
                        }
                    }
                }

                results[maxIndex]++;
            }

            return results;
        }
    }
}
