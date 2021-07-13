using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Modules.Pages.Combined
{
    
    public record ContentFragment
    (

        IRenderer<IModel> Renderer,

        IModel Model

    );

}
