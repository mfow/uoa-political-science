using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ElectionSystem
{
    public class Election
    {
        public Dictionary<string, Electorate> Electorates { get; private set; }
        public Dictionary<string, Party> Parties { get; private set; }

        public Election()
        {
            Electorates = new Dictionary<string, Electorate>();
            Parties = new Dictionary<string, Party>();
        }

        public Electorate GetElectorateByName(string name)
        {
            if (Electorates.ContainsKey(name))
            {
                return Electorates[name];
            }
            else
            {
                var elec = new Electorate(name);

                Electorates.Add(name, elec);

                return elec;
            }
        }

        public Party GetPartyByName(string name)
        {
            if (Parties.ContainsKey(name))
            {
                return Parties[name];
            }
            else
            {
                var p = new Party(name);

                Parties.Add(name, p);

                return p;
            }
        }

        public void SaveToStream(Stream strm)
        {
            var data = new List<IEnumerable<string>>();

            data.Add(new string[] { Parties.Count.ToString() });
            data.Add(new string[] { Electorates.Count.ToString() });

            var partiesAlphabetical = (from x in Parties orderby x.Key ascending select x.Value).ToList();

            var partiesByVotes = (from x in partiesAlphabetical
                                 let voteCount = (double)(from y in Electorates select y.Value.GetVotesForParty(x)).Sum()
                                 select new Tuple<Party, double>(x, voteCount)).ToList();

            bool useInts = true;

            // A list of electorates with conditional probabilities that a vote for a given party is in this electorate.
            var electoratesByWeight = (from x in Electorates
                                       orderby x.Key ascending
                                      let probability = from y in partiesByVotes select x.Value.GetVotesForParty(y.Item1) / (useInts ? 1 : y.Item2)
                                      select probability.ToList()).ToList();

            foreach (var electorateWeights in electoratesByWeight)
            {
                var row = new List<string>();

                foreach (var w in electorateWeights)
                {
                    row.Add(useInts ? ((int)w).ToString() : w.ToString());
                }

                data.Add(row);
            }

            CSV.WriteToStream(strm, data);
        }
    }

    public class Party
    {
        public string Name { get; private set; }

        public Party(string name)
        {
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Electorate
    {
        public string Name { get; private set; }

        public Dictionary<string, Candidate> Candidates { get; private set; }

        public Electorate(string name)
        {
            this.Name = name;

            Candidates = new Dictionary<string, Candidate>();
        }

        public Candidate GetCandidateByName(string name)
        {
            if (Candidates.ContainsKey(name))
            {
                return Candidates[name];
            }
            else
            {
                var cand = new Candidate(name);

                Candidates.Add(name, cand);

                return cand;
            }
        }

        public int GetVotesForParty(Party p)
        {
            return (from x in Candidates where x.Value.Party == p select x.Value.Votes).Sum();
        }
    }

    public class Candidate
    {
        public Candidate(string name)
        {
            this.Name = name;
            this.Votes = 0;
        }

        public string Name { get; private set; }
        public Party Party { get; set; }

        public int Votes { get; set; }
    }
}
