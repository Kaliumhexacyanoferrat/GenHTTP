using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapRouterBuilder : RouterBuilderBase<SitemapRouterBuilder>
    {
        private IRouter? _Content;

        #region Functionality

        public SitemapRouterBuilder Content(IRouter router)
        {
            _Content = router;
            return this;
        }

        public override IRouter Build()
        {
            var content = _Content ?? throw new BuilderMissingPropertyException("content");

            return new SitemapRouter(content, _Template, _ErrorHandler);
        }

        #endregion

    }

}
