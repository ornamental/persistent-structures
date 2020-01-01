using PersistentCollections;
using System;
using Xunit;

namespace PersistentCollectionsTest
{
    public class ThunkTest
    {
        [Fact]
        public void DeferredCalculationTest()
        {
            Thunk<int> thunk = Thunk<int>.Of(() => throw new Exception("Exception to throw upon calculation."));
            Assert.Throws<Exception>(() => thunk.Result);
        }

        [Fact]
        public void CalculationResultTest()
        {
            int expectedResult = 42;
            Thunk<int> thunk = Thunk<int>.Of(() => expectedResult);
            Assert.Equal(expectedResult, thunk.Result);
        }

        [Fact]
        public void AbsentCalculationTest()
        {
            Assert.Throws<ArgumentNullException>(() => Thunk<int>.Of(null));
        }
    }
}
