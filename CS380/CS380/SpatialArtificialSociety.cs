using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380
{
    public class SpatialArtificialSociety : ArtificialSocietyGenerator
    {
        private class Party
        {
            public double[] Coords { get; set; }
        }

        public bool EnableMajorLeftRight { get; set; }
        public double[] Dimensions { get; set; }
        
        private List<Party> parties;

        public SpatialArtificialSociety()
        {
            EnableMajorLeftRight = true;
        }

        public override void SetupElection()
        {
            parties = new List<Party>();

            if (EnableMajorLeftRight)
            {
                // Major Left

                var p1 = new Party();
                p1.Coords = new double[Dimensions.Length];
                p1.Coords[0] = -0.01;
                parties.Add(p1);

                // Major Right

                var p2 = new Party();
                p2.Coords = new double[Dimensions.Length];
                p2.Coords[0] = +0.01;
                parties.Add(p2);                
            }

            while (this.parties.Count < PartyCount)
            {
                var p = new Party();

                p.Coords = new double[Dimensions.Length];

                for (int j = 0; j < Dimensions.Length; j++)
                {
                    p.Coords[j] = Rand.SampleNormal(0.0, Dimensions[j]);
                }

                parties.Add(p);                
            }
        }

        public override Multiset<long> SampleElectorate(int voteCount)
        {
            var result = new HashMultiset<long>();

            var electorateOffset = new double[Dimensions.Length];

            for (int j = 0; j < Dimensions.Length; j++)
            {
                electorateOffset[j] = Rand.SampleNormal(0.0, Dimensions[j] / (2 * Math.Sqrt((double)this.DistrictMagnitude)));
            }

            for (int i = 0; i < voteCount; i++)
            {
                var voterCoords = new double[Dimensions.Length];

                for (int j = 0; j < Dimensions.Length; j++)
                {
                    voterCoords[j] = Rand.SampleNormal(electorateOffset[j], Dimensions[j]);
                }

                List<Tuple<double, int>> distancesByPartyIndex = new List<Tuple<double, int>>();

                for (int p = 0; p < PartyCount; p++)
                {
                    double x = 0.0;

                    for (int j = 0; j < Dimensions.Length; j++)
                    {
                        x += Math.Pow(parties[p].Coords[j] - voterCoords[j], 2.0);
                    }

                    distancesByPartyIndex.Add(new Tuple<double, int>(Math.Sqrt(x), p));
                }

                var prefOrder = (from x in distancesByPartyIndex orderby x.Item1 ascending select x.Item2).ToList();

                var index = DiscreteMath.GetIndexByPermutation(prefOrder);

                result[index]++;
            }

            return result;
        }
    }
}
