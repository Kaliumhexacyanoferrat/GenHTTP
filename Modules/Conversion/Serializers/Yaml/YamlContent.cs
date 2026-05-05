using System.Buffers;
using System.Text;

using GenHTTP.Api.Protocol;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

public sealed class YamlContent : IResponseContent
{
    private static readonly ReadOnlyMemory<byte> ContentType = "application/yaml"u8.ToArray();
    
    private static readonly ISerializer Serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    private static readonly Encoding Encoding = Encoding.UTF8;

    #region Get-/Setters

    public ulong? Length => null;

    public ReadOnlyMemory<byte> Type => ContentType;

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

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(Encoding.GetBytes(Serializer.Serialize(Data))); // ToDo: custom IEmitter
        return default;
    }
    
    #endregion

}
