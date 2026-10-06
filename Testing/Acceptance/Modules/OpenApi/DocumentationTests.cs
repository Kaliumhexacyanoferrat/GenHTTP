using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

using Microsoft.OpenApi;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public sealed class DocumentationTests
{

    #region Operations

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOperationIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Summary("Returns the answer")] [Remarks("To everything")] () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("Returns the answer", op.Summary);
        Assert.AreEqual("To everything", op.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUndocumentedOperation(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsNull(op.Summary);
        Assert.IsNull(op.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestParametersAreDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("/users/:id", ([Summary("The ID of the user")] int id, [Summary("The fields to return")] string? fields) => id);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The ID of the user", op.Parameters?.First(p => p.Name == "id").Description);
        Assert.AreEqual("The fields to return", op.Parameters?.First(p => p.Name == "fields").Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResultIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([return: Summary("The answer")] () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The answer", op.Responses?["200"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDynamicResultIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([return: Summary("Whatever happens")] (IRequest request) => request.Respond().Build());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("Whatever happens", op.Responses?["200"].Description);
    }

    #endregion

    #region Request bodies

    [TestMethod]
    [MultiEngineTest]
    public async Task TestContentParameterIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post(([Summary("The user to create")] DocumentedModel user) => user);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The user to create", op.RequestBody?.Description);
        Assert.IsTrue(op.RequestBody?.Content?.ContainsKey("application/json"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBodyParameterIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post(([FromBody] [Summary("The text")] string text) => text);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The text", op.RequestBody?.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestManuallyReadBody(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Put([RequestBody("The content of the file", ContentType = "application/octet-stream")] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The content of the file", op.RequestBody?.Description);
        Assert.IsTrue(op.RequestBody?.Required);
        Assert.AreEqual("binary", op.RequestBody?.Content?["application/octet-stream"].Schema?.Format);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestStreamedBodyWithContentType(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([RequestBody(ContentType = "application/zip")] ([Summary("The archive")] Stream body) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The archive", op.RequestBody?.Description);
        Assert.AreEqual(1, op.RequestBody?.Content?.Count);
        Assert.AreEqual("binary", op.RequestBody?.Content?["application/zip"].Schema?.Format);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestBodyWithMultipleContentTypes(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Put([RequestBody("The picture", ContentType = "image/png")] [RequestBody(ContentType = "image/jpeg")] (Stream body) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The picture", op.RequestBody?.Description);
        Assert.IsTrue(op.RequestBody?.Content?.ContainsKey("image/png"));
        Assert.IsTrue(op.RequestBody?.Content?.ContainsKey("image/jpeg"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredBodyType(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([RequestBody("The user", Type = typeof(DocumentedModel))] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.RequestBody?.Content?.ContainsKey("application/json"));
        Assert.IsNotNull(op.RequestBody?.Content?["application/json"].Schema);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredDescriptionKeepsContent(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([RequestBody("The user to create")] (DocumentedModel user) => user);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The user to create", op.RequestBody?.Description);
        Assert.IsTrue(op.RequestBody?.Content?.ContainsKey("application/json"));
        Assert.IsTrue(op.RequestBody?.Content?.ContainsKey("text/xml"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredTextBody(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([RequestBody("Some text", ContentType = "text/plain; charset=utf-8")] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual(JsonSchemaType.String, op.RequestBody?.Content?["text/plain; charset=utf-8"].Schema?.Type);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredBodyIgnoredForGet(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([RequestBody("Nothing", ContentType = "text/plain")] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsNull(op.RequestBody);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredBodyWithoutContent(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([RequestBody("Nothing to see")] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsNull(op.RequestBody);
    }

    #endregion

    #region Responses

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredSuccessReplacesDerived(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([Response(ResponseStatus.Created, "The newly created user")] (DocumentedModel user) => new Result<DocumentedModel>(user).Status(ResponseStatus.Created));

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsFalse(op.Responses?.ContainsKey("200"));
        Assert.IsFalse(op.Responses?.ContainsKey("204"));

        Assert.AreEqual("The newly created user", op.Responses?["201"].Description);
        Assert.IsTrue(op.Responses?["201"].Content?.ContainsKey("application/json"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDeclaredSuccessFallsBackToResultSummary(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Post([Response(ResponseStatus.Accepted)] [return: Summary("The accepted user")] (DocumentedModel user) => user);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The accepted user", op.Responses?["202"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestErrorResponse(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("/users/:id", [Response(ResponseStatus.NotFound, "There is no such user", Type = typeof(ErrorModel))] (int id) => new DocumentedModel("Test", 42));

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?.ContainsKey("200"));

        Assert.AreEqual("There is no such user", op.Responses?["404"].Description);
        Assert.IsTrue(op.Responses?["404"].Content?.ContainsKey("application/json"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestErrorResponseWithoutContent(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.TooManyRequests)] () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("Too Many Requests", op.Responses?["429"].Description);
        Assert.IsTrue(op.Responses?["429"].Content == null || op.Responses?["429"].Content?.Count == 0);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestNoContentDefaultDescription(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Delete([Response(ResponseStatus.NoContent)] () => { });

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("A response containing no body", op.Responses?["204"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFormattableResponseType(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.Conflict, "The number of conflicts", Type = typeof(int))] () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsTrue(op.Responses?["409"].Content?.ContainsKey("text/plain"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDynamicResponseWithContentTypes(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.Ok, "The picture", ContentType = "image/png")] [Response(ResponseStatus.Ok, ContentType = "image/jpeg")] (IRequest request) => request.Respond().Build());

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        var response = op.Responses?["200"];

        Assert.AreEqual("The picture", response?.Description);

        Assert.AreEqual(2, response?.Content?.Count);
        Assert.AreEqual("binary", response?.Content?["image/png"].Schema?.Format);
        Assert.AreEqual("binary", response?.Content?["image/jpeg"].Schema?.Format);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSerializedResponseWithContentType(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.Ok, "The user", ContentType = "application/json")] () => new DocumentedModel("Test", 42));

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual(1, op.Responses?["200"].Content?.Count);
        Assert.IsNotNull(op.Responses?["200"].Content?["application/json"].Schema);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestJsonContentTypeWithoutSchema(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.BadRequest, "The problem", ContentType = "application/problem+json")] () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsNull(op.Responses?["400"].Content?["application/problem+json"].Schema);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestAsyncResultIsUnwrapped(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(async () =>
                        {
                            await Task.Yield();
                            return new Result<UnwrappedModel>(new UnwrappedModel(42));
                        })
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var schemas = doc.Components?.Schemas?.Keys.ToList() ?? [];

        Assert.IsTrue(schemas.Contains("UnwrappedModel"));
        Assert.IsFalse(schemas.Any(s => s.StartsWith("Result")));
    }

    #endregion

    #region Services

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceIsTagged(ServerEngine engine)
    {
        var doc = await GetServiceDocumentAsync(engine);

        var tag = doc.Tags?.First(t => t.Name == "Users");

        Assert.AreEqual("Manages users\n\nAll of them", tag?.Description);

        var operation = doc.Paths["/users/{id}"].Operations?[HttpMethod.Get];

        Assert.IsTrue(operation?.Tags?.Any(t => t.Name == "Users"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestMethodTag(ServerEngine engine)
    {
        var doc = await GetServiceDocumentAsync(engine);

        var operation = doc.Paths["/users/admin"].Operations?[HttpMethod.Get];

        Assert.IsTrue(operation?.Tags?.Any(t => t.Name == "Administration"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestSharedResponses(ServerEngine engine)
    {
        var doc = await GetServiceDocumentAsync(engine);

        var get = doc.Paths["/users/{id}"].Operations?[HttpMethod.Get];

        Assert.AreEqual("Something went wrong", get?.Responses?["400"].Description);
        Assert.AreEqual("There is no such user", get?.Responses?["404"].Description);

        var admin = doc.Paths["/users/admin"].Operations?[HttpMethod.Get];

        Assert.AreEqual("Something went wrong", admin?.Responses?["400"].Description);
        Assert.AreEqual("Not Found", admin?.Responses?["404"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceOperationIsDescribed(ServerEngine engine)
    {
        var doc = await GetServiceDocumentAsync(engine);

        var get = doc.Paths["/users/{id}"].Operations?[HttpMethod.Get];

        Assert.AreEqual("Reads a user", get?.Summary);
        Assert.AreEqual("The ID of the user", get?.Parameters?.First().Description);
        Assert.AreEqual("The user with the given ID", get?.Responses?["200"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestUndocumentedServiceKeepsClassName(ServerEngine engine)
    {
        var api = Layout.Create()
                        .AddService<UndocumentedService>("plain")
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var tag = doc.Tags?.First();

        Assert.AreEqual(nameof(UndocumentedService), tag?.Name);
        Assert.IsNull(tag?.Description);
    }

    private static async Task<OpenApiDocument> GetServiceDocumentAsync(ServerEngine engine)
    {
        var api = Layout.Create()
                        .AddService<DocumentedService>("users")
                        .Add(ApiDescription.Create());

        return (await api.GetOpenApiAsync(engine)).Document!;
    }

    #endregion

    #region Schemas

    [TestMethod]
    [MultiEngineTest]
    public async Task TestModelIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => new DocumentedModel("Test", 42))
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var schema = doc.Components?.Schemas?[nameof(DocumentedModel)];

        Assert.AreEqual("A user\n\nWith details", schema?.Description);

        Assert.AreEqual("The name of the user", GetProperty(schema, "Name")?.Description);
        Assert.AreEqual("The age of the user", GetProperty(schema, "Age")?.Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestClassModelIsDescribed(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => new ClassModel())
                        .Add(ApiDescription.Create());

        var doc = (await api.GetOpenApiAsync(engine)).Document!;

        var schema = doc.Components?.Schemas?[nameof(ClassModel)];

        Assert.AreEqual("The value", GetProperty(schema, "Value")?.Description);
        Assert.IsNull(GetProperty(schema, "Other")?.Description);
    }

    private static IOpenApiSchema? GetProperty(IOpenApiSchema? schema, string name)
        => schema?.Properties?.First(p => string.Equals(p.Key, name, StringComparison.OrdinalIgnoreCase)).Value;

    #endregion

    #region Supporting data structures

    [Summary("A user")]
    [Remarks("With details")]
    public sealed record DocumentedModel([Summary("The name of the user")] string Name, [property: Summary("The age of the user")] int Age);

    public sealed record ErrorModel(string Message);

    public sealed record UnwrappedModel(int Value);

    public sealed class ClassModel
    {

        [Summary("The value")]
        public int Value { get; set; }

        public int Other { get; set; }

    }

    [Tag("Users")]
    [Summary("Manages users")]
    [Remarks("All of them")]
    [Response(ResponseStatus.BadRequest, "Something went wrong", Type = typeof(ErrorModel))]
    [Response(ResponseStatus.NotFound)]
    public sealed class DocumentedService
    {

        [ResourceMethod(":id")]
        [Summary("Reads a user")]
        [Response(ResponseStatus.NotFound, "There is no such user")]
        [return: Summary("The user with the given ID")]
        public DocumentedModel Get([Summary("The ID of the user")] int id) => new("Test", id);

        [ResourceMethod("admin")]
        [Tag("Administration")]
        public int GetAdmin() => 42;

    }

    public sealed class UndocumentedService
    {

        [ResourceMethod]
        public int Get() => 42;

    }

    #endregion

}
