using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Implemented by handlers that use the <see cref="MethodCollection" /> handler
/// for response generation. Allows logic interested in generically analyzing
/// such handlers (e.g. the OpenAPI concern) to stay loosely coupled.
/// </summary>
public interface IServiceMethodProvider
{

    /// <summary>
    /// Retrieves the methods that are provided by this handler.
    /// </summary>
    ValueTask<MethodCollection> GetMethodsAsync(IRequest request);

}
