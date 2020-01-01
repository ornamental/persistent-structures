using System;
using System.Threading;

namespace PersistentCollections
{
    internal class Thunk<T>
    {
        private sealed class ThunkResult<Q>
        {
            public ThunkResult(Q result)
            {
                Result = result;
            }

            public Q Result
            {
                get;
            }
        }

        private object _calculation;

        private object _result;

        protected Thunk(Func<T> calculation)
        {
            if (calculation == null)
            {
                throw new ArgumentNullException(nameof(calculation));
            }

            Thread.VolatileWrite(ref _calculation, calculation);
        }

        public static Thunk<T> Of(Func<T> calculation) => new Thunk<T>(calculation);

        public T Result
        {
            get
            {
                Thread.VolatileRead(ref _result);
                if (_result == null)
                {
                    Thread.VolatileWrite(ref _result, new ThunkResult<T>(((Func<T>)_calculation)()));
                    Thread.VolatileWrite(ref _calculation, null);
                }

                return ((ThunkResult<T>)_result).Result;
            }
        }
    }
}
