﻿using System.Net;
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GenHTTP.Testing.Acceptance.Modules;

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
    public async Task TestEmpty()
    {
        await WithResponse("", async r => { await r.AssertStatusAsync(HttpStatusCode.NoContent); });
    }

    [TestMethod]
    public async Task TestVoidReturn()
    {
        await WithResponse("nothing", async r => { await r.AssertStatusAsync(HttpStatusCode.NoContent); });
    }

    [TestMethod]
    public async Task TestPrimitives()
    {
        await WithResponse("primitive?input=42", async r => Assert.AreEqual("42", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestEnums()
    {
        await WithResponse("enum?input=One", async r => Assert.AreEqual("One", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestNullableSet()
    {
        await WithResponse("nullable?input=1", async r => Assert.AreEqual("1", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestNullableNotSet()
    {
        await WithResponse("nullable", async r => { await r.AssertStatusAsync(HttpStatusCode.NoContent); });
    }

    [TestMethod]
    public async Task TestGuid()
    {
        var id = Guid.NewGuid().ToString();

        await WithResponse($"guid?id={id}", async r => Assert.AreEqual(id, await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestParam()
    {
        await WithResponse("param/42", async r => Assert.AreEqual("42", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestConversionFailure()
    {
        await WithResponse("param/abc", async r => { await r.AssertStatusAsync(HttpStatusCode.BadRequest); });
    }

    [TestMethod]
    public async Task TestRegex()
    {
        await WithResponse("regex/42", async r => Assert.AreEqual("42", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestEntityWithNulls()
    {
        const string entity = "{\"id\":42}";
        await WithResponse("entity", HttpMethod.Post, entity, null, null, async r => Assert.AreEqual(entity, await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestEntityWithNoNulls()
    {
        const string entity = "{\"id\":42,\"nullable\":123.456}";
        await WithResponse("entity", HttpMethod.Post, entity, null, null, async r => Assert.AreEqual(entity, await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestNotSupportedUpload()
    {
        await WithResponse("entity", HttpMethod.Post, "123", "bla/blubb", null, async r => { await r.AssertStatusAsync(HttpStatusCode.UnsupportedMediaType); });
    }

    [TestMethod]
    public async Task TestUnsupportedDownloadEnforcesDefault()
    {
        const string entity = "{\"id\":42,\"nullable\":123.456}";
        await WithResponse("entity", HttpMethod.Post, entity, null, "bla/blubb", async r => Assert.AreEqual(entity, await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestWrongMethod()
    {
        await WithResponse("entity", HttpMethod.Put, "123", null, null, async r => { await r.AssertStatusAsync(HttpStatusCode.MethodNotAllowed); });
    }

    [TestMethod]
    public async Task TestNoMethod()
    {
        await WithResponse("idonotexist", async r => { await r.AssertStatusAsync(HttpStatusCode.NotFound); });
    }

    [TestMethod]
    public async Task TestStream()
    {
        await WithResponse("stream", HttpMethod.Put, "123456", null, null, async r => Assert.AreEqual("6", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestRequestResponse()
    {
        await WithResponse("requestResponse", async r => Assert.AreEqual("Hello World", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestRouting()
    {
        await WithResponse("request", async r => Assert.AreEqual("yes", await r.GetContentAsync()));
    }

    [TestMethod]
    public async Task TestEntityAsXml()
    {
        const string entity = "<TestEntity><ID>1</ID><Nullable>1234.56</Nullable></TestEntity>";

        await WithResponse("entity", HttpMethod.Post, entity, "text/xml", "text/xml", async r =>
        {
            var result = new XmlSerializer(typeof(TestEntity)).Deserialize(await r.Content.ReadAsStreamAsync()) as TestEntity;

            Assert.IsNotNull(result);

            Assert.AreEqual(1, result.ID);
            Assert.AreEqual(1234.56, result.Nullable);
        });
    }

    [TestMethod]
    public async Task TestException()
    {
        await WithResponse("exception", async r => { await r.AssertStatusAsync(HttpStatusCode.AlreadyReported); });
    }

    [TestMethod]
    public async Task TestDuplicate()
    {
        await WithResponse("duplicate", async r => { await r.AssertStatusAsync(HttpStatusCode.BadRequest); });
    }

    [TestMethod]
    public async Task TestWithInstance()
    {
        var layout = Layout.Create().AddService("t", new TestResource());

        using var runner = TestHost.Run(layout);

        using var response = await runner.GetResponseAsync("/t");

        await response.AssertStatusAsync(HttpStatusCode.NoContent);
    }

    #endregion

    #region Helpers

    private Task WithResponse(string uri, Func<HttpResponseMessage, Task> logic) => WithResponse(uri, HttpMethod.Get, null, null, null, logic);

    private async Task WithResponse(string uri, HttpMethod method, string? body, string? contentType, string? accept, Func<HttpResponseMessage, Task> logic)
    {
        using var service = GetService();

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

    private static TestHost GetService()
    {
        var service = ServiceResource.From<TestResource>()
                                     .Serializers(Serialization.Default())
                                     .Injectors(Injection.Default());

        return TestHost.Run(Layout.Create().Add("t", service));
    }

    #endregion

}
