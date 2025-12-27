using System.Net;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

using Cookie = GenHTTP.Api.Protocol.Cookie;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ResultTests
{

    #region Supporting data structures

    public record MyPayload(string Message);

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestResponseCanBeModified(TestEngine engine, ExecutionMode mode)
    {
        var result = new Result<MyPayload>(new MyPayload("Hello World!"))
                     .Status(ResponseStatus.Accepted)
                     .Status(202, "Accepted Custom")
                     .Connection(Connection.Close)
                     .Type(new FlexibleContentType(ContentType.TextRichText))
                     .Modified(DateTime.UtcNow)
                     .Expires(DateTime.UtcNow)
                     .Header("X-Custom", "Value")
                     .Cookie(new Cookie("Cookie", "Value"))
                     .Encoding("my-encoding");

        var inline = Inline.Create()
                           .Get(() => result)
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Accepted);

        Assert.AreEqual("Value", response.GetHeader("X-Custom"));

        Assert.AreEqual("close", response.GetHeader("Connection")?.ToLower());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestStreamsCanBeWrapped(TestEngine engine, ExecutionMode mode)
    {
        var stream = new MemoryStream("Hello World!"u8.ToArray());

        var inline = Inline.Create()
                           .Get(() => new Result<Stream>(stream).Status(ResponseStatus.Created))
                           .ExecutionMode(mode);

        await using var runner = await TestHost.RunAsync(inline, engine: engine);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Created);

        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    #endregion

}
