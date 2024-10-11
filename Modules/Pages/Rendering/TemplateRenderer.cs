using GenHTTP.Modules.IO.Tracking;

using Cottle;

namespace GenHTTP.Modules.Pages.Rendering;

public class TemplateRenderer
{
    private IDocument? _Document = null;

    public ChangeTrackingResource Template { get; }

    public TemplateRenderer(ChangeTrackingResource template)
    {
            Template = template;
        }

    /// <summary>
    /// Renders the template with the given model.
    /// </summary>
    /// <param name="model">The model to be used for rendering</param>
    /// <returns>The generated response</returns>
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
