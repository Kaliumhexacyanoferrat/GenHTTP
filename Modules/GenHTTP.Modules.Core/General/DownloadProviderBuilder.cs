using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProviderBuilder : IHandlerBuilder
    {
        private IResourceProvider? _ResourceProvider;
        private ContentType? _ContentType;

        #region Functionality

        public DownloadProviderBuilder Resource(IResourceProvider resource)
        {
            _ResourceProvider = resource;
            return this;
        }

        public DownloadProviderBuilder Type(ContentType contentType)
        {
            _ContentType = contentType;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_ResourceProvider == null)
            {
                throw new BuilderMissingPropertyException("Resource Provider");
            }

            if (_ContentType == null)
            {
                throw new BuilderMissingPropertyException("Content Type");
            }

            return new DownloadProvider(parent, _ResourceProvider, new FlexibleContentType((ContentType)_ContentType));
        }

        #endregion

    }

}
