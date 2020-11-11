using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance
{

    /// <summary>
    /// Compatibility assertions for XUnit.
    /// </summary>
    public static class AssertX
    {

        public static void Contains(string searchFor, string content) => Assert.IsTrue(content.Contains(searchFor));

        public static void DoesNotContain(string searchFor, string content) => Assert.IsFalse(content.Contains(searchFor));

        public static void StartsWith(string searchFor, string content) => Assert.IsTrue(content.StartsWith(searchFor));

        public static void EndsWith(string searchFor, string content) => Assert.IsTrue(content.EndsWith(searchFor));

        public static void Single<T>(IEnumerable<T> collection) => Assert.IsTrue(collection.Count() == 1);

        public static void Empty<T>(IEnumerable<T> collection) => Assert.IsTrue(collection.Count() == 0);

        public static void Contains<T>(T value, IEnumerable<T> collection) => Assert.IsTrue(collection.Contains(value));

        public static void DoesNotContain<T>(T value, IEnumerable<T> collection) => Assert.IsFalse(collection.Contains(value));

    }

}
