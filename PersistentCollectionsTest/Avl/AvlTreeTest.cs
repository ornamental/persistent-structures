using PersistentCollections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PersistentCollectionsTest.Avl
{
    public class AvlTreeTest
    {
        private class DeficientCollection : IReadOnlyCollection<int>
        {
            public int Count => 10;

            // one element short of the reported count
            public IEnumerator<int> GetEnumerator() => Enumerable.Range(0, Count - 1).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class NegativeCountCollection : IReadOnlyCollection<int>
        {
            public int Count => -1;

            public IEnumerator<int> GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void IncorrectSourceCollectionCountTest()
        {
            Assert.Throws<ArgumentException>(
                () => PersistentList<int>.OfReadonly(new DeficientCollection()));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PersistentList<int>.OfReadonly(new NegativeCountCollection()));
        }
    }
}
