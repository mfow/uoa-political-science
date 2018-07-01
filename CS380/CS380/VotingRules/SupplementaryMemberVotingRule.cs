using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380.VotingRules
{
    public class SupplementaryMemberVotingRule : VotingRule
    {
        public ApportionmentMethod Apportionment { get; set; }
        public int TotalSeats { get; set; }

        public SupplementaryMemberVotingRule()
        {
            Apportionment = ApportionmentMethod.StLague;
        }

        public override List<int> CalculateResults(VotingSituation situation)
        {
            var baseRule = new ProportionalByDistrictVotingRule() { Apportionment = Apportionment };
            var newSituation = new VotingSituation();

            newSituation.PartyCount = situation.PartyCount;
            newSituation.Electorates.AddRange(situation.Electorates);

            int LargeDistrictSize = TotalSeats - situation.Electorates.Count;

            if (LargeDistrictSize > 0)
            {
                var newElectorate = new ElectorateVotes();
                newElectorate.Magnitude = LargeDistrictSize;
                newElectorate.VoteCounts = new HashMultiset<Int64>();

                foreach (var smallDistrict in situation.Electorates)
                {
                    foreach (var prefOrder in smallDistrict.VoteCounts)
                    {
                        newElectorate.VoteCounts[prefOrder] += smallDistrict.VoteCounts[prefOrder];
                    }
                }

                newSituation.Electorates.Add(newElectorate);
            }

            return baseRule.CalculateResults(newSituation);
        }
    }
}
