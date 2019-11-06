using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{

    /// <summary>
    /// Provides a model for the given request.
    /// </summary>
    /// <typeparam name="T">The type of model to be returned</typeparam>
    /// <param name="request">The current request</param>
    /// <returns>The newly created model instance</returns>
    public delegate T ModelProvider<out T>(IRequest request);

}
