using System.Buffers;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Mcp.Protocol;

/// <summary>
/// A serialized JSON-RPC message sent as the body of a response.
/// </summary>
internal sealed class RpcContent : IResponseContent
{
    private readonly byte[] _content;

    #region Get-/Setters

    public ulong? Length => (ulong)_content.Length;

    public ContentType? Type => ContentType.ApplicationJson;

    public ReadOnlyMemory<byte>? Encoding => null;

    #endregion

    #region Initialization

    internal RpcContent(byte[] content)
    {
        _content = content;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync()
    {
        var hash = 14695981039346656037UL;

        foreach (var b in _content)
        {
            hash = (hash ^ b) * 1099511628211UL;
        }

        return new(hash);
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(_content);
        return default;
    }

    #endregion

}
