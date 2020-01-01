using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PersistentCollections;
using Xunit;

namespace PersistentCollectionsTest
{
    public class PersistentListTest
    {
        [Fact]
        public void AddFirstTest()
        {
            const int count = 100;

            IPersistentList<int> list = PersistentList<int>.Empty;

            foreach (int i in Enumerable.Range(1, count))
            {
                list = list.Add(0, count - i + 1);
                Assert.Equal(i, list.Count);

                int index = 0;
                foreach (int j in list)
                {
                    Assert.Equal(count - i + index + 1, j);
                    index++;
                }
            }
        }

        [Fact]
        public void AddLastTest()
        {
            const int count = 100;

            IPersistentList<int> list = PersistentList<int>.Empty;
            Assert.Equal(0, list.Count);

            foreach (int i in Enumerable.Range(1, count))
            {
                list = list.Add(i);
                Assert.Equal(i, list.Count);

                int index = 0;
                foreach (int j in list)
                {
                    Assert.Equal(index + 1, j);
                    index++;
                }
            }
        }

        [Theory]
        [MemberData(nameof(InsertionRemovalCases))]
        public void AddRemoveAtTest(InsertRemoveScenario scenario) => AddRemoveAtTestImpl(scenario);

        [Theory]
        [MemberData(nameof(RandomInsertionRemovalCases))]
        public void AddRemoveAtTestRandomized(InsertRemoveScenario scenario) => AddRemoveAtTestImpl(scenario);

        [Fact]
        public void AddOutOfRangeTest()
        {
            const int count = 100;

            IPersistentList<int> list = PersistentList<int>.Empty;
            foreach (int i in Enumerable.Range(1, count))
            {
                list = list.Add(i);
            }

            Assert.Throws<IndexOutOfRangeException>(() => list.Add(-1, 1));
            Assert.Throws<IndexOutOfRangeException>(() => list.Add(count + 1, 1));
        }

        [Theory]
        [MemberData(nameof(EmptyLists))]
        public void EmptyListTest(IPersistentList<int> empty)
        {
            CheckEmptyListBehaviour(empty);
            Assert.NotSame(empty, empty.Add(1));
        }

        [Fact]
        public void CreateListTest()
        {
            const int maxLength = 100;

            for (int i = 0; i < maxLength; i++)
            {
                int[] collection = Enumerable.Range(0, i).ToArray();
                CheckListEquality(collection, PersistentList<int>.Of(collection));
                CheckListEquality(collection, PersistentList<int>.OfReadonly(collection));
            }
        }

        [Fact]
        public void SetItemTest()
        {
            const int count = 200;

            IPersistentList<int> list = PersistentList<int>.Empty;
            for (int i = 0; i < count; i++)
            {
                list = list.Add(i);
            }
            IPersistentList<int> list2 = list;

            for (int i = 0; i < count; i++)
            {
                list = list.SetValue(i, j => j + 1, out int oldValue);
                Assert.Equal(i, oldValue);
                list2 = list2.SetValue(i, i + 1, out oldValue);
                Assert.Equal(i, oldValue);
            }

            CheckListEquality(Enumerable.Range(1, count).ToArray(), list);
            CheckListEquality(Enumerable.Range(1, count).ToArray(), list2);
        }

        [Fact]
        public void RemoveOutOfRangeTest()
        {
            const int count = 10;

            IPersistentList<int> list = PersistentList<int>.Of(new int[count]);
            Assert.Throws<IndexOutOfRangeException>(() => list.RemoveAt(-1, out _));
            Assert.Throws<IndexOutOfRangeException>(() => list.RemoveAt(count, out _));
        }

        [Fact]
        public void GetItemOutOfRangeTest()
        {
            const int count = 10;

            IPersistentList<int> list = PersistentList<int>.Of(new int[count]);
            Assert.Throws<IndexOutOfRangeException>(() => list[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => list[count]);
        }

        [Fact]
        public void SetItemOutOfRangeTest()
        {
            const int count = 10;

            IPersistentList<int> list = PersistentList<int>.Of(new int[count]);
            Assert.Throws<IndexOutOfRangeException>(() => list.SetValue(-1, -1, out _));
            Assert.Throws<IndexOutOfRangeException>(() => list.SetValue(count, -1, out _));
        }

        public static IEnumerable<object[]> EmptyLists
        {
            get
            {
                return TestUtilities.Singletons(
                    PersistentList<int>.Empty,
                    PersistentList<int>.Of(new List<int>()),
                    PersistentList<int>.OfReadonly(new List<int>()));
            }
        }

        public static IEnumerable<object[]> InsertionRemovalCases
        {
            get
            {
                return TestUtilities.Singletons(
                    new InsertRemoveScenario(new int[] { 0, 0, 2, 0, 2, 3 }),
                    new InsertRemoveScenario(new int[] { 0, 1, 1, 0, 2, 3, 6, 0, 8 }),
                    new InsertRemoveScenario(new int[] { 0, 1, 2, 0, 0, 3 }, new int[] { 5, 3, 3, 1, 1, 0 }),
                    new InsertRemoveScenario(new int[] { 0, 1, 2, 0, 0, 3 }, new int[] { 2, 4, 3, 0, 0, 0 }));
            }
        }

        public static IEnumerable<object[]> RandomInsertionRemovalCases()
        {
            int caseCount = 1024;
            int maxElements = 64;

            Random rnd = new Random(42);

            for (int i = 0; i < caseCount; i++)
            {
                int count = rnd.Next(maxElements);

                yield return new object[]
                {
                    new InsertRemoveScenario(
                        Enumerable.Range(0, count).Select(j => rnd.Next(j + 1)),
                        Enumerable.Range(0, count).Select(j => rnd.Next(count - j)))
                };
            }
        }

        private void CheckEmptyListBehaviour(IPersistentList<int> empty)
        {
            Assert.Equal(0, empty.Count);
            foreach (IEnumerator enumerator in
                new IEnumerator[] { empty.GetEnumerator(), ((IEnumerable)empty).GetEnumerator() })
            {
                Assert.False(enumerator.MoveNext());
            }
            Assert.Throws<IndexOutOfRangeException>(() => empty[0]);
            Assert.Throws<IndexOutOfRangeException>(() => empty.RemoveAt(0, out _));
        }

        private void AddRemoveAtTestImpl(InsertRemoveScenario scenario)
        {
            IPersistentList<int> list = PersistentList<int>.Empty;
            List<int> expected = new List<int>();

            int i = 0;
            foreach (int position in scenario.InsertionOrder)
            {
                list = list.Add(position, i);
                expected.Insert(position, i);
                CheckListEquality(expected, list);

                i++;
            }

            foreach (int position in scenario.RemovalOrder)
            {
                list = list.RemoveAt(position, out int removedValue);
                Assert.Equal(expected[position], removedValue);
                expected.RemoveAt(position);
                CheckListEquality(expected, list);
            }
        }

        private static void CheckListEquality<T>(IList<T> expected, IPersistentList<T> given)
        {
            Assert.Equal(expected.Count, given.Count);

            // check using indexer
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], given[i]);
            }

            // same check using enumerators
            foreach (IEnumerator enumerator in
                new IEnumerator[] { given.GetEnumerator(), ((IEnumerable)given).GetEnumerator() })
            {
                IEnumerator expectedEnumerator = expected.GetEnumerator();
                while (expectedEnumerator.MoveNext())
                {
                    Assert.True(enumerator.MoveNext());
                    Assert.Equal(expectedEnumerator.Current, enumerator.Current);
                }
                Assert.False(enumerator.MoveNext());
            }
        }
    }

    public class InsertRemoveScenario
    {
        public InsertRemoveScenario(IEnumerable<int> insertionOrder) : this(insertionOrder, new int[0]) { }

        public InsertRemoveScenario(IEnumerable<int> insertionOrder, IEnumerable<int> removalOrder)
        {
            InsertionOrder = insertionOrder;
            RemovalOrder = removalOrder;
        }

        public IEnumerable<int> InsertionOrder
        {
            get;
        }

        public IEnumerable<int> RemovalOrder
        {
            get;
        }

        public override string ToString()
        {
            string display = "Insertion order: " + string.Join(", ", InsertionOrder);
            if (RemovalOrder.Any())
            {
                display += Environment.NewLine + "Removal order: " + string.Join(", ", RemovalOrder);
            }

            return display;
        }
    }
}
