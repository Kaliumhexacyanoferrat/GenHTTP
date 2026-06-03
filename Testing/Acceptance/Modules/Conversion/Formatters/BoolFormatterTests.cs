using System.Text;
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
    public void ReturnsNullForUnrecognizedInput()
    {
        Assert.IsNull(_formatter.Read(Bytes("yes"), typeof(bool)));
        Assert.IsNull(_formatter.Read(Bytes("maybe"), typeof(bool)));
    }

    [TestMethod]
    public void WritesTrueAsOne() => Assert.AreEqual("1", _formatter.Write(true, typeof(bool)));

    [TestMethod]
    public void WritesFalseAsZero() => Assert.AreEqual("0", _formatter.Write(false, typeof(bool)));
    
}
