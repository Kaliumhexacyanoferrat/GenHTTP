using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.TreeViewer.Provider
{

    public class TreeViewHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceTree Tree { get; }

        public ITreeRenderer Renderer { get; }

        private IHandler Content { get; set; }

        #endregion

        #region Initialization

        public TreeViewHandler(IHandler parent, IResourceTree tree, ITreeRenderer renderer)
        {
            Parent = parent;

            Tree = tree;

            Renderer = renderer;

            Content = Layout.Create()
                            .Build(this);
        }

        public async ValueTask ReloadAsync()
        {
            Content = (await ScanContainerAsync(Tree)).Build(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => ReloadAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Content.HandleAsync(request);

        private async ValueTask<LayoutBuilder> ScanContainerAsync(IResourceContainer container)
        {
            var layout = Layout.Create();

            var index = await Renderer.GetIndexAsync(container);

            if (index != null)
            {
                layout.Index(index);
            }

            await foreach (var node in container.GetNodes())
            {
                layout.Add(node.Name.ToLowerInvariant(), await ScanContainerAsync(node));
            }

            await foreach (var resource in container.GetResources())
            {
                var view = await Renderer.GetHandlerAsync(container, resource);

                if (view != null)
                {
                    layout.Add(view.Path, view.Handler);
                }
            }

            return layout;
        }

        #endregion

    }

}
