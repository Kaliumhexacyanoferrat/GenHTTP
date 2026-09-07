using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Testing.Acceptance.Engine.Compliance;

[TestClass]
public class ComplianceTests : WireTest
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoMinorHttpVersion(ServerEngine engine)
    {
        await TestAsync(["GET / HTTP/1", "Host: localhost"], "505", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNonSupportedVersion(ServerEngine engine)
    {
        await TestAsync(["GET / HTTP/1.2", "Host: localhost"], "505", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyHost(ServerEngine engine)
    {
        await TestAsync(["GET / HTTP/1.0", "Host: "], "400", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPostNoBody(ServerEngine engine)
    {
        await TestAsync(["POST / HTTP/1.0", "Host: host"], "404", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetWithBody(ServerEngine engine)
    {
        await TestAsync(["GET / HTTP/1.0", "Host: host", "Content-Length: 3", "", "123"], "400", Layout.Create(), engine);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestChunkWithSemicolon(ServerEngine engine)
    {
        var handler = Inline.Create().Post(async (Stream body) =>
        {
            var memoryStream = new MemoryStream();
    
            await body.CopyToAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        });

        await TestAsync(["POST / HTTP/1.0", "Host: host", "Transfer-Encoding: chunked", "", "5;", "hello", "", "0", ""], "400", handler, engine);
    }

}
