using System.Net;
using System.Net.Sockets;

namespace GenHTTP.Engine.Protocol
{

    internal static class SocketExtensions
    {

        public static IPAddress? GetAddress(this Socket socket) => (socket.RemoteEndPoint as IPEndPoint)?.Address;

    }

}
