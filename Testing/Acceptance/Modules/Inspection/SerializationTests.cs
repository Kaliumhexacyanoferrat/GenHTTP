using System.Net;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Inspection;
using GenHTTP.Modules.Layouting;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules.Inspection;

[TestClass]
public sealed class SerializationTests
{

    [TestMethod]
    public async Task TestCustomFormat()
    {
        var serialization = Serialization.Empty()
                                         .Default(ContentType.ApplicationJson)
                                         .Add(ContentType.ApplicationJson, new JsonFormat())
                                         .Build();

        var inspection = Inspector.Create().Serialization(serialization);

        var app = Layout.Create().Add(inspection);

        await using var host = await TestHost.RunAsync(app);

        using var inspected = await host.GetResponseAsync("/one/two?inspect");

        await inspected.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("application/json", inspected.GetContentHeader("Content-Type"));
    }

    [TestMethod]
    public async Task TestNoFormats()
    {
        var serialization = Serialization.Empty()
                                         .Default(ContentType.AudioMp4)
                                         .Build();

        var inspection = Inspector.Create().Serialization(serialization);

        var app = Layout.Create().Add(inspection);

        await using var host = await TestHost.RunAsync(app);

        using var inspected = await host.GetResponseAsync("/one/two?inspect");

        await inspected.AssertStatusAsync(HttpStatusCode.UnsupportedMediaType);
    }

}
