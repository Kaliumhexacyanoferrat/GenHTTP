using System.IO;
using System.Net.Sockets;

namespace GenHTTP.Engine.Internal.Protocol;

/// <summary>
/// Helps to identify exceptions that are caused by a client disconnecting
/// gracefully (e.g. during a keep alive read or while the response is
/// flushed), so they do not need to be logged as actual server errors.
/// </summary>
internal static class ConnectionExceptions
{

    internal static bool IsGracefulDisconnect(Exception e) => e switch
    {
        OperationCanceledException => true,
        IOException { InnerException: SocketException { SocketErrorCode: SocketError.ConnectionReset or SocketError.ConnectionAborted or SocketError.Shutdown } } => true,
        IOException io when io.Message.Contains("Broken pipe", StringComparison.OrdinalIgnoreCase) => true,
        SocketException { SocketErrorCode: SocketError.ConnectionReset or SocketError.ConnectionAborted or SocketError.Shutdown } => true,
        _ => false
    };

}
