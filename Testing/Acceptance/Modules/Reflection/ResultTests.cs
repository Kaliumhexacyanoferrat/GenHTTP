using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public async Task TestResponseCanBeModified()
    {
        var result = new Result<MyPayload>(new MyPayload("Hello World!"))
                     .Status(ResponseStatus.Accepted)
                     .Status(202, "Accepted Custom")
                     .Type(new FlexibleContentType(ContentType.TextRichText))
                     .Modified(DateTime.UtcNow)
                     .Expires(DateTime.UtcNow)
                     .Header("X-Custom", "Value")
                     .Cookie(new Cookie("Cookie", "Value"))
                     .Encoding("my-encoding");

        var inline = Inline.Create()
                           .Get(() => result);

        await using var runner = await TestHost.RunAsync(inline);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Accepted);

        Assert.AreEqual("Value", response.GetHeader("X-Custom"));
    }

    [TestMethod]
    public async Task TestStreamsCanBeWrapped()
    {
        var stream = new MemoryStream("Hello World!"u8.ToArray());

        var inline = Inline.Create()
                           .Get(() => new Result<Stream>(stream).Status(ResponseStatus.Created));

        await using var runner = await TestHost.RunAsync(inline);

        using var response = await runner.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.Created);

        Assert.AreEqual("Hello World!", await response.GetContentAsync());
    }

    #endregion

}
