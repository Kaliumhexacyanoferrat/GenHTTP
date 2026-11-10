using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

using ProtoBuf;

namespace GenHTTP.Modules.Protobuf.Providers;

public sealed class ProtobufContent : PooledContent<ProtobufContent>, IResponseContent
{
    private static readonly object NoData = new();

    private object _data;

    #region Initialization

    public ProtobufContent(object data)
    {
        _data = data;
    }

    internal static ProtobufContent Rent(object data)
    {
        var content = FromPool(static () => new ProtobufContent(NoData));

        content._data = data;

        return content;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.ApplicationProtobuf;

    public ReadOnlyMemory<byte>? Encoding => null;

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_data.GetHashCode());

    public ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        Serializer.Serialize(target, _data);

        return default;
    }

    public ValueTask WriteAsync(IResponseSink sink)
    {
        Serializer.Serialize(sink.Writer, _data);

        return default;
    }

    protected override void Reset() => _data = NoData;

    #endregion

}
