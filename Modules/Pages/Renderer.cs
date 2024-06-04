using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Pages.Rendering;

namespace GenHTTP.Modules.Pages
{

    public static class Renderer
    {
        private static readonly ServerRenderer _ServerRenderer = new();

        public static ServerRenderer Server { get => _ServerRenderer; }

        public static TemplateRenderer From(IResource template) => new(template.Track());

    }

}
