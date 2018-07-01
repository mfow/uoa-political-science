using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CS380
{
    public class SpatialFile
    {
        public class District
        {
            public List<double> ResultsByParty { get; set; }

            public District()
            {
                ResultsByParty = new List<double>();
            }
        }

        public List<string> PartyNames { get; set; }
        public int PartyCount { get; set; }
        public List<District> Districts { get; set; }
        private PrincipalComponentAnalysis.PCAResults pcaResults = null;

        public SpatialFile()
        {
            Districts = new List<District>();
            PartyNames = new List<string>();
        }

        public static SpatialFile FromStream(string filename)
        {
            using (var strm = File.OpenRead(filename))
            {
                return FromStream(strm);
            }
        }

        public static SpatialFile FromStream(Stream strm)
        {
            var result = new SpatialFile();
            var csv = CSVReader.ReadFromStream(strm).ToList();

            var numberOfParties = int.Parse(csv[0].First());
            var numberOfDistricts = int.Parse(csv[1].First());

            result.PartyCount = numberOfParties;

            int rowIndex = 2;

            if (csv[rowIndex][0] == "names")
            {
                rowIndex++;

                for (int i = 0; i < result.PartyCount; i++)
                {
                    result.PartyNames.Add(csv[rowIndex][i]);
                }

                rowIndex++;
            }

            for (int i = 0; i < numberOfDistricts; i++)
            {
                var row = csv[rowIndex];

                var d = new District();
                for (int j = 0; j < numberOfParties; j++)
                {
                    d.ResultsByParty.Add(double.Parse(row[j]));
                }

                double districtTotal = (from x in d.ResultsByParty select x).Sum();
                d.ResultsByParty = (from x in d.ResultsByParty.ToList() select x / districtTotal).ToList();

                result.Districts.Add(d);

                rowIndex++;
            }

            return result;
        }

        private PrincipalComponentAnalysis.PCAResults GetPCAResults()
        {
            if (pcaResults == null)
            {
                var channels = new double[this.PartyCount][];

                for (int i = 0; i < this.PartyCount; i++)
                {
                    channels[i] = new double[this.Districts.Count];
                }

                for (int i = 0; i < this.Districts.Count; i++)
                {
                    var electorate = this.Districts[i];

                    int[] voteCounts = new int[this.PartyCount];     // Number of votes for each party.

                    for (int j = 0; j < this.PartyCount; j++)
                    {
                        channels[j][i] = electorate.ResultsByParty[j];
                    }
                }

                pcaResults = PrincipalComponentAnalysis.CalculateMatrix(channels, this.Districts.Count);
            }

            return pcaResults;
        }

        public double GetENV()
        {
            var m = GetPCAResults();

            List<List<double>> components = new List<List<double>>();

            for (int i = 0; i < m.VarianceExplainedByComponent.Length; i++)
            {
                components.Add(new List<double>());
            }

            foreach (var d in this.Districts)
            {
                var dummyData = new double[this.PartyCount];
                for (int i = 0; i < dummyData.Length; i++)
                {
                    dummyData[i] = d.ResultsByParty[i];
                }

                double dp = 0.0;
                for (int i = 0; i < this.PartyCount; i++)
                {
                    dp += m.Matrix[0][i] * m.Matrix[1][i];
                }

                var componentInfo = m.TransformSample(dummyData);
                var backtransform = m.BackTransformSample(componentInfo);

                double absErr = 0.0;

                for (int i = 0; i < this.PartyCount; i++)
                {
                    absErr += Math.Abs(backtransform[i] - dummyData[i]);
                }

                for (int i = 0; i < componentInfo.Length; i++)
                {
                    components[i].Add(componentInfo[i]);
                }
            }

            return 1.0 / (from x in m.VarianceExplainedByComponent select x * x).Sum();
        }

        public void DoTest()
        {
            var rdResult = Redistrict(30);
            var newSF = new SpatialFile() { Districts = rdResult, PartyCount = PartyCount };
            var rdResult2 = newSF.Redistrict(this.Districts.Count);
            var newSF2 = new SpatialFile() { Districts = rdResult2, PartyCount = PartyCount };

            var env1 = this.GetENV();
            var env2 = newSF.GetENV();
            var env3 = newSF2.GetENV();
        }

        public List<District> Redistrict(int numberOfDistricts)
        {
            var m = GetPCAResults();
            List<List<double>> components = new List<List<double>>();

            for (int i = 0; i < m.VarianceExplainedByComponent.Length; i++)
            {
                components.Add(new List<double>());
            }

            foreach (var d in this.Districts)
            {
                var dummyData = new double[this.PartyCount];
                for (int i = 0; i < dummyData.Length; i++)
                {
                    dummyData[i] = d.ResultsByParty[i];
                }

                var componentInfo = m.TransformSample(dummyData);
                
                for (int i = 0; i < componentInfo.Length; i++)
                {
                    components[i].Add(componentInfo[i]);
                }
            }

            var varianceMultiplier = Math.Sqrt(((double)numberOfDistricts) / ((double)this.Districts.Count));

            List<double> means = new List<double>();
            List<double> standardDeviations = new List<double>();

            for (int i = 0; i < m.VarianceExplainedByComponent.Length; i++)
            {
                var mean = components[i].Average();
                var sd = Math.Sqrt((from x in components[i] select Math.Pow(x - mean, 2.0)).Average());

                means.Add(mean);
                standardDeviations.Add(sd);
            }

            List<District> results = new List<District>();

            for (int newDistrictId = 0; newDistrictId < numberOfDistricts; newDistrictId++)
            {
                while (true)
                {
                    double[] zScores = new double[m.VarianceExplainedByComponent.Length];

                    for (int i = 0; i < zScores.Length; i++)
                    {
                        zScores[i] = Rand.SampleNormal(means[i], standardDeviations[i] * varianceMultiplier);
                        //zScores[i] = Rand.SampleNormal(0.0, 1.0 * varianceMultiplier);
                    }

                    var votesByParty = m.BackTransformSample(zScores);

                    if ((from x in votesByParty where x < 0 select x).Count() > 0)
                    {
                        // This sampled district is invalid.
                        continue;
                    }

                    results.Add(new District() { ResultsByParty = votesByParty.ToList() });

                    break;
                }
            }

            return results;
        }
    }
}
