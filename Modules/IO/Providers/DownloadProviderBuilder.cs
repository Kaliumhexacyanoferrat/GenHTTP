using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.Core.General
{

    public class DownloadProviderBuilder : IHandlerBuilder<DownloadProviderBuilder>
    {
        private IResourceProvider? _ResourceProvider;
        private ContentType? _ContentType;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

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

        public DownloadProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
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

            return Concerns.Chain(parent, _Concerns, (p) => new DownloadProvider(p, _ResourceProvider, new FlexibleContentType((ContentType)_ContentType)));
        }

        #endregion

    }

}
