using System.Runtime.CompilerServices;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection.Routing;

namespace GenHTTP.Modules.Reflection.Operations;

public static class ArgumentProvider
{

    public static object? GetInjectedArgument(IRequest request, IHandler handler, OperationArgument argument, MethodRegistry registry)
    {
        foreach (var injector in registry.Injection)
        {
            if (injector.Supports(request, argument.Type))
            {
                return injector.GetValue(handler, request, argument.Type);
            }
        }

        return null;
    }

    public static object? GetPathArgument(ArgumentName name, Type type, RoutingMatch match, MethodRegistry registry)
    {
        if (match.PathArguments?.TryGetValue(name, out var pathArgument) ?? false)
        {
            return pathArgument.ConvertTo(type, registry.Formatting);
        }

        return null;
    }

    public static async ValueTask<object?> GetBodyArgumentAsync(IRequest request, string name, Type type, MethodRegistry registry)
    {
        var content = request.GetBody();

        if (content == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, $"Argument '{name}' is expected to be read from the request body but the request does not contain any payload");
        }

        object? result = null;

        var buffer = await content.ReadToEndAsync();

        if (!buffer.IsEmpty)
        {
            result = buffer.ConvertTo(type, registry.Formatting);
        }

        return result;
    }

    public static object? GetQueryArgument(IRequest request, OperationArgument argument, MethodRegistry registry)
    {
        var queryValue = request.Header.Query.GetEntry(argument.Name.Value);

        if (queryValue is not null)
        {
            return queryValue.ConvertTo(argument.Type, registry.Formatting);
        }

        /* todo: remove first-class support for body args
        if (formArguments is not null)
        {
            if (formArguments.TryGetValue(argument.Name.ToString(), out var bodyValue))
            {
                return bodyValue.ConvertTo(argument.Type, registry.Formatting);
            }
        }
        */

        return null;
    }

    public static async ValueTask<object?> GetContentAsync(IRequest request, OperationArgument argument, MethodRegistry registry)
    {
        var deserializer = registry.Serialization.GetDeserialization(request);

        if (deserializer is null)
        {
            throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
        }

        var content = request.GetBody();

        if (content is null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
        }

        try
        {
            return await deserializer.DeserializeAsync(content.AsStream(), argument.Type);
        }
        catch (Exception e)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Failed to deserialize request body", e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Stream GetStream(IRequest request)
    {
        var content = request.GetBody();

        if (content == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
        }

        return content.AsStream();
    }

}
