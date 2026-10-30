# AGENTS.md

Guidance for coding agents working on GenHTTP. Humans should read [CONTRIBUTING.md](./CONTRIBUTING.md)
first. It defines the policy (Definition of Done, architecture principles, legal terms), and this file
explains how to meet it. If the two disagree, CONTRIBUTING.md wins. Please flag the conflict.

GenHTTP is an embeddable, modular HTTP server framework for .NET. It is used as a library, so its
public API, its performance and its secure behavior matter to everyone who depends on it. When goals
conflict, the order is: **correct and secure** → **compatible** (public API, all engines, all
platforms) → **fast on the request path** → convenient. Never trade the first two for speed.

## Commands

```bash
dotnet build GenHTTP.slnx -c Release                       # warnings are errors
dotnet test GenHTTP.slnx                                   # everything (slow, runs net10.0 and net11.0)

# Faster loops: one framework, one project, one class
dotnet test Testing/Acceptance/GenHTTP.Testing.Acceptance.csproj -f net11.0 \
    --filter "FullyQualifiedName~Modules.Redirects.RedirectTests"

# Only one engine (Internal | Kestrel | Ioxide)
TEST_ENGINE=Internal dotnet test Testing/Acceptance/GenHTTP.Testing.Acceptance.csproj -f net11.0

cd Playground && dotnet run                                # sample server on http://localhost:8080
```

- Filter with `FullyQualifiedName~...`. `ClassName=Foo` silently matches nothing.
- After changing a referenced project, do not pass `--no-build`. The test project would run against
  stale copies of the referenced assemblies.
- The SDK is pinned in `global.json` (.NET 11). Libraries target `net10.0;net11.0`. On net11.0 they compile
  with `runtime-async=on` (see `Directory.Build.props`), so performance and rare timing behavior
  can differ between the two frameworks.

## Repository Map

| Path                  | Contents                                                                                                                                 |
|-----------------------|------------------------------------------------------------------------------------------------------------------------------------------|
| `API/General`         | `GenHTTP.Api`: public contracts only (`IHandler`, `IRequest`, `ByteString`, ...). No 3rd-party dependencies except logging abstractions. |
| `API/Ioxide`          | `GenHTTP.Api.Ioxide`: the minimal ioxide contract that modules may use without referencing the engine.                                   |
| `Engine/Shared`       | Code shared by engines: request/response types, body framing, sinks, `CoreRouter`, compliance, logging.                                  |
| `Engine/Internal`     | Default engine: managed sockets + Glyph11 parser. HTTP/1.1.                                                                              |
| `Engine/Kestrel`      | Engine hosted on ASP.NET Core Kestrel. HTTP/1.1, 2, 3.                                                                                   |
| `Engine/Ioxide`       | io_uring engine (native ioxide). Linux only. HTTP/1.1, 2, 3.                                                                             |
| `Adapters/AspNetCore` | Runs GenHTTP handlers inside ASP.NET Core. Kestrel engine builds on it.                                                                  |
| `Modules/*`           | Feature modules (layouting, webservices, files, compression, auth, OpenAPI, ...). One NuGet package each.                                |
| `Generators/`         | Roslyn source generators (e.g. `[MemoryView]` structs such as `ByteString`).                                                             |
| `Testing/Testing`     | `GenHTTP.Testing` package (`TestHost`) for users and for our own tests.                                                                  |
| `Testing/Acceptance`  | The test suite. Folder structure mirrors `API/`, `Engine/`, `Modules/`, `Adapters/`.                                                     |
| `Benchmarks/`         | BenchmarkDotNet micro-benchmarks.                                                                                                        |
| `Packages/`           | Meta-package `.nuspec` files (`GenHTTP.Core`, `GenHTTP.Full`, and per-engine variants).                                                  |
| `Playground/`         | Runnable samples for manual checks and for users to understand the API.                                                                  |

The parser (Glyph11) and ioxide are separate projects consumed as NuGet packages. If they are checked
out next to this repository (`../Glyph11`, `../ioxide`), read their source rather than guessing. Flag
bugs in those projects upstream instead of working around them here.

## Architecture Rules

Dependencies point inwards to the API:

```
Modules ──► API ◄── Engine
   │                  │
   └──► Modules       └──► Engine.Shared, plus a few base modules (IO, ErrorHandling)
```

- **Modules never reference an engine** (`GenHTTP.Engine.*`). This rule has no exceptions. If a module
  needs an engine capability, add a minimal contract to the API (as `API/Ioxide` does) and let the
  engine fulfil it.
- Engines reference the API, `Engine.Shared` and a small fixed set of base modules. Do not add new module
  references to an engine.
- The API takes no new 3rd-party dependencies. Add a type to the API only if many modules need it
  (think `IResource`, `IUser`). Otherwise it belongs in a module.
- Modules may use other modules and 3rd-party packages. Each new package is a dependency for every
  user of that module, so justify it in the PR and prefer the BCL.
- Modules must work on every engine. `IServer.ServerEngine` exists only to pick an *optimized* path.
  Always keep a generic fallback, and never throw for an engine you don't know (`Custom` exists).

## Core Model

- **`IHandler`**: `PrepareAsync(IServer)` runs before serving and may run more than once. Do expensive
  setup there and pass the call on to child handlers. `HandleAsync(IRequest)` returns a response, or
  `null` for "not mine", which the router turns into a 404.
- **`IConcern`**: an `IHandler` that wraps `Content` (another handler) to add cross-cutting behavior
  such as compression, caching, auth, CORS. Builders attach concerns via `.Add(concernBuilder)`.
  `Concerns.Chain(...)` applies them.
- **Builders**: every handler and concern is created through a builder. The pattern is a public static
  factory class (`Redirect.To(...)`, `CompressedContent.Default()`), a builder implementing
  `IHandlerBuilder<TSelf>` / `IConcernBuilder` with fluent setters, and a `Build()` that validates input
  (`BuilderMissingPropertyException`) and returns the handler wrapped with `Concerns.Chain(_concerns, ...)`.
  `Build()` may be called repeatedly; don't hand mutable builder state to the built instance.
- **Errors**: throw `ProviderException(ResponseStatus, message)` for expected client-facing errors.
  The error handling concern renders it. Detailed error output is only allowed when `IServer.Development` is true.
- **Responses**: `request.Respond()` returns an `IResponseBuilder`. Bodies are `IResponseContent`.
  Report `Length` when you know it (otherwise the response is chunked) and keep `CalculateChecksumAsync`
  cheap, since caching concerns call it.
- **Branching behavior**: prefer composition. A dispatcher handler picks an inner handler in
  `PrepareAsync` (see `Modules/Files`, which selects an ioxide-native or a regular strategy). Do not make
  shared base class members `virtual` just to override them.

### Request Lifetime (where subtle bugs come from)

- `IRequest` instances and their memory are **reused** for the next request on the same connection,
  and connection contexts are pooled. Never store a request, or anything read from it, beyond the
  `HandleAsync` call.
- Header, path and query values are `ByteString` views into the connection's read buffer. Reading the
  body can invalidate them. If you need a header value after the body may have been consumed (e.g.
  after `await Content.HandleAsync(request)` in a concern), capture it first and call
  `.PreAllocate(request)`. That copies only when the request actually has a body.
- `GetBody()` may be called once. `HeaderAccess.Release` returns header memory to the engine and makes
  later header access throw. Use it only when nothing downstream needs headers.
- `Upgrade()` hands over the raw pipe for protocol upgrades (websockets). After that, the handler owns
  the connection and does its own flushing.

## Code Conventions

- File-scoped namespaces matching the folder (`GenHTTP.Modules.Compression.Providers`).
- `sealed` by default, for public and internal types. Keep implementation types `internal` unless users
  need them.
- `ValueTask` / `ValueTask<T>` on everything in the request path.
- Nullable reference types are on. Avoid `!` unless it is provably safe. Write plain null checks
  (`var x = ...; if (x != null)`), not `is { } x` patterns.
- Group members with `#region` blocks as the surrounding code does: `Get-/Setters`, `Initialization`,
  `Functionality`, `IDisposable Support`.
- **Public API needs XML docs** (`<summary>`, parameters, `<remarks>` for non-obvious behavior,
  `<example>` on factory methods). CS1591 is suppressed, so the compiler will not remind you.
- **Comments in code are rare.** Write code that explains itself. Comment only what the code cannot say,
  typically a workaround for a bug or quirk in another library, with a link if one exists.
- Logging goes through `IServer.Logging` (`ILoggerFactory`) with `[LoggerMessage]` source-generated
  methods. Don't resolve loggers or format messages per request unless something is actually logged.
- Match the style of the file you are editing. Don't reformat or "clean up" unrelated code in the same change.

## Performance

The request path is allocation-free by design, and engines are benchmarked publicly (HTTP Arena).
Regressions are taken seriously.

**The hot path** is anything that runs per request: engines, `Engine.Shared`, API protocol types, the
concerns added by `Defaults()` (compression, client caching, security), layout routing, the
Reflection/Webservices/Functional invocation path, file serving. Builders, `Build()`, `PrepareAsync`
and code generation run once, so allocate freely there.

On the hot path:

- No LINQ, no capturing lambdas, no `yield`/`IEnumerable` allocations, no `params` arrays. Use struct
  enumerators where iteration is needed (see `PreCompression.Accepted` in `Modules/Files/Multi`).
- Do not convert `ByteString` to `string` to compare or look things up. Use `KnownHeaders.*` and other
  `static readonly ByteString` constants. Header lookups compare precomputed hashes first, so keep it that way.
- Compute once, up front: sort, parse and precompute in the constructor, `Build()` or `PrepareAsync`.
- Be lazy: don't parse a header or build a structure unless the request actually needs it.
- Don't make code harder to read for gains you have not measured. Some changes measured as no gain,
  e.g. eliding a single `await` of a completed `ValueTask`.

Measuring:

- Use `Benchmarks/` (BenchmarkDotNet, `[MemoryDiagnoser]`) and report both time and allocations per
  operation, before and after. Prefer middleware-only benchmarks (parsed request → `HandleAsync`, no
  sockets) to isolate the change.
- Use the in-process toolchain for net11.0. BenchmarkDotNet 0.15.x cannot launch net11 out of process.
- Run-to-run noise is about ±4%. Use several launches and don't claim effects below ~5% without repeated runs.
- **Never use NSubstitute mocks in benchmark loops.** They record every call, the heap grows, and
  BenchmarkDotNet appears to hang. Write small stub classes instead.

## Security

GenHTTP terminates connections from the internet. Treat every byte of a request as hostile, including
path, query, headers (`Host`, `X-Forwarded-For`, `Forwarded`), cookies and body.

- **Validate after decoding.** Percent-decoding can introduce `/`, `\`, `..` or rooted paths (`%2f`,
  `%5c`). Check the decoded value, resolve it and verify it stays inside its root before touching the
  file system or an upstream URL.
- **Encode output.** HTML-escape anything request- or file-derived that ends up in a page (listings,
  error pages, templates).
- **Don't trust client-supplied routing headers.** `Host` and forwarding headers can be forged. Don't
  use them for security decisions, and be careful when building absolute URLs from them.
- **Secure defaults.** A feature's default configuration must be safe, e.g. tokens are validated and
  CORS never combines a wildcard or reflected origin with credentials. Unsafe behavior is opt-in and
  named as such.
- **Bound everything the client controls**: body size, nesting depth (a stack overflow kills the
  process and cannot be caught), decompression output, collection sizes, regex time (`MatchTimeout`),
  waiting time (honor timeouts and cancellation).
- **No unsafe deserialization.** No type-name handling, no `BinaryFormatter`, no loading types or
  assemblies or starting processes based on request data.
- Never return stack traces, generated source or internal paths outside development mode.
- Do not disclose a suspected vulnerability in a public issue, PR or commit message. Report it
  privately to the maintainer.

## HTTP Correctness and Engine Parity

- The same handler must behave identically on Internal, Kestrel and Ioxide. Kestrel is strict, and a
  Kestrel-only failure is often a real protocol bug that the other engines tolerate.
- Header names are case-insensitive. Repeated headers (`Set-Cookie`) must all survive. `Content-Length`
  must equal the bytes written. `101`, `204` and `304` responses carry no body and no `Content-*`
  headers. `HEAD` gets headers without a body. Add or extend `Vary` when you negotiate content.
- Don't swallow exceptions while writing a response body. Let them surface so the connection is
  handled correctly.
- **Root-cause every failure.** Before calling a failure an "engine limitation" or "flaky", check
  whether other tests of the same feature pass on that engine. If they do, there is a bug to find.

## Testing

Every change needs tests in `Testing/Acceptance`, in the folder that mirrors the code
(`Modules/Redirects/...` → `Testing/Acceptance/Modules/Redirects/RedirectTests.cs`). New code must
reach ≥85% coverage (SonarCloud checks this in CI). A bug fix includes a regression test that fails
without the fix.

**Tests are use-case centric.** Each acceptance test covers something a user actually does: configure
the handler the way an application would, send the request a client would send, and assert what the
client sees. Ask "what would a user call?", not "which combinations exist?".

- Write one test per use case, plus the edge cases that matter to users (error responses, limits a client
  can hit). Do not enumerate combinations of options, parameter types or inputs. If several cases share
  a code path, one representative test is enough. Reach coverage through meaningful scenarios, not
  permutations.
- Engines and execution modes are already multiplied by the test attributes. Don't add your own loops
  or `[DataRow]`s on top.
- Before adding a test, check whether an existing one already covers the behavior. Extend that test
  rather than adding a near-duplicate.
- **Unit tests only when they clearly pay off**, i.e. for logic that is hard or impossible to reach
  through HTTP, such as parser and framing edge cases. The default is an acceptance test. Don't unit
  test builders, wiring or trivial members, and don't mock `IRequest` to test what a real round trip
  can show.

```csharp
[TestClass]
public sealed class RedirectTests
{

    [TestMethod]
    [MultiEngineTest]
    public async Task TestTemporary(ServerEngine engine)
    {
        await using var host = await TestHost.RunAsync(Redirect.To("https://example.com/", true), engine: engine);

        using var response = await host.GetResponseAsync();

        await response.AssertStatusAsync(HttpStatusCode.TemporaryRedirect);
        Assert.AreEqual("https://example.com/", response.GetHeader("Location"));
    }

}
```

- Use `[MultiEngineTest]` (parameter `ServerEngine engine`) for anything that goes through a server.
  For anything built on Reflection (Webservices, Functional, Controllers) use `[MultiEngineFrameworkTest]`
  (`ServerEngine engine, ExecutionMode mode`), so that both reflection and code-generated invocation are
  covered. Pass `mode` into the builder.
- `TestHost` applies `Defaults()` (compression, client caching, HTTPS upgrade, HSTS) and development mode
  unless told otherwise. Pass `defaults: false` when asserting exact headers or bodies.
- Helpers: `GetResponseAsync`, `GetRequest(path, method)`, `GetContentAsync()`, `GetHeader()`,
  `AssertStatusAsync()`, `AssertX`. Dispose hosts with `await using` and responses with `using`.
- Tests run sequentially (`[assembly: DoNotParallelize]`) on incrementing ports. Don't hard-code ports.
- Ioxide runs on Linux only and is skipped elsewhere. On Windows or macOS, Ioxide behavior is first
  verified by CI (Linux, `ubuntu-latest`). Gate engine-specific tests with `Engines.IoxideEnabled()` /
  `Engines.KestrelEnabled()`.
- Name methods after the use case: `TestWhatIsVerified`. Where a unit test is justified, internal
  engine types are reachable via `InternalsVisibleTo`.

## Public API and Compatibility

GenHTTP follows semantic versioning (version in `Directory.Build.props`). Everything public in `API/`,
`Modules/`, `Testing/Testing` and the engines' `Host` entry points is used by applications.

- Don't remove, rename or change the signature of public members in a minor release. Add the new
  member and mark the old one `[Obsolete("Please use 'X' instead")]`, keeping it working.
- Adding a member to a public interface breaks implementers. Prefer a default interface method (as
  `ICompressionAlgorithm.FileExtension` does) or a new interface.
- Changing behavior (defaults, status codes, header output) can also be a breaking change. Call it out
  in the PR.
- Keep the public surface small. Expose new API through factory methods and builder setters, not
  public constructors.

## Workflow

Before writing code:

1. Read the module you change *and* its tests. Find the closest existing feature and follow its shape.
2. Note which projects and engines your change touches. That decides how much you need to test.

Before you are done:

1. `dotnet build GenHTTP.slnx -c Release` passes (warnings are errors).
2. The relevant tests pass. If you touched `API/`, `Engine/` or a module others depend on (IO,
   Reflection, Conversion, Layouting, ...), run the whole acceptance suite.
3. New behavior and fixes are covered by multi-engine tests.
4. Public API is documented. No new dependencies in `API/` or `Engine/`.
5. Hot path changes come with benchmark numbers, or a statement that none of the hot path code is affected.

Commits and PRs:

- Titles follow `type(scope): Summary`, e.g. `fix(reflection): Interceptors can now be used with more
  than one parameter`. Types are `feat`, `fix`, `perf`, `refactor`, `test`, `chore`. The scope is the
  module or area in lowercase (`engine`, `openapi`, `files`).
- One logical change per PR.
- The website documentation is maintained by the maintainer, so do not try to write it. Instead,
  describe in the PR what users will notice: new API with a short usage example, changed defaults or
  behavior, and anything that needs an upgrade note.
- A module's `README.md` is its NuGet package readme. Update it only if the module's purpose or entry
  points change.

## Environment Gotchas

- `.csproj` and `.slnx` files use Windows-style `\` paths. `sed`/`perl` in a POSIX shell easily
  mangle them, so use a proper editor tool (or PowerShell) for path edits.
- A test failing only on one target framework is often a runtime-async (net11.0) effect. Check both
  frameworks before concluding anything.
- CI runs the full suite with coverage on Linux (all three engines), then a build-and-test matrix for
  Linux, Windows and macOS on x64 and arm64 with the Internal engine. A change that passes locally on one
  OS can still fail elsewhere. Avoid OS-specific path, line-ending and file-locking assumptions.
