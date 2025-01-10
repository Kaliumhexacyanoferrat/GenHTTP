using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websockets.Handler;

public sealed class WrappedSocket : ISocket
{

    #region Get-/Setters

    public Socket Socket { get; }

    public Stream Stream { get; }

    public CancellationTokenSource TokenSource { get; }

    public bool Connected => Socket.Connected;

    public bool NoDelay
    {
        get => Socket.NoDelay;
        set => Socket.NoDelay = value;
    }

    public string RemoteIpAddress => (Socket.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? string.Empty;

    public int RemotePort => (Socket.RemoteEndPoint as IPEndPoint)?.Port ?? -1;

    #endregion

    #region Initialization

    public WrappedSocket(UpgradeInfo upgradeInfo)
    {
        Socket = upgradeInfo.Socket;
        Stream = upgradeInfo.Stream;

        TokenSource = new();
    }

    #endregion

    #region Functionality

    public async Task Send(byte[] buffer, Action callback, Action<Exception> error)
    {
        try
        {
            await Stream.WriteAsync(buffer.AsMemory(), TokenSource.Token);
            await Stream.FlushAsync();

            callback();
        }
        catch (Exception ex)
        {
            error(ex);
        }
    }

    public async Task<int> Receive(byte[] buffer, Action<int> callback, Action<Exception> error, int offset = 0)
    {
        try
        {
            var result = await Stream.ReadAsync(buffer.AsMemory(offset), TokenSource.Token);

            callback(result);

            return result;
        }
        catch (Exception ex)
        {
            error(ex);
            return -1;
        }
    }

    public void Close() => Dispose();

    public void Dispose()
    {
        TokenSource.Cancel();
        Stream.Dispose();
        Socket.Dispose();
    }

    #endregion

    #region Unnecessary stuff

    private const string NotRequiredByIntegration = "Not required by integration";

    public EndPoint LocalEndPoint => throw new NotImplementedException(NotRequiredByIntegration);

    public Task<ISocket> Accept(Action<ISocket> callback, Action<Exception> error)
        => throw new NotImplementedException(NotRequiredByIntegration);

    public void Bind(EndPoint ipLocal)
        => throw new NotImplementedException(NotRequiredByIntegration);

    public void Listen(int backlog)
        => throw new NotImplementedException(NotRequiredByIntegration);

    public Task Authenticate(X509Certificate2 certificate, SslProtocols enabledSslProtocols, Action callback, Action<Exception> error)
        => throw new NotImplementedException(NotRequiredByIntegration);

    #endregion

}
