using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using GenHTTP.Modules.IO.Tracking;

using Cottle;

namespace GenHTTP.Modules.Pages.Rendering
{

    public class TemplateRenderer
    {
        private IDocument? _Document = null;

        public ChangeTrackingResource Template { get; }

        public TemplateRenderer(ChangeTrackingResource template)
        {
            Template = template;
        }

        public async ValueTask<string> RenderAsync(IReadOnlyDictionary<Value, Value> model)
        {
            if ((_Document == null) || await Template.HasChanged())
            {
                using var reader = new StreamReader(await Template.GetContentAsync());

                _Document = Document.CreateDefault(reader).DocumentOrThrow;
            }

            return _Document.Render(Context.CreateBuiltin(model));
        }

    }

}
