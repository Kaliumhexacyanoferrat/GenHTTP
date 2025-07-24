using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DependencyInjection;

/// <summary>
/// Represents a dependency injection enabled concern that can be used to
/// serve requests.
/// </summary>
/// <remarks>
/// In contrast to regular concerns there is no concern or content preparation as
/// the lifecycle of the handler is managed by the dependency injection
/// container.
/// </remarks>
public interface IDependentConcern
{

    /// <summary>
    /// Handles the given request and returns a response, if applicable.
    /// </summary>
    /// <remarks>
    /// Not returning a response causes the server to respond with a not found
    /// response code.
    /// </remarks>
    /// <param name="request">The request to be handled</param>
    /// <param name="content">The (initialized) content to be served by the concern</param>
    /// <returns>The response to be sent to the requesting client</returns>
    ValueTask<IResponse?> HandleAsync(IHandler content, IRequest request);

}
