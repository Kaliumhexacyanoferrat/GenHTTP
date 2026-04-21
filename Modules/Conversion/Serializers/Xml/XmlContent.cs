using System.Xml.Serialization;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Xml;

public sealed class XmlContent : IResponseContent
{
    private static readonly ReadOnlyMemory<byte> ContentType = "application/xml"u8.ToArray();

    #region Initialization

    public XmlContent(object data)
    {
        Data = data;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ReadOnlyMemory<byte> Type => ContentType;

    private object Data { get; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var target = sink.Stream;

        var serializer = new XmlSerializer(Data.GetType());

        serializer.Serialize(target, Data);

        return new ValueTask();
    }

    #endregion

}
