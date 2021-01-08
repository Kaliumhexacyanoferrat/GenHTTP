using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Robots;
using GenHTTP.Modules.Robots.Provider;
using GenHTTP.Modules.Sitemaps.Provider;

namespace GenHTTP.Modules.StaticWebsites.Provider
{

    public sealed class StaticWebsiteHandler : IHandler
    {
        private static readonly string[] INDEX_FILES = new[] { "index.html", "index.html" };

        #region Get-/Setters

        public IHandler Parent { get; }

        private IResourceTree Tree { get; }

        private IHandler Resources { get; }

        private IHandler? Sitemap { get; }

        private IHandler? Robots { get; }

        #endregion

        #region Initialization

        public StaticWebsiteHandler(IHandler parent, IResourceTree tree, SitemapProviderBuilder? sitemap, RobotsProviderBuilder? robots)
        {
            Parent = parent;
            Tree = tree;

            Resources = IO.Resources.From(tree)
                                    .Build(this);

            if (sitemap != null)
            {
                Sitemap = sitemap.Build(this);
            }

            if (robots != null)
            {
                Robots = robots.Build(this);
            }
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.Target.Path.TrailingSlash)
            {
                var (node, _) = Tree.Find(request.Target);

                if (node != null)
                {
                    foreach (var indexFile in INDEX_FILES)
                    {
                        if (node.TryGetResource(indexFile, out var file))
                        {
                            return Content.From(file)
                                          .Build(this)
                                          .HandleAsync(request);
                        }
                    }
                }

                return new ValueTask<IResponse?>();
            }
            else
            {
                if ((Sitemap != null) && ServeInternal(request.Target, Sitemaps.Sitemap.FILE_NAME))
                {
                    return Sitemap.HandleAsync(request);
                }

                if ((Robots != null) && ServeInternal(request.Target, BotInstructions.FILE_NAME))
                {
                    return Robots.HandleAsync(request);
                }

                return Resources.HandleAsync(request);
            }
        }

        private bool ServeInternal(RoutingTarget target, string fileName)
        {
            if (target.Last && target.Current == fileName)
            {
                // do not serve the file if it explicitly exists in the tree
                var (_, file) = Tree.Find(target);

                return (file == null);
            }

            return false;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Tree.GetContent(request, this, (node, path, children) =>
            {
                if (INDEX_FILES.Any(file => node.TryGetResource(file, out var _)))
                {
                    return new ContentElement(path, new ContentInfo($"Index of {path}", null), ContentType.TextHtml, children);
                }

                return new ContentElement(path, ContentInfo.Empty, ContentType.ApplicationForceDownload, children);
            });
        }

        public async ValueTask PrepareAsync()
        {
            await Resources.PrepareAsync();

            if (Sitemap != null)
            {
                await Sitemap.PrepareAsync();
            }

            if (Robots != null)
            {
                await Robots.PrepareAsync();
            }
        }

        #endregion

    }

}
