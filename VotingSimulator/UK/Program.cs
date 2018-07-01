using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using ElectionSystem;

namespace UK
{
    class Program
    {
        static void Main(string[] args)
        {
            string votesFilename = args[0];
            string surveyFilename = args[1];

            ProcessVotes(votesFilename);
            ProcessSurvey(surveyFilename);

            Console.WriteLine("Complete");
        }

        private static void ProcessVotes(string filename)
        {
            var election = new Election();

            List<List<string>> table;

            using (var strm = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                table = CSV.ReadFromStream(strm).ToList();
            }

            var headerRow = table.First();

            Func<string, int> getHeaderIndex = (string headerText) =>
            {
                return headerRow.IndexOf((from x in headerRow where x.Contains(headerText) select x).Single());
            };

            var districtIndex = getHeaderIndex("Constituency name");
            var candidateNameIndex = getHeaderIndex("Candidate");
            var partyNameIndex = getHeaderIndex("Party");
            var voteCountIndex = getHeaderIndex("Votes");

            int index = 1;
            while (index < table.Count)
            {
                List<string> row = table[index];

                var districtName = row[districtIndex];
                var district = election.GetElectorateByName(districtName);

                while (table[index][candidateNameIndex].Trim() != string.Empty)
                {
                    row = table[index];

                    var candidateName = row[candidateNameIndex];
                    var partyName = ConvertPartyName(row[partyNameIndex]);
                    var voteCount = int.Parse(row[voteCountIndex], System.Globalization.NumberStyles.AllowThousands);

                    var candidate = district.GetCandidateByName(candidateName);
                    var party = election.GetPartyByName(partyName);

                    candidate.Party = party;
                    candidate.Votes += voteCount;

                    index++;
                }

                while (index < table.Count && table[index][candidateNameIndex].Trim() == string.Empty)
                {
                    index++;
                }
            }

            using (var strm = new FileStream("spatial.csv", FileMode.OpenOrCreate))
            {
                strm.SetLength(0);
                election.SaveToStream(strm);
            }   
        }

        private static void ProcessSurvey(string filename)
        {
            List<List<string>> table;

            using (var strm = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                table = CSV.ReadFromStream(strm).ToList();
            }

            var survey = new Survey();

            int rowNumber = 0;

            foreach (var row in table)
            {
                var actuallyVotedFor = survey.GetPartyByName(ConvertIntStrToPartyName(row[179 - 1]));
                
                if (actuallyVotedFor == null)
                {
                    // We don't care about people who don't vote...
                    continue;
                }


                Party firstPreference = survey.GetPartyByName(ConvertIntStrToPartyName(row[187 - 1]));
                //528

                var labourFeel = scoreOrDefault(row[192 - 1]);
                var conservativeFeel = scoreOrDefault(row[193 - 1]);
                var liberalDemocratFeel = scoreOrDefault(row[194 - 1]);
                var snpFeel = scoreOrDefault(row[195 - 1]);
                var plaidCymruFeel = scoreOrDefault(row[196 - 1]);
                
                var preferences = new List<Tuple<Party, int?>>();

                if (actuallyVotedFor != null)
                {
                    preferences.Add(new Tuple<Party, int?>(actuallyVotedFor, 102));
                }

                if (firstPreference != null)
                {
                    preferences.Add(new Tuple<Party, int?>(firstPreference, 102));
                }

                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Labour"), labourFeel));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Conservative"), conservativeFeel));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Liberal Democrat"), liberalDemocratFeel));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("SNP"), snpFeel));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Plaid Cymru"), plaidCymruFeel));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Other"), 5));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Greens"), 5));

                for (int i = 0; i < 100; i++)
                {
                    var sample = new SurveySample();

                    sample.FillPreferencesWithScores(preferences);

                    var nationalWeight = double.Parse(row[644 - 1]);

                    sample.Weight = nationalWeight;
                    sample.VotedFor = actuallyVotedFor;

                    survey.Samples.Add(sample);
                }

                rowNumber++;

                Console.WriteLine("Processed " + rowNumber);
            }

            survey.Calculate();

            using (var strm = new FileStream("conditionalprobability.csv", FileMode.OpenOrCreate))
            {
                strm.SetLength(0);
                survey.SaveToStream(strm);
            }
        }

        private static int? scoreOrDefault(string scoreStr)
        {
            int score;

            try
            {
                score = int.Parse(scoreStr);
            }
            catch (Exception)
            {
                score = 9999;
            }

            if (score >= 0 && score <= 10)
            {
                return score;
            }
            else
            {
                return 5;
            }
        }

        private static string ConvertIntStrToPartyName(string value)
        {
            try
            {
                var valueInt = int.Parse(value);

                return ConvertIntToPartyName(valueInt);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ConvertIntToPartyName(int value)
        {
            switch (value)
            {
                case 1:
                    return "Conservative";
                case 2:
                    return "Labour";
                case 3:
                    return "Liberal Democrat";
                case 4:
                    return "SNP";
                case 5:
                    return "Plaid Cymru";
                case 6:
                    return "Greens";
                default:
                    return null;
            }
        }

        private static string ConvertPartyName(string name)
        {
            switch (name)
            {
                case "Con":
                    return "Conservative";
                case "Lab":
                    return "Labour";
                case "LD":
                    return "Liberal Democrat";
                case "SNP":
                    return "SNP";
                case "PC":
                    return "Plaid Cymru";
                case "Grn":
                    return "Greens";
                default:
                    return "Other";
            }
        }
    }
}
