using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class BoolFormatterTests
{
    private readonly BoolFormatter _formatter = new();

    private static ByteString Bytes(string s) => new(s);

    [TestMethod]
    public void HandlesOnlyBool()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(bool)));
        Assert.IsFalse(_formatter.CanHandle(typeof(int)));
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
    }

    [TestMethod]
    public void ReadsTruthyValues()
    {
        Assert.AreEqual(true, _formatter.Read(Bytes("1"), typeof(bool)));
        Assert.AreEqual(true, _formatter.Read(Bytes("on"), typeof(bool)));
        Assert.AreEqual(true, _formatter.Read(Bytes("true"), typeof(bool)));
    }

    [TestMethod]
    public void ReadsFalsyValues()
    {
        Assert.AreEqual(false, _formatter.Read(Bytes("0"), typeof(bool)));
        Assert.AreEqual(false, _formatter.Read(Bytes("off"), typeof(bool)));
        Assert.AreEqual(false, _formatter.Read(Bytes("false"), typeof(bool)));
    }

    [TestMethod]
    public void ReadsCaseInsensitive()
    {
        Assert.AreEqual(true, _formatter.Read(Bytes("True"), typeof(bool)));
        Assert.AreEqual(true, _formatter.Read(Bytes("ON"), typeof(bool)));
        Assert.AreEqual(false, _formatter.Read(Bytes("False"), typeof(bool)));
        Assert.AreEqual(false, _formatter.Read(Bytes("OFF"), typeof(bool)));
    }

    [TestMethod]
    public void ThrowsForUnrecognizedInput()
    {
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("yes"), typeof(bool)));
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("maybe"), typeof(bool)));
    }

    [TestMethod]
    public void ReadGenericTruthyValues()
    {
        Assert.AreEqual(true, _formatter.Read<bool>(Bytes("1")));
        Assert.AreEqual(true, _formatter.Read<bool>(Bytes("on")));
        Assert.AreEqual(true, _formatter.Read<bool>(Bytes("true")));
    }

    [TestMethod]
    public void ReadGenericFalsyValues()
    {
        Assert.AreEqual(false, _formatter.Read<bool>(Bytes("0")));
        Assert.AreEqual(false, _formatter.Read<bool>(Bytes("off")));
        Assert.AreEqual(false, _formatter.Read<bool>(Bytes("false")));
    }

    [TestMethod]
    public void ReadGenericThrowsForUnrecognizedInput() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read<bool>(Bytes("yes")));

    [TestMethod]
    public void WritesTrueAsOne() => Assert.AreEqual("1", _formatter.Write(true, typeof(bool)));

    [TestMethod]
    public void WritesFalseAsZero() => Assert.AreEqual("0", _formatter.Write(false, typeof(bool)));

    [TestMethod]
    public void GetContentReturnsTrueContent() => Assert.IsNotNull(_formatter.GetContent(true));

    [TestMethod]
    public void GetContentReturnsFalseContent() => Assert.IsNotNull(_formatter.GetContent(false));

    [TestMethod]
    public void GetContentThrowsForNonBool() =>
        Assert.ThrowsExactly<NotSupportedException>(() => _formatter.GetContent(42));

}
