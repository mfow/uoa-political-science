using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380
{
    public class HashMultiset<T> : Multiset<T>
    {
        private Dictionary<T, int> elementCounts;

        public HashMultiset()
        {
            elementCounts = new Dictionary<T, int>();
        }

        public HashMultiset(Multiset<T> baseSet)
        {
            elementCounts = new Dictionary<T, int>();

            foreach (var x in baseSet)
            {
                this[x] = baseSet[x];
            }
        }

        public override int ElementCount(T element)
        {
            return elementCounts.ContainsKey(element) ? elementCounts[element] : 0;
        }

        public override void SetElementCount(T element, int count)
        {
            if (elementCounts.ContainsKey(element))
            {
                elementCounts[element] = count;
            }
            else
            {
                elementCounts.Add(element, count);
            }
        }

        public override IEnumerable<T> getEnumerator()
        {
            foreach (var x in elementCounts)
            {
                if (x.Value != 0)
                {
                    yield return x.Key;
                }
            }
        }
    }
}
