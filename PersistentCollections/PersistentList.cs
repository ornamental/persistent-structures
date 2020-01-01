using System;
using System.Collections;
using System.Collections.Generic;
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

        private sealed class DeletionPointLocator : AvlTreeTraversal<WeightedAvlNode<T>, T>
        {
            private int _index;

            private WeightedAvlNode<T> _current;

            public DeletionPointLocator(int index, WeightedAvlNode<T> root)
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

            // DoInsert(..) cannot return null as InsertionPointLocator does not return Descent.NotFound
            return new PersistentList<T>(DoInsert(item, new InsertionPointLocator(index, root)));
        }

        public IPersistentList<T> RemoveAt(int index, out T removedValue)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            DeletionPointLocator locator = new DeletionPointLocator(index, root);
            // DoDelete(..) cannot return null as DeletionPointLocator does not return Descent.NotFound
            WeightedAvlNode<T> node = DoDelete(locator);

            removedValue = locator.CurrentNode.Payload;
            return new PersistentList<T>(node);
        }

        public IPersistentList<T> SetValue(int index, Func<T, T> update, out T oldValue)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }

            return new PersistentList<T>(ChangeValue(index, root, update, out oldValue));
        }

        public IEnumerator<T> GetEnumerator()
        {
            WeightedAvlNode<T> current = root;
            Stack<WeightedAvlNode<T>> stack = new Stack<WeightedAvlNode<T>>(root.Height);
            while (current.Height > 0 || stack.Count > 0)
            {
                while (current.Height > 0)
                {
                    stack.Push(current);
                    current = current.Left;
                }

                current = stack.Pop();
                yield return current.Payload;
                current = current.Right;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        protected override WeightedAvlNode<T> NilNode => WeightedAvlNode<T>.Nil;

        protected override WeightedAvlNode<T> NewNode(
            T payload, WeightedAvlNode<T> left, WeightedAvlNode<T> right)
                => new WeightedAvlNode<T>(payload, left, right);

        private WeightedAvlNode<T> ChangeValue(
            int index, WeightedAvlNode<T> node, Func<T, T> update, out T oldValue)
        {
            WeightedAvlNode<T> left;
            WeightedAvlNode<T> right;
            T payload;

            int leftWeight = node.Left.Weight;

            if (index < leftWeight)
            {
                payload = node.Payload;
                left = ChangeValue(index, node.Left, update, out oldValue);
                right = node.Right;
            }
            else if (index > leftWeight)
            {
                payload = node.Payload;
                left = node.Left;
                right = ChangeValue(index - leftWeight - 1, node.Right, update, out oldValue);
            }
            else
            {
                oldValue = node.Payload;
                payload = update(oldValue);
                left = node.Left;
                right = node.Right;
            }

            return new WeightedAvlNode<T>(payload, left, right);
        }
    }
}
