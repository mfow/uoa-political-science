using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ElectionSystem
{
    public class PreferenceOrder : IEquatable<PreferenceOrder>
    {
        public List<Party> Preferences { get; private set; }

        public PreferenceOrder()
        {
            Preferences = new List<Party>();
        }

        public PreferenceOrder(IEnumerable<Party> preferences) : this()
        {
            this.Preferences.AddRange(preferences);
        }

        public override bool Equals(object obj)
        {
            if (obj is PreferenceOrder)
            {
                var B = obj as PreferenceOrder;

                if (this.Preferences.Count != B.Preferences.Count)
                {
                    return false;
                }

                for (int i = 0; i < this.Preferences.Count; i++)
                {
                    if (this.Preferences[i] != B.Preferences[i])
                    {
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(PreferenceOrder other)
        {
            return Equals(other as object);
        }

        public override int GetHashCode()
        {
            var builder = new StringBuilder();

            foreach (var item in Preferences)
            {
                if (item != null)
                {
                    builder.Append(item.Name);
                }
                else
                {
                    builder.Append("null");
                }

                builder.Append(" ");
            }

            return builder.ToString().GetHashCode();
        }
    }

    public class SurveySample
    {
        public double Weight { get; set; }

        public PreferenceOrder Preferences { get; private set; }

        public Party VotedFor { get; set; }

        public SurveySample()
        {
            Preferences = new PreferenceOrder();
        }

        /// <summary>
        /// Fills in preferences with party scores.
        /// NOTE: If a party is listed multiple times, the last value is counted.
        /// </summary>
        /// <param name="list"></param>
        public void FillPreferencesWithScores(List<Tuple<Party, int?>> list)
        {
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var data = new byte[8 * list.Count];

                rng.GetBytes(data);

                var strm = new System.IO.MemoryStream(data);
                strm.Seek(0, System.IO.SeekOrigin.Begin);

                var r = new System.IO.BinaryReader(strm);

                var parties = (from x in list select x.Item1).Distinct().ToList();
                
                var partyByScore = from p in parties
                                   let scores = (from y in list where y.Item2.HasValue && y.Item1 == p select y.Item2)
                                   let rValue = r.ReadDouble()
                                   where scores.Count() > 0
                                   orderby scores.Last() descending, rValue
                                   select p;

                foreach (var item in parties)
                {
                    if (item == null)
                    {
                        throw new Exception();
                    }
                }

                Preferences.Preferences.Clear();

                foreach (var p in partyByScore)
                {
                    Preferences.Preferences.Add(p);
                }
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Weight);

            builder.Append(" ");
            builder.Append(VotedFor);
            builder.Append(":");

            foreach (var p in Preferences.Preferences)
            {
                builder.Append(" ");
                builder.Append(p);
            }

            return builder.ToString();
        }
    }

    public class Survey
    {
        public List<SurveySample> Samples { get; private set; }
        public Dictionary<string, Party> Parties { get; private set; }
        private Dictionary<Party, List<Tuple<PreferenceOrder, double>>> preferenceConditionalProbability;

        public Survey()
        {
            Samples = new List<SurveySample>();
            Parties = new Dictionary<string, Party>();
            preferenceConditionalProbability = new Dictionary<Party, List<Tuple<PreferenceOrder, double>>>();
        }

        public Party GetPartyByName(string name)
        {
            if (name == null)
            {
                return null;
            }

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

        public void Calculate()
        {
            //preferenceConditionalProbability.Clear();

            //var preferenceOrders = (from x in Samples select x.Preferences).Distinct().ToList();
            
            //foreach (var p in Parties)
            //{
            //    //var preferenceOrdersAndWeight = (from x in preferenceOrders
            //    //                                let weight = (from y in Samples where y.VotedFor == p.Value && y.Preferences.Equals(x) select y.Weight).Sum()
            //    //                                select new Tuple<PreferenceOrder, double>(x, weight)).ToList();

            //    var preferenceOrdersAndWeight = (from x in preferenceOrders
            //                                     let weight = (from y in Samples where y.Preferences.Preferences.First() == p.Value && y.Preferences.Equals(x) select y.Weight).Sum()
            //                                     select new Tuple<PreferenceOrder, double>(x, weight)).ToList();

            //    var prefOrders = (from x in preferenceOrdersAndWeight where x.Item2 > 0.0 orderby x.Item2 descending select x).ToList();
            //    var totalWeight = (from x in prefOrders select x.Item2).Sum();

            //    // Conditional probabilty for a given preference order given a vote for a particular party.
            //    var prefsConditionalProbability = (from x in prefOrders select new Tuple<PreferenceOrder, double>(x.Item1, x.Item2 / totalWeight)).ToList();

            //    preferenceConditionalProbability.Add(p.Value, prefsConditionalProbability);
            //}
        }

        public void SaveToStream(Stream strm)
        {
            var data = new List<IEnumerable<string>>();

            var partiesAlphabetical = (from x in Parties orderby x.Key ascending select x.Value).ToList();
            
            var preferenceOrders = (from x in Samples select x.Preferences).Distinct().ToList();
            var preferenceOrdersByWeight = (from x in preferenceOrders
                                           let weight = (from y in Samples where y.Preferences.Equals(x) select y.Weight).Sum()
                                           select new Tuple<PreferenceOrder, double>(x, weight)).ToList();

            var weightTotalByParty = (from p in partiesAlphabetical
                                     let weight = (from order in preferenceOrdersByWeight
                                                   where order.Item1.Preferences.First() == p
                                                   select order.Item2).Sum()
                                     select weight).ToList();

            data.Add(new string[] { Parties.Count.ToString() });
            data.Add(new string[] { preferenceOrdersByWeight.Count.ToString() });

            foreach (var order in preferenceOrdersByWeight)
            {
                var row = new List<string>();

                for (int i = 0; i < Parties.Count; i++)
                {
                    row.Add(partiesAlphabetical.IndexOf(order.Item1.Preferences[i]).ToString());
                }

                double probability = order.Item2 / weightTotalByParty[partiesAlphabetical.IndexOf(order.Item1.Preferences[0])];

                row.Add(probability.ToString());

                data.Add(row);
            }


            /*
            foreach (var p in partiesAlphabetical)
            {
                Console.WriteLine(p.Name);

                var row = new List<string>();

                var orders = EnumeratePreferenceOrders(new Party[] { p }, partiesAlphabetical);
                var ordersByWeight = (from x in orders
                                     let weight = (from y in preferenceOrdersByWeight where y.Item1.Equals(x) select y.Item2).Sum()
                                     select new Tuple<PreferenceOrder, double>(x, weight)).ToList();

                var weightTotal = (from x in ordersByWeight select x.Item2).Sum();
                Console.WriteLine(weightTotal);

                var ordersByProbability = (from x in ordersByWeight select new Tuple<PreferenceOrder, double>(x.Item1, x.Item2 / weightTotal)).ToList();
                Console.WriteLine(ordersByProbability.Count);

                foreach (var order in ordersByProbability) 
                {
                    //// Check if there is any value for this particular order...
                    //var probability = (from x in preferenceConditionalProbability[p] where x.Item1.Equals(order) select x).ToList();

                    //double conditionalProbability = probability.Count == 0 ? 0.0 : probability.Single().Item2;

                    double conditionalProbability = order.Item2;

                    row.Add(conditionalProbability.ToString());
                }

                data.Add(row);
            }
            */

            CSV.WriteToStream(strm, data);
        }

        private static IEnumerable<PreferenceOrder> EnumeratePreferenceOrders(IEnumerable<Party> start, IEnumerable<Party> list)
        {
            var listWithoutStart = (from w in list where !start.Contains(w) select w).ToList();

            foreach (var x in listWithoutStart)
            {
                var start2 = start.ToList();
                start2.Add(x);

                var list2 = listWithoutStart.ToList();
                list2.Remove(x);

                if (list2.Count == 0)
                {
                    yield return new PreferenceOrder(start2);
                }

                foreach (var y in EnumeratePreferenceOrders(start2, list2))
                {
                    yield return y;
                }
            }
        }
    }
}
