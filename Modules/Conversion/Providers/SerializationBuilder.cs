using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Providers
{

    public class SerializationBuilder : IBuilder<SerializationRegistry>
    {
        private FlexibleContentType? _Default = null;

        private readonly Dictionary<FlexibleContentType, ISerializationFormat> _Registry = new Dictionary<FlexibleContentType, ISerializationFormat>();

        #region Functionality

        public SerializationBuilder Default(ContentType contentType) => Default(new FlexibleContentType(contentType));

        public SerializationBuilder Default(FlexibleContentType contentType)
        {
            _Default = contentType;
            return this;
        }

        public SerializationBuilder Add(ContentType contentType, ISerializationFormat format) => Add(new FlexibleContentType(contentType), format);

        public SerializationBuilder Add(FlexibleContentType contentType, ISerializationFormat format)
        {
            _Registry[contentType] = format;
            return this;
        }

        public SerializationRegistry Build()
        {
            return new SerializationRegistry(_Default ?? throw new BuilderMissingPropertyException("default"), _Registry);
        }

        #endregion

    }

}
