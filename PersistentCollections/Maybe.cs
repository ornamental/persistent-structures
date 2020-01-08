using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PersistentCollections
{
    public readonly struct Maybe<T> : IEnumerable<T>
    {
        public static readonly Maybe<T> Nothing = new Maybe<T>();

        public Maybe(T value)
        {
            Value = value;
            HasValue = true;
        }

        public T Value { get; }

        public bool HasValue { get; }

        public T OrElse(T elseValue) => HasValue ? Value : elseValue;

        public Maybe<V> Map<V>(Func<T, V> mapping)
            => HasValue ? new Maybe<V>(mapping(Value)) : Maybe<V>.Nothing;

        public Maybe<V> FlatMap<V>(Func<T, Maybe<V>> mapping)
            => HasValue ? mapping(Value) : Maybe<V>.Nothing;

        public T OrThrow(Func<Exception> exceptionFactory)
        {
            if (HasValue)
            {
                return Value;
            }

            throw exceptionFactory();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (HasValue)
            {
                yield return Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
