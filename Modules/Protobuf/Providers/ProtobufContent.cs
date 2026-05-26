using GenHTTP.Api.Protocol;

using ProtoBuf;

namespace GenHTTP.Modules.Protobuf.Providers;

public sealed class ProtobufContent : IResponseContent
{

    #region Initialization

    public ProtobufContent(object data)
    {
        Data = data;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.ApplicationProtobuf;

    public ReadOnlyMemory<byte>? Encoding => null;

    private object Data { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        Serializer.Serialize(target, Data);

        return default;
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        Serializer.Serialize(sink.Writer, Data);

        return default;
    }

    #endregion

}
