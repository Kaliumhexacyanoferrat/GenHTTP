using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.IO.Tracking;

using Cottle;

namespace GenHTTP.Modules.Pages.Rendering
{

    public sealed class ServerRenderer
    {
        private static readonly ChangeTrackingResource _ServerTemplate = Resource.FromAssembly("ServerPage.html").BuildWithTracking();

        private readonly TemplateRenderer _TemplateRender;

        public ServerRenderer()
        {
            _TemplateRender = new TemplateRenderer(_ServerTemplate);
        }

        public async ValueTask<string> RenderAsync(string title, string content)
        {
            return await _TemplateRender.RenderAsync(new Dictionary<Value, Value>()
            {
                ["title"] = title,
                ["content"] = content
            });
        }

        public ValueTask<string> RenderAsync(string title, Exception error, bool developmentMode)
            => RenderAsync(title, developmentMode ? error.ToString() : error.Message);

    }

}
