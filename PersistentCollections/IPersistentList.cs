using System;
using System.Collections.Generic;

namespace PersistentCollections
{
    public interface IPersistentList<T> : IReadOnlyList<T>
    {
        IPersistentList<T> Add(T item) => Add(Count, item);

        IPersistentList<T> Add(int index, T item);

        IPersistentList<T> RemoveAt(int index, out T removedValue);

        IPersistentList<T> SetValue(int index, T item, out T oldValue)
            => SetValue(index, _ => item, out oldValue);

        IPersistentList<T> SetValue(int index, Func<T, T> update, out T oldValue);
    }

    public static class PersistentListExtension
    {
        public static IPersistentList<T> RemoveAt<T>(this IPersistentList<T> list, int index)
            => list.RemoveAt(index, out _);

        public static IPersistentList<T> SetValue<T>(this IPersistentList<T> list, int index, T item)
            => list.SetValue(index, item, out _);

        public static IPersistentList<T> SetValue<T>(
            this IPersistentList<T> list, int index, Func<T, T> update)
                => list.SetValue(index, update, out _);
    }
}
