using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380
{
    public abstract class Multiset<T> : IEnumerable<T>
    {
        public abstract int ElementCount(T element);
        public abstract void SetElementCount(T element, int count);

        public int this[T x]
        {
            get
            {
                return ElementCount(x);
            }
            set
            {
                SetElementCount(x, value);
            }
        }

        public bool NextMultiset(List<T> allElements, int weight)
        {
            if (Weight == 0)
            {
                this[allElements[0]] = weight;
                return true;
            }

            if (this[allElements[0]] != 0)
            {
                this[allElements[0]]--;
                this[allElements[1]]++;    
            }
            else
            {
                int i = 1;

                while (this[allElements[i]] == 0)
                {
                    i++;
                }

                int y = this[allElements[i]];
                this[allElements[i]] = 0;
                this[allElements[i + 1]]++;
                this[allElements[0]] = y - 1;
            }

            return !(this[allElements.Last()] == weight);

            //if (this[allElements[0]] > 0)
            //{
            //    this[allElements[0]]--;
            //    this[allElements[1]]++;
            //}
            //else
            //{
            //    int i = 1;

            //    while (this[allElements[i]] == 0)
            //    {
            //        i++;
            //    }

            //    while (true)
            //    {
            //        if ((i + 1) == allElements.Count)
            //        {
            //            return false;
            //        }

            //        var old = this[allElements[i]];

            //        this[allElements[i]] = 0;
            //        this[allElements[i + 1]]++;

            //        if (this[allElements[i + 1]] != weight)
            //        {
            //            i++;   
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}

            //this[allElements[0]] = 0;
            //this[allElements[0]] = weight - Weight;

            //return true;
        }

        public static Multiset<T> SampleMultisetUniformNaive(List<T> elements, int count)
        {
            var result = new HashMultiset<T>();

            for (int i = 0; i < count; i++)
            {
                result[elements[Rand.NextInt(0, elements.Count)]]++;
            }

            return result;
        }

        /// <summary>
        /// Returns the number of multisets that meet the given criteria.
        /// </summary>
        /// <param name="n">Number of elements in multiset</param>
        /// <param name="k">Length of multiset.</param>
        /// <returns></returns>
        public static Int64 Multichoose(int n, int k)
        {
            // http://mathworld.wolfram.com/Multichoose.html

            return MultinomialCoEfficient(n - 1, k);
        }

        public static Int64 MultinomialCoEfficient(int m, int n)
        {
            // http://mathworld.wolfram.com/MultinomialCoefficient.html
            return BinomialCoEfficient(m + n, n);
        }

        public static Int64 BinomialCoEfficient(int n, int k)
        {
            // http://mathworld.wolfram.com/BinomialCoefficient.html
            return Factorial(n) / (Factorial(n - k) * Factorial(k));
        }

        public static Int64 Factorial(int x)
        {
            int result = 1;

            for (int i = 1; i <= x; i++)
            {
                result *= i;
            }

            return result;
        }

        public static IEnumerable<Multiset<T>> GetAllMultisets(List<T> elements, int maxWeightPerElement)
        {
            int[] indicies = new int[elements.Count];

            while (true)
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    indicies[i]++;

                    if (indicies[i] > maxWeightPerElement)
                    {
                        indicies[i] = 0;

                        if (i == elements.Count - 1)
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                var resultMset = new HashMultiset<T>();

                for (int i = 0; i < elements.Count; i++)
                {
                    resultMset[elements[i]] = indicies[i];
                }

                yield return resultMset;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var x in this)
            {
                builder.Append(x);
                builder.Append(":");
                builder.Append(this[x]);
                builder.Append(" ");
            }

            builder.Append("|");
            builder.Append(Weight);

            return builder.ToString();
        }

        public int Weight
        {
            get
            {
                return (from x in this select this[x]).Sum();
            }
        }

        public abstract IEnumerable<T> getEnumerator();

        public IEnumerator<T> GetEnumerator()
        {
            return getEnumerator().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in getEnumerator())
            {
                yield return item;
            }
        }
    }
}
