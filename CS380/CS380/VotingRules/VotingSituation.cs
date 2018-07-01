using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380.VotingRules
{
    public class ElectorateVotes
    {
        public Multiset<Int64> VoteCounts { get; set; }
        public int Magnitude { get; set; }
    }

    public class VotingSituation
    {
        public int PartyCount { get; set; }
        public List<ElectorateVotes> Electorates { get; set; }
        public List<List<int>> LegacyPreferenceOrders { get; set; }
        public Func<Int64, List<int>> GetPreferenceOrderByIndex { get; set; }

        public VotingSituation()
        {
            Electorates = new List<ElectorateVotes>();
            //LegacyPreferenceOrders = new List<List<int>>();
            LegacyPreferenceOrders = null;

            GetPreferenceOrderByIndex = (Int64 index) =>
            {
                if (this.LegacyPreferenceOrders == null)
                {
                    return DiscreteMath.GetPermutationByIndex(index, PartyCount);
                }
                else
                {
                    return this.LegacyPreferenceOrders[(int)index];
                }
            };
        }

        public static VotingSituation FromInfo(IEnumerable<HashMultiset<Int64>> districts, int partyCount, int districtMagnitude)
        {
            var electorates = new List<ElectorateVotes>();

            foreach (var d in districts)
	        {
                var ev = new ElectorateVotes();

                ev.Magnitude = districtMagnitude;
                ev.VoteCounts = d;

                electorates.Add(ev);
            }

            var result = new VotingSituation();

            result.Electorates = electorates;
            result.PartyCount = partyCount;

            return result;
        }

        public int TotalSeatsOverride { get; set; }
        public List<int> TotalVotesByPartyOverride { get; set; }
        public List<int> MMPElectorateSeatsOverride { get; set; }

        public void SetElectorates(IEnumerable<ElectorateVotes> evList)
        {
            Electorates = evList.ToList();
        }

        public int GetSeatTotal()
        {
            if (TotalSeatsOverride > 0)
            {
                return TotalSeatsOverride;
            }

            return (from x in Electorates select x.Magnitude).Sum();
        }

        public List<int> GetTotalVotesByParty()
        {
            if (TotalVotesByPartyOverride != null)
            {
                return TotalVotesByPartyOverride;
            }

            List<int> totalVotesByParty = new int[PartyCount].ToList();

            foreach (var electorate in Electorates)
            {
                foreach (var index in electorate.VoteCounts)
                {
                    var preferenceOrder = GetPreferenceOrderByIndex(index);

                    totalVotesByParty[preferenceOrder[0]] += electorate.VoteCounts[index];
                }
            }

            return totalVotesByParty;
        }

        public int PreferenceOrdersCount { get { return (int)DiscreteMath.FactorialInt64(PartyCount); } }
    }
}
