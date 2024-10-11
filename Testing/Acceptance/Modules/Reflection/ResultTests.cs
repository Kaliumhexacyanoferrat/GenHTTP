using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public sealed class ResultTests
{

    #region Supporting data structures

    public record class MyPayload(string Message);

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestResponseCanBeModified()
    {
            var result = new Result<MyPayload>(new("Hello World!"))
                .Status(ResponseStatus.Accepted)
                .Status(202, "Accepted Custom")
                .Type(new(ContentType.TextRichText))
                .Modified(DateTime.UtcNow)
                .Expires(DateTime.UtcNow)
                .Header("X-Custom", "Value")
                .Cookie(new("Cookie", "Value"))
                .Encoding("my-encoding");

            var inline = Inline.Create()
                               .Get(() => result);

            using var runner = TestHost.Run(inline);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.Accepted);

            Assert.AreEqual("Value", response.GetHeader("X-Custom"));
        }

    [TestMethod]
    public async Task TestStreamsCanBeWrapped()
    {
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!"));

            var inline = Inline.Create()
                               .Get(() => new Result<Stream>(stream).Status(ResponseStatus.Created));

            using var runner = TestHost.Run(inline);

            using var response = await runner.GetResponseAsync();

            await response.AssertStatusAsync(HttpStatusCode.Created);

            Assert.AreEqual("Hello World!", await response.GetContentAsync());
        }

    #endregion

}
