using System;
using System.Collections.Generic;
using System.Text;

namespace PersistentCollections.Avl
{
    public sealed class WeightedAvlNode<P> : AvlNode<WeightedAvlNode<P>, P>
    {
        public static readonly WeightedAvlNode<P> Nil = new WeightedAvlNode<P>();

        protected WeightedAvlNode() : base()
        {
            Payload = default;
            Weight = 0;
        }

        public WeightedAvlNode(P payload, WeightedAvlNode<P> left, WeightedAvlNode<P> right)
            : base(left, right)
        {
            Payload = payload;
            Weight = 1 + left.Weight + right.Weight;
        }

        public override P Payload
        {
            get;
        }

        public int Weight
        {
            get;
        }
    }
}
