using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS380
{
    public static class DiscreteMath
    {
        public static double PolyaEggenberger<T>(Multiset<int> mset, List<T> parties, double alpha)
        {
            int m = parties.Count;
            double M = Factorial(m);
            int n = mset.Weight;

            double result;

            result = Factorial(n);
            // Divide by M(a,n)?
            //result /= C(

            for (int i = 0; i < M; i++)
            {
                var elementWeight = mset[i];

                result *= C(alpha, elementWeight);

                // Divide n[i]!
                //result /= 
            }

            return result;
        }

        private static double C(double alpha, int t)
        {
            double result = 1.0;

            for (int i = 0; i < t; i++)
            {
                result *= (1 + (((double)i) * alpha));
            }

            return result;
        }

        public static double Factorial(int x)
        {
            double y = 1.0;

            for (int i = 1; i <= x; i++)
            {
                y *= (double)i;
            }

            return y;
        }

        public static Int64 FactorialInt64(int x)
        {
            Int64 y = 1;

            for (int i = 1; i <= x; i++)
            {
                y *= (Int64)i;
            }

            return y;
        }

        public static IEnumerable<Multiset<T>> GetMultisets<T>(List<T> elements, int weight)
        {
            Multiset<T> mset = new HashMultiset<T>();

            while (true)
            {
                bool moreResults = mset.NextMultiset(elements, weight);

                yield return new HashMultiset<T>(mset);

                if (!moreResults)
                {
                    break;
                }
            }
        }

        public static IEnumerable<List<T>> Combinations<T>(List<T> list) where T : IEquatable<T>
        {
            int[] indicies = new int[list.Count];

            while (true)
            {
                for (int i = 0; i < indicies.Length; i++)
                {
                    indicies[i]++;

                    if (indicies[i] == 2)
                    {
                        indicies[i] -= 2;
                    }
                    else
                    {
                        break;
                    }

                    if (i == indicies.Length - 1)
                    {
                        yield break;
                    }
                }

                List<T> result = new List<T>();

                for (int i = 0; i < list.Count; i++)
                {
                    if (indicies[i] == 1)
                    {
                        result.Add(list[i]);
                    }
                }

                yield return result;
            }
        }

        public static List<int> GetIntList(int count)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < count; i++)
            {
                result.Add(i);
            }

            return result;
        }

        private static ConcurrentDictionary<int, List<List<int>>> permuteIntResults = new ConcurrentDictionary<int, List<List<int>>>();

        public static Int64 GetIndexByPermutation(List<int> vector)
        {
            Int64 result = 0;

            var indicies = GetIntList(vector.Count);

            for (int i = 0; i < vector.Count; i++)
            {
                result = result + indicies.IndexOf(vector[i]) * FactorialInt64(vector.Count - (i + 1));
                indicies.Remove(vector[i]);
            }

#if DEBUG
            // Verify result.
            var x = GetPermutationByIndex(result, vector.Count);

            for (int i = 0; i < vector.Count; i++)
            {
                if (x[i] != vector[i])
                {
                    throw new Exception();
                }
            }
#endif
   
            return result;
        }

        public static List<int> GetPermutationByIndex(Int64 index, int count)
        {
            List<int> result = new List<int>();

            var indicies = GetIntList(count);
            Int64 x = index;

            for (int i = 0; i < count; i++)
            {
                var f = FactorialInt64(indicies.Count - 1);
                var y = (int)(x / f);

                result.Add(indicies[y]);
                indicies.RemoveAt(y);

                x = x - y * f;
            }

            return result;
        }

        public static List<List<int>> PermuteIntegers(int count)
        {
            return permuteIntResults.GetOrAdd(count, (int c) =>
            {
                List<int> values = new List<int>();

                for (int i = 0; i < c; i++)
                {
                    values.Add(i);
                }

                return Permute(values).ToList();
            });
        }

        public static IEnumerable<List<T>> Permute<T>(List<T> list) where T : IEquatable<T>
        {
            if (list.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            foreach (var x in list)
            {
                var y = Permute((from z in list where !z.Equals(x) select z).ToList());

                foreach (var l in y)
                {
                    var l2 = l.ToList();

                    l2.Insert(0, x);

                    yield return l2;
                }
            }

            yield break;
        }

        public static Multiset<Int64> SampleMultisetUrn(Int64 elementCount, int setWeight, double alpha)
        {
            var result = new HashMultiset<Int64>();
            var urn = new CumulativeProbabilityDistribution(elementCount);

            for (int i = 0; i < setWeight; i++)
            {
                var index = urn.Sample();

                urn.AddWeightByIndex(index, alpha);

                result[index]++;
            }

            return result;
        }
    }
}
