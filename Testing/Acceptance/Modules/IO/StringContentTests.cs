using System.Net;
using System.Text;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.IO;

[TestClass]
public sealed class StringContentTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLongUnicodeTextIsWrittenCompletely(ServerEngine engine)
    {
        var text = new StringBuilder().Insert(0, "Grüße 👋 ", 5000).Append('\uD800').ToString();

        var handler = Handler.From(r => r.Respond().Content(text).Build());

        await using var host = await TestHost.RunAsync(handler, defaults: false, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual(Encoding.UTF8.GetByteCount(text), response.Content.Headers.ContentLength);
        Assert.AreEqual(text.Replace('\uD800', '�'), await response.GetContentAsync());
    }

}
