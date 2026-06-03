using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class PrimitiveFormatterTests
{
    private readonly PrimitiveFormatter _formatter = new();

    private static ByteString Bytes(string s) => new(s);

    [TestMethod]
    public void HandlesPrimitiveTypes()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(int)));
        Assert.IsTrue(_formatter.CanHandle(typeof(long)));
        Assert.IsTrue(_formatter.CanHandle(typeof(short)));
        Assert.IsTrue(_formatter.CanHandle(typeof(byte)));
        Assert.IsTrue(_formatter.CanHandle(typeof(double)));
        Assert.IsTrue(_formatter.CanHandle(typeof(float)));
        Assert.IsTrue(_formatter.CanHandle(typeof(bool)));
        Assert.IsTrue(_formatter.CanHandle(typeof(char)));
    }

    [TestMethod]
    public void DoesNotHandleNonPrimitives()
    {
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
        Assert.IsFalse(_formatter.CanHandle(typeof(Guid)));
        Assert.IsFalse(_formatter.CanHandle(typeof(decimal)));
    }

    [TestMethod]
    public void ReadsInt() => Assert.AreEqual(42, _formatter.Read(Bytes("42"), typeof(int)));

    [TestMethod]
    public void ReadsNegativeInt() => Assert.AreEqual(-100, _formatter.Read(Bytes("-100"), typeof(int)));

    [TestMethod]
    public void ThrowsForInvalidInt() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("abc"), typeof(int)));

    [TestMethod]
    public void ReadsLong() => Assert.AreEqual(1_000_000_000_000L, _formatter.Read(Bytes("1000000000000"), typeof(long)));

    [TestMethod]
    public void ReadsShort() => Assert.AreEqual((short)32767, _formatter.Read(Bytes("32767"), typeof(short)));

    [TestMethod]
    public void ReadsByte() => Assert.AreEqual((byte)255, _formatter.Read(Bytes("255"), typeof(byte)));

    [TestMethod]
    public void ReadsDouble() => Assert.AreEqual(3.14, (double)_formatter.Read(Bytes("3.14"), typeof(double))!, 1e-10);

    [TestMethod]
    public void ReadsFloat() => Assert.AreEqual(1.5f, (float)_formatter.Read(Bytes("1.5"), typeof(float))!, 1e-6f);

    [TestMethod]
    public void ReadsBoolCaseInsensitive()
    {
        Assert.AreEqual(true, _formatter.Read(Bytes("true"), typeof(bool)));
        Assert.AreEqual(true, _formatter.Read(Bytes("True"), typeof(bool)));
        Assert.AreEqual(true, _formatter.Read(Bytes("TRUE"), typeof(bool)));
        Assert.AreEqual(false, _formatter.Read(Bytes("false"), typeof(bool)));
        Assert.AreEqual(false, _formatter.Read(Bytes("False"), typeof(bool)));
    }

    [TestMethod]
    public void ThrowsForInvalidBool() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("yes"), typeof(bool)));

    [TestMethod]
    public void ReadsChar() => Assert.AreEqual('A', _formatter.Read(Bytes("A"), typeof(char)));

    [TestMethod]
    public void ThrowsForMultiCharInput() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("AB"), typeof(char)));

    [TestMethod]
    public void WritesInt() => Assert.AreEqual("42", _formatter.Write(42, typeof(int)));

    [TestMethod]
    public void WritesDoubleInInvariantCulture() => Assert.AreEqual("3.14", _formatter.Write(3.14, typeof(double)));

    [TestMethod]
    public void WritesBoolAsWord() => Assert.AreEqual("True", _formatter.Write(true, typeof(bool)));
}
