using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PersistentCollections
{
    internal sealed class LazilyReversedStack<T> : Thunk<IPersistentStack<T>>, IPersistentStack<T>
    {
        public LazilyReversedStack(IPersistentStack<T> original)
            : base(() => Reverse(original))
        {
            Count = original.Count;
        }

        public int Count
        {
            get;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Result.GetEnumerator();
        }

        public (T Item, IPersistentStack<T> Tail) Pop()
        {
            return Result.Pop();
        }

        [ExcludeFromCodeCoverage]
        public IPersistentStack<T> Push(T item)
        {
            return Result.Push(item);
        }

        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static IPersistentStack<T> Reverse(IPersistentStack<T> original)
        {
            return PersistentStack<T>.Of(original);
        }
    }
}
