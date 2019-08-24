using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PersistentCollections
{
    internal sealed class StackConcat<T> : IPersistentStack<T>
    {
        private readonly IPersistentStack<T> _head;

        private readonly IPersistentStack<T> _tail;

        private StackConcat(IPersistentStack<T> head, IPersistentStack<T> tail)
        {
            if (_head is StackConcat<T>)
            {
                throw new ArgumentException("Head cannot be a concatenated stack.");
            }

            _head = head;
            _tail = tail;
            Count = _head.Count + _tail.Count;
        }

        public static StackConcat<T> Of(IPersistentStack<T> head, IPersistentStack<T> tail)
        {
            // an invariant must hold: the concatenated stack's head must not be a concatenated stack;
            // this ensures that popping from the concatenated stack is O(1), though construction takes longer
            switch (head)
            {
                case StackConcat<T> concatenatedHead:
                    return new StackConcat<T>(concatenatedHead._head, Of(concatenatedHead._tail, tail));
                default:
                    return new StackConcat<T>(head, tail);
            }
        }

        public int Count
        {
            get;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Concat(_head, _tail).GetEnumerator();
        }

        public (T Item, IPersistentStack<T> Tail) Pop()
        {
            if (_head.Count == 0)
            {
                return _tail.Pop();
            }
            else
            {
                (T head, IPersistentStack<T> tail) = _head.Pop();
                return (head, tail.Count == 0 ? _tail : new StackConcat<T>(tail, _tail));
            }
        }

        [ExcludeFromCodeCoverage]
        public IPersistentStack<T> Push(T item)
        {
            return new StackConcat<T>(_head.Push(item), _tail);
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
