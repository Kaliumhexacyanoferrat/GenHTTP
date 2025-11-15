using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Serializers;

public sealed class SerializationBuilder : IBuilder<SerializationRegistry>
{

    private readonly Dictionary<FlexibleContentType, ISerializationFormat> _registry = new();
    private FlexibleContentType? _default;

    #region Functionality

    public SerializationBuilder Default(ContentType contentType) => Default(FlexibleContentType.Get(contentType));

    public SerializationBuilder Default(FlexibleContentType contentType)
    {
        _default = contentType;
        return this;
    }

    public SerializationBuilder Add(ContentType contentType, ISerializationFormat format) => Add(FlexibleContentType.Get(contentType), format);

    public SerializationBuilder Add(FlexibleContentType contentType, ISerializationFormat format)
    {
        _registry[contentType] = format;
        return this;
    }

    public SerializationRegistry Build() => new(_default ?? throw new BuilderMissingPropertyException("default"), _registry);

    #endregion

}
