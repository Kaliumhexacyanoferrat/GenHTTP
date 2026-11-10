using System.Net;
using System.Text.Json;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Testing.Acceptance.Modules.Conversion;

[TestClass]
public class SerializerTests
{

    #region Supporting data structures

    public record MyType(string StringValue, int IntValue);

    public sealed class BrokenType
    {

        public string Valid => "valid";

        public string Broken => throw new InvalidOperationException("Cannot be serialized");

    }

    #endregion

    #region Tests

    [TestMethod]
    public async Task TestCustomJsonOptions()
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = false
        };

        var serialization = Serialization.Default(jsonOptions: options);

        var api = Inline.Create()
                        .Get(() => new MyType("123", 456))
                        .Serializers(serialization);

        await using var host = await TestHost.RunAsync(api);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        AssertX.DoesNotContain("\n", await response.GetContentAsync());
    }

    [TestMethod]
    [MultiEngineFrameworkTest]
    public async Task TestJsonResponseAfterSerializationFailure(ServerEngine engine, ExecutionMode mode)
    {
        var api = Inline.Create()
                        .Get("broken", () => new BrokenType())
                        .Get("valid", () => new MyType("123", 456))
                        .ExecutionMode(mode);

        await using var host = await TestHost.RunAsync(api, defaults: false, engine: engine);

        try
        {
            using var _ = await host.GetResponseAsync("/broken");
        }
        catch (HttpRequestException)
        {
            // depending on the engine, the connection is aborted or the body stays empty
        }

        using var response = await host.GetResponseAsync("/valid");

        await response.AssertStatusAsync(HttpStatusCode.OK);

        Assert.AreEqual("{\"stringValue\":\"123\",\"intValue\":456}", await response.GetContentAsync());
    }

    #endregion

}
