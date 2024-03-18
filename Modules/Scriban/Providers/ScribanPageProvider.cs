using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.Scriban.Providers
{

    public sealed class ScribanPageProvider<T> : PageProvider<T> where T : class, IModel
    {
        private readonly IRenderer<T> _Renderer;

        public override IRenderer<T> Renderer => _Renderer;

        public ScribanPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo, PageAdditions? additions) : base(parent, modelProvider, pageInfo, additions)
        {
            _Renderer = ModScriban.Template<T>(templateProvider)
                                  .Build();
        }

    }

}
