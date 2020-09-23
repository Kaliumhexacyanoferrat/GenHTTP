using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Bundles
{

    public class BundleBuilder : IHandlerBuilder<BundleBuilder>
    {
        private readonly List<IResourceProvider> _Items = new List<IResourceProvider>();

        private FlexibleContentType _ContentType = new FlexibleContentType(Api.Protocol.ContentType.ApplicationForceDownload);

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public BundleBuilder Add(IResourceProvider resource)
        {
            _Items.Add(resource);
            return this;
        }

        public BundleBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public BundleBuilder ContentType(ContentType type) => ContentType(new FlexibleContentType(type));

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
