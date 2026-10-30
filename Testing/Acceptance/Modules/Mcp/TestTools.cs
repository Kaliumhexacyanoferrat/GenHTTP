using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Documentation;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Mcp;
using GenHTTP.Modules.Mcp.Provider;

using ComponentDescription = System.ComponentModel.DescriptionAttribute;

namespace GenHTTP.Testing.Acceptance.Modules.Mcp;

public enum Unit { Celsius, Fahrenheit }

public sealed record Alert(string Kind, string? Note);

public sealed record Forecast(string City, double Temperature, string? Warning, IReadOnlyList<string> Conditions, Unit? Unit, IReadOnlyList<Alert>? Alerts);

public sealed class TestTools
{

    [Tool]
    [Summary("Adds two integers")]
    public int Add([ComponentDescription("The first summand")] int a, [Summary("The second summand")] int b) => a + b;

    [Tool(Title = "Weather forecast", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Summary("Returns the weather forecast for a city")]
    public async Task<Forecast> GetForecast([Summary("The city to forecast")] string city, Unit unit = Unit.Celsius, int? days = null)
    {
        await Task.Yield();

        return new Forecast(city, unit == Unit.Celsius ? 21.5 : 70.7, null, ["sunny", $"{days ?? 1} day(s)"], unit, [new Alert("wind", null)]);
    }

    [Tool]
    [Summary("Returns the value of the X-Test header")]
    public string ReadHeader(IRequest request) => request.Header.Headers.GetEntry("X-Test") ?? string.Empty;

    [Tool]
    [Summary("Always fails")]
    public string Fail() => throw new ProviderException(ResponseStatus.BadRequest, "The date needs to be in the future");

    [Tool]
    [Summary("Always crashes")]
    public ValueTask<string> Crash() => throw new InvalidOperationException("Database is down");

    [Tool]
    [Summary("Crashes after doing some work")]
    public async Task<string> CrashLater()
    {
        await Task.Yield();
        throw new InvalidOperationException("Disk is full");
    }

    [Tool]
    [Summary("Returns Markdown")]
    public ToolResult Markdown() => ToolResult.Text("# Title");

    [Tool]
    [Summary("Returns some content blocks")]
    public ToolResult Image() => ToolResult.From(new TextContent("A pixel"),
                                                 new ImageContent(new byte[] { 1, 2, 3 }, "image/png"),
                                                 new AudioContent(new byte[] { 4, 5 }, "audio/wav"),
                                                 new ResourceLinkContent("https://example.com/a.txt", "a.txt") { Title = "A", Description = "A file", MimeType = "text/plain" });

    [Tool]
    [Summary("Returns structured data")]
    public ToolResult Structured() => ToolResult.Json(new { ok = true, count = 2 });

    [Tool]
    [Summary("Returns a list")]
    public List<string> List() => ["a", "b"];

    [Tool]
    [Summary("Echoes the text")]
    public string Echo(string text) => text;

    [Tool]
    [Summary("Returns an error result")]
    public ToolResult ErrorResult() => ToolResult.Error("Not today");

}

internal static class TestServer
{

    internal const int ToolCount = 12;

    internal static ToolsHandlerBuilder Create() => Tools.Create()
                                                         .Server("test-server", "1.0.0", "Test Server")
                                                         .Instructions("Call add to add numbers.")
                                                         .Add<TestTools>();

    internal static Task<TestHost> RunAsync(ServerEngine engine = ServerEngine.Internal) => TestHost.RunAsync(Create(), engine: engine);

}
