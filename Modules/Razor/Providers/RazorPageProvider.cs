using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Basics.Rendering;

namespace GenHTTP.Modules.Razor.Providers
{

    public sealed class RazorPageProvider<T> : IHandler where T : PageModel
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        public ModelProvider<T> ModelProvider { get; }

        public IRenderer<T> Renderer { get; }

        public ContentInfo PageInfo { get; }

        #endregion

        #region Initialization

        public RazorPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo,
                                 List<Assembly> additionalReferences, List<string> additionalUsings)
        {
            Parent = parent;

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
