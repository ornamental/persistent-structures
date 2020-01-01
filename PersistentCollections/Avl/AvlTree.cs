using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace PersistentCollections.Avl
{
    public abstract class AvlTree<N, P> where N : AvlNode<N, P>
    {
        protected readonly N root;

        protected AvlTree(N root)
        {
            this.root = root;
        }

        protected AvlTree(IEnumerator<P> enumerator, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            this.root = BuildTree(enumerator, count);
        }

        public abstract int Count
        {
            get;
        }

        protected abstract N NilNode
        {
            get;
        }

        protected abstract N NewNode(P payload, N left, N right);

        [return: MaybeNull]
        protected N DoInsert(P payload, AvlTreeTraversal<N, P> insertionLeafLocator)
        {
            N currentNode = insertionLeafLocator.CurrentNode; // memorize the current nde before descending
            Descent direction = insertionLeafLocator.Descend(); // descend: insertionLeafLocator.CurrentNode changes

            if (direction == Descent.NotFound)
            {
                return null;
            }

            P value;
            N left;
            N rigth;

            if (direction == Descent.Found)
            {
                value = payload;
                left = NilNode;
                rigth = NilNode;
            }
            else
            {
                value = currentNode.Payload; // preserve payload
                N rebuiltSubtree = DoInsert(payload, insertionLeafLocator); // insert recursively
                if (rebuiltSubtree == null)
                {
                    return null; // propagate the 'insertion point not found' result up the stack
                }

                if (direction == Descent.Left)
                {
                    left = rebuiltSubtree;
                    rigth = currentNode.Right;
                }
                else // direction == Descent.Right
                {
                    left = currentNode.Left;
                    rigth = rebuiltSubtree;
                }
            }

            return Rebalance(value, left, rigth);
        }

        [return: MaybeNull]
        protected N DoDelete([DisallowNull] AvlTreeTraversal<N, P> deletionPointLocator)
        {
            N currentNode = deletionPointLocator.CurrentNode;
            Descent direction = deletionPointLocator.Descend();

            if (direction == Descent.NotFound)
            {
                return null;
            }
            else if (direction == Descent.Found)
            {
                if (currentNode.Left.IsNil)
                {
                    return currentNode.Right;
                }
                else if (currentNode.Right.IsNil)
                {
                    return currentNode.Left;
                }
                else // has both children
                {
                    (P payload, N residue) = DeleteMin(currentNode.Right);
                    return Rebalance(payload, currentNode.Left, residue);
                }
            }
            else
            {
                N left;
                N right;

                if (direction == Descent.Left)
                {
                    left = DoDelete(deletionPointLocator);
                    if (left == null)
                    {
                        return null; // propagate the 'deletion point not found' result up the stack
                    }

                    right = currentNode.Right;
                }
                else
                {
                    right = DoDelete(deletionPointLocator);
                    if (right == null)
                    {
                        return null;
                    }

                    left = currentNode.Left;
                }

                return Rebalance(currentNode.Payload, left, right);
            }
        }

        private N Rebalance(P payload, N left, N right)
        {
            int balanceDefect = left.Height - right.Height;
            if (balanceDefect == 2)
            {
                if (left.Left.Height > left.Right.Height) // LL
                {
                    return NewNode(left.Payload, left.Left, NewNode(payload, left.Right, right));
                }
                else // LR
                {
                    return NewNode(
                        left.Right.Payload,
                        NewNode(left.Payload, left.Left, left.Right.Left),
                        NewNode(payload, left.Right.Right, right));
                }
            }
            else if (balanceDefect == -2)
            {
                if (right.Right.Height > right.Left.Height) // RR
                {
                    return NewNode(right.Payload, NewNode(payload, left, right.Left), right.Right);
                }
                else // RL
                {
                    return NewNode(
                        right.Left.Payload,
                        NewNode(payload, left, right.Left.Left),
                        NewNode(right.Payload, right.Left.Right, right.Right));
                }
            }
            else
            {
                return NewNode(payload, left, right);
            }
        }

        private (P, N) DeleteMin(N node)
        {
            if (node.Left.IsNil)
            {
                return (node.Payload, node.Right);
            }
            else
            {
                (P payload, N residue) = DeleteMin(node.Left);
                return (payload, Rebalance(node.Payload, residue, node.Right));
            }
        }

        private N BuildTree(IEnumerator<P> enumerator, int count)
        {
            switch (count)
            {
                case 0:
                    return NilNode;
                case 1:
                    return NewNode(NextValue(enumerator), NilNode, NilNode);
                default:
                    int rightWeight = (count - 1) / 2;
                    N left = BuildTree(enumerator, count - 1 - rightWeight);
                    P value = NextValue(enumerator);
                    N right = BuildTree(enumerator, rightWeight);

                    return NewNode(value, left, right);
            }
        }

        private static P NextValue(IEnumerator<P> enumerator)
        {
            if (!enumerator.MoveNext())
            {
                throw new ArgumentException("The enumerator did not produce enough elements.");
            }

            return enumerator.Current;
        }
    }
}
