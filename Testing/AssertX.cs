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

        public static void Contains(string searchFor, string? content) => Assert.IsTrue(content?.Contains(searchFor) ?? false);

        public static void DoesNotContain(string searchFor, string? content) => Assert.IsFalse(content?.Contains(searchFor) ?? false);

        public static void StartsWith(string searchFor, string? content) => Assert.IsTrue(content?.StartsWith(searchFor) ?? false);

        public static void EndsWith(string searchFor, string? content) => Assert.IsTrue(content?.EndsWith(searchFor) ?? false);

        public static void Single<T>(IEnumerable<T> collection) => Assert.IsTrue(collection.Count() == 1);

        public static void Empty<T>(IEnumerable<T> collection) => Assert.IsFalse(collection.Any());

        public static void Contains<T>(T value, IEnumerable<T> collection) => Assert.IsTrue(collection.Contains(value));

        public static void DoesNotContain<T>(T value, IEnumerable<T> collection) => Assert.IsFalse(collection.Contains(value));

        public static void IsNullOrEmpty(string? value) => Assert.IsTrue(string.IsNullOrEmpty(value));

    }

}
