using GenHTTP.Api.Infrastructure;

using GenHTTP.Engine.InternalH3Experimental.Infrastructure;

namespace GenHTTP.Engine.InternalH3Experimental;

/// <summary>
/// Entry point to host an application over HTTP/3.
/// </summary>
/// <remarks>
/// EXPERIMENTAL. QUIC comes from System.Net.Quic, which needs libmsquic present: Windows ships it
/// with the .NET runtime, Linux and macOS install it separately. HTTP/3 comes from Glyph3.
///
/// <para>Browsers do not reach HTTP/3 directly. They connect over HTTP/1.1 or HTTP/2 first and
/// only try QUIC once a server advertises it, so this engine is meant to run beside one that
/// serves TCP. See <see cref="AltSvc"/>.</para>
/// </remarks>
public static class Host
{

    /// <summary>
    /// Provides a new server host serving HTTP/3 over QUIC.
    /// </summary>
    public static IServerHost Create() => new H3ServerHost();

}
