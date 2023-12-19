using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using GenHTTP.Engine.Utilities;

namespace GenHTTP.Testing.Acceptance.Engine
{

    [TestClass]
    public sealed class PooledDictionaryTests
    {

        [TestMethod]
        public void TestReplace()
        {
            using var dict = new PooledDictionary<string, string>();

            dict["one"] = "One";
            dict["one"] = "Two";

            Assert.IsTrue(dict.ContainsKey("one"));
            Assert.IsTrue(dict.Contains(new KeyValuePair<string, string>("one", "Two")));

            Assert.AreEqual("Two", dict["one"]);
        }

        [TestMethod]
        public void TestNotFound()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.ThrowsException<KeyNotFoundException>(() => dict["nope"]);
        }

        [TestMethod]
        public void TestCounts()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.AreEqual(0, dict.Keys.Count);
            Assert.AreEqual(0, dict.Values.Count);

            AssertX.Empty(dict);

            dict["one"] = "one";

            Assert.AreEqual(1, dict.Keys.Count);
            Assert.AreEqual(1, dict.Values.Count);

            AssertX.Single(dict);
        }

        [TestMethod]
        public void TestClear()
        {
            using var dict = new PooledDictionary<string, string>();
            dict["one"] = "One";

            dict.Clear();

            Assert.IsFalse(dict.ContainsKey("one"));
        }

        [TestMethod]
        public void TestResize()
        {
            using var dict = new PooledDictionary<int, int>();

            for (int i = 0; i < 25; i++)
            {
                dict.Add(new KeyValuePair<int, int>(i, i));
            }

            Assert.IsTrue(dict.Capacity > 25);

            for (int i = 0; i < 25; i++)
            {
                Assert.AreEqual(i, dict[i]);
            }
        }

        [TestMethod]
        public void TestNoRemove()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.ThrowsException<NotSupportedException>(() => dict.Remove(""));
        }

        [TestMethod]
        public void TestNoRemove2()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.ThrowsException<NotSupportedException>(() => dict.Remove(new KeyValuePair<string, string>("", "")));
        }

    }

}
