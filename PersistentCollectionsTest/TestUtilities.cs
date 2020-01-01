using System;
using System.Collections.Generic;
using System.Linq;

namespace PersistentCollectionsTest
{
    public static class TestUtilities
    {
        public static IEnumerable<object[]> Singletons(params object[] cases)
        {
            return from o in cases select new object[] { o };
        }
    }
}
