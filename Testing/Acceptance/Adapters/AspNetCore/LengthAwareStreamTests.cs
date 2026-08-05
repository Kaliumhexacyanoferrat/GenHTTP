using System.Text;

using GenHTTP.Adapters.AspNetCore.Context;

namespace GenHTTP.Testing.Acceptance.Adapters.AspNetCore;

[TestClass]
public sealed class LengthAwareStreamTests
{

    [TestMethod]
    public async Task TestReadReportsGivenLength()
    {
        using var inner = new MemoryStream("Hello"u8.ToArray());

        using var stream = new LengthAwareStream(inner, 42);

        Assert.AreEqual(42, stream.Length);

        var buffer = new byte[5];
        var read = await stream.ReadAsync(buffer);

        Assert.AreEqual(5, read);
        Assert.AreEqual("Hello", Encoding.ASCII.GetString(buffer, 0, read));
    }

    [TestMethod]
    public void TestBasics()
    {
        using var stream = new LengthAwareStream(new MemoryStream(), 0);

        Assert.IsTrue(stream.CanRead);
        Assert.IsFalse(stream.CanWrite);
        Assert.IsFalse(stream.CanSeek);

        Assert.ThrowsExactly<NotSupportedException>(() => stream.Flush());
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Seek(0, SeekOrigin.Begin));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(0));
        Assert.ThrowsExactly<NotSupportedException>(() => stream.Write([], 0, 0));
        Assert.ThrowsExactly<NotSupportedException>(() => _ = stream.Position);
    }

}
