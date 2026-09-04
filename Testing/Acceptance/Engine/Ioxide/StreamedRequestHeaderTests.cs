using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Ioxide.Protocol.Requests;

namespace GenHTTP.Testing.Acceptance.Engine.Ioxide;

/// <summary>
/// The request head built for an HTTP/2 or HTTP/3 stream: :authority folded in as a Host header, the
/// query taken off the path, and the version fixed by the protocol. Built directly rather than over a
/// socket, since none of this touches the reactor - so it also covers the cases a real client cannot
/// produce, such as a request that carries no authority at all.
/// </summary>
[TestClass]
public sealed class StreamedRequestHeaderTests
{

    [TestMethod]
    public void TestAuthorityIsFoldedInAsHost()
    {
        var header = Build(authority: "localhost:8080", headers: [Entry("accept", "text/plain")]);

        // The synthesized Host leads, ahead of the headers the client sent.
        Assert.AreEqual(2, header.Headers.Count);

        var first = header.Headers.GetStringEntry(0);

        Assert.AreEqual("host", first.Key.ToString());
        Assert.AreEqual("localhost:8080", first.Value.ToString());

        Assert.AreEqual("localhost:8080", header.Headers.GetEntry("host"));
    }

    [TestMethod]
    public void TestClientHostHeaderIsNotDuplicated()
    {
        // The client's own Host wins, matched case-insensitively so a mixed-case name still counts.
        var header = Build(authority: "localhost:8080", headers: [Entry("Host", "client.example")]);

        Assert.AreEqual(1, header.Headers.Count);
        Assert.AreEqual("client.example", header.Headers.GetEntry("host"));
    }

    [TestMethod]
    public void TestNoAuthorityLeavesTheHeadersAlone()
    {
        var header = Build(authority: "", headers: [Entry("accept", "text/plain")]);

        Assert.AreEqual(1, header.Headers.Count);
        Assert.IsNull(header.Headers.GetEntry("host"));
    }

    [TestMethod]
    public void TestQueryIsTakenOffThePath()
    {
        var header = Build(path: "/resource?x=1&y=2");

        Assert.AreEqual("/resource", header.Path.ToString());
    }

    [TestMethod]
    public void TestPathWithoutAQueryIsKeptWhole()
    {
        var header = Build(path: "/resource");

        Assert.AreEqual("/resource", header.Path.ToString());
    }

    [TestMethod]
    public void TestMethodIsExposed()
    {
        var header = Build(method: "POST");

        Assert.AreEqual(RequestMethod.Post, header.Method);
    }

    [TestMethod]
    public void TestHttp2SetsItsProtocolAndVersion()
    {
        var header = Build(protocol: HttpProtocol.Http2);

        Assert.AreEqual(HttpProtocol.Http2, header.Protocol);
        Assert.AreEqual("HTTP/2.0", Encoding.ASCII.GetString(header.Version.Span));
    }

    [TestMethod]
    public void TestHttp3SetsItsProtocolAndVersion()
    {
        var header = Build(protocol: HttpProtocol.Http3);

        Assert.AreEqual(HttpProtocol.Http3, header.Protocol);
        Assert.AreEqual("HTTP/3.0", Encoding.ASCII.GetString(header.Version.Span));
    }

    [TestMethod]
    public void TestQueryParametersArePreserved()
    {
        var header = Build(query: [Entry("x", "1"), Entry("y", "2")]);

        Assert.AreEqual(2, header.Query.Count);
        Assert.AreEqual("1", header.Query.GetEntry("x"));
        Assert.AreEqual("2", header.Query.GetEntry("y"));
    }

    #region Infrastructure

    private static StreamedRequestHeader Build(
        string method = "GET",
        string path = "/",
        string authority = "localhost:8080",
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)>? headers = null,
        List<(ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value)>? query = null,
        HttpProtocol? protocol = null)
        => new(Bytes(method), Bytes(path), authority.Length == 0 ? default : Bytes(authority),
               headers ?? [], query ?? [], protocol ?? HttpProtocol.Http2);

    private static (ReadOnlyMemory<byte> Name, ReadOnlyMemory<byte> Value) Entry(string name, string value)
        => (Bytes(name), Bytes(value));

    private static ReadOnlyMemory<byte> Bytes(string value) => Encoding.ASCII.GetBytes(value);

    #endregion

}
