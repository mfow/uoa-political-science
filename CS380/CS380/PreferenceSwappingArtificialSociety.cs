using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380
{
    public class PreferenceSwappingArtificialSociety : ArtificialSocietyGenerator
    {
        public PreferenceFile PreferenceInformation { get; set; }
        public SpatialFile SpatialInformation { get; set; }
        public double MaxChanceSwap2 { get; set; }
        public double MaxChanceSwap3 { get; set; }

        private List<SpatialFile.District> districts;
        private int districtIndex;

        public override void SetupElection()
        {
            districts = SpatialInformation.Redistrict(this.ElectorateCount);
            districtIndex = 0;
        }

        public override Multiset<long> SampleElectorate(int voteCount)
        {
            var prefInfo = PreferenceInformation.InferPreferences(districts[districtIndex]);

            districtIndex++;

            double swap2 = Rand.NextDouble() * MaxChanceSwap2;
            double swap3 = Math.Min(Rand.NextDouble() * MaxChanceSwap3, swap2);

            var evolved = prefInfo.Evolve(swap2, swap3);
            
            var result = evolved.ToMultiset(voteCount);

            return result;
        }
    }
}
