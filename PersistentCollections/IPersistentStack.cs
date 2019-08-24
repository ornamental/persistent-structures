using System.Collections.Generic;

namespace PersistentCollections
{
    public interface IPersistentStack<T> : IReadOnlyCollection<T>
    {
        IPersistentStack<T> Push(T item);

        (T Item, IPersistentStack<T> Tail) Pop();
    }
}
