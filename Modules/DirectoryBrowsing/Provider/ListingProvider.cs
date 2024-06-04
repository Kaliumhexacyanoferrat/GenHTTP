using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;


namespace GenHTTP.Modules.DirectoryBrowsing.Provider
{

    public sealed class ListingProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResourceContainer Container { get; }

        #endregion

        #region Initialization

        public ListingProvider(IHandler parent, IResourceContainer container)
        {
            Parent = parent;
            Container = container;
        }

        #endregion

        #region Functionality

        public ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            // todo
            return new();

            /*var model = new ListingModel(request, this, Container, !request.Target.Ended);

            var renderer = new ListingRenderer();

            var templateModel = new TemplateModel(request, this, GetPageInfo(request), null, await renderer.RenderAsync(model));

            return (await this.GetPageAsync(request, templateModel)).Build();*/
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        #endregion

    }

}
