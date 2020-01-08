using System;
using System.Collections.Generic;

namespace PersistentCollections
{
    public interface IPersistentList<T> : IReadOnlyList<T>
    {
        IPersistentList<T> Add(T item) => Add(Count, item);

        IPersistentList<T> Add(int index, T item);

        IPersistentList<T> RemoveAt(int index, out T removedValue);

        IPersistentList<T> Set(int index, T item, out T oldValue)
            => Set(index, _ => item, out oldValue);

        IPersistentList<T> Set(int index, Func<T, T> update, out T oldValue);
    }

    public static class PersistentListExtension
    {
        public static IPersistentList<T> RemoveAt<T>(this IPersistentList<T> list, int index)
            => list.RemoveAt(index, out _);

        public static IPersistentList<T> Set<T>(this IPersistentList<T> list, int index, T item)
            => list.Set(index, item, out _);

        public static IPersistentList<T> Set<T>(
            this IPersistentList<T> list, int index, Func<T, T> update)
                => list.Set(index, update, out _);
    }
}
