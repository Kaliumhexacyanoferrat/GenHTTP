using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class StringFormatterTests
{
    private readonly StringFormatter _formatter = new();

    private static ByteString Bytes(string s) => new(Encoding.UTF8.GetBytes(s));

    [TestMethod] public void HandlesOnlyString()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(string)));
        Assert.IsFalse(_formatter.CanHandle(typeof(int)));
        Assert.IsFalse(_formatter.CanHandle(typeof(bool)));
    }

    [TestMethod] public void ReadsAsciiString() => Assert.AreEqual("hello", _formatter.Read(Bytes("hello"), typeof(string)));

    [TestMethod] public void ReadsEmptyString() => Assert.AreEqual("", _formatter.Read(Bytes(""), typeof(string)));

    [TestMethod] public void ReadsUtf8String() => Assert.AreEqual("héllo", _formatter.Read(Bytes("héllo"), typeof(string)));

    [TestMethod] public void WritesString() => Assert.AreEqual("hello", _formatter.Write("hello", typeof(string)));
    
}
