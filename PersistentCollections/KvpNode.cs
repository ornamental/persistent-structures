using PersistentCollections.Avl;
using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentCollections
{
    public sealed class KvpNode<K, V> : AvlNode<KvpNode<K, V>, KeyValuePair<K, V>>
    {
        public static readonly KvpNode<K, V> Nil = new KvpNode<K, V>();

        private KvpNode() { }

        public KvpNode(KeyValuePair<K, V> kvp, KvpNode<K, V> left, KvpNode<K, V> right)
            : base(left, right)
        {
            Payload = kvp;
        }

        public override KeyValuePair<K, V> Payload { get; }
    }
}
