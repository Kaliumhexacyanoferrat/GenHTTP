using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Pages.Combined
{
    
    public record PageFragment
    (

        IRenderer<IModel> Renderer,

        ModelProvider<IModel> Model

    );

}
