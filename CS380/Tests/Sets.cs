using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CS380;

namespace Tests
{
    /// <summary>
    /// Summary description for Sets
    /// </summary>
    [TestClass]
    public class Sets
    {
        public Sets()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestFactorial()
        {
            Assert.AreEqual(1, Multiset<int>.Factorial(0));
            Assert.AreEqual(1, Multiset<int>.Factorial(1));
            Assert.AreEqual(2, Multiset<int>.Factorial(2));
            Assert.AreEqual(6, Multiset<int>.Factorial(3));
            Assert.AreEqual(5040, Multiset<int>.Factorial(7));
        }

        [TestMethod]
        public void TestMultichoose()
        {
            Assert.AreEqual(126, Multiset<int>.Multichoose(5, 5));
            Assert.AreEqual(35, Multiset<int>.Multichoose(4, 4));
            Assert.AreEqual(56, Multiset<int>.Multichoose(4, 5));
            Assert.AreEqual(1, Multiset<int>.Multichoose(1, 1));
            Assert.AreEqual(2, Multiset<int>.Multichoose(2, 1));
            Assert.AreEqual(1, Multiset<int>.Multichoose(1, 2));
            Assert.AreEqual(1, Multiset<int>.Multichoose(1, 5));
            Assert.AreEqual(20, Multiset<int>.Multichoose(20, 1));
            Assert.AreEqual(3, Multiset<int>.Multichoose(2, 2));
        }

        [TestMethod]
        public void TestNextMultiset()
        {
            for (int elementCount = 2; elementCount < 5; elementCount++)
            {
                for (int weight = 1; weight < 5; weight++)
                {
                    List<int> elements = new List<int>();

                    for (int i = 0; i < elementCount; i++)
                    {
                        elements.Add(i);
                    }

                    var allSets = Multiset<int>.GetAllMultisets(elements, weight).ToList();
                    var allSetsWithWeight = (from x in allSets where x.Weight == weight select x).ToList();

                    List<Multiset<int>> sets = new List<Multiset<int>>();

                    Multiset<int> tempSet = new HashMultiset<int>();

                    while (true)
                    {
                        bool moreResults = tempSet.NextMultiset(elements, weight);
                        sets.Add(new HashMultiset<int>(tempSet));

                        if (!moreResults)
                        {
                            break;
                        }
                    }

                    var set1 = (from x in sets let y = x.ToString() orderby y select y).ToList();
                    var set2 = (from x in allSetsWithWeight let y = x.ToString() orderby y select y).ToList();

                    for (int i = 0; i < Math.Min(set1.Count, set2.Count); i++)
                    {
                        Assert.AreEqual(set2[i], set1[i]);
                    }

                    Assert.AreEqual(set1.Count, set2.Count);
                }
            }
        }

        [TestMethod]
        public void TestPermutations()
        {
            for (int m = 1; m < 5; m++)
            {
                var perm = DiscreteMath.PermuteIntegers(m);

                var f = DiscreteMath.FactorialInt64(m);
                Assert.AreEqual(f, perm.Count);

                for (int i = 0; i < f; i++)
                {
                    var order = DiscreteMath.GetPermutationByIndex(i, m);

                    TestLists(perm[i], order);
                }
            }
        }

        private static void TestLists(List<int> a, List<int> b)
        {
            Assert.AreEqual(a.Count, b.Count);

            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i], b[i]);
            }
        }
    }
}
