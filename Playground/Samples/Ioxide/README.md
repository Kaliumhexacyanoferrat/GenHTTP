# Ioxide engine samples

Each file here builds one server and returns it unstarted, so a sample is run by handing it to
`Program.cs`:

```csharp
using GenHTTP.Playground.Samples.Ioxide;

var server = TlsSample.Create();

await server.RunAsync();
```

Certificates are minted into `./certs` on startup by `Certs`, so every sample runs with no setup.
A deployment points the providers at the files its ACME client already writes instead.

| Sample | Serves | Shows |
| --- | --- | --- |
| `Http1Sample` | `http://localhost:8080` | the smallest binding there is - no certificate, no negotiation |
| `Http2Sample` | `http://localhost:8081`, `:8082` | h2c: HTTP/2 alone, and sharing one socket with HTTP/1.1 |
| `Http3Sample` | `https://localhost:8443` | QUIC only, and why it needs the certificate as files |
| `TlsSample` | `https://localhost:8443` | TLS in OpenSSL, with ALPN choosing h2 or http/1.1 |
| `KernelTlsSample` | `https://localhost:8443` | the record layer in the kernel, and what it pins |
| `SniSample` | `https://localhost:8443` | a certificate per host name, chosen by the client |
| `CertificateRotationSample` | `https://localhost:8443` | replacing certificates on a running server (`SIGHUP`) |
| `AllProtocolsSample` | `:8080`, `:8082`, `https://:8443` | all three protocols on one host, as a browser meets them |
| `ShowcaseSample` | `:8080`-`:8082`, `https://:8443`, `:8444` | everything above at once, plus mutual TLS and the two static handlers |

`ShowcaseSample` is what `Program.cs` runs by default. The others are the same ideas taken one at a
time, which is usually the easier place to start reading.

## Things worth knowing before reading them

**HTTP/3 needs the certificate as files.** ngtcp2 loads PEM by path and takes nothing else, and the
engine will not write a private key out on your behalf - so a port serving HTTP/3 must be bound
with an `IFileCertificateProvider`. The TCP transports take either files or an `X509Certificate2`.

**Only one endpoint may serve HTTP/3.** The engine binds a single QUIC listener; a second is
refused when the server starts.

**A browser will not find an HTTP/3-only port.** Browsers connect over TCP and move to QUIC once an
`Alt-Svc` header points them there, so HTTP/3 wants a port that also serves HTTP/1.1 or HTTP/2 -
which is what `AllProtocolsSample` binds.

**kTLS is off by default and is not a free win.** It needs the Linux `tls` module, pins TLS 1.3 and
one ciphersuite, and disables session resumption. Check `cat /proc/sys/net/ipv4/tcp_available_ulp`
before assuming it is doing anything.
