using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection;

public sealed class InterceptionResult(object? payload = null) : Result<object?>(payload);

public interface IOperationInterceptor
{

    void Configure(object attribute);

    ValueTask<InterceptionResult?> InterceptAsync(IRequest request, Operation operation, IReadOnlyDictionary<string, object?> arguments);

}
