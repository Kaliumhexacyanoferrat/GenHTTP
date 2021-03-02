using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Api.Content
{

    /// <summary>
    /// Handlers implementing this interface will be queried to render
    /// errors that occur when handling requests.
    /// </summary>
    public interface IErrorHandler : IRenderer<ErrorModel>
    {

    }

}
