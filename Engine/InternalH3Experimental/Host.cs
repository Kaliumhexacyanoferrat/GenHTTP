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
    /// <param name="qpackDynamicTableCapacity">
    /// Bytes of QPACK dynamic table advertised to clients, and the ceiling on what this server will
    /// use for its own responses. 0 (the default) switches the mechanism off: headers are encoded
    /// with the static table and literals only.
    ///
    /// A nonzero value lets a client compress headers it repeats - cookies and user-agent, mostly -
    /// to about two bytes each. Most clients decline: curl, and .NET's own HTTP/3 client, advertise
    /// no table at all. Browsers are the ones that may use it.
    /// </param>
    public static IServerHost Create(int qpackDynamicTableCapacity = 0)
        => new H3ServerHost(qpackDynamicTableCapacity);

}
