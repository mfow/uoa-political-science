using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CS380
{
    public class CumulativeProbabilityDistribution
    {
        private class Element
        {
            public Element()
            {
                NextElement = null;
            }

            public Int64 Index { get; set; }
            public double Weight { get; set; }

            public Element NextElement { get; set; }
        }

        private class SkipListElement
        {
            public Element Reference { get; set; }
            public SkipListElement Next { get; set; }
        }

        private class SkipList
        {
            public SkipListElement StartElement { get; set; }
            public int ModuloPrime { get; set; }

            public Element GetHighestElementLowerThan(Int64 elementId)
            {
                SkipListElement prev = null;
                SkipListElement current = StartElement;

                while (current != null)
                {
                    if (current.Reference.Index >= elementId)
                    {
                        break;
                    }

                    prev = current;
                    current = current.Next;
                }

                if (prev == null)
                {
                    return null;
                }
                else
                {
                    return prev.Reference;
                }
            }

            public void AddIntoSkipList(Element x)
            {
                SkipListElement prev = null;
                SkipListElement current = StartElement;

                while (current != null)
                {
                    if (current.Reference.Index > x.Index)
                    {
                        break;    
                    }

                    prev = current;
                    current = current.Next;
                }

                var newElement = new SkipListElement();

                newElement.Reference = x;
                newElement.Next = current;

                if (prev == null)
                {
                    StartElement = newElement;
                }
                else
                {
                    prev.Next = StartElement;
                }
            }
        }

        private Int64 elementCount;
        private double sum;
        private Element startElement;
        private int elementsAddedCount; // Represents the number of elements added. We use this in our skip list calculations.
        private List<SkipList> skipLists = new List<SkipList>();

        private TreeNode root;

        private class TreeNode
        {
            public Int64 StartIndex { get; set; }
            public Int64 Length { get; set; }
            public double AddedWeight { get; set; }

            public TreeNode Parent { get; set; }
            public TreeNode ChildLeft { get; set; }
            public TreeNode ChildRight { get; set; }

            public double Weight
            {
                get
                {
                    return AddedWeight + (double)Length;
                }
            }

            public bool ContainsElement(Int64 elementId)
            {
                return StartIndex >= elementId && StartIndex + Length < elementId;
            }

            public void Split()
            {
                if (ChildLeft != null || ChildRight != null || AddedWeight > 0.0)
                {
                    throw new Exception();
                }

                var midLength = Length / 2;
                
                ChildLeft = new TreeNode() { StartIndex = StartIndex, Length = midLength, Parent = this };
                ChildRight = new TreeNode() { StartIndex = StartIndex + midLength, Length = Length - midLength, Parent = this };
            }

            public TreeNode GetNodeById(Int64 elementId)
            {
                if (this.StartIndex == elementId && this.Length == 1)
	            {
		            return this;
	            }

                if (ChildLeft == null || ChildRight == null)
	            {
		            Split();
	            }

                if (ChildRight.StartIndex > elementId)
                {
		            return ChildLeft.GetNodeById(elementId);
                }
                else
	            {
                    return ChildRight.GetNodeById(elementId);
	            }
            }

            public Tuple<TreeNode, double> SkipAheadByWeight(double weight)
            {
                double weightRemaining = weight;

                if (ChildLeft != null)
                {
                    if (ChildLeft.Weight < weightRemaining)
                    {
                        weightRemaining -= ChildLeft.Weight;
                    }
                    else
                    {
                        return ChildLeft.SkipAheadByWeight(weightRemaining);
                    }
                }

                if (ChildRight != null)
                {
                    if (ChildRight.Weight < weightRemaining)
                    {
                        weightRemaining -= ChildRight.Weight;
                    }
                    else
                    {
                        return ChildRight.SkipAheadByWeight(weightRemaining);
                    }                    
                }

                return new Tuple<TreeNode,double>(this, weight);
            }

            public void AddWeightToNode(double weight)
            {
                AddedWeight += weight;

                if (Parent != null)
                {
                    Parent.AddWeightToNode(weight);
                }
            }

            public TreeNode GetSmallestNodeContainingElement(Int64 elementId)
            {
                if (ChildLeft != null && ChildLeft.ContainsElement(elementId))
                    return ChildLeft.GetSmallestNodeContainingElement(elementId);

                if (ChildRight != null && ChildRight.ContainsElement(elementId))
                    return ChildRight.GetSmallestNodeContainingElement(elementId);

                return this;
            }
        }

        public CumulativeProbabilityDistribution(Int64 elementCount)
        {
            this.elementCount = elementCount;

            //var lastElement = new Element() { Index = elementCount - 1, Weight = 1.0 };
            //startElement = new Element() { Index = 0, Weight = 1.0, NextElement = lastElement };

            startElement = new Element() { Index = elementCount - 1, Weight = 1.0 }; ;

            sum = elementCount;
            elementsAddedCount = 0;

            skipLists = new List<SkipList>();

            skipLists.Add(new SkipList() { ModuloPrime = 337, StartElement = new SkipListElement() { Next = null, Reference = startElement } });

            root = new TreeNode();
            root.StartIndex = 0;
            root.Length = elementCount;
        }

        public Int64 Sample()
        {
            var x = Rand.NextDouble() * sum;

            var containingNode = root.SkipAheadByWeight(x);

            if (containingNode.Item1.Length == 1)
            {
                return containingNode.Item1.StartIndex;
            }
            else
            {
                var y = containingNode.Item1.GetNodeById(containingNode.Item1.StartIndex + (Int64)containingNode.Item2);

                return y.StartIndex;
            }
        }

        public void AddWeightByIndex(Int64 index, double weight)
        {
            var node = root.GetNodeById(index);
            
            node.AddWeightToNode(weight);

            sum += weight;
        }


        //public CumulativeProbabilityDistribution(Int64 elementCount)
        //{
        //    this.elementCount = elementCount;

        //    //var lastElement = new Element() { Index = elementCount - 1, Weight = 1.0 };
        //    //startElement = new Element() { Index = 0, Weight = 1.0, NextElement = lastElement };

        //    startElement = new Element() { Index = elementCount - 1, Weight = 1.0 }; ;

        //    sum = elementCount;
        //    elementsAddedCount = 0;

        //    skipLists = new List<SkipList>();

        //    skipLists.Add(new SkipList() { ModuloPrime = 337, StartElement = new SkipListElement() { Next = null, Reference = startElement } });
        //}

        //public Int64 Sample()
        //{
        //    var x = Rand.NextDouble() * sum;
        //    int i = 0;
        //    Int64 lastElement = 0;

        //    Element element = startElement;

        //    while (true)
        //    {
        //        var elementsSkipped = element.Index - lastElement;

        //        if (x < elementsSkipped)
        //        {
        //            return lastElement + (int)x;
        //        }

        //        x -= elementsSkipped;

        //        if (x < element.Weight)
        //        {
        //            return element.Index;
        //        }

        //        x -= element.Weight;

        //        lastElement = element.Index;

        //        element = element.NextElement;
        //    }
        //}

        //public void AddWeightByIndex(Int64 index, double weight)
        //{
        //    sum += weight;
        //    GetOrAddElement(index).Weight += weight;
        //}

        //private Element GetOrAddElement(Int64 elementId)
        //{
        //    Element last = null;
        //    Element current;

        //    //current = startElement;
        //    current = skipLists[0].GetHighestElementLowerThan(elementId);

        //    if (current == null)
        //    {
        //        current = startElement;
        //    }
        //    else
        //    {
        //        int test = 1;
        //    }

        //    while (true)
        //    {
        //        if (current.Index == elementId)
        //        {
        //            return current;
        //        }
        //        else if (current.Index > elementId)
        //        {
        //            var newElement = new Element();

        //            newElement.Index = elementId;
        //            newElement.Weight = 1.0;

        //            if (last == null)
        //            {
        //                startElement = newElement;
        //            }
        //            else
        //            {
        //                last.NextElement = newElement;
        //            }
                    
        //            newElement.NextElement = current;

        //            elementsAddedCount++;

        //            foreach (var item in skipLists)
        //            {
        //                if (elementsAddedCount % item.ModuloPrime == 0)
        //                {
        //                    item.AddIntoSkipList(newElement);
        //                }
        //            }

        //            return newElement;
        //        }

        //        last = current;
        //        current = current.NextElement;
        //    }

        //    //return GetOrAddElement(elementId, 0, elements.Count);
        //}

        //private Element GetOrAddElement(Int64 element, int begin, int end)
        //{
        //    if (end - begin <= 10)
        //    {
        //        for (int i = 0; i < elements.Count; i++)
        //        {
        //            if (elements[i].Index == element)
        //            {
        //                return elements[i];
        //            }
        //            else if (elements[i].Index > element)
        //            {
        //                var result = new Element();

        //                result.Index = element;
        //                result.Weight = 1.0;

        //                elements.Insert(i, result);

        //                return result;
        //            }
        //        }

        //        throw new Exception();
        //    }
        //    else
        //    {
        //        var mid = (begin + end) / 2;

        //        if (elements[mid].Index == element)
        //        {
        //            return elements[mid];
        //        }
        //        else if (elements[mid].Index < element)
        //        {
        //            return GetOrAddElement(element, begin, mid);
        //        }
        //        else
        //        {
        //            return GetOrAddElement(element, mid, end);
        //        }
        //    }
        //}
    }
}
