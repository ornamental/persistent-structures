using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PersistentCollections;

namespace PersistentCollectionsTest
{
    [TestClass]
    public class ThunkTest
    {
        [TestMethod]
        public void DeferredCalculationTest()
        {
            Thunk<int> thunk = Thunk<int>.Of(() => throw new Exception("Exception to throw upon calculation."));
            Assert.ThrowsException<Exception>(() => thunk.Result);
        }

        [TestMethod]
        public void CalculationResultTest()
        {
            int expectedResult = 42;
            Thunk<int> thunk = Thunk<int>.Of(() => expectedResult);
            Assert.AreEqual(expectedResult, thunk.Result);
        }

        [TestMethod]
        public void AbsentCalculationTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => Thunk<int>.Of(null));
        }
    }
}
