using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersistentCollections;

namespace PersistentCollectionsTest
{
    [TestClass]
    public class PersistentStackTest
    {
        [DynamicData("EmptyStacks")]
        [DataTestMethod]
        public void EmptyStackTest(IPersistentStack<int> empty)
        {
            CheckEmptyBehaviour(empty);
            Assert.AreNotSame(empty, empty.Push(1));
        }

        [TestMethod]
        public void CreateStackTest()
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);
            IEnumerable<int> reversedItems = items.Reverse();

            IPersistentStack<int> stack = PersistentStack<int>.Of(items);

            int itemsLeft = count;
            foreach (int expectedItem in reversedItems)
            {
                Assert.AreEqual(itemsLeft, stack.Count);

                int head;
                (head, stack) = stack.Pop();
                itemsLeft--;

                Assert.AreEqual(expectedItem, head);
            }

            CheckEmptyBehaviour(stack);
        }

        [DynamicData("EmptyStacks")]
        [DataTestMethod]
        public void PushPopTest(IPersistentStack<int> empty)
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);
            IEnumerable<int> reversed = items.Reverse();

            IPersistentStack<int> stack = empty;
            foreach (int item in items)
            {
                int countBefore = stack.Count;

                stack = stack.Push(item);
                Assert.AreEqual(countBefore + 1, stack.Count);

                (int head, IPersistentStack<int> tail) = stack.Pop();
                Assert.AreEqual(item, head);
                Assert.AreEqual(countBefore, tail.Count);
            }

            foreach (int item in reversed)
            {
                int countBefore = stack.Count;

                int head;
                (head, stack) = stack.Pop();
                Assert.AreEqual(item, head);
            }

            CheckEmptyBehaviour(stack);
        }

        [TestMethod]
        public void EnumeratorTest()
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);
            IEnumerable<int> reversed = items.Reverse();

            IPersistentStack<int> stack = PersistentStack<int>.Of(items);

            foreach (IEnumerator enumerator in
                new IEnumerator[] { stack.GetEnumerator(), ((IEnumerable)stack).GetEnumerator()})
            {
                foreach (int item in reversed)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                    Assert.AreEqual(item, enumerator.Current);
                }
                Assert.IsFalse(enumerator.MoveNext());
            }
        }

        public static IEnumerable<object[]> EmptyStacks
        {
            get
            {
                yield return new object[] { PersistentStack<int>.Empty };
                yield return new object[] { PersistentStack<int>.Of(Enumerable.Empty<int>()) };
            }
        }

        private static void CheckEmptyBehaviour<T>(IPersistentStack<T> stack)
        {
            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(stack.GetEnumerator().MoveNext());
            Assert.ThrowsException<InvalidOperationException>(() => stack.Pop());
        }
    }
}
