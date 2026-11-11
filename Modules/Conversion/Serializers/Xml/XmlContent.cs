using System.Xml.Serialization;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.Conversion.Serializers.Xml;

public sealed class XmlContent : PooledContent<XmlContent>, IResponseContent
{
    private static readonly object NoData = new();

    private object _data;

    #region Initialization

    public XmlContent(object data)
    {
        _data = data;
    }

    internal static XmlContent Rent(object data)
    {
        var content = FromPool(static () => new XmlContent(NoData));

        content._data = data;

        return content;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.TextXml;

    public ReadOnlyMemory<byte>? Encoding => null;

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)_data.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink)
    {
        var target = sink.Stream;

        var serializer = new XmlSerializer(_data.GetType());

        serializer.Serialize(target, _data);

        return new ValueTask();
    }

    protected override void Reset() => _data = NoData;

    #endregion

}
