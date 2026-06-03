using System.Text;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class DateOnlyFormatterTests
{
    private readonly DateOnlyFormatter _formatter = new();

    private static readonly DateOnly TestDate = new(2024, 6, 15);
    private const string TestDateString = "2024-06-15";

    private static ReadOnlyMemory<byte> Bytes(string s) => Encoding.UTF8.GetBytes(s);

    [TestMethod]
    public void HandlesOnlyDateOnly()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(DateOnly)));
        Assert.IsFalse(_formatter.CanHandle(typeof(DateTime)));
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
    }

    [TestMethod]
    public void ReadsValidDate() => Assert.AreEqual(TestDate, _formatter.Read(Bytes(TestDateString), typeof(DateOnly)));

    [TestMethod]
    public void ThrowsForWrongLength() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("2024-6-15"), typeof(DateOnly)));

    [TestMethod]
    public void ThrowsForWrongSeparator() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("2024/06/15"), typeof(DateOnly)));

    [TestMethod]
    public void ThrowsForInvalidMonth() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("2024-13-01"), typeof(DateOnly)));

    [TestMethod]
    public void ThrowsForInvalidDay() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("2024-02-30"), typeof(DateOnly)));

    [TestMethod]
    public void WritesDateInIsoFormat() => Assert.AreEqual(TestDateString, _formatter.Write(TestDate, typeof(DateOnly)));

    [TestMethod]
    public void PadsMonthAndDay() => Assert.AreEqual("2024-01-05", _formatter.Write(new DateOnly(2024, 1, 5), typeof(DateOnly)));
    
}
