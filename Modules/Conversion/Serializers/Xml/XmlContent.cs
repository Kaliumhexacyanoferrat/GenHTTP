using System.Xml.Serialization;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers.Xml;

public sealed class XmlContent : IResponseContent
{

    #region Initialization

    public XmlContent(object data)
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
        var serializer = new XmlSerializer(Data.GetType());
        serializer.Serialize(target, Data);

        return new ValueTask();
    }

    #endregion

}
