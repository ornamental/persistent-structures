using System;
using System.Collections;
using System.Collections.Generic;

namespace PersistentCollections
{
    public sealed class PersistentStack<T> : IPersistentStack<T>
    {
        public static readonly PersistentStack<T> Empty = new PersistentStack<T>(default, null);

        private readonly T _head;

        private readonly PersistentStack<T> _tail;

        public static IPersistentStack<T> Of(IEnumerable<T> original)
        {
            IPersistentStack<T> result = Empty;
            foreach (T item in original)
            {
                result = result.Push(item);
            }

            return result;
        }

        private PersistentStack(T head, PersistentStack<T> tail)
        {
            _head = head;
            _tail = tail;
            Count = tail == null ? 0 : (1 + tail.Count);
        }

        public int Count
        {
            get;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (PersistentStack<T> current = this; current._tail != null; current = current._tail)
            {
                yield return current._head;
            }
        }

        public (T Item, IPersistentStack<T> Tail) Pop()
        {
            if (_tail == null)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            return (_head, _tail);
        }

        public IPersistentStack<T> Push(T item)
        {
            return new PersistentStack<T>(item, this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
