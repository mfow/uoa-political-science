using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ElectionSystem;

namespace Canada
{
    class Program
    {
        static void Main(string[] args)
        {
            string votesFolder = args[0];
            string surveyFile = args[1];

            ProcessVotes(votesFolder);
            ProcessSurvey(surveyFile);

            Console.WriteLine("Complete");
        }

        private static void ProcessVotes(string folder)
        {
            var election = new Election();

            foreach (var filename in from x in Directory.GetFiles(folder)
                                     let y = new FileInfo(x).Name.ToLower()
                                     where y.StartsWith("pollresults_resultatsbureau") && y.EndsWith(".csv")
                                     select x)
            {
                Console.WriteLine("Processing file: " + new FileInfo(filename).Name);

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

                var districtIndex = headerRow.IndexOf("Electoral District Name_English/Nom de circonscription_Anglais");
                var pollingStationIndex = headerRow.IndexOf("Polling Station Name/Nom du bureau de scrutin");
                var candidateFamilyNameIndex = headerRow.IndexOf("Candidate's Family Name/Nom de famille du candidat");
                var candidateFirstNameIndex = getHeaderIndex("Candidate's First Name");
                var candidatePartyIndex = headerRow.IndexOf("Political Affiliation Name_English/Appartenance politique_Anglais");
                var candidateVotesInStationIndex = getHeaderIndex("Candidate Poll Votes Count");

                foreach (var stationRow in table.Skip(1))
                {
                    var district = stationRow[districtIndex];
                    var pollingStation = stationRow[pollingStationIndex];
                    var candidateFamilyName = stationRow[candidateFamilyNameIndex];
                    var candidateFirstName = stationRow[candidateFirstNameIndex];
                    var candidateParty = ConvertPartyName(stationRow[candidatePartyIndex]);
                    var candidateVotesInStation = stationRow[candidateVotesInStationIndex] == "N" ? 0 : int.Parse(stationRow[candidateVotesInStationIndex]);

                    var candidateName = candidateFirstName + " " + candidateFamilyName;

                    var electorate = election.GetElectorateByName(district);
                    var candidate = electorate.GetCandidateByName(candidateName);

                    candidate.Votes += candidateVotesInStation;
                    candidate.Party = election.GetPartyByName(candidateParty);
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
                var actuallyVotedFor =          survey.GetPartyByName(ConvertIntStrToPartyName(row[184 - 1]));

                if (actuallyVotedFor == null)
                {
                    // We don't care about people who don't vote...
                    continue;
                }

                var votedForTopPreference = row[187 - 1].Trim() == "1";

                Party firstPreference;

                if (votedForTopPreference)
                {
                    firstPreference = actuallyVotedFor;
                }
                else
                {
                    firstPreference = survey.GetPartyByName(ConvertIntStrToPartyName(row[188 - 1]));
                }

                var secondPreference = survey.GetPartyByName(ConvertIntStrToPartyName(row[189 - 1]));
                var notVoteFor = survey.GetPartyByName(ConvertIntStrToPartyName(row[190 - 1]));

                var conservativePartyScore =    scoreOrDefault(row[201 - 1]);
                var liberalPartyScore =         scoreOrDefault(row[202 - 1]);
                var ndpPartyScore =             scoreOrDefault(row[203 - 1]);
                var blocQuebecoisPartyScore =   scoreOrDefault(row[204 - 1]);
                var greenPartyScore =           scoreOrDefault(row[204 - 1]);

                var preferences = new List<Tuple<Party, int?>>();

                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Conservative"), conservativePartyScore));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Liberal"), liberalPartyScore));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("NDP"), ndpPartyScore));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Bloc Quebecois"), blocQuebecoisPartyScore));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Green Party"), greenPartyScore));
                preferences.Add(new Tuple<Party, int?>(survey.GetPartyByName("Other"), 50));

                if (actuallyVotedFor != null)
                {
                    preferences.Add(new Tuple<Party, int?>(actuallyVotedFor, 102));
                }

                if (firstPreference != null)
                {
                    preferences.Add(new Tuple<Party, int?>(firstPreference, 102));
                }

                if (secondPreference != null)
                {
                    preferences.Add(new Tuple<Party, int?>(secondPreference, 101));
                }

                if (notVoteFor != null)
                {
                    preferences.Add(new Tuple<Party, int?>(notVoteFor, -1));
                }

                for (int i = 0; i < 100; i++)
                {
                    var sample = new SurveySample();

                    sample.FillPreferencesWithScores(preferences);

                    double householdWeight = 0.0;
                    double provincialWeight = 0.0;
                    double nationalWeight = 0.0;

                    try
                    {
                        householdWeight = double.Parse(row[329 - 1]);
                    }
                    catch (Exception)
                    {
                        
                    }

                    try
                    {
                        provincialWeight = double.Parse(row[330 - 1]);
                    }
                    catch (Exception)
                    {
                        
                    }

                    try
                    {
                        nationalWeight = double.Parse(row[331 - 1]);
                    }
                    catch (Exception)
                    {
                        
                    }

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

            if (score >= 0 && score <= 100)
            {
                return score;
            }
            else
            {
                return 50;
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
                case 0:
                    return "Other";
                case 1:
                    return "Liberal";
                case 2:
                    return "Conservative";
                case 3:
                    return "NDP";
                case 4:
                    return "Bloc Quebecois";
                case 5:
                    return "Green Party";
                default:
                    return null;
            }
        }

        private static string ConvertPartyName(string name)
        {
            switch (name)
            {
                case "Liberal":
                    return "Liberal";
                case "Conservative":
                    return "Conservative";
                case "NDP-New Democratic Party":
                    return "NDP";
                case "Green Party":
                    return "Green Party";                    
                default:
                    if (name.Contains("Bloc"))
                    {
                        return "Bloc Quebecois";
                    }
                    return "Other";
            }
        }
    }
}
