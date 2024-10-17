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
    public async Task TestSerializationNotPossible()
    {
        var serialization = Serialization.Empty()
                                         .Default(ContentType.AudioMp4);

        var api = Inline.Create()
                        .Get(() => new HashSet<int>())
                        .Serializers(serialization);

        using var host = TestHost.Run(api);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.UnsupportedMediaType);
    }
}
