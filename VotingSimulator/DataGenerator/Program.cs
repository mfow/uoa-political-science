using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Voting Simulator Data Generator");
            Console.WriteLine();

            //TestGeneratePreferentialData();

            Stream strm;

            while (true)
            {
                Console.Write("Output Filename:");
                string filename = Console.ReadLine();

                try
                {
                    strm = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                }
                catch (Exception)
                {
                    Console.WriteLine("Error.");
                    continue;
                }

                if (strm.Length != 0)
                {
                    Console.Write("File is not empty. Overwrite? (y/n)");

                    if (Console.ReadLine().ToLower().StartsWith("y"))
                    {
                        strm.SetLength(0);
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            var w = new StreamWriter(strm);

            Console.Write("Method:");
            string method = Console.ReadLine();

            w.WriteLine("Voting Preferences,");
            w.WriteLine("Version,Method,");
            w.Write("1000,");
            w.Write(method);
            w.WriteLine(",");

            List<Tuple<string, string>> metadata = new List<Tuple<string, string>>();

            Console.Write("Title:");
            metadata.Add(new Tuple<string, string>("Name", Console.ReadLine()));

            Console.Write("Year:");
            metadata.Add(new Tuple<string, string>("Year", Console.ReadLine()));

            Console.Write("Author:");
            metadata.Add(new Tuple<string, string>("Author", Console.ReadLine()));

            metadata.Add(new Tuple<string, string>("Date Created", DateTime.Now.ToLongDateString()));
            metadata.Add(new Tuple<string, string>("Time Created", DateTime.Now.ToLongTimeString()));

            WriteCSVData(w, metadata);

            switch (method.ToLower())
            {
                case "fpp":
                    FPP(w);
                    break;
                default:
                    Console.WriteLine("Unknown method.");
                    break;
            }

            w.Flush();
            w.Close();
        }

        static void FPP(StreamWriter w)
        {
            int electorateCount = ReadInt("Number of electorates");

            List<string> parties = new List<string>();

            List<List<List<Tuple<string, string>>>> electorates = new List<List<List<Tuple<string, string>>>>();

            for (int i = 0; i < electorateCount; i++)
            {
                List<List<Tuple<string, string>>> candidates = new List<List<Tuple<string, string>>>();

                Console.WriteLine("Electorate " + (i + 1).ToString());

                int candidateCount = ReadInt("Number of candidates");

                for (int j = 0; j < candidateCount; j++)
                {
                    List<Tuple<string, string>> candidate = new List<Tuple<string, string>>();

                    Console.Write("Candidate Name:");
                    string name = Console.ReadLine();

                    Console.Write("Party:");
                    string party = Console.ReadLine();

                    var votes = ReadInt("Number of Votes");

                    candidate.Add(new Tuple<string, string>("Name", name));
                    candidate.Add(new Tuple<string, string>("Party", party));
                    candidate.Add(new Tuple<string, string>("Votes", votes.ToString()));

                    int partyIndex;

                    if (party == string.Empty)
                    {
                        partyIndex = -1;
                    }
                    else
                    {
                        if (parties.Contains(party))
                        {
                            partyIndex = parties.IndexOf(party);
                        }
                        else
                        {
                            parties.Add(party);
                            partyIndex = parties.Count - 1;
                        }
                    }

                    candidates.Add(candidate);
                }

                electorates.Add(candidates);
            }

            w.WriteLine(parties.Count + ",");
            w.WriteLine("Name,");

            foreach (var partyName in parties)
            {
                w.WriteLine(partyName + ",");
            }

            w.WriteLine(electorateCount + ",");

            for (int i = 0; i < electorateCount; i++)
            {
                var candidates = electorates[i];

                w.WriteLine(candidates.Count + ",");

                WriteCSVData(w, candidates);

                var numberOfVotes = (from x in candidates select int.Parse(GetValueFromTupleList("Votes", x, "0"))).ToList();

                w.WriteLine(numberOfVotes.Count + ",");

                List<int> votes = new List<int>();

                for (int j = 0; j < numberOfVotes.Count; j++)
                {
                    for (int k = 0; k < numberOfVotes[j]; k++)
                    {
                        votes.Add(j);
                    }
                }

                var r = new Random();

                votes = (from x in votes let y = r.NextDouble() orderby y select x).ToList();

                foreach (var vote in votes)
                {
                    w.WriteLine(vote.ToString() + ",");
                }

                w.Flush();
            }
        }

        static int ReadInt(string name)
        {
            while (true)
            {
                Console.Write(name + ":");

                try
                {
                    return int.Parse(Console.ReadLine());
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        static void WriteCSVData(StreamWriter w, IEnumerable<Tuple<string, string>> data)
        {
            WriteCSVData(w, (from x in data select x.Item1));
            WriteCSVData(w, (from x in data select x.Item2));
        }

        static string GetValueFromTupleList(string key, IEnumerable<Tuple<string, string>> tuples, string defaultValue)
        {
            foreach (var item in tuples)
            {
                if (item.Item1 == key)
                {
                    return item.Item2;                    
                }
            }

            return defaultValue;
        }

        static void WriteCSVData(StreamWriter w, IEnumerable<IEnumerable<Tuple<string, string>>> data)
        {
            List<string> keys = new List<string>();

            foreach (var item in data)
            {
                keys.AddRange(from x in item select x.Item1);
            }

            keys = keys.Distinct().ToList();

            WriteCSVData(w, from k in keys select new Tuple<string, List<string>>(k, (from r in data select GetValueFromTupleList(k, r, string.Empty)).ToList()));
        }

        static void WriteCSVData(StreamWriter w, IEnumerable<Tuple<string, List<string>>> data)
        {
            int numberOfElements = (from x in data select x.Item2.Count).Max();

            foreach (var x in data)
            {
                if (x.Item2.Count != numberOfElements)
                {
                    throw new ArgumentException();
                }
            }

            WriteCSVData(w, from x in data select x.Item1);

            for (int i = 0; i < numberOfElements; i++)
            {
                WriteCSVData(w, from x in data select x.Item2[i]);
            }
        }

        static void WriteCSVData(StreamWriter w, IEnumerable<string> data)
        {
            foreach (var s in data)
            {
                w.Write(s);
                w.Write(',');
            }

            w.WriteLine();
            w.Flush();
        }

        class VoterType
        {
            public int Count { get; set; }
            public double[] CandidateProbability { get; set; }
        }
        
        class Party
        {
            public string Name { get; set; }
            public double ChanceOfPlacingCandidate { get; set; }
        }

        class Candidate
        {
            public string Name { get; set; }
            public Party Party { get; set; }
        }

        class Vote
        {
            public Vote()
            {
                Candidates = new List<Candidate>();
            }

            public List<Candidate> Candidates { get; set; }
        }

        class Electorate
        {
            public Electorate()
            {
                Candidates = new List<Candidate>();
                VoterTypes = new List<VoterType>();
                Vote = new List<Vote>();
            }

            public string Name { get; set; }
            public List<Candidate> Candidates { get; set; }
            public List<Vote> Vote { get; set; }
            public List<VoterType> VoterTypes { get; set; }
        }

        static void TestGeneratePreferentialData(string method, Stream strm)
        {
            var r = new Random();

            List<Party> parties = new List<Party>();
            int partyCount = r.Next(2, 10);

            for (int i = 0; i < partyCount; i++)
            {
                var party = new Party() { Name = "Party " + i };
                parties.Add(party);

                party.ChanceOfPlacingCandidate = r.NextDouble();
            }

            int electorateCount = r.Next(1, 10);
            List<Electorate> electorates = new List<Electorate>();

            for (int i = 0; i < electorateCount; i++)
            {
                var electorate = new Electorate() { Name = "Electorate " + i };
                electorates.Add(electorate);

                while (true)
                {
                    foreach (var p in parties)
                    {
                        if (r.NextDouble() <= p.ChanceOfPlacingCandidate)
                        {
                            var candidate = new Candidate();
                            candidate.Name = "Candidate " + p.Name + " " + electorate.Name;                        
                            electorate.Candidates.Add(candidate);
                        }
                    }

                    if (electorate.Candidates.Count > 0)
                    {
                        break;
                    }
                }

                int population = r.Next(1000, 10000);
                int voterTypeCount = r.Next(2, 20);

                for (int m = 0; m < voterTypeCount; m++)
                {
                    var vt = new VoterType();

                    vt.CandidateProbability = new double[electorate.Candidates.Count];

                    vt.Count = (int)(population * ((m == voterTypeCount - 1) ? 1.0 : r.NextDouble()));

                    var partiesRandomOrder = (from p in parties let randValue = r.NextDouble() orderby randValue select p).ToList();

                    for (int k = 0; k < electorate.Candidates.Count; k++)
                    {
                        vt.CandidateProbability[k] = k == partyCount - 1 ? 1.0 : r.NextDouble();
                    }

                    electorate.VoterTypes.Add(vt);
                }

                foreach (var vt in electorate.VoterTypes)
                {
                    for (int voteIndex = 0; voteIndex < vt.Count; voteIndex++)
                    {
                        var vote = new Vote();

                        for (int v = 0; v < electorate.Candidates.Count; v++)
                        {
                            for (int u = 0; u < vt.CandidateProbability.Length; u++)
                            {
                                if (!vote.Candidates.Contains(electorate.Candidates[u]))
                                {
                                    if (r.NextDouble() <= vt.CandidateProbability[u])
                                    {
                                        vote.Candidates.Add(electorate.Candidates[u]);
                                        break;
                                    }
                                }
                            }                            
                        }

                        electorate.Vote.Add(vote);
                    }
                }
            }

            var w = new StreamWriter(strm);

            Console.Write("Method:");

            w.WriteLine("Voting Preferences,");
            w.WriteLine("Version,Method,");
            w.Write("1000,");
            w.Write(method);
            w.WriteLine(",");

            List<Tuple<string, string>> metadata = new List<Tuple<string, string>>();

            Console.Write("Title:");
            metadata.Add(new Tuple<string, string>("Name", "TEST DATA"));

            Console.Write("Year:");
            metadata.Add(new Tuple<string, string>("Year", "0000"));

            Console.Write("Author:");
            metadata.Add(new Tuple<string, string>("Author", "AUTOMATICALLY GENERATED"));

            metadata.Add(new Tuple<string, string>("Date Created", DateTime.Now.ToLongDateString()));
            metadata.Add(new Tuple<string, string>("Time Created", DateTime.Now.ToLongTimeString()));

            WriteCSVData(w, metadata);

            var electorateData = new List<List<List<Tuple<string, string>>>>();

            for (int i = 0; i < electorateCount; i++)
            {
                List<List<Tuple<string, string>>> candidates = new List<List<Tuple<string, string>>>();

                Console.WriteLine("Electorate " + (i + 1).ToString());

                var electorate = electorates[i];

                int candidateCount = electorate.Candidates.Count;

                var candidatesData = new List<List<Tuple<string, string>>>();
                electorateData.Add(candidatesData);

                for (int j = 0; j < candidateCount; j++)
                {
                    List<Tuple<string, string>> candidateData = new List<Tuple<string, string>>();

                    var c = electorate.Candidates[j];

                    Console.Write("Candidate Name:");
                    string name = c.Name;

                    Console.Write("Party:");
                    string party = parties.IndexOf(c.Party).ToString();

                    var votes = ReadInt("Number of Votes");

                    candidateData.Add(new Tuple<string, string>("Name", name));
                    candidateData.Add(new Tuple<string, string>("Party", party));
                    candidateData.Add(new Tuple<string, string>("Votes", votes.ToString()));

                    candidatesData.Add(candidateData);
                }
            }

            w.WriteLine(parties.Count + ",");
            w.WriteLine("Name,");

            foreach (var partyName in parties)
            {
                w.WriteLine(partyName + ",");
            }

            w.WriteLine(electorateCount + ",");

            for (int i = 0; i < electorateCount; i++)
            {
                var candidates = electorates[i].Candidates;

                w.WriteLine(candidates.Count + ",");

                WriteCSVData(w, electorateData[i]);

                var numberOfVotes = electorates[i].Vote.Count;

                w.WriteLine(numberOfVotes.ToString() + ",");

                foreach (var vote in electorates[i].Vote)
                {
                    
                }

                w.Flush();
            }

            w.Flush();
            w.Close();

            Console.WriteLine("Generation Complete.");
        }
    }
}
