using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion;

namespace GenHTTP.Modules.Reflection.Operations;

public static class ArgumentProvider
{

    public static object? GetInjectedArgument(IRequest request, IHandler handler, OperationArgument argument, MethodExtensions extensions)
    {
        foreach (var injector in extensions.Injection)
        {
            if (injector.Supports(argument.Type))
            {
                return injector.GetValue(handler, request, argument.Type);
            }
        }

        return null;
    }

    public static object? GetPathArgument(OperationArgument argument, Match? matchedPath, MethodExtensions extensions)
    {
        if (matchedPath != null)
        {
            var sourceArgument = matchedPath.Groups[argument.Name];

            if (sourceArgument.Success)
            {
                return sourceArgument.Value.ConvertTo(argument.Type, extensions.Formatting);
            }
        }

        return null;
    }

    public static async ValueTask<object?> GetBodyArgumentAsync(IRequest request, OperationArgument argument, MethodExtensions extensions)
    {
        if (request.Content == null)
        {
            throw new ProviderException(ResponseStatus.BadRequest, $"Argument '{argument.Name}' is expected to be read from the request body but the request does not contain any payload");
        }

        object? result = null;

        using var reader = new StreamReader(request.Content, leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        if (!string.IsNullOrWhiteSpace(body))
        {
            result = body.ConvertTo(argument.Type, extensions.Formatting);
        }

        request.Content.Seek(0, SeekOrigin.Begin);

        return result;
    }

    public static object? GetQueryArgument(IRequest request, Dictionary<string, string>? formArguments, OperationArgument argument, MethodExtensions extensions)
    {
        if (request.Query.TryGetValue(argument.Name, out var queryValue))
        {
            return queryValue.ConvertTo(argument.Type, extensions.Formatting);
        }

        if (formArguments is not null)
        {
            if (formArguments.TryGetValue(argument.Name, out var bodyValue))
            {
                return bodyValue.ConvertTo(argument.Type, extensions.Formatting);
            }
        }

        return null;
    }

    public static async ValueTask<object?> GetContentAsync(IRequest request, OperationArgument argument, MethodExtensions extensions)
    {
        var deserializer = extensions.Serialization.GetDeserialization(request);

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

}
