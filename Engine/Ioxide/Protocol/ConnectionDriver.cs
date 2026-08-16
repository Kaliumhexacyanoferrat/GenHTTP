using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.InteropServices;

using GenHTTP.Api.Infrastructure;

using IoConnection = ioxide.TcpConnection;

namespace GenHTTP.Engine.Ioxide.Protocol;

/// <summary>
/// Takes an accepted TCP connection, establishes its transport, and hands it to the protocol it
/// turns out to be speaking.
/// </summary>
/// <remarks>
/// Everything arriving over TCP comes through here - HTTP/1.1 and HTTP/2 both, since they share the
/// socket. Which of them a connection is settles two ways: ALPN during the TLS handshake on a secure
/// endpoint, and the HTTP/2 connection preface on a plaintext one. HTTP/3 never reaches this: QUIC
/// is a UDP listener and goes straight to <see cref="Http3Driver"/>.
///
/// <para>Once decided, the connection belongs to <see cref="Http1Driver"/> or
/// <see cref="Http2Driver"/> for its lifetime, and this only tears it down again.</para>
/// </remarks>
internal static partial class ConnectionDriver
{
    /// <summary>
    /// Half-close (SHUT_WR = 1) the socket's write side to send FIN. ioxide's refcounted teardown does not
    /// FIN a server-initiated close by itself (the reactor's active recv keeps a reference), so an
    /// EOF-delimited response (connection-close / upgrade) would otherwise hang the client. The read side
    /// stays open so the client's own close is still observed and the reactor reclaims the connection.
    /// </summary>
    private const int ShutWrite = 1;

    [LibraryImport("libc", EntryPoint = "shutdown")]
    private static partial int Shutdown(int sockfd, int how);

    [LibraryImport("libc", EntryPoint = "getpeername")]
    private static partial int GetPeerName(int sockfd, [Out] byte[] addr, ref int addrlen);

    private static readonly ReadOnlyMemory<byte> Preface = "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n"u8.ToArray();

    internal static async Task HandleAsync(IServer server, IEndPoint endPoint, IoConnection conn,
        Func<IoConnection, ValueTask<IDuplexPipe>>? connectionFactory, IoxideProtocols protocols = IoxideProtocols.Http1)
    {
        IDuplexPipe pipe;

        // What ALPN settled on, when the transport negotiated anything. Null on a plaintext port,
        // and on a TLS port whose client offered nothing we serve.
        string? negotiated = null;

        try
        {
            if (connectionFactory is not null)
            {
                pipe = await connectionFactory(conn);
            }
            else if (endPoint.Secure)
            {
                // A secure port with no certificate (an SNI-only provider yielded none) is advertised
                // for redirects but cannot handshake - FIN the connection so the client's handshake
                // fails fast rather than a plaintext response landing on an https port.
                if (!IoxideReactor.Current.GetService<TlsRegistry>().TryFor(conn.ListenerPort, out var service))
                {
                    _ = Shutdown(conn.ClientFd, ShutWrite);
                    conn.DecRef();
                    return;
                }

                (pipe, negotiated) = await IoxideTls.AcceptWithAlpnAsync(conn, service);
            }
            else
            {
                pipe = new ioxide.TcpConnectionDualPipe(conn);
            }
        }
        catch
        {
            // failed handshake (or factory fault) - release the connection instead of leaking it
            conn.DecRef();
            return;
        }

        // The peer address is constant for the connection; resolve it once from the socket fd.
        var remoteAddress = GetPeerAddress(conn.ClientFd);

        var http2 = protocols.HasFlag(IoxideProtocols.Http2);
        var http1 = protocols.HasFlag(IoxideProtocols.Http1);

        // Only worth peeking when both share the port. On an HTTP/2-only port every connection is
        // HTTP/2 by definition, and on an HTTP/1.1-only port the preface would be a malformed
        // request line either way.
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
                // client or protocol fault - teardown happens below
            }
            finally
            {
                await CloseAsync(pipe, conn);
            }

            return;
        }

        // The port does not serve HTTP/1.1, and this connection is not HTTP/2 - which on a secure
        // port means the client offered no ALPN this endpoint accepts. Close rather than answer it
        // with a protocol the endpoint was configured not to speak.
        if (!http1)
        {
            await CloseAsync(pipe, conn);
            return;
        }

        // HTTP/1.1 tears the connection down itself, so that it can return its pooled request first.
        await Http1Driver.RunAsync(server, endPoint, pipe, conn, remoteAddress);
    }

    /// <summary>
    /// Peeks for the HTTP/2 connection preface without consuming it, so a plaintext client using
    /// prior knowledge (h2c) is recognised and the same bytes are handed to the HTTP/2 layer.
    /// </summary>
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

                // Nothing consumed AND nothing examined: marking these bytes examined would tell the
                // pipe we are waiting for more, and whichever protocol reads next would block on data
                // that has already arrived.
                reader.AdvanceTo(buffer.Start, buffer.Start);

                return head.SequenceEqual(Preface.Span);
            }

            // Too short to decide yet - examined to the end, so the next read waits for more.
            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Ends a connection: complete both halves, tear down the transport, FIN, release.
    /// </summary>
    /// <remarks>
    /// Completing the writer matters beyond tidiness. Length-delimited responses do not need it, but
    /// connection-close and upgrade (101) responses are delimited by FIN, and without it the client
    /// waits for bytes that never come.
    /// </remarks>
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

    // The connected client's remote address, read once per connection straight from the socket fd
    // (ioxide exposes the fd but not the peer address). Mirrors the Internal engine's
    // Socket.RemoteEndPoint.Address - returned as-is (IPv4-mapped IPv6 on a dual-stack listener), which
    // IPAddress.IsLoopback and the rest of the pipeline already handle.
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
