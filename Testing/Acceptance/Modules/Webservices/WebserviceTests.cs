using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Serialization;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using GenHTTP.Testing.Acceptance.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GenHTTP.Testing.Acceptance.Modules.Webservices;

[TestClass]
public sealed class WebserviceTests
{

    #region Supporting structures

    public sealed class TestEntity
    {

        public int ID { get; set; }

        public double? Nullable { get; set; }

    }

    public enum TestEnum
    {
        One,
        Two
    }

    public sealed class TestResource
    {

        [ResourceMethod("nothing")]
        public void DoNothing() { }

        [ResourceMethod("primitive")]
        public int Primitive(int input) => input;

        [ResourceMethod("guid")]
        public Guid Guid(Guid id) => id;

        [ResourceMethod(RequestMethod.Post, "entity")]
        public TestEntity Entity(TestEntity entity) => entity;

        [ResourceMethod(RequestMethod.Put, "stream")]
        public Stream Stream(Stream input) => new MemoryStream(Encoding.UTF8.GetBytes(input.Length.ToString()));

        [ResourceMethod("requestResponse")]
        public ValueTask<IResponse?> RequestResponse(IRequest request) => request.Respond()
                                                                                 .Content("Hello World")
                                                                                 .Type(ContentType.TextPlain)
                                                                                 .BuildTask();

        [ResourceMethod("exception")]
        public void Exception() => throw new ProviderException(ResponseStatus.AlreadyReported, "Already reported!");

        [ResourceMethod("duplicate")]
        public void Duplicate1() { }

        [ResourceMethod("duplicate")]
        public void Duplicate2() { }

        [ResourceMethod("param/:param")]
        public int PathParam(int param) => param;

        [ResourceMethod("regex/(?<param>[0-9]+)")]
        public int RegexParam(int param) => param;

        [ResourceMethod]
        public void Empty() { }

        [ResourceMethod("enum")]
        public TestEnum Enum(TestEnum input) => input;

        [ResourceMethod("nullable")]
        public int? Nullable(int? input) => input;

        [ResourceMethod("request")]
        public string Request(IHandler handler, IRequest request) => "yes";
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmpty(TestEngine engine)
    {
        await WithResponse(engine, "", async r => { await r.AssertStatusAsync(HttpStatusCode.NoContent); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestVoidReturn(TestEngine engine)
    {
        await WithResponse(engine, "nothing", async r => { await r.AssertStatusAsync(HttpStatusCode.NoContent); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPrimitives(TestEngine engine)
    {
        await WithResponse(engine, "primitive?input=42", async r => Assert.AreEqual("42", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEnums(TestEngine engine)
    {
        await WithResponse(engine, "enum?input=One", async r => Assert.AreEqual("One", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNullableSet(TestEngine engine)
    {
        await WithResponse(engine, "nullable?input=1", async r => Assert.AreEqual("1", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNullableNotSet(TestEngine engine)
    {
        await WithResponse(engine, "nullable", async r => { await r.AssertStatusAsync(HttpStatusCode.NoContent); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGuid(TestEngine engine)
    {
        var id = Guid.NewGuid().ToString();

        await WithResponse(engine, $"guid?id={id}", async r => Assert.AreEqual(id, await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestParam(TestEngine engine)
    {
        await WithResponse(engine, "param/42", async r => Assert.AreEqual("42", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConversionFailure(TestEngine engine)
    {
        await WithResponse(engine, "param/abc", async r => { await r.AssertStatusAsync(HttpStatusCode.BadRequest); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRegex(TestEngine engine)
    {
        await WithResponse(engine, "regex/42", async r => Assert.AreEqual("42", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEntityWithNulls(TestEngine engine)
    {
        const string entity = "{\"id\":42}";
        await WithResponse(engine, "entity", HttpMethod.Post, entity, null, null, async r => Assert.AreEqual(entity, await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEntityWithNoNulls(TestEngine engine)
    {
        const string entity = "{\"id\":42,\"nullable\":123.456}";
        await WithResponse(engine, "entity", HttpMethod.Post, entity, null, null, async r => Assert.AreEqual(entity, await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNotSupportedUpload(TestEngine engine)
    {
        await WithResponse(engine, "entity", HttpMethod.Post, "123", "bla/blubb", null, async r => { await r.AssertStatusAsync(HttpStatusCode.UnsupportedMediaType); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUnsupportedDownloadEnforcesDefault(TestEngine engine)
    {
        const string entity = "{\"id\":42,\"nullable\":123.456}";
        await WithResponse(engine, "entity", HttpMethod.Post, entity, null, "bla/blubb", async r => Assert.AreEqual(entity, await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestWrongMethod(TestEngine engine)
    {
        await WithResponse(engine, "entity", HttpMethod.Put, "123", null, null, async r => { await r.AssertStatusAsync(HttpStatusCode.MethodNotAllowed); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoMethod(TestEngine engine)
    {
        await WithResponse(engine, "idonotexist", async r => { await r.AssertStatusAsync(HttpStatusCode.NotFound); });
    }

    [TestMethod]
    public async Task TestStream()
    {
        await WithResponse(TestEngine.Internal, "stream", HttpMethod.Put, "123456", null, null, async r => Assert.AreEqual("6", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestResponse(TestEngine engine)
    {
        await WithResponse(engine, "requestResponse", async r => Assert.AreEqual("Hello World", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRouting(TestEngine engine)
    {
        await WithResponse(engine, "request", async r => Assert.AreEqual("yes", await r.GetContentAsync()));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEntityAsXml(TestEngine engine)
    {
        const string entity = "<TestEntity><ID>1</ID><Nullable>1234.56</Nullable></TestEntity>";

        await WithResponse(engine, "entity", HttpMethod.Post, entity, "text/xml", "text/xml", async r =>
        {
            var result = new XmlSerializer(typeof(TestEntity)).Deserialize(await r.Content.ReadAsStreamAsync()) as TestEntity;

            Assert.IsNotNull(result);

            Assert.AreEqual(1, result.ID);
            Assert.AreEqual(1234.56, result.Nullable);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEntityAsYaml(TestEngine engine)
    {
        const string entity = """
                              id: 1
                              nullable: 1234.56
                              """;

        await WithResponse(engine, "entity", HttpMethod.Post, entity, "application/yaml", "application/yaml", async r =>
        {
            await r.AssertStatusAsync(HttpStatusCode.OK);

            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                        .Build();

            using var reader = new StreamReader(await r.Content.ReadAsStreamAsync(), leaveOpen: true);

            var result = deserializer.Deserialize(reader, typeof(TestEntity)) as TestEntity;

            Assert.IsNotNull(result);

            Assert.AreEqual(1, result.ID);
            Assert.AreEqual(1234.56, result.Nullable);
        });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestException(TestEngine engine)
    {
        await WithResponse(engine, "exception", async r => { await r.AssertStatusAsync(HttpStatusCode.AlreadyReported); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDuplicate(TestEngine engine)
    {
        await WithResponse(engine, "duplicate", async r => { await r.AssertStatusAsync(HttpStatusCode.BadRequest); });
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestWithInstance(TestEngine engine)
    {
        var layout = Layout.Create().AddService("t", new TestResource());

        await using var runner = await TestHost.RunAsync(layout);

        using var response = await runner.GetResponseAsync("/t");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public void TestConcernChaining()
    {
        var service = ServiceResource.From<TestResource>();

        Chain.Works(service);
    }

    #endregion

    #region Helpers

    private Task WithResponse(TestEngine engine, string uri, Func<HttpResponseMessage, Task> logic) => WithResponse(engine, uri, HttpMethod.Get, null, null, null, logic);

    private async Task WithResponse(TestEngine engine, string uri, HttpMethod method, string? body, string? contentType, string? accept, Func<HttpResponseMessage, Task> logic)
    {
        await using var service = await GetServiceAsync(engine);

        var request = service.GetRequest($"/t/{uri}");

        request.Method = method;

        if (accept is not null)
        {
            request.Headers.Add("Accept", accept);
        }

        if (body is not null)
        {
            if (contentType is not null)
            {
                request.Content = new StringContent(body, null, contentType);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            else
            {
                request.Content = new StringContent(body);
                request.Content.Headers.ContentType = null;
            }
        }

        using var response = await service.GetResponseAsync(request);

        await logic(response);
    }

    private static async Task<TestHost> GetServiceAsync(TestEngine engine)
    {
        var service = ServiceResource.From<TestResource>()
                                     .Serializers(Serialization.Default())
                                     .Injectors(Injection.Default());

        return await TestHost.RunAsync(Layout.Create().Add("t", service), engine: engine);
    }

    #endregion

}
