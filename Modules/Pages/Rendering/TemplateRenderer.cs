using Cottle;
using GenHTTP.Modules.IO.Tracking;

namespace GenHTTP.Modules.Pages.Rendering;

public class TemplateRenderer
{
    private IDocument? _document;

    public TemplateRenderer(ChangeTrackingResource template)
    {
        Template = template;
    }

    public ChangeTrackingResource Template { get; }

    /// <summary>
    /// Renders the template with the given model.
    /// </summary>
    /// <param name="model">The model to be used for rendering</param>
    /// <returns>The generated response</returns>
    public async ValueTask<string> RenderAsync(IReadOnlyDictionary<Value, Value> model)
    {
        if (_document == null || await Template.HasChanged())
        {
            using var reader = new StreamReader(await Template.GetContentAsync());

            _document = Document.CreateDefault(reader).DocumentOrThrow;
        }

        return _document.Render(Context.CreateBuiltin(model));
    }
}
