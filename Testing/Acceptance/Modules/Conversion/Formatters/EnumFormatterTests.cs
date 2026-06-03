using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class EnumFormatterTests
{
    private readonly EnumFormatter _formatter = new();

    private static ByteString Bytes(string s) => new(s);

    [TestMethod]
    public void HandlesEnumsOnly()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(DayOfWeek)));
        Assert.IsFalse(_formatter.CanHandle(typeof(int)));
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
    }

    [TestMethod]
    public void ReadsEnumByName() => Assert.AreEqual(DayOfWeek.Monday, _formatter.Read(Bytes("Monday"), typeof(DayOfWeek)));

    [TestMethod]
    public void IsCaseSensitive() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("monday"), typeof(DayOfWeek)));

    [TestMethod]
    public void ThrowsForUnknownName() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("Bogus"), typeof(DayOfWeek)));

    [TestMethod]
    public void WritesEnumName() => Assert.AreEqual("Monday", _formatter.Write(DayOfWeek.Monday, typeof(DayOfWeek)));
    
}
