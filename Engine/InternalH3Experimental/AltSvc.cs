using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.InternalH3Experimental;

/// <summary>
/// Advertises an HTTP/3 endpoint from a server that speaks HTTP/1.1 or HTTP/2.
/// </summary>
/// <remarks>
/// Browsers never start on HTTP/3. They connect over TCP, and only try QUIC once a response has
/// told them where to find it (RFC 7838). Without this header the HTTP/3 endpoint is reachable by
/// clients told to use it explicitly, and by nobody else.
///
/// <para>Two things stop it working, both silently: the advertisement is only honoured when it
/// arrives over TLS, and the certificate on the HTTP/3 port must be valid for the ORIGIN's host
/// name. A wrong port produces no error at all, the browser simply keeps using HTTP/1.1.</para>
/// </remarks>
public sealed class AltSvcConcern : IConcern
{
    private readonly ByteString _value;

    public IHandler Content { get; }

    public AltSvcConcern(IHandler content, ushort port, uint maxAge)
    {
        Content = content;
        _value = new ByteString($"h3=\":{port}\"; ma={maxAge}");
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        IResponse? response = await Content.HandleAsync(request);

        // Pointless on a connection that is already HTTP/3, and ignored by clients anyway.
        if (response is not null && request.Header.Protocol != HttpProtocol.Http3)
        {
            response.Rebuild().Header(AltSvcName, _value);
        }

        return response;
    }

    private static readonly ByteString AltSvcName = new("alt-svc");
}

/// <summary>
/// Builder for <see cref="AltSvcConcern"/>.
/// </summary>
public sealed class AltSvcConcernBuilder : IConcernBuilder
{
    private ushort _port = 443;

    private uint _maxAge = 86400;

    /// <summary>
    /// The UDP port the HTTP/3 endpoint listens on. Must match what that server bound, or clients
    /// silently never upgrade.
    /// </summary>
    public AltSvcConcernBuilder Port(ushort port)
    {
        _port = port;
        return this;
    }

    /// <summary>How long a client may cache the advertisement, in seconds.</summary>
    public AltSvcConcernBuilder MaxAge(uint seconds)
    {
        _maxAge = seconds;
        return this;
    }

    public IConcern Build(IHandler content) => new AltSvcConcern(content, _port, _maxAge);
}

/// <summary>
/// Advertises an HTTP/3 endpoint to clients arriving over TCP.
/// </summary>
public static class AltSvc
{

    /// <summary>
    /// Adds an <c>Alt-Svc</c> header pointing at an HTTP/3 endpoint on the given UDP port.
    /// </summary>
    /// <example>
    /// <code>
    /// var h1 = GenHTTP.Engine.Internal.Host.Create()
    ///                 .Handler(app)
    ///                 .Add(AltSvc.To(443))
    ///                 .Bind(IPAddress.Any, 443, certificate);
    /// </code>
    /// </example>
    public static AltSvcConcernBuilder To(ushort port) => new AltSvcConcernBuilder().Port(port);

}
