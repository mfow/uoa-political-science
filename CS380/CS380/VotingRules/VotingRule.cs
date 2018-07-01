using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380.VotingRules
{
    public abstract class VotingRule
    {
        public abstract List<int> CalculateResults(VotingSituation situation);

        public string Name { get; set; }

        public List<int> CalculateResults(int partyCount, Multiset<Int64> votingSituation, List<List<int>> preferenceOrders)
        {
            // For legacy purposes.

            var situation = new VotingSituation();

            situation.PartyCount = partyCount;
            situation.LegacyPreferenceOrders = preferenceOrders;
            situation.Electorates.Add(new ElectorateVotes() { VoteCounts = votingSituation, Magnitude = 1 });

            return CalculateResults(situation);
        }
    }
}
