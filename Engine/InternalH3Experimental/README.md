# GenHTTP HTTP/3 Engine (experimental)

Serves an application over HTTP/3, using [Glyph3](https://github.com/dotnet-web-stack/Glyph3) for
HTTP/3 and `System.Net.Quic` (MsQuic) for QUIC.

```csharp
var h3 = GenHTTP.Engine.InternalH3Experimental.Host.Create()
                .Handler(app)
                .Bind(IPAddress.Any, 443, certificate);
```

Browsers never start on HTTP/3. They connect over TCP first and only try QUIC once a response tells
them where to find it, so this engine is meant to run beside one that serves HTTP/1.1:

```csharp
var h1 = GenHTTP.Engine.Internal.Host.Create()
                .Handler(app)
                .Add(AltSvc.To(443))     // Alt-Svc: h3=":443"; ma=86400
                .Bind(IPAddress.Any, 443, certificate);
```

TCP:443 and UDP:443 are different sockets, so both bind at once. Two things stop the upgrade
working, both silently: `Alt-Svc` is only honoured when it arrives over TLS, and the certificate on
the HTTP/3 port must be valid for the origin's host name.

## Requirements

QUIC needs libmsquic present. Windows 11 / Server 2022+ ships it with the .NET runtime; Linux
installs it (`sudo apt install libmsquic`); macOS uses Homebrew plus `DYLD_FALLBACK_LIBRARY_PATH`.
The engine throws at startup when it is missing rather than failing later.

## Status

Experimental. Known gaps:

- Request bodies are assembled before the handler runs, so a large upload is held in memory.
  Glyph3 can stream them; the engine does not yet.
- Response bodies are buffered for the same reason.
- `IRequest.Upgrade()` throws: connection upgrades are an HTTP/1.1 mechanism.
- No server push, no trailers, no 0-RTT.
- Requests carry their own `IRequest` implementation rather than the shared `Request`, whose
  `Source` is a Glyph11 `BinaryRequest` and therefore assumes an HTTP/1.1 parse.
