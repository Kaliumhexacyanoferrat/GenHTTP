using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.DependencyInjection;

/// <summary>
/// Represents a dependency injection enabled handler that can be used to
/// serve requests.
/// </summary>
/// <remarks>
/// In contrast to regular handlers there is no handler preparation as
/// the lifecycle of the handler is managed by the dependency injection
/// container.
/// </remarks>
public interface IDependentHandler
{

    /// <summary>
    /// Handles the given request and returns a response, if applicable.
    /// </summary>
    /// <remarks>
    /// Not returning a response causes the server to respond with a not found
    /// response code.
    /// </remarks>
    /// <param name="request">The request to be handled</param>
    /// <returns>The response to be sent to the requesting client</returns>
    ValueTask<IResponse?> HandleAsync(IRequest request);

}
