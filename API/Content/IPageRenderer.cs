using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Handlers implementing this interface will be queried to render
    /// pages into a template.
    /// </summary>
    public interface IPageRenderer : IRenderer<TemplateModel>
    {


    }

}
