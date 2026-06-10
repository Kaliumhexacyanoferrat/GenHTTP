using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion.Formatters;

[TestClass]
public sealed class DateFormatterTests
{
    private readonly DateFormatter _formatter = new();

    private static ByteString Bytes(string s) => new(Encoding.UTF8.GetBytes(s));

    #region CanHandle

    [TestMethod]
    public void HandlesDateTypes()
    {
        Assert.IsTrue(_formatter.CanHandle(typeof(DateTime)));
        Assert.IsTrue(_formatter.CanHandle(typeof(DateTimeOffset)));
        Assert.IsTrue(_formatter.CanHandle(typeof(DateOnly)));
        Assert.IsTrue(_formatter.CanHandle(typeof(TimeOnly)));
        Assert.IsTrue(_formatter.CanHandle(typeof(TimeSpan)));
    }

    [TestMethod]
    public void DoesNotHandleOtherTypes()
    {
        Assert.IsFalse(_formatter.CanHandle(typeof(int)));
        Assert.IsFalse(_formatter.CanHandle(typeof(string)));
        Assert.IsFalse(_formatter.CanHandle(typeof(bool)));
    }

    #endregion

    #region Read

    [TestMethod]
    public void ReadsDateTime()
    {
        var result = (DateTime)_formatter.Read(Bytes("01/15/2024 10:30:00"), typeof(DateTime));
        Assert.AreEqual(new DateTime(2024, 1, 15, 10, 30, 0), result);
    }

    [TestMethod]
    public void ReadsDateTimeOffset()
    {
        var result = (DateTimeOffset)_formatter.Read(Bytes("01/15/2024 10:30:00 +00:00"), typeof(DateTimeOffset));
        Assert.AreEqual(new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero), result);
    }

    [TestMethod]
    public void ReadsDateOnly()
    {
        var result = (DateOnly)_formatter.Read(Bytes("2024-01-15"), typeof(DateOnly));
        Assert.AreEqual(new DateOnly(2024, 1, 15), result);
    }

    [TestMethod]
    public void ReadsTimeOnly()
    {
        var result = (TimeOnly)_formatter.Read(Bytes("10:30:00"), typeof(TimeOnly));
        Assert.AreEqual(new TimeOnly(10, 30, 0), result);
    }

    [TestMethod]
    public void ReadsTimeSpan()
    {
        var result = (TimeSpan)_formatter.Read(Bytes("01:30:00"), typeof(TimeSpan));
        Assert.AreEqual(new TimeSpan(1, 30, 0), result);
    }

    [TestMethod]
    public void ThrowsForInvalidDateTime() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("not-a-date"), typeof(DateTime)));

    [TestMethod]
    public void ThrowsForInvalidDateOnly() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("15-01-2024"), typeof(DateOnly)));

    [TestMethod]
    public void ThrowsForInvalidTimeSpan() =>
        Assert.ThrowsExactly<ArgumentException>(() => _formatter.Read(Bytes("not-a-timespan"), typeof(TimeSpan)));

    #endregion

    #region Write

    [TestMethod]
    public void WritesDateTime()
    {
        var value = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        Assert.AreEqual("2024-01-15T10:30:00.0000000Z", _formatter.Write(value, typeof(DateTime)));
    }

    [TestMethod]
    public void WritesDateTimeOffset()
    {
        var value = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero);
        Assert.AreEqual("2024-01-15T10:30:00.0000000+00:00", _formatter.Write(value, typeof(DateTimeOffset)));
    }

    [TestMethod]
    public void WritesDateOnly()
    {
        var value = new DateOnly(2024, 1, 15);
        Assert.AreEqual("2024-01-15", _formatter.Write(value, typeof(DateOnly)));
    }

    [TestMethod]
    public void WritesTimeOnly()
    {
        var value = new TimeOnly(10, 30, 0);
        Assert.AreEqual("10:30:00.0000000", _formatter.Write(value, typeof(TimeOnly)));
    }

    [TestMethod]
    public void WritesTimeSpan()
    {
        var value = new TimeSpan(1, 30, 0);
        Assert.AreEqual("01:30:00", _formatter.Write(value, typeof(TimeSpan)));
    }

    [TestMethod]
    public void WritesDateOnlyRoundTrip()
    {
        var original = new DateOnly(2024, 6, 10);
        var written = _formatter.Write(original, typeof(DateOnly));
        var readBack = (DateOnly)_formatter.Read(Bytes(written), typeof(DateOnly));
        Assert.AreEqual(original, readBack);
    }

    [TestMethod]
    public void WritesTimeOnlyRoundTrip()
    {
        var original = new TimeOnly(14, 5, 59);
        var written = _formatter.Write(original, typeof(TimeOnly));
        var readBack = (TimeOnly)_formatter.Read(Bytes(written), typeof(TimeOnly));
        Assert.AreEqual(original, readBack);
    }

    [TestMethod]
    public void WritesTimeSpanRoundTrip()
    {
        var original = new TimeSpan(2, 15, 30);
        var written = _formatter.Write(original, typeof(TimeSpan));
        var readBack = (TimeSpan)_formatter.Read(Bytes(written), typeof(TimeSpan));
        Assert.AreEqual(original, readBack);
    }

    #endregion

}
