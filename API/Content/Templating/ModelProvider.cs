using System.Threading.Tasks;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content.Templating
{

    /// <summary>
    /// Provides a model for the given request.
    /// </summary>
    /// <typeparam name="T">The type of model to be returned</typeparam>
    /// <param name="request">The current request</param>
    /// <param name="handler">The handler responsible for this request</param>
    /// <returns>The newly created model instance</returns>
    public delegate ValueTask<T> ModelProvider<T>(IRequest request, IHandler handler);

}
