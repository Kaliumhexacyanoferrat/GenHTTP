using System.Collections.Generic;
using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.Razor.Providers
{

    public sealed class RazorPageProvider<T> : PageProvider<T> where T : class, IModel
    {
        private readonly IRenderer<T> _Renderer;

        public override IRenderer<T> Renderer => _Renderer;

        public RazorPageProvider(IHandler parent, IResource templateProvider, ModelProvider<T> modelProvider, ContentInfo pageInfo,
                                 List<Assembly> additionalReferences, List<string> additionalUsings) : base(parent, modelProvider, pageInfo)
        {
            _Renderer = ModRazor.Template<T>(templateProvider)
                                .AddAssemblyReferences(additionalReferences)
                                .AddUsings(additionalUsings)
                                .Build();
        }

    }

}
