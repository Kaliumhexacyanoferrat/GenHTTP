using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.Ioxide.Infrastructure;
using GenHTTP.Engine.Ioxide.Infrastructure.Endpoints;
using GenHTTP.Engine.Ioxide.Protocol.Http1;
using GenHTTP.Engine.Ioxide.Protocol.Multiplexed;

using ioxide.tls;

using IoConnection = ioxide.TcpConnection;

namespace GenHTTP.Engine.Ioxide.Protocol;

internal static partial class ConnectionDriver
{
    private const int ShutWrite = 1;

    [LibraryImport("libc", EntryPoint = "shutdown")]
    private static partial int Shutdown(int sockfd, int how);

    [LibraryImport("libc", EntryPoint = "getpeername")]
    private static partial int GetPeerName(int sockfd, [Out] byte[] addr, ref int addrlen);

    private static readonly ReadOnlyMemory<byte> Preface = "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8.ToArray();

    internal static async Task HandleAsync(IServer server, IEndPoint endPoint, IoConnection conn,
        Protocols protocols = Protocols.Http1)
    {
        IDuplexPipe pipe;

        string? negotiated = null;

        try
        {
            if (endPoint.Secure)
            {
                if (!IoxideReactor.Current.GetService<TcpTlsRegistry>().TryFor(conn.ListenerPort, out var service))
                {
                    _ = Shutdown(conn.ClientFd, ShutWrite);
                    conn.DecRef();
                    return;
                }

                (pipe, negotiated) = await AcceptTlsAsync(conn, service, endPoint as SecureEndPoint);
            }
            else
            {
                pipe = new ioxide.TcpConnectionDualPipe(conn);
            }
        }
        catch
        {
            conn.DecRef();
            return;
        }

        var remoteAddress = GetPeerAddress(conn.ClientFd);

        var http2 = protocols.HasFlag(Protocols.Http2);
        var http1 = protocols.HasFlag(Protocols.Http1);

        var isHttp2 = http2 && (negotiated == "h2" || (negotiated is null && http1 && await StartsWithPrefaceAsync(pipe.Input)));

        if (http2 && !http1)
        {
            isHttp2 = true;
        }

        if (isHttp2)
        {
            try
            {
                await Http2Driver.RunAsync(server, endPoint, pipe, remoteAddress, endPoint.Secure);
            }
            catch
            {
            }
            finally
            {
                await CloseAsync(pipe, conn);
            }

            return;
        }

        if (!http1)
        {
            await CloseAsync(pipe, conn);
            return;
        }

        await Http1Driver.RunAsync(server, endPoint, pipe, conn, remoteAddress);
    }

    private static async ValueTask<(IDuplexPipe Pipe, string? Protocol)> AcceptTlsAsync(IoConnection conn, TlsService service,
        SecureEndPoint? endPoint)
    {
        var session = await service.AcceptAsync(conn);

        if (endPoint?.SecurityConfiguration.CertificateValidator is { } validator && !Accepts(validator, session))
        {
            session.Dispose();

            throw new AuthenticationException("The certificate validator rejected the peer.");
        }

        return (new TlsConnectionDualPipe(conn, session), session.NegotiatedAlpn);
    }

    private static bool Accepts(ICertificateValidator validator, TlsSession session)
    {
        var der = session.PeerCertificateDer;

        if (der is null || der.Length == 0)
        {
            return validator.Validate(null, null, SslPolicyErrors.RemoteCertificateNotAvailable);
        }

        using var certificate = X509CertificateLoader.LoadCertificate(der);

        using var chain = new X509Chain();
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.DisableCertificateDownloads = true;
        chain.Build(certificate);

        return validator.Validate(certificate, chain, SslPolicyErrors.None);
    }

    private static async ValueTask<bool> StartsWithPrefaceAsync(PipeReader reader)
    {
        while (true)
        {
            var result = await reader.ReadAsync();
            var buffer = result.Buffer;

            if (buffer.Length >= Preface.Length)
            {
                Span<byte> head = stackalloc byte[Preface.Length];
                buffer.Slice(0, Preface.Length).CopyTo(head);

                // Nothing consumed AND nothing examined: marking these examined would tell the
                // pipe we want more, and the protocol reading next would block on data already here.
                reader.AdvanceTo(buffer.Start, buffer.Start);

                return head.SequenceEqual(Preface.Span);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                return false;
            }
        }
    }

    internal static async ValueTask CloseAsync(IDuplexPipe pipe, IoConnection conn)
    {
        await pipe.Input.CompleteAsync();
        await pipe.Output.CompleteAsync();

        if (pipe is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync(); // tears down a TLS transport (stops the decrypt pump, close_notify)
        }

        Shutdown(conn.ClientFd, ShutWrite);
        conn.DecRef();
    }

    private static IPAddress? GetPeerAddress(int fd)
    {
        var addr = new byte[128]; // sockaddr_storage
        var len = addr.Length;

        if (GetPeerName(fd, addr, ref len) != 0)
        {
            return null;
        }

        var family = addr[0] | (addr[1] << 8);

        return family switch
        {
            2  => new IPAddress(addr[4..8]),   // AF_INET  -> sin_addr
            10 => new IPAddress(addr[8..24]),  // AF_INET6 -> sin6_addr
            _  => null,
        };
    }
}
