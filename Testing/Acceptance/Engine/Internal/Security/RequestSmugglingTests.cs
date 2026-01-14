namespace GenHTTP.Testing.Acceptance.Engine.Internal.Security;

[TestClass]
public class RequestSmugglingTests : WireTest
{

    [TestMethod]
    public async Task TestOnlyOneContentLength()
    {
        var request = new []
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Content-Length: 19",
            "Content-Length: 0"
        };

        await TestAsync(request, "Multiple 'Content-Length' headers specified");
    }

    [TestMethod]
    public async Task TestMixedContent()
    {
        var request = new []
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Content-Length: 19",
            "Transfer-Encoding: chunked"
        };

        await TestAsync(request, "Both 'Content-Length' and 'Transfer-Encoding' have been specified");
    }

    [TestMethod]
    public async Task TestNegativeContentLength()
    {
        var request = new []
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Content-Length: -1"
        };

        await TestAsync(request, "Unable to parse the given 'Content-Length' header");
    }

    [TestMethod]
    public async Task TestOnlyOneTransferEncoding()
    {
        var request = new []
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Transfer-Encoding: chunked",
            "Transfer-Encoding: chunked"
        };

        await TestAsync(request, "Multiple 'Transfer-Encoding' headers specified");
    }

    [TestMethod]
    public async Task TestMultipleContentLenghtViaSingleHeader()
    {
        var request = new []
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Content-Length: 19, 0"
        };

        await TestAsync(request, "Unable to parse the given 'Content-Length' header");
    }

    [TestMethod]
    public async Task TestMultipleTransferEncodingViaSingleHeader()
    {
        var request = new []
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Transfer-Encoding: chunked, identity"
        };

        await TestAsync(request, "Only transfer encoding mode 'chunked' is allowed by this endpoint");
    }

    [TestMethod]
    public async Task TestPlusPrefixedContentLength()
    {
        var request = new[]
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Content-Length: +10"
        };

        await TestAsync(request, "Unable to parse the given 'Content-Length' header");
    }

}
