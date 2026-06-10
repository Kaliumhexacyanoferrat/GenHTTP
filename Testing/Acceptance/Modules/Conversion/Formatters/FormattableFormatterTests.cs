using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class FormattableFormatterTests
{
    private readonly FormattableFormatter _formatter = new();

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
        Assert.IsTrue(_formatter.CanHandle(typeof(Guid)));
        Assert.IsTrue(_formatter.CanHandle(typeof(decimal)));
    }

    [TestMethod]
    public void DoesNotHandleNonIUtf8SpanFormattables()
    {
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
        Assert.IsFalse(_formatter.CanHandle(typeof(bool)));
        Assert.IsFalse(_formatter.CanHandle(typeof(char)));
    }

    [TestMethod]
    public void ReadsInt() => Assert.AreEqual(42, _formatter.Read(Bytes("42"), typeof(int)));

    [TestMethod]
    public void ReadsNegativeInt() => Assert.AreEqual(-100, _formatter.Read(Bytes("-100"), typeof(int)));

    [TestMethod]
    public void ThrowsForInvalidInt() =>
        Assert.ThrowsExactly<FormatException>(() => _formatter.Read(Bytes("abc"), typeof(int)));

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
    public void ReadsUInt() => Assert.AreEqual(4294967295u, _formatter.Read(Bytes("4294967295"), typeof(uint)));

    [TestMethod]
    public void ReadsULong() => Assert.AreEqual(18446744073709551615ul, _formatter.Read(Bytes("18446744073709551615"), typeof(ulong)));

    [TestMethod]
    public void ReadsDecimal() => Assert.AreEqual(3.14m, (decimal)_formatter.Read(Bytes("3.14"), typeof(decimal))!);

    [TestMethod]
    public void ReadsGuid()
    {
        var guid = new Guid("d4a6b7c8-1234-5678-abcd-ef0123456789");
        Assert.AreEqual(guid, _formatter.Read(Bytes("d4a6b7c8-1234-5678-abcd-ef0123456789"), typeof(Guid)));
    }

    [TestMethod]
    public void WritesInt() => Assert.AreEqual("42", _formatter.Write(42, typeof(int)));

    [TestMethod]
    public void WritesDoubleInInvariantCulture() => Assert.AreEqual("3.14", _formatter.Write(3.14, typeof(double)));

    [TestMethod]
    public void WritesDecimalInInvariantCulture() => Assert.AreEqual("3.14", _formatter.Write(3.14m, typeof(decimal)));

    [TestMethod]
    public void WritesGuid()
    {
        var guid = new Guid("d4a6b7c8-1234-5678-abcd-ef0123456789");
        Assert.AreEqual("d4a6b7c8-1234-5678-abcd-ef0123456789", _formatter.Write(guid, typeof(Guid)));
    }

}
