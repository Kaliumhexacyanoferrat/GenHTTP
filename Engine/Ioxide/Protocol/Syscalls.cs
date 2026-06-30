using System.Runtime.InteropServices;

namespace GenHTTP.Engine.Ioxide.Protocol;

public static partial class Syscalls
{

    /// <summary>
    /// Half-close (SHUT_WR = 1) the socket's write side to send FIN. ioxide's refcounted teardown does not
    /// FIN a server-initiated close by itself (the reactor's active recv keeps a reference), so an
    /// EOF-delimited response (connection-close / upgrade) would otherwise hang the client. The read side
    /// stays open so the client's own close is still observed and the reactor reclaims the connection.
    /// </summary>
    public const int ShutWrite = 1;

    [LibraryImport("libc", EntryPoint = "shutdown")]
    public static partial int Shutdown(int sockfd, int how);

    [LibraryImport("libc", EntryPoint = "getpeername")]
    public static partial int GetPeerName(int sockfd, byte[] addr, ref int addrlen);

}
