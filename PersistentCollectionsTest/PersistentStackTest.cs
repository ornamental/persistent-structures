using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PersistentCollections;
using Xunit;

namespace PersistentCollectionsTest
{
    public class PersistentStackTest
    {
        [Theory]
        [MemberData(nameof(EmptyStacks))]
        public void EmptyStackTest(IPersistentStack<int> empty)
        {
            CheckEmptyBehaviour(empty);
            Assert.NotSame(empty, empty.Push(1));
        }

        [Fact]
        public void CreateStackTest()
        {
            int count = 100;

            IEnumerable<int> items = Enumerable.Range(0, count);
            IEnumerable<int> reversedItems = items.Reverse();

            IPersistentStack<int> stack = PersistentStack<int>.Of(items);

            int itemsLeft = count;
            foreach (int expectedItem in reversedItems)
            {
                Assert.Equal(itemsLeft, stack.Count);

                int head;
                (head, stack) = stack.Pop();
                itemsLeft--;

                Assert.Equal(expectedItem, head);
            }

            CheckEmptyBehaviour(stack);
        }

        [Theory]
        [MemberData(nameof(EmptyStacks))]
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
                Assert.Equal(countBefore + 1, stack.Count);

                (int head, IPersistentStack<int> tail) = stack.Pop();
                Assert.Equal(item, head);
                Assert.Equal(countBefore, tail.Count);
            }

            foreach (int item in reversed)
            {
                int countBefore = stack.Count;

                int head;
                (head, stack) = stack.Pop();
                Assert.Equal(item, head);
            }

            CheckEmptyBehaviour(stack);
        }

        [Fact]
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
                    Assert.True(enumerator.MoveNext());
                    Assert.Equal(item, enumerator.Current);
                }
                Assert.False(enumerator.MoveNext());
            }
        }

        public static IEnumerable<object[]> EmptyStacks
        {
            get
            {
                return TestUtilities.Singletons(
                    PersistentStack<int>.Empty,
                    PersistentStack<int>.Of(Enumerable.Empty<int>()));
            }
        }

        private static void CheckEmptyBehaviour<T>(IPersistentStack<T> stack)
        {
            Assert.Equal(0, stack.Count);
            Assert.False(stack.GetEnumerator().MoveNext());
            Assert.Throws<InvalidOperationException>(() => stack.Pop());
        }
    }
}
