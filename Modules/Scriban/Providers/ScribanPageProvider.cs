using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Basics.Rendering;

namespace GenHTTP.Modules.Scriban.Providers
{

    public sealed class ScribanPageProvider<T> : IHandler where T : class, IModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public ModelProvider<T> ModelProvider { get; }

        public IRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public ScribanPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo)
        {
            Parent = parent;

            ModelProvider = modelProvider;
            PageInfo = pageInfo;

            Renderer = ModScriban.Template<T>(templateProvider).Build();
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var model = await ModelProvider(request, this).ConfigureAwait(false);

            var content = new RenderedContent<T>(Renderer, model, PageInfo);

            return request.Respond()
                          .Content(content)
                          .Type(ContentType.TextHtml)
                          .Build();
        }

        public ValueTask PrepareAsync() => Renderer.PrepareAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
