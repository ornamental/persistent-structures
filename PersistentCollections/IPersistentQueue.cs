using System.Collections.Generic;

namespace PersistentCollections
{
    public interface IPersistentQueue<T> : IReadOnlyCollection<T>
    {
        IPersistentQueue<T> Enqueue(T item);

        (T Item, IPersistentQueue<T> Tail) Dequeue();
    }
}
