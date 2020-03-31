using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Sitemaps
{

    public class SitemapRouter : RouterBase
    {
        private const string FILENAME = "sitemap.xml";

        #region Get-/Setters

        private SitemapProvider Sitemap { get; }

        #endregion

        #region Initialization

        public SitemapRouter(IRouter content, IRenderer<TemplateModel>? template, IContentProvider? errorHandler) : base(template, errorHandler)
        {
            Sitemap = new SitemapProvider(content, null);
        }

        #endregion

        #region Functionality

        public override void HandleContext(IEditableRoutingContext current)
        {
            if (current.ScopedPath == $"/{FILENAME}")
            {
                current.RegisterContent(Sitemap);
            }
        }

        public override IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            yield return new ContentElement($"{basePath}{FILENAME}", "Sitemap", ContentType.TextXml, null);
        }

        public override string? Route(string path, int currentDepth) => Parent.Route(path, currentDepth);

        #endregion

    }

}
