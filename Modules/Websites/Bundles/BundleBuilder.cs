using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Bundles
{

    public sealed class BundleBuilder : IHandlerBuilder<BundleBuilder>
    {
        private readonly List<IResource> _Items = new();

        private FlexibleContentType _ContentType = FlexibleContentType.Get(Api.Protocol.ContentType.ApplicationForceDownload);

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public BundleBuilder Add(IResource resource)
        {
            _Items.Add(resource);
            return this;
        }

        public BundleBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public BundleBuilder ContentType(ContentType type, string? charset = null) => ContentType(FlexibleContentType.Get(type, charset));

        public BundleBuilder ContentType(FlexibleContentType type)
        {
            _ContentType = type;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            return Concerns.Chain(parent, _Concerns, (p) => new BundleProvider(parent, _Items, _ContentType));
        }

        #endregion

    }

}
