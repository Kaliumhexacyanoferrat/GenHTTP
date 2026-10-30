using System.Net;
using System.Text.Json.Nodes;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Mcp;
using GenHTTP.Modules.Mcp.Provider;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;
using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.Webservices;

using ComponentDescription = System.ComponentModel.DescriptionAttribute;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

[TestClass]
public sealed class IntegrationTests
{

    #region Supporting data structures

    public enum Genre { Fiction, Science }

    public sealed record Book(int Id, string Title, Genre Genre);

    public sealed record Receipt(int BookId, string? Note);

    public sealed class TestUser(string name, params string[] roles) : IUser
    {

        public string DisplayName => name;

        public string[] Roles => roles;

    }

    public sealed class ShortCircuit : IOperationInterceptor
    {

        public void Configure(object attribute) { }

        public ValueTask<InterceptionResult?> InterceptAsync(IRequest request, Operation operation, IReadOnlyDictionary<ByteString, object?> arguments)
        {
            var value = (int)arguments[new ByteString("value")]!;

            return new(value == 0 ? new InterceptionResult("short circuit") : null);
        }

    }

    public sealed class InterceptWithShortCircuitAttribute : InterceptWithAttribute<ShortCircuit>;

    public sealed class LibraryTools
    {

        [Tool(Title = "Find books", ReadOnly = true)]
        [Summary("Searches the library")]
        public async Task<List<Book>> FindBooksAsync([ComponentDescription("Part of the title")] string query, IRequest request, int limit = 10, Genre? genre = null)
        {
            await Task.Yield();

            var source = request.Header.Headers.GetEntry("X-Source") ?? "default";

            return [new Book(limit, $"{query} ({source})", genre ?? Genre.Fiction)];
        }

        [Tool("lend", Destructive = false)]
        [ComponentDescription("Lends a book")]
        public ValueTask<Result<Receipt>> Lend(int bookId) => new(new Result<Receipt>(new Receipt(bookId, null)));

        [Tool]
        [Summary("Returns a book")]
        public Task ReturnBook(int bookId) => Task.CompletedTask;

        [Tool]
        [Summary("Touches a book")]
        public ValueTask Touch() => ValueTask.CompletedTask;

        [Tool]
        [Summary("Resets the library")]
        public void Reset() { }

        [Tool]
        [Summary("Fails")]
        [Remarks("Fails on purpose.")]
        [Response(ResponseStatus.Ok, "Never returned")]
        [Response(ResponseStatus.Conflict, "The book is already lent")]
        [Response(ResponseStatus.Gone)]
        public string Fail() => throw new ProviderException(ResponseStatus.Conflict, "The book is already lent");

        [Tool]
        [Summary("Crashes")]
        public string Crash() => throw new InvalidOperationException("Database is down");

        [Tool]
        [Summary("Crashes asynchronously")]
        public async Task<string> CrashAsyncLater()
        {
            await Task.Yield();
            throw new InvalidOperationException("Later");
        }

        [Tool]
        [Summary("Removes all books")]
        [RequireRole("admin")]
        public string Purge() => "purged";

        [Tool]
        [Summary("Tells who is calling")]
        public string WhoAmI(TestUser user) => user.DisplayName;

        [Tool]
        [Summary("Might be intercepted")]
        [InterceptWithShortCircuit]
        public string Intercepted(int value) => $"value {value}";

        [Tool]
        [Summary("Returns the HTTP status")]
        public static int GetHTTPStatus() => 200;

        public string NotATool() => "nope";

    }

    public sealed class MixedService
    {

        [ResourceMethod]
        public string Get() => "service";

        [Tool]
        [Summary("A tool")]
        public string Tool() => "tool";

    }

    public sealed class EmptyTools
    {

        public string Nothing() => string.Empty;

    }

    public sealed class UndescribedTools
    {

        [Tool]
        public string Undescribed() => string.Empty;

    }

    public sealed class InvalidNameTools
    {

        [Tool("in valid")]
        [Summary("Invalid")]
        public string Invalid() => string.Empty;

    }

    public sealed class HandlerTools
    {

        [Tool]
        [Summary("Returns a handler")]
        public IHandler Handler() => throw new NotSupportedException();

    }

    public sealed class StreamTools
    {

        [Tool]
        [Summary("Takes a stream")]
        public string Upload(Stream data) => string.Empty;

    }

    #endregion

    #region Helpers

    private static ToolsHandlerBuilder Library() => Tools.Create()
                                                         .Add<LibraryTools>()
                                                         .Injectors(Injection.Default().Add(new UserInjector<TestUser>()))
                                                         .Add(Concern.From((request, content) =>
                                                         {
                                                             var name = request.Header.Headers.GetEntry("X-User");

                                                             if (name != null)
                                                             {
                                                                 var roles = request.Header.Headers.GetEntry("X-Roles")?.Split(',') ?? [];

                                                                 request.SetUser(new TestUser(name, roles));
                                                             }

                                                             return content.HandleAsync(request);
                                                         }));

    private static async Task<JsonNode> GetToolAsync(TestHost host, string name)
    {
        var response = await McpClient.Modern(host).RequestAsync("tools/list");

        return response.Result["tools"]!.AsArray().Single(t => t!["name"]!.GetValue<string>() == name)!;
    }

    private static Action<HttpRequestMessage> As(string user, string roles = "") => r =>
    {
        r.Headers.Add("X-User", user);
        r.Headers.Add("X-Roles", roles);
    };

    #endregion

    #region Definitions

    [TestMethod]
    public async Task TestNamesAreDerived()
    {
        await using var host = await TestHost.RunAsync(Library());

        var tools = await McpClient.Modern(host).RequestAsync("tools/list");

        var names = tools.Result["tools"]!.AsArray().Select(t => t!["name"]!.GetValue<string>()).ToHashSet();

        Assert.Contains("find_books", names);
        Assert.Contains("lend", names);
        Assert.Contains("return_book", names);
        Assert.Contains("crash_async_later", names);
        Assert.Contains("who_am_i", names);
        Assert.Contains("get_http_status", names);
        Assert.DoesNotContain("not_a_tool", names);
    }

    [TestMethod]
    public async Task TestArgumentSchema()
    {
        await using var host = await TestHost.RunAsync(Library());

        var tool = await GetToolAsync(host, "find_books");

        Assert.AreEqual("Searches the library", tool["description"]!.GetValue<string>());
        Assert.AreEqual("Find books", tool["title"]!.GetValue<string>());

        var schema = tool["inputSchema"]!;

        Assert.AreEqual("""["query"]""", schema["required"]!.ToJsonString());

        var properties = schema["properties"]!.AsObject();

        Assert.HasCount(3, properties);
        Assert.IsFalse(properties.ContainsKey("request"));

        Assert.AreEqual("Part of the title", properties["query"]!["description"]!.GetValue<string>());
        Assert.AreEqual(10, properties["limit"]!["default"]!.GetValue<int>());
        Assert.IsNotNull(properties["genre"]!["enum"]);
    }

    [TestMethod]
    public async Task TestInterceptorDocumentation()
    {
        await using var host = await TestHost.RunAsync(Library());

        var description = (await GetToolAsync(host, "purge"))["description"]!.GetValue<string>();

        Assert.AreEqual("Removes all books\n\nPossible errors:\n- Authentication is required to access this operation\n- The user does not have the required role: admin", description);
    }

    [TestMethod]
    public async Task TestDeclaredDocumentation()
    {
        await using var host = await TestHost.RunAsync(Library());

        var description = (await GetToolAsync(host, "fail"))["description"]!.GetValue<string>();

        Assert.AreEqual("Fails\n\nFails on purpose.", description);

        Assert.AreEqual("Resets the library", (await GetToolAsync(host, "reset"))["description"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestAnnotations()
    {
        await using var host = await TestHost.RunAsync(Library());

        var find = await GetToolAsync(host, "find_books");

        Assert.IsTrue(find["annotations"]!["readOnlyHint"]!.GetValue<bool>());
        Assert.IsNull(find["annotations"]!["destructiveHint"]);

        var lend = await GetToolAsync(host, "lend");

        Assert.IsFalse(lend["annotations"]!["destructiveHint"]!.GetValue<bool>());
        Assert.IsNull(lend["annotations"]!["readOnlyHint"]);

        Assert.IsNull((await GetToolAsync(host, "reset"))["annotations"]);
    }

    [TestMethod]
    public async Task TestOutputSchemas()
    {
        await using var host = await TestHost.RunAsync(Library());

        var lend = await GetToolAsync(host, "lend");

        Assert.AreEqual("""["bookId"]""", lend["outputSchema"]!["required"]!.ToJsonString());

        Assert.IsNull((await GetToolAsync(host, "find_books"))["outputSchema"]);
        Assert.IsNull((await GetToolAsync(host, "reset"))["outputSchema"]);
    }

    [TestMethod]
    public void TestServicesAreRejected()
    {
        var e = Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add<MixedService>());

        AssertX.Contains("dedicated class", e.Message);
    }

    [TestMethod]
    public void TestEmptyClassIsRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add<EmptyTools>());
    }

    [TestMethod]
    public void TestDescriptionIsRequired()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add<UndescribedTools>());
    }

    [TestMethod]
    public void TestInvalidNameIsRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add<InvalidNameTools>());
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add("in valid", "Invalid", () => 1));
    }

    [TestMethod]
    public void TestDuplicatesAreRejected()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add("lend", "Lends", () => 1).Add<LibraryTools>().Build());
        Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add("a", "A", () => 1).Add("a", "A", () => 2).Build());
    }

    [TestMethod]
    public async Task TestUnsupportedResultIsRejected()
    {
        await AssertPreparationFailsAsync(Tools.Create().Add<HandlerTools>(), "cannot be sent to a client");
    }

    [TestMethod]
    public async Task TestStreamArgumentIsRejected()
    {
        await AssertPreparationFailsAsync(Tools.Create().Add<StreamTools>(), "stream");
    }

    [TestMethod]
    public async Task TestPreparationIsIdempotent()
    {
        var handler = Tools.Create().Add<LibraryTools>().Build();

        await using var host = await TestHost.RunAsync(Tools.Create().Add<TestTools>());

        await handler.PrepareAsync(host.Host.Instance!);
        await handler.PrepareAsync(host.Host.Instance!);
    }

    private static async Task AssertPreparationFailsAsync(ToolsHandlerBuilder builder, string message)
    {
        var handler = builder.Build();

        await using var host = await TestHost.RunAsync(Tools.Create().Add<TestTools>());

        var e = await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () => await handler.PrepareAsync(host.Host.Instance!));

        AssertX.Contains(message, e.Message);
    }

    #endregion

    #region Invocation

    [TestMethod]
    [MultiEngineTest]
    public async Task TestInvocation(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(Library(), engine: engine);

        var response = await McpClient.Modern(host).CallAsync("find_books", new JsonObject { ["query"] = "Dune", ["genre"] = "Science" }, r => r.Headers.Add("X-Source", "test"));

        var book = response.Result["structuredContent"]![0]!;

        Assert.AreEqual("Dune (test)", book["title"]!.GetValue<string>());
        Assert.AreEqual(10, book["id"]!.GetValue<int>());
        Assert.AreEqual("Science", book["genre"]!.GetValue<string>());
    }

    [TestMethod]
    public async Task TestResultIsUnwrapped()
    {
        await using var host = await TestHost.RunAsync(Library());

        var response = await McpClient.Legacy(host).CallAsync("lend", new JsonObject { ["bookId"] = 7 });

        Assert.AreEqual(7, response.Result["structuredContent"]!["bookId"]!.GetValue<int>());
        Assert.AreEqual("""{"bookId":7}""", response.Text);
    }

    [TestMethod]
    [DataRow("return_book", """{"bookId":1}""")]
    [DataRow("touch", "{}")]
    [DataRow("reset", "{}")]
    public async Task TestVoidResults(string tool, string arguments)
    {
        await using var host = await TestHost.RunAsync(Library());

        var response = await McpClient.Modern(host).CallAsync(tool, JsonNode.Parse(arguments)!.AsObject());

        Assert.IsFalse(response.IsError);
        Assert.AreEqual(0, response.Result["content"]!.AsArray().Count);
    }

    [TestMethod]
    public async Task TestStaticMethod()
    {
        await using var host = await TestHost.RunAsync(Library());

        var response = await McpClient.Modern(host).CallAsync("get_http_status");

        Assert.AreEqual("200", response.Text);
    }

    #endregion

    #region Injection and interception

    [TestMethod]
    public async Task TestRoles()
    {
        await using var host = await TestHost.RunAsync(Library());

        var client = McpClient.Modern(host);

        var anonymous = await client.CallAsync("purge");

        Assert.IsTrue(anonymous.IsError);
        AssertX.Contains("Authorization required", anonymous.Text);

        var user = await client.CallAsync("purge", configure: As("joe", "reader"));

        Assert.IsTrue(user.IsError);
        AssertX.Contains("not authorized", user.Text);

        var admin = await client.CallAsync("purge", configure: As("ann", "admin"));

        Assert.IsFalse(admin.IsError);
        Assert.AreEqual("purged", admin.Text);
    }

    [TestMethod]
    public async Task TestUserInjection()
    {
        await using var host = await TestHost.RunAsync(Library());

        var client = McpClient.Modern(host);

        Assert.AreEqual("ann", (await client.CallAsync("who_am_i", configure: As("ann"))).Text);

        var anonymous = await client.CallAsync("who_am_i");

        Assert.IsTrue(anonymous.IsError);
        AssertX.Contains("Authentication required", anonymous.Text);
    }

    [TestMethod]
    public async Task TestInterception()
    {
        await using var host = await TestHost.RunAsync(Library());

        var client = McpClient.Modern(host);

        Assert.AreEqual("short circuit", (await client.CallAsync("intercepted", new JsonObject { ["value"] = 0 })).Text);
        Assert.AreEqual("value 1", (await client.CallAsync("intercepted", new JsonObject { ["value"] = 1 })).Text);
    }

    #endregion

    #region Functions

    [TestMethod]
    [MultiEngineTest]
    public async Task TestFunction(ServerEngine engine)
    {
        var tools = Tools.Create()
                         .Add("multiply", "Multiplies two numbers", [Tool(Title = "Multiplication", ReadOnly = true)] ([ComponentDescription("The first factor")] int a, int b) => a * b);

        await using var host = await TestHost.RunAsync(tools, engine: engine);

        var tool = await GetToolAsync(host, "multiply");

        Assert.AreEqual("Multiplies two numbers", tool["description"]!.GetValue<string>());
        Assert.AreEqual("Multiplication", tool["title"]!.GetValue<string>());
        Assert.IsTrue(tool["annotations"]!["readOnlyHint"]!.GetValue<bool>());
        Assert.AreEqual("The first factor", tool["inputSchema"]!["properties"]!["a"]!["description"]!.GetValue<string>());

        var response = await McpClient.Modern(host).CallAsync("multiply", new JsonObject { ["a"] = 6, ["b"] = 7 });

        Assert.AreEqual("42", response.Text);
    }

    [TestMethod]
    public async Task TestFunctionWithInterceptor()
    {
        var tools = Tools.Create()
                         .Add("secret", "Reveals the secret", [RequireRole("admin")] () => "42")
                         .Add(Concern.From((request, content) =>
                         {
                             request.SetUser(new TestUser("joe", "reader"));
                             return content.HandleAsync(request);
                         }));

        await using var host = await TestHost.RunAsync(tools);

        var response = await McpClient.Modern(host).CallAsync("secret");

        Assert.IsTrue(response.IsError);
    }

    [TestMethod]
    public async Task TestAsyncFunctionWithRequest()
    {
        var tools = Tools.Create()
                         .Add("echo", "Echoes a header", async (IRequest request) =>
                         {
                             await Task.Yield();
                             return request.Header.Headers.GetEntry("X-Echo");
                         });

        await using var host = await TestHost.RunAsync(tools);

        var response = await McpClient.Modern(host).CallAsync("echo", configure: r => r.Headers.Add("X-Echo", "hello"));

        Assert.AreEqual("hello", response.Text);
        Assert.IsFalse((await GetToolAsync(host, "echo"))["inputSchema"]!["properties"]!.AsObject().ContainsKey("request"));
    }

    [TestMethod]
    public async Task TestSingleDefaultInstance()
    {
        CountingTools.Created = 0;

        await using var host = await TestHost.RunAsync(Tools.Create().Add<CountingTools>());

        var client = McpClient.Modern(host);

        Assert.AreEqual("1", (await client.CallAsync("instances")).Text);
        Assert.AreEqual("1", (await client.CallAsync("instances")).Text);
    }

    [TestMethod]
    public async Task TestCustomInstanceProvider()
    {
        var created = new List<Type>();

        var tools = Tools.Create()
                         .InstanceProvider((type, request) =>
                         {
                             created.Add(type);
                             return new ValueTask<object>(new ConstructedTools(request.Header.Headers.GetEntry("X-Prefix") ?? "none"));
                         })
                         .Add<ConstructedTools>();

        await using var host = await TestHost.RunAsync(tools);

        var response = await McpClient.Modern(host).CallAsync("prefixed", new JsonObject { ["value"] = "x" }, r => r.Headers.Add("X-Prefix", "p"));

        Assert.AreEqual("p:x", response.Text);
        Assert.HasCount(1, created);
    }

    [TestMethod]
    public void TestConstructorIsRequired()
    {
        var e = Assert.ThrowsExactly<InvalidOperationException>(() => Tools.Create().Add<ConstructedTools>().Build());

        AssertX.Contains("parameterless constructor", e.Message);
    }

    public sealed class CountingTools
    {

        internal static int Created;

        public CountingTools() => Interlocked.Increment(ref Created);

        [Tool]
        [Summary("Returns how many instances were created")]
        public int Instances() => Created;

    }

    public sealed class ConstructedTools(string prefix)
    {

        [Tool]
        [Summary("Prefixes a value")]
        public string Prefixed(string value) => $"{prefix}:{value}";

    }

    [TestMethod]
    public async Task TestMixedRegistrations()
    {
        var tools = Tools.Create()
                         .Add<LibraryTools>()
                         .Add(new ValidationTests.SkillTools())
                         .Add("add", "Adds two numbers", (int a, int b) => a + b)
                         .Add("hello", "Says hello", () => "hello");

        await using var host = await TestHost.RunAsync(tools);

        var client = McpClient.Modern(host);

        Assert.AreEqual("3", (await client.CallAsync("add", new JsonObject { ["a"] = 1, ["b"] = 2 })).Text);
        Assert.AreEqual("200", (await client.CallAsync("get_http_status")).Text);
        Assert.AreEqual("hello", (await client.CallAsync("hello")).Text);
    }

    #endregion

}
