using PersistentCollections.Avl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PersistentCollections
{
    public sealed class PersistentMap<K, V> : AvlTree<KvpNode<K, V>, KeyValuePair<K, V>>, IPerststentMap<K, V>
    {
        private enum NodeLocationPolicy
        {
            Default,
            NoInsert,
            NoUpdate
        }

        private sealed class NodeLocator : AvlTreeTraversal<KvpNode<K, V>, KeyValuePair<K, V>>
        {
            private readonly K _targetKey;

            private readonly IComparer<K> _comparer;

            private KvpNode<K, V> _current;

            private NodeLocationPolicy _policy;

            public NodeLocator(
                K targetKey, IComparer<K> comparer, NodeLocationPolicy policy, KvpNode<K, V> root)
            {
                _targetKey = targetKey;
                _comparer = comparer;
                _policy = policy;
                _current = root;
            }

            public KvpNode<K, V> CurrentNode => _current;

            public Descent Descend()
            {
                if (_current.IsNil)
                {
                    return _policy == NodeLocationPolicy.NoInsert ? Descent.NotFound : Descent.Found;
                }
                else
                {
                    int comparison = _comparer.Compare(_targetKey, _current.Payload.Key);
                    if (comparison < 0)
                    {
                        _current = _current.Left;
                        return Descent.Left;
                    }
                    else if (comparison > 0)
                    {
                        _current = _current.Right;
                        return Descent.Right;
                    }
                    else
                    {
                        return _policy == NodeLocationPolicy.NoUpdate ? Descent.NotFound : Descent.Found;
                    }
                }
            }
        }

        private readonly IComparer<K> _comparer;

        private PersistentMap(KvpNode<K, V> root, int count, IComparer<K> comparer) : base(root)
        {
            Count = count;
            _comparer = comparer;
        }

        public PersistentMap(IComparer<K> comparer) : this(KvpNode<K, V>.Nil, 0, comparer) { }

        public PersistentMap(SortedDictionary<K, V> source) : base(source.GetEnumerator(), source.Count)
        {
            _comparer = source.Comparer;
        }

        public PersistentMap(SortedList<K, V> source) : base(source.GetEnumerator(), source.Count)
        {
            _comparer = source.Comparer;
        }

        public static PersistentMap<K, V> Empty = new PersistentMap<K, V>(Comparer<K>.Default);

        public V this[K key]
        {
            get
            {
                if (TryGetValue(key, out V result))
                {
                    return result;
                }

                throw new KeyNotFoundException();
            }
        }

        public bool TryGetValue(K key, out V value)
        {
            KvpNode<K, V> current = root;
            while (!current.IsNil)
            {
                int comparison = _comparer.Compare(key, current.Payload.Key);
                if (comparison < 0)
                {
                    current = current.Left;
                }
                else if (comparison > 0)
                {
                    current = current.Right;
                }
                else
                {
                    value = current.Payload.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public override int Count { get; }

        public IEnumerable<K> Keys => this.Select(kvp => kvp.Key);

        public IEnumerable<V> Values => this.Select(kvp => kvp.Value);

        public bool ContainsKey(K key)
        {
            KvpNode<K, V> current = root;
            while (!current.IsNil)
            {
                int comparison = _comparer.Compare(key, current.Payload.Key);
                if (comparison < 0)
                {
                    current = current.Left;
                }
                else if (comparison > 0)
                {
                    current = current.Right;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            => root.Tree().Select(n => n.Payload).GetEnumerator();

        public IPerststentMap<K, V> Put(K key, V value, out Maybe<V> oldValue)
        {
            KeyValuePair<K, V> kvp = new KeyValuePair<K, V>(key, value);
            NodeLocator locator = new NodeLocator(key, _comparer, NodeLocationPolicy.Default, root);
            KvpNode<K, V> newRoot = DoInsertOrUpdate(kvp, _ => kvp, locator);
            if (locator.CurrentNode.IsNil) // insertion
            {
                oldValue = Maybe<V>.Nothing;
                return new PersistentMap<K, V>(newRoot, Count + 1, _comparer);
            }
            else // update
            {
                oldValue = new Maybe<V>(locator.CurrentNode.Payload.Value);
                return new PersistentMap<K, V>(newRoot, Count, _comparer);
            }
        }

        public IPerststentMap<K, V> PutIfAbsent(K key, V value)
        {
            KeyValuePair<K, V> kvp = new KeyValuePair<K, V>(key, value);
            NodeLocator locator = new NodeLocator(key, _comparer, NodeLocationPolicy.NoUpdate, root);
            // the update calculation is not executed
            KvpNode<K, V> newRoot = DoInsertOrUpdate(kvp, _ => default, locator);
            return newRoot != null
                ? new PersistentMap<K, V>(newRoot, Count + 1, _comparer)
                : this;
        }

        public IPerststentMap<K, V> Update(K key, Func<V, V> update, out Maybe<V> oldValue)
        {
            NodeLocator locator = new NodeLocator(key, _comparer, NodeLocationPolicy.NoInsert, root);
            // no insertion is performed
            KvpNode<K, V> newRoot = DoInsertOrUpdate(
                default, kvp => new KeyValuePair<K, V>(kvp.Key, update(kvp.Value)), locator);
            if (newRoot == null)
            {
                oldValue = Maybe<V>.Nothing;
                return this;
            }
            else
            {
                oldValue = new Maybe<V>(locator.CurrentNode.Payload.Value);
                return new PersistentMap<K, V>(newRoot, Count, _comparer);
            }
        }

        public IPerststentMap<K, V> Remove(K key, out Maybe<V> removedValue)
        {
            NodeLocator locator = new NodeLocator(key, _comparer, NodeLocationPolicy.NoInsert, root);
            KvpNode<K, V> newRoot = DoDelete(locator);
            if (newRoot == null)
            {
                removedValue = Maybe<V>.Nothing;
                return this;
            }
            else
            {
                removedValue = new Maybe<V>(locator.CurrentNode.Payload.Value);
                return new PersistentMap<K, V>(newRoot, Count - 1, _comparer);
            }
        }

        protected override KvpNode<K, V> NilNode => KvpNode<K, V>.Nil;

        protected override KvpNode<K, V> NewNode(
            KeyValuePair<K, V> payload, KvpNode<K, V> left, KvpNode<K, V> right)
                => new KvpNode<K, V>(payload, left, right);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
