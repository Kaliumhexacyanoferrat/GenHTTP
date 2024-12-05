using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// A result returned by an interceptor.
/// </summary>
/// <param name="payload">The payload of the response</param>
public sealed class InterceptionResult(object? payload = null) : Result<object?>(payload);

/// <summary>
/// A piece of logic to be executed before the
/// actual method invocation. Triggered by methods
/// annotated with the <see cref="InterceptWithAttribute{T}" />
/// attribute.
/// </summary>
public interface IOperationInterceptor
{

    /// <summary>
    /// Invoked after the instance has been created to configure
    /// the interceptor with the originally used attribute. Allows
    /// the interceptor to read configuration data as needed.
    /// </summary>
    /// <param name="attribute">The original attribute instance on the method definition</param>
    void Configure(object attribute);

    /// <summary>
    /// Invoked on every operation call by the client.
    /// </summary>
    /// <param name="request">The request which caused this invocation</param>
    /// <param name="operation">The currently executed operation</param>
    /// <param name="arguments">The operation arguments as derived by the framework</param>
    /// <returns>If a result is returned, it will be converted into a response and the method is not invoked</returns>
    ValueTask<InterceptionResult?> InterceptAsync(IRequest request, Operation operation, IReadOnlyDictionary<string, object?> arguments);

}
