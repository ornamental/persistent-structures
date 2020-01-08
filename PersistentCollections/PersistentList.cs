using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PersistentCollections.Avl;

namespace PersistentCollections
{
    public sealed class PersistentList<T> : AvlTree<WeightedAvlNode<T>, T>, IPersistentList<T>
    {
        private sealed class InsertionPointLocator : AvlTreeTraversal<WeightedAvlNode<T>, T>
        {
            private int _position;

            private WeightedAvlNode<T> _current;

            public InsertionPointLocator(int position, WeightedAvlNode<T> root)
            {
                _position = position;
                _current = root;
            }

            public WeightedAvlNode<T> CurrentNode => _current;

            public Descent Descend()
            {
                if (_current.IsNil)
                {
                    return Descent.Found;
                }
                else
                {
                    if (_position <= _current.Left.Weight)
                    {
                        _current = _current.Left;
                        return Descent.Left;
                    }
                    else
                    {
                        _position -= _current.Left.Weight + 1;
                        _current = _current.Right;
                        return Descent.Right;
                    }
                }
            }
        }

        private sealed class NodeLocator : AvlTreeTraversal<WeightedAvlNode<T>, T>
        {
            private int _index;

            private WeightedAvlNode<T> _current;

            public NodeLocator(int index, WeightedAvlNode<T> root)
            {
                _index = index;
                _current = root;
            }

            public WeightedAvlNode<T> CurrentNode => _current;

            public Descent Descend()
            {
                int leftWeight = _current.Left.Weight;

                if (_index < leftWeight)
                {
                    _current = _current.Left;
                    return Descent.Left;
                }
                else if (_index > leftWeight)
                {
                    _index -= leftWeight + 1;
                    _current = _current.Right;
                    return Descent.Right;
                }
                else
                {
                    return Descent.Found;
                }
            }
        }

        public static readonly PersistentList<T> Empty = new PersistentList<T>();

        public static PersistentList<T> Of(ICollection<T> collection)
            => new PersistentList<T>(collection.GetEnumerator(), collection.Count);

        public static PersistentList<T> OfReadonly(IReadOnlyCollection<T> collection)
            => new PersistentList<T>(collection.GetEnumerator(), collection.Count);

        private PersistentList() : base(WeightedAvlNode<T>.Nil) { }

        private PersistentList(IEnumerator<T> enumerator, int count) : base(enumerator, count) { }

        private PersistentList(WeightedAvlNode<T> root) : base(root) { }

        public override int Count => root.Weight;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }

                WeightedAvlNode<T> current = root;
                while (true)
                {
                    int leftWeight = current.Left.Weight;
                    if (index < leftWeight)
                    {
                        current = current.Left;
                    }
                    else if (index > leftWeight)
                    {
                        index -= leftWeight + 1;
                        current = current.Right;
                    }
                    else
                    {
                        return current.Payload;
                    }
                }
            }
        }

        public IPersistentList<T> Add(int index, T item)
        {
            if (index < 0 || index > Count)
            {
                throw new IndexOutOfRangeException();
            }

            // DoInsert(..) cannot return null as InsertionPointLocator does not return Descent.NotFound;
            // InsertionPointLocator always stops in Nil leaf, so an insertion is executed, never an update
            return new PersistentList<T>(
                DoInsertOrUpdate(item, _ => default, new InsertionPointLocator(index, root)));
        }

        public IPersistentList<T> RemoveAt(int index, out T removedValue)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            NodeLocator locator = new NodeLocator(index, root);
            // DoDelete(..) cannot return null as NodeLocator does not return Descent.NotFound
            WeightedAvlNode<T> node = DoDelete(locator);

            removedValue = locator.CurrentNode.Payload;
            return new PersistentList<T>(node);
        }

        public IPersistentList<T> Set(int index, Func<T, T> update, out T oldValue)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            NodeLocator nodeLocator = new NodeLocator(index, root);
            // DoInsertOrUpdate(..) cannot return null as NodeLocator does not return Descent.NotFound
            // NodeLocator always finds a node to update, so insertion is never executed
            WeightedAvlNode<T> newRoot = DoInsertOrUpdate(default(T), update, nodeLocator);

            oldValue = nodeLocator.CurrentNode.Payload;
            return new PersistentList<T>(newRoot);
        }

        public IEnumerator<T> GetEnumerator()
            => root.Tree().Select(n => n.Payload).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override WeightedAvlNode<T> NilNode => WeightedAvlNode<T>.Nil;

        protected override WeightedAvlNode<T> NewNode(
            T payload, WeightedAvlNode<T> left, WeightedAvlNode<T> right)
                => new WeightedAvlNode<T>(payload, left, right);
    }
}
