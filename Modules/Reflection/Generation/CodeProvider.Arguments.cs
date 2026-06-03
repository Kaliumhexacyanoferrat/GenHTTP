using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderArgumentExtensions
{

    public static void AppendArguments(this StringBuilder sb, Operation operation, StringBuilder declarations)
    {
        if (operation.Arguments.Count > 0)
        {
            var index = 0;

            foreach (var arg in operation.Arguments)
            {
                sb.AppendArgument(arg.Value, ++index, declarations);
                sb.AppendLine();
            }
        }
    }

    private static void AppendArgument(this StringBuilder sb, OperationArgument argument, int index, StringBuilder declarations)
    {
        switch (argument.Source)
        {
            case OperationArgumentSource.Path:
                {
                    sb.AppendPathArgument(argument, index, declarations);
                    break;
                }
            case OperationArgumentSource.Query:
                {
                    sb.AppendQueryArgument(argument, index, declarations);
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

    private static void AppendQueryArgument(this StringBuilder sb, OperationArgument argument, int index, StringBuilder declarations)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        declarations.AppendDeclaration(argument, index);

        sb.AppendLine($"        {safeType}? arg{index} = null;");
        sb.AppendLine();

        sb.AppendLine($"        var queryArg{index} = request.Header.Query.GetEntry(QueryArg{index}Name);");

        sb.AppendLine($"        if (queryArg{index} != null)");
        sb.AppendArgumentAssignment(argument, index, "query");
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
        sb.AppendLine("        var content = request.GetBody(HeaderAccess.Release) ?? throw new ProviderException(ResponseStatus.BadRequest, \"Request body expected\");");
        sb.AppendLine();
        sb.AppendLine($"        {safeType}? arg{index} = null;");
        sb.AppendLine();
        sb.AppendLine("        try");
        sb.AppendLine("        {");
        sb.AppendLine($"            arg{index} = ({safeType}?)await deserializer.DeserializeAsync(content.AsStream(), typeof({safeType}));");
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

        sb.AppendLine($"        {safeType}? arg{index} = ({safeType}?)await ArgumentProvider.GetBodyArgumentAsync(request, {safeName}, typeof({safeType}), registry);");
        sb.AppendLine();
    }

    private static void AppendPathArgument(this StringBuilder sb, OperationArgument argument, int index, StringBuilder declarations)
    {
        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        declarations.AppendDeclaration(argument, index);

        sb.AppendLine($"        {safeType}? arg{index} = null;");
        sb.AppendLine();

        sb.AppendLine($"        if (routingMatch.PathArguments?.TryGetValue(PathArg{index}Name, out var pathArg{index}) ?? false)");
        sb.AppendArgumentAssignment(argument, index, "path");
    }

    private static void AppendArgumentAssignment(this StringBuilder sb, OperationArgument argument, int index, string readFrom)
    {
        sb.AppendLine("        {");
        sb.AppendLine($"            if (!{readFrom}Arg{index}.Value.Bytes.IsEmpty)");
        sb.AppendLine("            {");

        var sourceName = $"{readFrom}Arg{index}";

        var safeType = CompilationUtil.GetQualifiedName(argument.Type, false);

        sb.AppendLine($"                arg{index} = ({safeType}?)registry.Formatting.Read({sourceName}.Value, typeof({safeType}));");

        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private static void AppendDeclaration(this StringBuilder sb, OperationArgument argument, int index)
    {
        var source = argument.Source switch
        {
            OperationArgumentSource.Query => "Query",
            OperationArgumentSource.Path => "Path",
            _ => throw new InvalidOperationException($"Unsupported declaration type '{argument.Source}'")
        };

        sb.AppendLine($"    private static ByteString {source}Arg{index}Name = new({CompilationUtil.GetSafeString(argument.Name)});");
    }

}
