using System.Collections.Generic;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Providers
{

    /// <summary>
    /// Registers formats that can be used to serialize and
    /// deserialize objects sent to or received from a 
    /// service oriented handler.
    /// </summary>
    public class SerializationRegistry
    {

        #region Get-/Setters

        private FlexibleContentType Default { get; }

        private Dictionary<FlexibleContentType, ISerializationFormat> Formats { get; }

        #endregion

        #region Initialization

        public SerializationRegistry(FlexibleContentType defaultType,
                                     Dictionary<FlexibleContentType, ISerializationFormat> formats)
        {
            Default = defaultType;
            Formats = formats;
        }

        #endregion

        #region Functionality

        public ISerializationFormat? GetDeserialization(IRequest request)
        {
            if (request.Headers.TryGetValue("Content-Type", out string requested))
            {
                return GetFormat(new FlexibleContentType(requested));
            }

            return GetFormat(Default);
        }

        public ISerializationFormat? GetSerialization(IRequest request)
        {
            if (request.Headers.TryGetValue("Accept", out string accepted))
            {
                return GetFormat(new FlexibleContentType(accepted)) ?? GetFormat(Default);
            }

            return GetFormat(Default);
        }

        private ISerializationFormat? GetFormat(FlexibleContentType contentType)
        {
            return Formats.ContainsKey(contentType) ? Formats[contentType] : null;
        }

        #endregion

    }

}
