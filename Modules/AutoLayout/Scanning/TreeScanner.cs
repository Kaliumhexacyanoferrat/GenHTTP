using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.AutoLayout.Scanning
{

    public static class TreeScanner
    {

        public static ValueTask<LayoutBuilder> ScanAsync(IResourceTree tree, params string[] indexNames) => ScanContainerAsync(tree, indexNames);

        private static async ValueTask<LayoutBuilder> ScanContainerAsync(IResourceContainer container, params string[] indexNames)
        {
            var layout = Layout.Create();

            await foreach (var node in container.GetNodes())
            {
                layout.Add(node.Name, await ScanContainerAsync(node));
            }

            await foreach (var resource in container.GetResources())
            {
                if (resource.Name is not null)
                {
                    var handler = HandlerResolver.Resolve(resource);

                    var fileName = Path.GetFileNameWithoutExtension(resource.Name).ToLowerInvariant();

                    var isIndex = indexNames.Any(n => n == fileName);

                    if (isIndex)
                    {
                        layout.Index(handler);
                    }
                    else
                    {
                        layout.Add(resource.Name, handler);
                    }
                }
            }

            return layout;
        }

    }

}
