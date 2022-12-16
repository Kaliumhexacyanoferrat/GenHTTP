using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.AutoLayout.Scanning;
using GenHTTP.Modules.Layouting;

namespace GenHTTP.Modules.AutoLayout.Provider
{

    public class AutoLayoutHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceTree Tree { get; }

        public string[] IndexNames { get; }

        private IHandler Content { get; set; }

        #endregion

        #region Initialization

        public AutoLayoutHandler(IHandler parent, IResourceTree tree, string[] indexNames)
        {
            Parent = parent;

            Tree = tree;
            IndexNames = indexNames;

            Content = Layout.Create()
                            .Build(this);
        }

        public async ValueTask ReloadAsync()
        {
            Content = (await TreeScanner.ScanAsync(Tree, IndexNames)).Build(this);
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => ReloadAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Content.GetContentAsync(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Content.HandleAsync(request);

        #endregion

    }

}
