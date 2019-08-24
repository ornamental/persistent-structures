using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersistentCollections;

namespace PersistentCollectionsTest
{
    [TestClass]
    public class PersistentQueueTest
    {
        [DynamicData("EmptyQueues")]
        [DataTestMethod]
        public void EmptyQueueTest(IPersistentQueue<int> empty)
        {
            CheckEmptyBehaviour(empty);
            Assert.AreNotSame(empty, empty.Enqueue(1));
        }

        [TestMethod]
        public void CreateQueueTest()
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);
            IPersistentQueue<int> queue = PersistentQueue<int>.Of(items);

            int itemsLeft = count;
            foreach (int expectedItem in items)
            {
                Assert.AreEqual(itemsLeft, queue.Count);

                int head;
                (head, queue) = queue.Dequeue();
                itemsLeft--;

                Assert.AreEqual(expectedItem, head);
            }

            CheckEmptyBehaviour(queue);
        }

        [DynamicData("EmptyQueues")]
        [DataTestMethod]
        public void EnqueueDequeueTest(IPersistentQueue<int> empty)
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);

            IPersistentQueue<int> queue = empty;
            foreach (int item in items)
            {
                int countBefore = queue.Count;

                queue = queue.Enqueue(item);
                Assert.AreEqual(countBefore + 1, queue.Count);

                (int head, IPersistentQueue<int> tail) = queue.Dequeue();
                Assert.AreEqual(items.First(), head);
                Assert.AreEqual(countBefore, tail.Count);
            }

            foreach (int item in items)
            {
                int countBefore = queue.Count;

                int head;
                (head, queue) = queue.Dequeue();
                Assert.AreEqual(item, head);
            }

            CheckEmptyBehaviour(queue);
        }

        [TestMethod]
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
                    Assert.IsTrue(enumerator.MoveNext());
                    Assert.AreEqual(item, enumerator.Current);
                }
                Assert.IsFalse(enumerator.MoveNext());
            }
        }

        public static IEnumerable<object[]> EmptyQueues
        {
            get
            {
                yield return new object[] { PersistentQueue<int>.Empty };
                yield return new object[] { PersistentQueue<int>.Of(Enumerable.Empty<int>()) };
            }
        }

        private static void CheckEmptyBehaviour<T>(IPersistentQueue<T> queue)
        {
            Assert.AreEqual(0, queue.Count);
            Assert.IsFalse(queue.GetEnumerator().MoveNext());
            Assert.ThrowsException<InvalidOperationException>(() => queue.Dequeue());
        }
    }
}
