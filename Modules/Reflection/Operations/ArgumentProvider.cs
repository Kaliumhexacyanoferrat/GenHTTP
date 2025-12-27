using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Conversion;

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

    public static Match? MatchPath(Operation operation, IRequest request)
    {
        Match? pathArguments = null;
        
        if (!operation.Path.IsIndex)
        {
            pathArguments = operation.Path.Matcher.Match(request.Target.GetRemaining().ToString());

            var matchedPath = WebPath.FromString(pathArguments.Value);

            foreach (var _ in matchedPath.Parts) request.Target.Advance();
        }

        return pathArguments;
    }

    public static object? GetPathArgument(string name, Type type, Match? matchedPath, MethodRegistry registry)
    {
        if (matchedPath != null)
        {
            var sourceArgument = matchedPath.Groups[name];

            if (sourceArgument.Success)
            {
                return sourceArgument.Value.ConvertTo(type, registry.Formatting);
            }
        }

        return null;
    }

    public static async ValueTask<object?> GetBodyArgumentAsync(IRequest request, string name, Type type, MethodRegistry registry)
    {
        if (request.Content == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, $"Argument '{name}' is expected to be read from the request body but the request does not contain any payload");
        }

        object? result = null;

        using var reader = new StreamReader(request.Content, leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        if (!string.IsNullOrWhiteSpace(body))
        {
            result = body.ConvertTo(type, registry.Formatting);
        }

        if (request.Content.CanSeek)
        {
            request.Content.Seek(0, SeekOrigin.Begin);
        }

        return result;
    }

    public static object? GetQueryArgument(IRequest request, Dictionary<string, string>? formArguments, OperationArgument argument, MethodRegistry registry)
    {
        if (request.Query.TryGetValue(argument.Name, out var queryValue))
        {
            return queryValue.ConvertTo(argument.Type, registry.Formatting);
        }

        if (formArguments is not null)
        {
            if (formArguments.TryGetValue(argument.Name, out var bodyValue))
            {
                return bodyValue.ConvertTo(argument.Type, registry.Formatting);
            }
        }

        return null;
    }

    public static async ValueTask<object?> GetContentAsync(IRequest request, OperationArgument argument, MethodRegistry registry)
    {
        var deserializer = registry.Serialization.GetDeserialization(request);

        if (deserializer is null)
        {
            throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
        }

        if (request.Content is null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
        }

        try
        {
            return await deserializer.DeserializeAsync(request.Content, argument.Type);
        }
        catch (Exception e)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Failed to deserialize request body", e);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Stream GetStream(IRequest request)
    {
        if (request.Content == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
        }

        return request.Content;
    }
    
}
