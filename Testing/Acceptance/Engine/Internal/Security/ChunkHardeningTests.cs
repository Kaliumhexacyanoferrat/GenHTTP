namespace GenHTTP.Testing.Acceptance.Engine.Internal.Security;

[TestClass]
public class ChunkHardeningTests : WireTest
{

    [TestMethod]
    public async Task TestInvalidChunkSize()
    {
        var request = new[]
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Transfer-Encoding: chunked",
            "",
            "Z",
            "test",
            "0",
            ""
        };

        await TestAsync(request, "Invalid chunk size format");
    }

    [TestMethod]
    public async Task TestChunkExtensionsAreRejected()
    {
        var request = new[]
        {
            "POST / HTTP/1.1",
            "Host: bla",
            "Transfer-Encoding: chunked",
            "",
            "5;ext=test",
            "Hello",
            "0",
            ""
        };

        await TestAsync(request, "Invalid chunk size format");
    }

}
