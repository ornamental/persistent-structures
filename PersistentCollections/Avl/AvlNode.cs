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

        public IEnumerable<AvlNode<N, P>> Tree()
        {
            AvlNode<N, P> current = this;
            Stack<AvlNode<N, P>> stack = new Stack<AvlNode<N, P>>(Height);
            while (current.Height > 0 || stack.Count > 0)
            {
                while (current.Height > 0)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                yield return current;
                current = current.Right;
            }
        }
    }
}
