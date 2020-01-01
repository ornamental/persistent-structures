using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PersistentCollections;
using Xunit;

namespace PersistentCollectionsTest
{
    public class PersistentQueueTest
    {
        [Theory]
        [MemberData(nameof(EmptyQueues))]
        public void EmptyQueueTest(IPersistentQueue<int> empty)
        {
            CheckEmptyBehaviour(empty);
            Assert.NotSame(empty, empty.Enqueue(1));
        }

        [Fact]
        public void CreateQueueTest()
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);
            IPersistentQueue<int> queue = PersistentQueue<int>.Of(items);

            int itemsLeft = count;
            foreach (int expectedItem in items)
            {
                Assert.Equal(itemsLeft, queue.Count);

                int head;
                (head, queue) = queue.Dequeue();
                itemsLeft--;

                Assert.Equal(expectedItem, head);
            }

            CheckEmptyBehaviour(queue);
        }

        [Theory]
        [MemberData(nameof(EmptyQueues))]
        public void EnqueueDequeueTest(IPersistentQueue<int> empty)
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);

            IPersistentQueue<int> queue = empty;
            foreach (int item in items)
            {
                int countBefore = queue.Count;

                queue = queue.Enqueue(item);
                Assert.Equal(countBefore + 1, queue.Count);

                (int head, IPersistentQueue<int> tail) = queue.Dequeue();
                Assert.Equal(items.First(), head);
                Assert.Equal(countBefore, tail.Count);
            }

            foreach (int item in items)
            {
                int countBefore = queue.Count;

                int head;
                (head, queue) = queue.Dequeue();
                Assert.Equal(item, head);
            }

            CheckEmptyBehaviour(queue);
        }

        [Fact]
        public void EnumeratorTest()
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);

            IPersistentQueue<int> queue = PersistentQueue<int>.Empty;
            foreach (int item in items)
            {
                queue = queue.Enqueue(item);
            }

            foreach (IEnumerator enumerator in
                new IEnumerator[] { queue.GetEnumerator(), ((IEnumerable)queue).GetEnumerator() })
            {
                foreach (int item in items)
                {
                    Assert.True(enumerator.MoveNext());
                    Assert.Equal(item, enumerator.Current);
                }
                Assert.False(enumerator.MoveNext());
            }
        }

        public static IEnumerable<object[]> EmptyQueues
        {
            get
            {
                return TestUtilities.Singletons(
                    PersistentQueue<int>.Empty,
                    PersistentQueue<int>.Of(Enumerable.Empty<int>()));
            }
        }

        private static void CheckEmptyBehaviour<T>(IPersistentQueue<T> queue)
        {
            Assert.Equal(0, queue.Count);
            Assert.False(queue.GetEnumerator().MoveNext());
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }
    }
}
