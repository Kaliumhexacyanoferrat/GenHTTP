using System.Buffers;
using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Pooling;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GenHTTP.Modules.Conversion.Serializers.Yaml;

public sealed class YamlContent : PooledContent<YamlContent>, IResponseContent
{
    private static readonly ISerializer Serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();

    private static readonly Encoding TextEncoding = System.Text.Encoding.UTF8;

    private static readonly object NoData = new();

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.ApplicationYaml;

    public ReadOnlyMemory<byte>? Encoding => null;

    public object Data { get; private set; }

    #endregion

    #region Initialization

    public YamlContent(object data)
    {
        Data = data;
    }

    internal static YamlContent Rent(object data)
    {
        var content = FromPool(static () => new YamlContent(NoData));

        content.Data = data;

        return content;
    }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public ValueTask WriteAsync(Stream target, uint bufferSize)
        => target.WriteAsync(TextEncoding.GetBytes(Serializer.Serialize(Data)));

    public ValueTask WriteAsync(IResponseSink sink)
    {
        sink.Writer.Write(TextEncoding.GetBytes(Serializer.Serialize(Data)));
        return default;
    }

    protected override void Reset() => Data = NoData;

    #endregion

}
