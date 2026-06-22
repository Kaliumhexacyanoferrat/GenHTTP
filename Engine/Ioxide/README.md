# GenHTTP.Engine.Ioxide

A GenHTTP server engine backed by the [ioxide](https://github.com/MDA2AV/ioxide)
io_uring runtime (thread-per-core, shared-nothing) — a third engine alongside
`Internal` (socket + `Stream`) and `Kestrel` (ASP.NET Core).

Unlike the Kestrel engine (which hands GenHTTP a fully-parsed `HttpContext`),
this engine runs **GenHTTP's own HTTP/1.1 conversation** — exactly like the
Internal engine — but on an ioxide connection instead of a socket. No Kestrel,
no ASP.NET Core.

## How it works

```
ioxide reactor (one per core, io_uring, SO_REUSEPORT)
  └─ accept → Connection
       └─ new ConnectionDualPipe(conn)            // .Input = PipeReader, .Output = PipeWriter (zero-copy, inline IVTS)
            └─ ConnectionDriver loop:
                 Glyph11 parser → GenHTTP Request (reused, public) → Handler.HandleAsync → ResponseWriter → PipeWriter
```

The integration seam is ioxide's `ConnectionDualPipe`: GenHTTP's parse/handle/respond
loop is already pure `PipeReader`/`PipeWriter`, so ioxide's native pipe bridge drops
straight in. Reused from GenHTTP unchanged: the public `Request` model, the Glyph11
parser, the `IResponseSink` content contract. Forked (thin): the per-connection loop
(`ConnectionDriver`), the response writer (`ResponseWriter`, `StatusLine`), and a
`PipeWriter`-backed sink/stream.

## Usage

```csharp
using GenHTTP.Engine.Ioxide;
using GenHTTP.Modules.IO;

await Host.Create()
          .Handler(Content.From(Resource.FromString("hello from ioxide-genhttp")))
          .RunAsync();
```

### Tuning the io_uring runtime

`Host.Create` (and `Server.Create`) accept an optional hook to customize ioxide's
`ServerConfig` — reactor count, ring/queue sizes, recv/write buffers. The hook
receives a config pre-seeded with defaults (reactors = `Environment.ProcessorCount`);
return a modified copy. The listen **port** always comes from the GenHTTP endpoint
binding (`.Port()`/`.Bind()`), so any port set in the hook is overridden.

```csharp
await Host.Create(c => c with
                  {
                      ReactorCount      = 16,        // one io_uring reactor per core
                      RingEntries       = 16384,
                      RecvBufferSize    = 64 * 1024,
                      BufferRingEntries = 8192,
                  })
          .Handler(app)
          .RunAsync();
```

## Dependency on ioxide

References the published [`ioxide`](https://www.nuget.org/packages/ioxide) NuGet
package (`0.0.5`). The BCL pipe bridges the engine builds on
(`ConnectionDualPipe`/`ConnectionPipeReader`/`ConnectionPipeWriter`/`ConnectionStream`)
ship in that package.

**Build note:** requires a .NET SDK with Roslyn 5.3+ (SDK 10.0.301+) because
GenHTTP's `MemoryView` source generator references `Microsoft.CodeAnalysis 5.3`.

## Status — spike

Validated: HTTP/1.1 `GET`/`POST`, fixed-length **and chunked** (unknown-length)
responses, keep-alive (connection reuse), `HEAD`, and GenHTTP webservices.
Clean build, 0 warnings.

Response handling mirrors the Internal engine: cached status lines, a once-a-second
`Date` header (per-reactor, thread-static), and a `ChunkedWriter` for unknown-length
content. `Handler.PrepareAsync` runs at startup so handlers initialise before serving.

Not yet implemented:

- TLS / HTTPS (ioxide is plaintext `AF_INET` only here; `ioxide.tls`/kTLS would wire HTTPS endpoints).
- IPv6 bind and multiple endpoints (first endpoint only).
- Graceful shutdown / connection drain (reactors are background threads; `DisposeAsync` only flips `Running`).
- `IServerCompanion` callbacks, the `Host`-header check, and the error-response path.

**Thread-per-core caveat:** ioxide resumes awaited continuations *inline on the
reactor thread*. A handler that blocks or does sync-over-async will stall the
entire reactor (every connection it owns), unlike the Internal engine's
thread-pool model. Ideal for non-blocking / CPU-bound handlers; handlers that go
truly async off-reactor need care.
