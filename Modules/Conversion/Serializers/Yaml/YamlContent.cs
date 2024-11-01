using System.Text;

using GenHTTP.Api.Protocol;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

public sealed class YamlContent : IResponseContent
{
    private static readonly ISerializer Serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    private static readonly Encoding Encoding = Encoding.UTF8;

    #region Get-/Setters

    public ulong? Length => null;

    public object Data { get; }

    #endregion

    #region Initialization

    public YamlContent(object data)
    {
        Data = data;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public ValueTask WriteAsync(Stream target, uint bufferSize)
         => target.WriteAsync(Encoding.GetBytes(Serializer.Serialize(Data)));

    #endregion

}
