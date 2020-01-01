using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PersistentCollections
{
    public sealed class PersistentQueue<T> : IPersistentQueue<T>
    {
        public static readonly PersistentQueue<T> Empty =
            new PersistentQueue<T>(PersistentStack<T>.Empty, PersistentStack<T>.Empty);

        private readonly IPersistentStack<T> _head;

        private readonly IPersistentStack<T> _tail;

        private PersistentQueue(IPersistentStack<T> head, IPersistentStack<T> tail)
        {
            _head = head;
            _tail = tail;
        }

        public static PersistentQueue<T> Of(IEnumerable<T> items)
            => new PersistentQueue<T>(PersistentStack<T>.Of(items.Reverse()), PersistentStack<T>.Empty);

        public int Count => _head.Count + _tail.Count;

        public IPersistentQueue<T> Enqueue(T item)
        {
            IPersistentStack<T> extendedTail = _tail.Push(item);
            return CreateChecked(_head, extendedTail);
        }

        public (T Item, IPersistentQueue<T> Tail) Dequeue()
        {
            (T head, IPersistentStack<T> headTail) = _head.Pop();
            return (head, CreateChecked(headTail, _tail));
        }

        public IEnumerator<T> GetEnumerator()
            => Enumerable.Concat(_head, new LazilyReversedStack<T>(_tail)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static PersistentQueue<T> CreateChecked(IPersistentStack<T> head, IPersistentStack<T> tail)
        {
            if (tail.Count <= head.Count)
            {
                return new PersistentQueue<T>(head, tail);
            }
            else
            {
                return new PersistentQueue<T>(
                    StackConcat<T>.Of(head, new LazilyReversedStack<T>(tail)),
                    PersistentStack<T>.Empty);
            }
        }
    }
}
