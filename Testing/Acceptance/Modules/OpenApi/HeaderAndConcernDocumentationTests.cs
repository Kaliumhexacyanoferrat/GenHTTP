using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.ClientCaching;
using GenHTTP.Modules.Compression;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Functional.Provider;
using GenHTTP.Modules.I18n;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Webservices;

using Microsoft.OpenApi;

namespace GenHTTP.Testing.Acceptance.Modules.OpenApi;

[TestClass]
public sealed class HeaderAndConcernDocumentationTests
{

    #region Request headers

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRequestHeader(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([RequestHeader("X-Page", "The page to return", Required = true, Type = typeof(int))] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        var header = op.Parameters?.First(p => p.Name == "X-Page");

        Assert.AreEqual(ParameterLocation.Header, header?.In);
        Assert.AreEqual("The page to return", header?.Description);
        Assert.IsTrue(header?.Required);
        Assert.AreEqual(JsonSchemaType.Integer, header?.Schema?.Type & JsonSchemaType.Integer);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOptionalStringRequestHeader(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([RequestHeader("X-Request-Id")] (IRequest request) => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        var header = op.Parameters?.First(p => p.Name == "X-Request-Id");

        Assert.IsFalse(header?.Required);
        Assert.AreEqual(JsonSchemaType.String, header?.Schema?.Type & JsonSchemaType.String);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceRequestHeaders(ServerEngine engine)
    {
        var doc = await GetServiceDocumentAsync(engine);

        var get = doc.Paths["/service/"].Operations?[HttpMethod.Get];

        var parameters = get?.Parameters?.Where(p => p.In == ParameterLocation.Header).ToList();

        Assert.AreEqual(2, parameters?.Count);
        Assert.AreEqual("Overridden by the method", parameters?.First(p => p.Name == "X-Tenant").Description);
        Assert.AreEqual("Correlates the request", parameters?.First(p => p.Name == "X-Request-Id").Description);
    }

    #endregion

    #region Response headers

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResponseHeaderOnSuccess(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([ResponseHeader("Content-Disposition", "The suggested file name")]
                             [Response(ResponseStatus.NotFound, "Nothing here")]
                             () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.AreEqual("The suggested file name", op.Responses?["200"].Headers?["Content-Disposition"].Description);

        Assert.IsFalse(op.Responses?["404"].Headers?.ContainsKey("Content-Disposition") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResponseHeaderOnStatus(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.TooManyRequests, "Slow down")]
                             [ResponseHeader(ResponseStatus.TooManyRequests, "Retry-After", "Seconds to wait", Type = typeof(int))]
                             () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        var header = op.Responses?["429"].Headers?["Retry-After"];

        Assert.AreEqual("Seconds to wait", header?.Description);
        Assert.AreEqual(JsonSchemaType.Integer, header?.Schema?.Type & JsonSchemaType.Integer);

        Assert.IsFalse(op.Responses?["200"].Headers?.ContainsKey("Retry-After") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestResponseHeaderRequiresResponse(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([ResponseHeader(ResponseStatus.NotModified, "ETag")] () => 42);

        var (_, op) = await Extensions.GetOperationAsync(engine, api);

        Assert.IsFalse(op.Responses?.ContainsKey("304"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceResponseHeaders(ServerEngine engine)
    {
        var doc = await GetServiceDocumentAsync(engine);

        var headers = doc.Paths["/service/"].Operations?[HttpMethod.Get].Responses?["200"].Headers;

        Assert.AreEqual(2, headers?.Count);
        Assert.AreEqual("Specific to the method", headers?["X-Version"].Description);
        Assert.AreEqual("The server time", headers?["X-Time"].Description);
    }

    private static async Task<OpenApiDocument> GetServiceDocumentAsync(ServerEngine engine)
    {
        var api = Layout.Create()
                        .AddService<HeaderService>("service")
                        .Add(ApiDescription.Create());

        return (await api.GetOpenApiAsync(engine)).Document!;
    }

    #endregion

    #region Concerns

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConcernDocumentationIsApplied(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("/a", () => 42)
                        .Post("/b", () => 42)
                        .Add(new DocumentedConcernBuilder([
                            new RequestHeaderAttribute("Authorization", "The token") { Required = true },
                            new ResponseAttribute(ResponseStatus.Unauthorized, "No token given"),
                            new ResponseHeaderAttribute(ResponseStatus.Unauthorized, "WWW-Authenticate", "The supported scheme")
                        ]));

        var doc = await GetDocumentAsync(api, engine);

        foreach (var op in GetOperations(doc))
        {
            Assert.IsTrue(op.Parameters?.Any(p => p.Name == "Authorization" && p.In == ParameterLocation.Header && p.Required));

            Assert.AreEqual("No token given", op.Responses?["401"].Description);
            Assert.AreEqual("The supported scheme", op.Responses?["401"].Headers?["WWW-Authenticate"].Description);

            Assert.IsTrue(op.Responses?.ContainsKey("200"));
        }
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestOperationOverridesConcern(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get([Response(ResponseStatus.Unauthorized, "Declared by the operation")] () => 42)
                        .Add(new DocumentedConcernBuilder([
                            new ResponseAttribute(ResponseStatus.Unauthorized, "Declared by the concern")
                        ]));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("Declared by the operation", op.Responses?["401"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestServiceOverridesConcern(ServerEngine engine)
    {
        var api = Layout.Create()
                        .AddService<HeaderService>("service")
                        .Add(new DocumentedConcernBuilder([
                            new RequestHeaderAttribute("X-Request-Id", "Declared by the concern"),
                            new ResponseHeaderAttribute("X-Time", "Declared by the concern")
                        ]));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("Correlates the request", op.Parameters?.First(p => p.Name == "X-Request-Id").Description);
        Assert.AreEqual("The server time", op.Responses?["200"].Headers?["X-Time"].Description);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInnerConcernOverridesOuter(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Add(new DocumentedConcernBuilder([
                            new ResponseAttribute(ResponseStatus.Forbidden, "Inner"),
                            new RequestHeaderAttribute("X-Inner")
                        ]))
                        .Add(new DocumentedConcernBuilder([
                            new ResponseAttribute(ResponseStatus.Forbidden, "Outer"),
                            new ResponseAttribute(ResponseStatus.Unauthorized, "Outer"),
                            new RequestHeaderAttribute("X-Outer")
                        ]));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("Inner", op.Responses?["403"].Description);
        Assert.AreEqual("Outer", op.Responses?["401"].Description);

        Assert.IsTrue(op.Parameters?.Any(p => p.Name == "X-Inner"));
        Assert.IsTrue(op.Parameters?.Any(p => p.Name == "X-Outer"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConcernResponsesAreMerged(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Add(new DocumentedConcernBuilder([
                            new ResponseAttribute(ResponseStatus.BadRequest, "The error") { ContentType = "application/problem+json" },
                            new ResponseAttribute(ResponseStatus.BadRequest) { ContentType = "text/plain" }
                        ]));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.AreEqual("The error", op.Responses?["400"].Description);
        Assert.AreEqual(2, op.Responses?["400"].Content?.Count);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConcernOnlyAppliesToContent(ServerEngine engine)
    {
        var api = Layout.Create()
                        .Add("documented", Inline.Create().Get(() => 42).Add(new DocumentedConcernBuilder([new RequestHeaderAttribute("X-Documented")])))
                        .Add("plain", Inline.Create().Get(() => 42));

        var doc = await GetDocumentAsync(api, engine);

        Assert.IsTrue(doc.Paths["/documented/"].Operations?[HttpMethod.Get].Parameters?.Any(p => p.Name == "X-Documented"));
        Assert.IsFalse(doc.Paths["/plain/"].Operations?[HttpMethod.Get].Parameters?.Any(p => p.Name == "X-Documented") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestConcernDecidesPerOperation(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("/a", () => 42)
                        .Post("/a", () => 42)
                        .Add(new DocumentedConcernBuilder([new RequestHeaderAttribute("X-Read-Only")], RequestMethod.Get));

        var doc = await GetDocumentAsync(api, engine);

        var operations = doc.Paths["/a"].Operations;

        Assert.IsTrue(operations?[HttpMethod.Get].Parameters?.Any(p => p.Name == "X-Read-Only"));
        Assert.IsFalse(operations?[HttpMethod.Post].Parameters?.Any(p => p.Name == "X-Read-Only") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestEmptyConcernDocumentation(ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get(() => 42)
                        .Add(new DocumentedConcernBuilder([]));

        var op = GetOperations(await GetDocumentAsync(api, engine)).Single();

        Assert.IsTrue(op.Responses?.ContainsKey("200"));
    }

    private static async Task<OpenApiDocument> GetDocumentAsync(InlineBuilder api, ServerEngine engine)
        => (await api.Add(ApiDescription.Create()).GetOpenApiAsync(engine)).Document!;

    private static async Task<OpenApiDocument> GetDocumentAsync(LayoutBuilder api, ServerEngine engine)
        => (await api.Add(ApiDescription.Create()).GetOpenApiAsync(engine)).Document!;

    private static IEnumerable<OpenApiOperation> GetOperations(OpenApiDocument doc)
        => doc.Paths.Values.SelectMany(p => p.Operations?.Values.AsEnumerable() ?? []);

    #endregion

    #region Built-in concerns

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCacheValidation(ServerEngine engine)
    {
        var (get, post) = await GetBuiltInOperationsAsync(ClientCache.Validation(), engine);

        Assert.IsTrue(get.Parameters?.Any(p => p.Name == "If-None-Match" && p.In == ParameterLocation.Header));
        Assert.IsTrue(get.Responses?["200"].Headers?.ContainsKey("ETag"));
        Assert.IsTrue(get.Responses?["304"].Headers?.ContainsKey("ETag"));

        Assert.IsFalse(post.Parameters?.Any(p => p.Name == "If-None-Match") ?? false);
        Assert.IsFalse(post.Responses?.ContainsKey("304"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestCachePolicy(ServerEngine engine)
    {
        var (get, post) = await GetBuiltInOperationsAsync(ClientCache.Policy().Duration(1), engine);

        Assert.IsTrue(get.Responses?["200"].Headers?.ContainsKey("Expires"));
        Assert.IsFalse(post.Responses?["200"].Headers?.ContainsKey("Expires") ?? false);
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestRangeSupport(ServerEngine engine)
    {
        var (get, post) = await GetBuiltInOperationsAsync(RangeSupport.Create(), engine);

        Assert.IsTrue(get.Parameters?.Any(p => p.Name == "Range" && p.In == ParameterLocation.Header));
        Assert.IsTrue(get.Responses?["200"].Headers?.ContainsKey("Accept-Ranges"));
        Assert.IsTrue(get.Responses?["206"].Headers?.ContainsKey("Content-Range"));
        Assert.IsTrue(get.Responses?["416"].Headers?.ContainsKey("Content-Range"));
        Assert.IsTrue(get.Responses?["416"].Content?.ContainsKey("text/plain"));

        Assert.IsFalse(post.Responses?.ContainsKey("206"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestLocalization(ServerEngine engine)
    {
        var (get, post) = await GetBuiltInOperationsAsync(Localization.Create(), engine);

        Assert.IsTrue(get.Responses?["200"].Headers?.ContainsKey("Content-Language"));
        Assert.IsTrue(post.Responses?["200"].Headers?.ContainsKey("Content-Language"));
    }

    [TestMethod]
    [MultiEngineTest]
    public async Task TestDecompression(ServerEngine engine)
    {
        var (get, post) = await GetBuiltInOperationsAsync(DecompressedContent.Default(), engine);

        var header = post.Parameters?.First(p => p.Name == "Content-Encoding");

        Assert.AreEqual(ParameterLocation.Header, header?.In);
        AssertX.Contains("gzip", header?.Description);

        Assert.IsFalse(get.Parameters?.Any(p => p.Name == "Content-Encoding") ?? false);
    }

    private static async Task<(OpenApiOperation Get, OpenApiOperation Post)> GetBuiltInOperationsAsync(IConcernBuilder concern, ServerEngine engine)
    {
        var api = Inline.Create()
                        .Get("/a", () => 42)
                        .Post("/a", () => 42)
                        .Add(concern);

        var operations = (await GetDocumentAsync(api, engine)).Paths["/a"].Operations!;

        return (operations[HttpMethod.Get], operations[HttpMethod.Post]);
    }

    #endregion

    #region Supporting data structures

    [RequestHeader("X-Tenant", "Declared by the service")]
    [RequestHeader("X-Request-Id", "Correlates the request")]
    [ResponseHeader("X-Version", "Declared by the service")]
    [ResponseHeader("X-Time", "The server time")]
    public sealed class HeaderService
    {

        [ResourceMethod]
        [RequestHeader("X-Tenant", "Overridden by the method")]
        [ResponseHeader("X-Version", "Specific to the method")]
        public int Get() => 42;

    }

    private sealed class DocumentedConcernBuilder(IReadOnlyList<DocumentationAttribute> documentation, RequestMethod? method = null) : IConcernBuilder
    {

        public IConcern Build(IHandler content) => new DocumentedConcern(content, documentation, method);

    }

    private sealed class DocumentedConcern(IHandler content, IReadOnlyList<DocumentationAttribute> documentation, RequestMethod? method) : IDocumentedConcern
    {

        public IHandler Content => content;

        public void AddDocumentation(OperationDocumentation operation)
        {
            if (method == null || operation.Method == method)
            {
                foreach (var entry in documentation)
                {
                    operation.Add(entry);
                }
            }
        }

        public ValueTask PrepareAsync(IServer server) => content.PrepareAsync(server);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => content.HandleAsync(request);

    }

    #endregion

}
