using System.Threading.Tasks;

using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Handlers implementing this interface will be queried to render
    /// pages into a template.
    /// </summary>
    public interface IPageRenderer
    {

        /// <summary>
        /// Renders the given model into a web page to be served to the client.
        /// </summary>
        /// <param name="model">The model to be rendered</param>
        /// <returns>The response to be returned to the client</returns>
        ValueTask<IResponseBuilder> RenderAsync(TemplateModel model);

    }

}
