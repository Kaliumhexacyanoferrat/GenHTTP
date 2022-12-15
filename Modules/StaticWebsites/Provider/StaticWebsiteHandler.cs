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
        private static readonly string[] INDEX_FILES = new[] { "index.html", "index.htm" };

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

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            if (request.Target.Path.TrailingSlash)
            {
                var (node, _) = await Tree.Find(request.Target).ConfigureAwait(false);

                if (node != null)
                {
                    foreach (var indexFile in INDEX_FILES)
                    {
                        IResource? file;

                        if ((file = await node.TryGetResourceAsync(indexFile)) != null)
                        {
                            return await Content.From(file)
                                                .Build(this)
                                                .HandleAsync(request);
                        }
                    }
                }

                return null;
            }
            else
            {
                if ((Sitemap != null) && await ServeInternal(request.Target, Sitemaps.Sitemap.FILE_NAME))
                {
                    return await Sitemap.HandleAsync(request);
                }

                if ((Robots != null) && await ServeInternal(request.Target, BotInstructions.FILE_NAME))
                {
                    return await Robots.HandleAsync(request);
                }

                return await Resources.HandleAsync(request);
            }
        }

        private async ValueTask<bool> ServeInternal(RoutingTarget target, string fileName)
        {
            if (target.Last && target.Current?.Value == fileName)
            {
                // do not serve the file if it explicitly exists in the tree
                var (_, file) = await Tree.Find(target);

                return (file == null);
            }

            return false;
        }

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            return Tree.GetContent(request, this, async (node, path, children) =>
            {
                foreach (var indexFile in INDEX_FILES)
                {
                    if (await node.TryGetResourceAsync(indexFile) != null)
                    {
                        return new ContentElement(path, new ContentInfo($"Index of {path}", null), ContentType.TextHtml, children.ToEnumerable());
                    }
                }

                return new ContentElement(path, ContentInfo.Empty, ContentType.ApplicationForceDownload, children.ToEnumerable());
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
