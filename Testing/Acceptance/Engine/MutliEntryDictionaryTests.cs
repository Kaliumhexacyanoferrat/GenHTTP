using GenHTTP.Engine.Shared.Types;

namespace GenHTTP.Testing.Acceptance.Engine;

[TestClass]
public sealed class MultiEntryDictionaryTests
{

    [TestMethod]
    public void TestReplace()
    {
        var dict = GetDictionary();

        dict["one"] = "One";
        dict["one"] = "Two";

        Assert.IsTrue(dict.ContainsKey("one"));
        Assert.IsTrue(dict.Contains(new KeyValuePair<string, string>("one", "Two")));

        Assert.AreEqual("Two", dict["one"]);
    }

    [TestMethod]
    public void TestNotFound()
    {
        var dict = GetDictionary();

        Assert.ThrowsExactly<KeyNotFoundException>(() => dict["nope"]);
    }

    [TestMethod]
    public void TestCounts()
    {
        var dict = GetDictionary();

        Assert.IsEmpty(dict.Keys);
        Assert.IsEmpty(dict.Values);

        AssertX.Empty(dict);

        dict["one"] = "one";

        Assert.HasCount(1, dict.Keys);
        Assert.HasCount(1, dict.Values);

        AssertX.Single(dict);
    }

    [TestMethod]
    public void TestClear()
    {
        var dict = GetDictionary();
        dict["one"] = "One";

        dict.Clear();

        Assert.IsFalse(dict.ContainsKey("one"));
    }

    [TestMethod]
    public void TestResize()
    {
        var dict = GetDictionary();

        for (var i = 0; i < 25; i++)
        {
            dict.Add(new KeyValuePair<string, string>(i.ToString(), i.ToString()));
        }

        Assert.IsGreaterThan(25, dict.Capacity);

        for (var i = 0; i < 25; i++)
        {
            Assert.AreEqual(i.ToString(), dict[i.ToString()]);
        }
    }

    [TestMethod]
    public void TestNoRemove()
    {
        var dict = GetDictionary();

        Assert.ThrowsExactly<NotSupportedException>(() => dict.Remove(""));
    }

    [TestMethod]
    public void TestNoRemove2()
    {
        var dict = GetDictionary();

        Assert.ThrowsExactly<NotSupportedException>(() => dict.Remove(new KeyValuePair<string, string>("", "")));
    }

    private static MultiEntryDictionary<string, string> GetDictionary() => new(4, StringComparer.InvariantCultureIgnoreCase);

}
