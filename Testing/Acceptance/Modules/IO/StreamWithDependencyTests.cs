using System.Text;

using GenHTTP.Modules.IO.Streaming;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public class StreamWithDependencyTests
{

    [TestMethod]
    public void TestWrapper()
    {
        using var source = new MemoryStream("Hello World "u8.ToArray());

        using var stream = new StreamWithDependency(source, source);

        stream.SetLength(11);

        Assert.IsTrue(stream.CanRead);
        Assert.IsTrue(stream.CanWrite);
        Assert.IsTrue(stream.CanSeek);

        Assert.AreEqual(0, stream.Position);
        Assert.AreEqual(11, stream.Length);

        stream.Seek(0, SeekOrigin.End);

        stream.Write("!"u8.ToArray(), 0, 1);
        stream.Flush();

        stream.Position = 1;

        var buffer = new byte[stream.Length - 1];

        Assert.AreEqual(11, stream.Read(buffer, 0, buffer.Length));

        Assert.AreEqual("ello World!", Encoding.UTF8.GetString(buffer, 0, buffer.Length));
    }

}
