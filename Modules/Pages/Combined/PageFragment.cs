using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Pages.Combined
{
    
    /// <summary>
    /// A dynamically rendered page element to be evaluated on request.
    /// </summary>
    public record PageFragment
    (

        IRenderer<IModel> Renderer,

        ModelProvider<IModel> Model

    );

}
