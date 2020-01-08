using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentCollections
{
    public interface IPerststentMap<K, V> : IReadOnlyDictionary<K, V>
    {
        IPerststentMap<K, V> Put(K key, V value, out Maybe<V> oldValue);

        IPerststentMap<K, V> Update(K key, Func<V, V> update, out Maybe<V> oldValue);

        IPerststentMap<K, V> PutIfAbsent(K key, V value);

        IPerststentMap<K, V> Remove(K key, out Maybe<V> removedValue);
    }

    public static class PersistentMapExtension
    {
        public static IPerststentMap<K, V> Put<K, V>(this IPerststentMap<K, V> map, K key, V value)
            => map.Put(key, value, out _);

        public static IPerststentMap<K, V> Update<K, V>(this IPerststentMap<K, V> map, K key, Func<V, V> update)
            => map.Update(key, update, out _);

        public static IPerststentMap<K, V> Remove<K, V>(this IPerststentMap<K, V> map, K key)
            => map.Remove(key, out _);
    }
}
