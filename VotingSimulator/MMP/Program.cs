using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MMP
{
    class Program
    {
        public class Party
        {
            public double PartyVoteCount { get; set; }
            public int LocalSeatsWon { get; set; }
            public string Name { get; set; }
            public int OverallSeatsWon { get; set; }
        }

        static void Main(string[] args)
        {
            //List<string> partyNames;

            //using (var strm = File.OpenRead("parties.csv"))
            //{
            //    var csv = CSV.ReadFromStream(strm).ToList();
            //    int csvCount = int.Parse(csv.First().First());

            //    partyNames = (from x in csv.Skip(1).Take(csvCount) select x.First()).ToList();
            //}

            //List<Party> parties = (from x in partyNames select new Party() { Name = x }).ToList();

            ProcessYear("2011");
            ProcessYear("2008");
            ProcessYear("2005");
            ProcessYear("2002");
            ProcessYear("1999");
            ProcessYear("1996");

            Console.ReadLine();
        }

        private static void ProcessYear(string year)
        {
            List<Party> parties = new List<Party>();
            Dictionary<Party, List<int>> resultsByParty = new Dictionary<Party, List<int>>();

            using (var strm = File.OpenRead(year + ".csv"))
            {
                var csv = CSV.ReadFromStream(strm).ToList();

                foreach (var r in csv)
                {
                    var partyName = r[0];
                    double voteCount = double.Parse(r[1]);
                    int localCount = int.Parse(r[2]);

                    Party p = new Party() { Name = partyName };

                    p.PartyVoteCount = voteCount;
                    p.LocalSeatsWon = localCount;

                    parties.Add(p);

                    resultsByParty.Add(p, new List<int>());
                }
            }

            for (int threshhold = 0; threshhold < 6; threshhold++)
            {
                CalculateResults(parties, 120, 0.01 * (double)threshhold);

                foreach (var p in parties)
                {
                    resultsByParty[p].Add(p.OverallSeatsWon);
                }

                int totalSeats = (from x in parties select x.OverallSeatsWon).Sum();
                Console.WriteLine("Total " + totalSeats);

                using (var strm = File.OpenWrite("out/" + year + "_" + threshhold + ".csv"))
                {
                    strm.SetLength(0);

                    List<List<string>> data = new List<List<string>>();

                    foreach (var p in parties)
                    {
                        Console.Write(p.Name + " " + p.OverallSeatsWon);
                        Console.WriteLine();

                        var row = new List<string>();

                        row.Add(p.Name);
                        row.Add(p.OverallSeatsWon.ToString());

                        data.Add(row);
                    }

                    CSV.WriteToStream(strm, data);

                    strm.Flush();
                }

                Console.WriteLine();
            }

            using (var strm = File.OpenWrite("out/" + year + ".csv"))
            {
                strm.SetLength(0);

                List<List<string>> data = new List<List<string>>();

                var headerRow = new List<string>();

                headerRow.Add("Party Name");
                headerRow.Add("Local seats");
                headerRow.Add("Party votes");

                for (int threshhold = 0; threshhold < 6; threshhold++)
                {
                    headerRow.Add("Threshold: " + threshhold.ToString() + "%");
                }

                data.Add(headerRow);

                foreach (var p in parties)
                {
                    var row = new List<string>();

                    row.Add(p.Name);
                    row.Add(p.LocalSeatsWon.ToString());
                    row.Add((p.PartyVoteCount / (from x in parties select x.PartyVoteCount).Sum()).ToString());

                    foreach (var x in resultsByParty[p])
                    {
                        row.Add(x.ToString());
                    }

                    data.Add(row);
                }

                CSV.WriteToStream(strm, data);

                strm.Flush();
            }
        }

        static void CalculateResults(List<Party> parties, int totalSeats, double threshold)
        {
            double totalVotes = (from x in parties select x.PartyVoteCount).Sum();

            List<int> seatsShouldHaveWon = new List<int>();

            for (int i = 0; i < parties.Count; i++)
            {
                seatsShouldHaveWon.Add((int)((parties[i].PartyVoteCount / totalVotes) * (double)totalSeats));
                parties[i].OverallSeatsWon = 0;
            }

            List<Party> partiesElected = new List<Party>();

            for (int i = 0; i < parties.Count; i++)
            {
                if (parties[i].PartyVoteCount / totalVotes > threshold || parties[i].LocalSeatsWon >= 1)
                {
                    partiesElected.Add(parties[i]);
                }
            }

            double totalVotes2 = (from x in partiesElected select x.PartyVoteCount).Sum();

            for (int i = 0; i < partiesElected.Count; i++)
            {
                var p = partiesElected[i];

                var fairCountD = (p.PartyVoteCount / totalVotes2) * (double)totalSeats;
                var fairCountRounded = Math.Round(fairCountD);
                var fairSeatCount = (int)fairCountRounded;

                p.OverallSeatsWon = Math.Max(p.LocalSeatsWon, fairSeatCount);
            }

            for (int i = 0; i < parties.Count; i++)
            {
                parties[i].OverallSeatsWon = 0;
            }

            for (int j = 0; j < totalSeats; j++)
            {
                var partiesByQuotient = (from x in partiesElected
                                         let quot = x.PartyVoteCount / (2.0 * x.OverallSeatsWon + 1.0)
                                         orderby quot descending
                                         select x).ToList();

                partiesByQuotient.First().OverallSeatsWon++;
            }

            for (int i = 0; i < parties.Count; i++)
            {
                parties[i].OverallSeatsWon = Math.Max(parties[i].OverallSeatsWon, parties[i].LocalSeatsWon);
            }

            int overhang = (from x in parties select x.OverallSeatsWon).Sum() - totalSeats;
        }
    }
}
