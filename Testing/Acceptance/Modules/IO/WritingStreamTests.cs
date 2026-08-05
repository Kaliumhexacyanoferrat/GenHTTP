using System.IO.Pipelines;
using System.Text;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class WritingStreamTests
{

    [TestMethod]
    public void TestReadWrite()
    {
        using var target = new MemoryStream();

        var writer = PipeWriter.Create(target);

        using var stream = new WritingStream(writer, target);

        stream.Write("Hello"u8.ToArray(), 0, 5);
        stream.WriteByte((byte)'!');
        stream.Flush();

        Assert.AreEqual(6, stream.Length);

        target.Position = 0;

        var buffer = new byte[6];
        Assert.AreEqual(6, stream.Read(buffer, 0, 6));
        Assert.AreEqual("Hello!", Encoding.ASCII.GetString(buffer));
    }

    [TestMethod]
    public void TestDelegatesToBaseStream()
    {
        using var target = new MemoryStream(new byte[10]);

        var writer = PipeWriter.Create(target);

        using var stream = new WritingStream(writer, target);

        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanSeek);
        Assert.IsTrue(stream.CanWrite);

        stream.Seek(2, SeekOrigin.Begin);
        Assert.AreEqual(2, stream.Position);

        stream.SetLength(5);
        Assert.AreEqual(5, target.Length);
    }

}
