using System.Net.Http.Headers;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Protobuf;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using ProtoBuf;

namespace GenHTTP.Testing.Acceptance.Modules.Protobuf;

[TestClass]
public sealed class ProtobufTests
{

    #region Supporting structures

    [ProtoContract]
    public sealed class TestEntity
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string? Name { get; set; }

        [ProtoMember(3)]
        public double? Nullable { get; set; }
    }

    public sealed class TestResource
    {

        [ResourceMethod]
        public TestEntity GetEntity()
        {
            var entity = new TestEntity
            {
                Id = 1,
                Name = "test1"
            };

            return entity;
        }

        [ResourceMethod(RequestMethod.Post)]
        public TestEntity PostEntity(TestEntity entity) => entity;
    }

    #endregion

    #region Tests

    [TestMethod]
    [MultiEngineTest]
    public async Task TestGetEntityAsProtobuf(TestEngine engine)
    {
        TestEntity? result = null;
        await WithResponse(string.Empty, HttpMethod.Get, null, "application/protobuf", "application/protobuf", async r =>
        {
            result = Serializer.Deserialize<TestEntity>(await r.Content.ReadAsStreamAsync());
        }, engine);

        Assert.AreEqual(1, result!.Id);
        Assert.AreEqual("test1", result!.Name);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestPostEntityAsProtobuf(TestEngine engine)
    {
        var entity = new TestEntity
        {
            Id = 2,
            Name = "test2",
            Nullable = null
        };

        byte[] encodedEntity;
        using (var memoryStream = new MemoryStream())
        {
            Serializer.Serialize(memoryStream, entity);
            encodedEntity = memoryStream.ToArray();
        }

        TestEntity? result = null;
        await WithResponse(string.Empty, HttpMethod.Post, encodedEntity, "application/protobuf", "application/protobuf", async r =>
        {
            result = Serializer.Deserialize<TestEntity>(await r.Content.ReadAsStreamAsync());
        }, engine);

        Assert.IsNotNull(result);
        Assert.AreEqual(entity.Id, result!.Id);
        Assert.AreEqual(entity.Name, result!.Name);
        Assert.IsNull(result!.Nullable);

    }

    #endregion

    #region Helpers

    private async Task WithResponse(string uri, HttpMethod method, byte[]? body, string? contentType, string? accept, Func<HttpResponseMessage, Task> logic, TestEngine engine)
    {
        await using var service = await GetService(engine);

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
                request.Content = new ByteArrayContent(body);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }
            else
            {
                request.Content = new ByteArrayContent(body);
                request.Content.Headers.ContentType = null;
            }
        }

        using var response = await service.GetResponseAsync(request);

        await logic(response);
    }

    private static async Task<TestHost> GetService(TestEngine engine)
    {
        var service = ServiceResource.From<TestResource>()
                                     .Serializers(Serialization.Default().AddProtobuf())
                                     .Injectors(Injection.Default());

        return await TestHost.RunAsync(Layout.Create().Add("t", service), engine: engine);
    }

    #endregion

}
