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

    private object Data { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public ValueTask WriteAsync(Stream target, uint bufferSize)
    {
        Serializer.Serialize(target, Data);

        return new ValueTask();
    }

    #endregion

}
