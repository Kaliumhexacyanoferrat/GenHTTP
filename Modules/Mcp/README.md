# GenHTTP.Modules.Mcp

This framework allows to provide tools to AI agents such as Claude
via the [Model Context Protocol](https://modelcontextprotocol.io/) (MCP),
in the same way webservices are provided with the webservice or
controller frameworks.

```csharp
var mcp = Tools.Create()
               .Server("library", "1.0.0")
               .Instructions("Search for books before lending them.")
               .Add<LibraryTools>()
               .Add("add", "Adds two numbers", [Tool(ReadOnly = true)] (int a, int b) => a + b);

await Host.Create()
          .Handler(Layout.Create().Add("mcp", mcp))
          .RunAsync();

public sealed class LibraryTools
{

    [Tool(ReadOnly = true)]
    [Summary("Searches the library for books")]
    public List<Book> FindBooks([Summary("Part of the title")] string query, [Range(1, 50)] int limit = 10) => ...;

    [Tool(Destructive = true)]
    [Summary("Removes all books from the library")]
    [RequireRole("admin")]
    public void Purge(IUser user) => ...;

}
```

The parameters of a tool become its arguments, except for values injected by the
framework (such as the request or the authenticated user). Validation attributes such
as `[Range]` are advertised in the schema and checked before the tool is invoked.
Interceptors such as role checks run before the tool is invoked, and their
documentation is added to the description of the tool.

With `GenHTTP.Modules.DependencyInjection`, tool classes are created per call with
their dependencies resolved, and services can be injected into tool parameters:

```csharp
var mcp = DependentTools.Create()
                        .Add<LibraryTools>();
```

The endpoint can then be added to Claude Code:

```bash
claude mcp add --transport http library http://localhost:8080/mcp
```

The endpoint implements the Streamable HTTP transport for both the stateless revision
2026-07-28 and the earlier revisions 2025-03-26 to 2025-11-25.

For details and examples, please refer to [the documentation](https://genhttp.org/documentation/content/frameworks/mcp/).
