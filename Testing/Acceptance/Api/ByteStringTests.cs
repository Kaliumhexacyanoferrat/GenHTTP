using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Api;

[TestClass]
public sealed class ByteStringTests
{

    [TestMethod]
    public void StringAndMemoryConstructors_CreateEqualValues()
    {
        var fromString = new ByteString("Content-Type");
        var fromMemory = new ByteString(Encoding.ASCII.GetBytes("Content-Type"));

        Assert.AreEqual(fromString, fromMemory);
        Assert.AreEqual("Content-Type", fromString.ToString());
    }

    [TestMethod]
    public void Equality_IsCaseInsensitive()
    {
        var lower = new ByteString("Content-Type");
        var upper = new ByteString("CONTENT-TYPE");
        var mixed = new ByteString("CoNtEnT-TyPe");

        Assert.IsTrue(lower == upper);
        Assert.IsTrue(lower.Equals(mixed));
        Assert.IsFalse(lower != upper);
    }

    [TestMethod]
    public void Equality_DifferentValuesAreNotEqual()
    {
        var first = new ByteString("Content-Type");
        var second = new ByteString("Content-Length");

        Assert.IsFalse(first == second);
        Assert.IsTrue(first != second);
        Assert.IsFalse(first.Equals(second));
    }

    [TestMethod]
    public void Equality_WithReadOnlyMemory_IsCaseInsensitive()
    {
        var value = new ByteString("Content-Type");
        ReadOnlyMemory<byte> memory =
            Encoding.ASCII.GetBytes("CONTENT-TYPE");

        Assert.IsTrue(value == memory);
        Assert.IsTrue(memory == value);
        Assert.IsFalse(value != memory);
        Assert.IsFalse(memory != value);
        Assert.IsTrue(value.Equals(memory));
    }

    [TestMethod]
    public void Equality_WithReadOnlyMemory_DifferentValueIsNotEqual()
    {
        var value = new ByteString("Content-Type");
        ReadOnlyMemory<byte> memory =
            Encoding.ASCII.GetBytes("Content-Length");

        Assert.IsFalse(value == memory);
        Assert.IsFalse(memory == value);
        Assert.IsTrue(value != memory);
        Assert.IsTrue(memory != value);
    }

    [TestMethod]
    public void EqualValues_HaveEqualHashCodes()
    {
        var lower = new ByteString("Content-Type");
        var upper = new ByteString("CONTENT-TYPE");

        Assert.AreEqual(
            lower.GetHashCode(),
            upper.GetHashCode());
    }

    [TestMethod]
    public void Dictionary_IsCaseInsensitive()
    {
        var dictionary = new Dictionary<ByteString, string>
        {
            [new ByteString("Content-Type")] = "text/plain"
        };

        Assert.AreEqual(
            "text/plain",
            dictionary[new ByteString("content-type")]);

        Assert.IsTrue(
            dictionary.ContainsKey(new ByteString("CONTENT-TYPE")));
    }

    [TestMethod]
    public void Dictionary_DifferentCasingDoesNotCreateDuplicateKey()
    {
        var dictionary = new Dictionary<ByteString, int>
        {
            [new ByteString("Subdir")] = 1
        };

        dictionary[new ByteString("SUBDIR")] = 2;

        Assert.AreEqual(1, dictionary.Count);
        Assert.AreEqual(2, dictionary[new ByteString("subdir")]);
    }

    [TestMethod]
    public void HashSet_IsCaseInsensitive()
    {
        var set = new HashSet<ByteString>
        {
            new ByteString("Subdir")
        };

        Assert.IsTrue(set.Contains(new ByteString("SUBDIR")));
        Assert.IsFalse(set.Add(new ByteString("subdir")));
        Assert.AreEqual(1, set.Count);
    }

    [TestMethod]
    public void ObjectAndGenericEquality_AreCaseInsensitive()
    {
        var value = new ByteString("Subdir");

        object other = new ByteString("SUBDIR");
        IEquatable<ByteString> equatable = value;

        Assert.IsTrue(value.Equals(other));
        Assert.IsTrue(equatable.Equals(new ByteString("SUBDIR")));
    }

    [TestMethod]
    public void ReadOnlyMemorySlice_IsComparedUsingVisibleRange()
    {
        var bytes = Encoding.ASCII.GetBytes("xxxCONTENT-TYPExxx");

        ReadOnlyMemory<byte> slice =
            bytes.AsMemory(3, "CONTENT-TYPE".Length);

        var value = new ByteString("content-type");

        Assert.IsTrue(value == slice);
        Assert.IsTrue(slice == value);
    }

    [TestMethod]
    public void EmptyValues_AreEqual()
    {
        var value = new ByteString(string.Empty);

        Assert.IsTrue(value == new ByteString(ReadOnlyMemory<byte>.Empty));
        Assert.IsTrue(value == ReadOnlyMemory<byte>.Empty);
        Assert.IsTrue(ReadOnlyMemory<byte>.Empty == value);
    }

    [TestMethod]
    public void EqualityAndInequality_AreConsistent()
    {
        var equal = new ByteString("Subdir");
        var same = new ByteString("SUBDIR");
        var different = new ByteString("Other");

        Assert.AreEqual(equal == same, ! (equal != same));
        Assert.AreEqual(equal == different, !(equal != different));
    }
}
