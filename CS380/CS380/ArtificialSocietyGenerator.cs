using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380
{
    public abstract class ArtificialSocietyGenerator
    {
        public int PartyCount { get; set; }
        public int ElectorateCount { get; set; }
        public int DistrictMagnitude { get; set; }

        public abstract void SetupElection();
        public abstract Multiset<long> SampleElectorate(int voteCount);
    }
}
