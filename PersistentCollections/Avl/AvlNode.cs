using System;
using System.Collections;
using System.Collections.Generic;

namespace PersistentCollections.Avl
{
    public abstract class AvlNode<N, P> where N : AvlNode<N, P>
    {
        protected AvlNode()
        {
            Left = null;
            Right = null;
            Height = 0;
        }

        protected AvlNode(N left, N right)
        {
            Left = left;
            Right = right;
            Height = (byte)(1 + Math.Max(left.Height, right.Height));
        }

        public N Left
        {
            get;
        }

        public N Right
        {
            get;
        }

        public int Height
        {
            get;
        }

        public bool IsNil => Height == 0;

        public abstract P Payload
        {
            get;
        }
    }
}
