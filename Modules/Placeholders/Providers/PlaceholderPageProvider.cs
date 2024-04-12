using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PlaceholderPageProvider<T> : IHandler where T : class, IModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public ContentInfo PageInfo { get; }

        public PageAdditions? Additions { get; }

        public ResponseModifications? Modifications { get; }

        #endregion

        #region Initialization

        public PlaceholderPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo, PageAdditions? additions, ResponseModifications? modifications)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;

            PageInfo = pageInfo;
            Additions = additions;
            Modifications = modifications;
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var renderer = new PlaceholderRender<T>(TemplateProvider, null);

            var model = await ModelProvider(request, this);

            var content = await renderer.RenderAsync(model).ConfigureAwait(false);

            var templateModel = new TemplateModel(request, this, PageInfo, Additions, content);

            var page = await this.GetPageAsync(request, templateModel).ConfigureAwait(false);

            if (Modifications != null)
            {
                Modifications.Apply(page);
            }

            return page.Build();
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
