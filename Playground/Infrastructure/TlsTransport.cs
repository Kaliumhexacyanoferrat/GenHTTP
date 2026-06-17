using System.Buffers;
using System.IO.Pipelines;

using GenHTTP.Engine.Ioxide;

using ioxide;
using ioxide.tls;

namespace genhttp.Infrastructure;

/// <summary>
/// TLS termination for the json-tls profile: a second listener on :8081 with kTLS TX offload.
/// Configured from TLS_CERT / TLS_KEY (defaulting to the harness-mounted /certs). The service is
/// registered per reactor (OnStart) and the connection factory wraps :8081 connections.
/// </summary>
public static class Tls
{
    public const ushort Port = 8081;

    public static string? CertPath { get; private set; }

    public static string? KeyPath { get; private set; }

    public static bool Enabled => CertPath is not null;

    public static bool Configure()
    {
        var cert = Environment.GetEnvironmentVariable("TLS_CERT") ?? "/certs/server.crt";
        var key = Environment.GetEnvironmentVariable("TLS_KEY") ?? "/certs/server.key";

        if (File.Exists(cert) && File.Exists(key))
        {
            CertPath = cert;
            KeyPath = key;
            return true;
        }

        return false;
    }

    // onReactorStart: register the ring-native TLS service (OpenSSL ctx) on this reactor.
    public static void StartService(Reactor reactor)
        => TlsService.Start(reactor, new TlsOptions { CertificatePath = CertPath!, KeyPath = KeyPath! });

    // connectionFactory: TLS-terminate the :8081 listener; the main (:8080) port stays plaintext.
    public static async ValueTask<IDuplexPipe> CreatePipe(Connection conn)
    {
        if (conn.ListenerPort == Port)
        {
            var session = await IoxideReactor.Current.GetService<TlsService>().AcceptAsync(conn);
            return new TlsDuplexPipe(conn, session);
        }

        return new ConnectionDualPipe(conn);
    }
}

/// <summary>
/// Adapts a TLS connection to the duplex pipe GenHTTP serves over. Inbound: a pump reads raw recv
/// slices, decrypts each via the <see cref="TlsSession" />, and writes the plaintext into a Pipe the
/// engine reads. Outbound: the engine writes plaintext to the connection's writer and kTLS TX (enabled
/// during the handshake) has the kernel produce the records — so no explicit encrypt step.
/// </summary>
internal sealed class TlsDuplexPipe : IDuplexPipe, IAsyncDisposable
{
    private readonly Connection _conn;

    private readonly TlsSession _tls;

    private readonly Pipe _inbound;

    private readonly ConnectionDualPipe _outer; // only its writer is used (plaintext + kTLS TX)

    private readonly CancellationTokenSource _cts;

    private readonly Task _pump;

    public TlsDuplexPipe(Connection conn, TlsSession session)
    {
        _conn = conn;
        _tls = session;
        _inbound = new Pipe();
        _outer = new ConnectionDualPipe(conn);
        _cts = new CancellationTokenSource();
        _pump = PumpAsync(_cts.Token);
    }

    public PipeReader Input => _inbound.Reader;

    public PipeWriter Output => _outer.Output;

    private async Task PumpAsync(CancellationToken ct)
    {
        var writer = _inbound.Writer;

        try
        {
            // The client's first request can ride in bundled with its Finished flight.
            var initial = _tls.DrainPlaintext();
            if (!initial.IsEmpty)
            {
                writer.Write(initial);
                await writer.FlushAsync(ct);
            }

            while (!ct.IsCancellationRequested)
            {
                var snapshot = await _conn.ReadAsync();

                var produced = false;

                unsafe
                {
                    while (_conn.TryGetItem(snapshot, out var item))
                    {
                        if (item.HasBuffer)
                        {
                            var plain = _tls.Decrypt(item.Ptr, item.Len);
                            if (!plain.IsEmpty)
                            {
                                writer.Write(plain);
                                produced = true;
                            }
                        }

                        _conn.ReturnBuffer(in item);
                    }
                }

                _conn.ResetRead();

                if (produced)
                {
                    var flush = await writer.FlushAsync(ct);
                    if (flush.IsCompleted)
                    {
                        break;
                    }
                }

                if (snapshot.IsClosed)
                {
                    break;
                }
            }
        }
        catch
        {
            // connection fault / cancellation — the reader is completed in finally
        }
        finally
        {
            await writer.CompleteAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();

        try
        {
            await _pump;
        }
        catch
        {
            // ignore teardown faults
        }

        _tls.Dispose();
        _cts.Dispose();
    }
}
