using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Serializers.Json;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.OpenApi;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public sealed class SchemaTests
{

    #region Supporting data structures

    public record Key(string PublicKey, int KeyLength);

    public sealed class FileModel
    {

        public string Name { get; set; } = "file.txt";

        [JsonIgnore]
        public bool IsCompiled { get; set; }

        [JsonPropertyName("size_in_bytes")]
        [Summary("The size of the file")]
        public long Size { get; set; } = 42;

    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPropertiesFollowDefaultNamingPolicy(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => new Key("abc", 2048))
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var properties = doc.Components?.Schemas?[nameof(Key)].Properties!;

        CollectionAssert.AreEquivalent(new[] { "publicKey", "keyLength" }, properties.Keys.ToArray());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSchemaMatchesWireFormat(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => new FileModel())
                        .Add(ApiDescription.Create());

        await using var host = await TestHost.RunAsync(api, engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        var sent = json.RootElement.EnumerateObject().Select(p => p.Name).ToArray();

        using var specResponse = await host.GetResponseAsync("/openapi.json");

        var doc = (await specResponse.AsOpenApiAsync()).Document!;

        var described = doc.Components?.Schemas?[nameof(FileModel)].Properties!.Keys.ToArray();

        CollectionAssert.AreEquivalent(sent, described);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestIgnoredPropertiesAreNotDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => new FileModel())
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var properties = doc.Components?.Schemas?[nameof(FileModel)].Properties!;

        Assert.IsFalse(properties.Keys.Any(k => string.Equals(k, nameof(FileModel.IsCompiled), StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRenamedPropertyKeepsDescription(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => new FileModel())
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var properties = doc.Components?.Schemas?[nameof(FileModel)].Properties!;

        Assert.AreEqual("The size of the file", properties["size_in_bytes"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCustomSerializerOptionsAreRespected(ServerEngine engine)
    {
        var options = JsonFormat.GetDefaultOptions();

        options.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

        var api = Inline.Create()
                        .Serializers(Serialization.Default(options))
                        .Get(() => new Key("abc", 2048))
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var properties = doc.Components?.Schemas?[nameof(Key)].Properties!;

        CollectionAssert.AreEquivalent(new[] { "public_key", "key_length" }, properties.Keys.ToArray());
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestBodyFollowsNamingPolicy(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post((Key key) => key.KeyLength)
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var properties = doc.Components?.Schemas?[nameof(Key)].Properties!;

        CollectionAssert.AreEquivalent(new[] { "publicKey", "keyLength" }, properties.Keys.ToArray());
    }

    #endregion

}
