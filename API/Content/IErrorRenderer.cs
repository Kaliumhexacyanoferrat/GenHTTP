using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Classes implementing this interface will be queried to render
    /// errors that occur when handling requests.
    /// </summary>
    public interface IErrorRenderer : IRenderer<ErrorModel>
    {

    }

}
