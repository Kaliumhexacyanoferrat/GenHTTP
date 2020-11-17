using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public IResource TemplateProvider { get; }

        public ModelProvider<T> ModelProvider { get; }

        public IRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public RazorPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo,
                                 List<Assembly> additionalReferences, List<string> additionalUsings)
        {
            Parent = parent;

            TemplateProvider = templateProvider;
            ModelProvider = modelProvider;
            PageInfo = pageInfo;

            Renderer = ModRazor.Template<T>(templateProvider)
                               .AddAssemblyReferences(additionalReferences)
                               .AddUsings(additionalUsings)
                               .Build();
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var model = ModelProvider(request, this);

            var content = await Renderer.RenderAsync(model).ConfigureAwait(false);

            var templateModel = new TemplateModel(request, this, PageInfo, content);

            var page = await this.GetPageAsync(templateModel).ConfigureAwait(false);

            return page.Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, PageInfo, ContentType.TextHtml);

        #endregion

    }

}
