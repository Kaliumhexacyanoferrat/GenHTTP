using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderArgumentExtensions
{

    public static void AppendArguments(this StringBuilder sb, Operation operation)
    {
        if (operation.Arguments.Count > 0)
        {
            sb.AppendLine("        var pathArgs = ArgumentProvider.MatchPath(operation, request);");
            sb.AppendLine();
            
            var hasQueryArgs = operation.Arguments.Any(a => a.Value.Source == OperationArgumentSource.Query);

            var mayHasBody = operation.Configuration.SupportedMethods.Any(m => m != RequestMethod.Get && m != RequestMethod.Head);

            var supportBodyArguments = hasQueryArgs && mayHasBody;

            if (supportBodyArguments)
            {
                sb.AppendLine("        Dictionary<string, string>? bodyArgs = null;");
                sb.AppendLine();

                sb.AppendLine("        if (request.ContentType?.KnownType == ContentType.ApplicationWwwFormUrlEncoded)");
                sb.AppendLine("        {");
                sb.AppendLine("            bodyArgs = FormFormat.GetContent(request);");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            var index = 0;

            foreach (var arg in operation.Arguments)
            {
                sb.AppendArgument(arg.Value, ++index, supportBodyArguments);
                sb.AppendLine();
            }
        }
    }

    private static void AppendArgument(this StringBuilder sb, OperationArgument argument, int index, bool supportBodyArguments)
    {
        switch (argument.Source)
        {
            case OperationArgumentSource.Path:
                {
                    sb.AppendPathArgument(argument, index);
                    break;
                }
            case OperationArgumentSource.Query:
                {
                    sb.AppendQueryArgument(argument, index, supportBodyArguments);
                    break;
                }
            case OperationArgumentSource.Streamed:
                {
                    sb.AppendStreamArgument(index);
                    break;
                }
            case OperationArgumentSource.Content:
                {
                    sb.AppendContentArgument(argument, index);
                    break;
                }
            case OperationArgumentSource.Injected:
                {
                    sb.AppendInjectedArgument(argument, index);
                    break;
                }
            case OperationArgumentSource.Body:
                {
                    sb.AppendBodyArgument(argument, index);
                    break;
                }
            default:
                throw new NotSupportedException();
        }
    }

    private static void AppendQueryArgument(this StringBuilder sb, OperationArgument argument, int index, bool supportBodyArguments)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        sb.AppendLine($"        {safeType}? arg{index} = null;");
        sb.AppendLine();

        sb.AppendLine($"        if (request.Query.TryGetValue({CompilationUtil.GetSafeString(argument.Name)}, out var queryArg{index}))");
        sb.AppendLine("        {");
        sb.AppendQueryArgumentAssignment(argument, index, "query");
        sb.AppendLine("        }");

        if (supportBodyArguments)
        {
            sb.AppendLine($"        else if (bodyArgs?.TryGetValue({CompilationUtil.GetSafeString(argument.Name)}, out var bodyArg{index}) == true)");
            sb.AppendLine("        {");
            sb.AppendQueryArgumentAssignment(argument, index, "body");
            sb.AppendLine("        }");
        }
    }

    private static void AppendQueryArgumentAssignment(this StringBuilder sb, OperationArgument argument, int index, string readFrom)
    {
        var sourceName = $"{readFrom}Arg{index}";

        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        // todo: high performance support for int.TryParse etc.

        if (argument.Type == typeof(string))
        {
            sb.AppendLine($"            arg{index} = {sourceName};");
        }
        else
        {
            // todo: try and bad format exception
            sb.AppendLine($"            arg{index} = ({safeType}?)registry.Formatting.Read({sourceName}, typeof({safeType}));");
        }
    }

    private static void AppendStreamArgument(this StringBuilder sb, int index)
    {
        sb.AppendLine($"        var arg{index} = ArgumentProvider.GetStream(request);");
        sb.AppendLine();
    }

    private static void AppendContentArgument(this StringBuilder sb, OperationArgument argument, int index)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        sb.AppendLine("        var deserializer = registry.Serialization.GetDeserialization(request) ?? throw new ProviderException(ResponseStatus.UnsupportedMediaType, \"Requested format is not supported\");");
        sb.AppendLine();
        sb.AppendLine("        var content = request.Content ?? throw new ProviderException(ResponseStatus.BadRequest, \"Request body expected\");");
        sb.AppendLine();
        sb.AppendLine($"        {safeType}? arg{index} = null;");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            arg{index} = ({safeType}?)await deserializer.DeserializeAsync(content, typeof({safeType}));");
        sb.AppendLine("        }");
        sb.AppendLine("        catch (Exception e)");
        sb.AppendLine("        {");
        sb.AppendLine("            throw new ProviderException(ResponseStatus.BadRequest, \"Failed to deserialize request body\", e);");
        sb.AppendLine("        }");
        sb.AppendLine();
    }

    private static void AppendInjectedArgument(this StringBuilder sb, OperationArgument argument, int index)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        // todo: optimizations

        if (argument.Type == typeof(IRequest))
        {
            sb.AppendLine($"        var arg{index} = request;");
        }
        else if (argument.Type == typeof(IHandler))
        {
            sb.AppendLine($"        var arg{index} = handler;");
        }
        else
        {
            sb.AppendLine($"        {safeType}? arg{index} = null;");
            sb.AppendLine();

            sb.AppendLine("        foreach (var injector in registry.Injection)");
            sb.AppendLine("        {");
            sb.AppendLine($"            if (injector.Supports(request, typeof({safeType})))");
            sb.AppendLine("            {");
            sb.AppendLine($"                arg{index} = ({safeType})injector.GetValue(handler, request, typeof({safeType}));");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
        }
    }

    private static void AppendBodyArgument(this StringBuilder sb, OperationArgument argument, int index)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);
        var safeName = CompilationUtil.GetSafeString(argument.Name);
        
        // todo: reflection based
        
        sb.AppendLine($"        {safeType}? arg{index} = ({safeType}?)await ArgumentProvider.GetBodyArgumentAsync(request, {safeName}, typeof({safeType}), registry);");
        sb.AppendLine();
    }

    private static void AppendPathArgument(this StringBuilder sb, OperationArgument argument, int index)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);
        
        var safeName = CompilationUtil.GetSafeString(argument.Name);
        
        // todo: reflection based
        
        sb.AppendLine($"        {safeType}? arg{index} = ({safeType}?)ArgumentProvider.GetPathArgument({safeName}, typeof({safeType}), pathArgs, registry);");
        sb.AppendLine();
    }

}
