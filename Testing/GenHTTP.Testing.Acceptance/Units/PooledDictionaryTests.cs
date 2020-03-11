using System;
using System.Collections.Generic;

using Xunit;

using GenHTTP.Core.Utilities;

namespace GenHTTP.Testing.Acceptance.Units
{

    public class PooledDictionaryTests
    {

        [Fact]
        public void TestReplace()
        {
            using var dict = new PooledDictionary<string, string>();

            dict["one"] = "One";
            dict["one"] = "Two";

            Assert.True(dict.ContainsKey("one"));

#pragma warning disable xUnit2017
            Assert.True(dict.Contains(new KeyValuePair<string, string>("one", "Two")));
#pragma warning restore

            Assert.Equal("Two", dict["one"]);
        }

        [Fact]
        public void TestNotFound()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.Throws<KeyNotFoundException>(() => dict["nope"]);
        }

        [Fact]
        public void TestCounts()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.Equal(0, dict.Keys.Count);
            Assert.Equal(0, dict.Values.Count);

            Assert.Empty(dict);

            dict["one"] = "one";

            Assert.Equal(1, dict.Keys.Count);
            Assert.Equal(1, dict.Values.Count);

            Assert.Single(dict);
        }

        [Fact]
        public void TestClear()
        {
            using var dict = new PooledDictionary<string, string>();
            dict["one"] = "One";

            dict.Clear();

            Assert.False(dict.ContainsKey("one"));
        }

        [Fact]
        public void TestResize()
        {
            using var dict = new PooledDictionary<int, int>();

            for (int i = 0; i < 25; i++)
            {
                dict.Add(new KeyValuePair<int, int>(i, i));
            }

            Assert.True(dict.Capacity > 25);

            for (int i = 0; i < 25; i++)
            {
                Assert.Equal(i, dict[i]);
            }
        }

        [Fact]
        public void TestNoRemove()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.Throws<NotSupportedException>(() => dict.Remove(""));
        }

        [Fact]
        public void TestNoRemove2()
        {
            using var dict = new PooledDictionary<string, string>();

            Assert.Throws<NotSupportedException>(() => dict.Remove(new KeyValuePair<string, string>("", "")));
        }

    }

}
