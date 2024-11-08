using System.Net;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Functional;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Reflection;

[TestClass]
public class ErrorHandlingTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSerializationNotPossible(TestEngine engine)
    {
        var serialization = Serialization.Empty()
                                         .Default(ContentType.AudioMp4);

        var api = Inline.Create()
                        .Get(() => new HashSet<int>())
                        .Serializers(serialization);

        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.UnsupportedMediaType);
    }

}
