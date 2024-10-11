using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers;

public sealed class SerializationBuilder : IBuilder<SerializationRegistry>
{

    private readonly Dictionary<FlexibleContentType, ISerializationFormat> _Registry = new();
    private FlexibleContentType? _Default;

    #region Functionality

    public SerializationBuilder Default(ContentType contentType) => Default(FlexibleContentType.Get(contentType));

    public SerializationBuilder Default(FlexibleContentType contentType)
    {
        _Default = contentType;
        return this;
    }

    public SerializationBuilder Add(ContentType contentType, ISerializationFormat format) => Add(FlexibleContentType.Get(contentType), format);

    public SerializationBuilder Add(FlexibleContentType contentType, ISerializationFormat format)
    {
        _Registry[contentType] = format;
        return this;
    }

    public SerializationRegistry Build() => new(_Default ?? throw new BuilderMissingPropertyException("default"), _Registry);

    #endregion

}
