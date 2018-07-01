using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380
{
    public class UrnArtificialSociety : ArtificialSocietyGenerator
    {
        public Func<double> AlphaGenerator { get; set; }

        public override void SetupElection()
        {
            return;
        }

        public override Multiset<long> SampleElectorate(int voteCount)
        {
            return DiscreteMath.SampleMultisetUrn(DiscreteMath.FactorialInt64(this.PartyCount), voteCount, AlphaGenerator());
        }
    }
}
