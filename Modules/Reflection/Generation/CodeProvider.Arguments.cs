using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderArgumentExtensions
{

    public static void AppendArguments(this StringBuilder sb, Operation operation)
    {
        if (operation.Arguments.Count > 0)
        {
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
            case OperationArgumentSource.Query:
                {
                    sb.AppendQueryArgument(argument, index, supportBodyArguments);
                    break;
                }
            default:
                throw new NotSupportedException();
        }
    }

    private static void AppendQueryArgument(this StringBuilder sb, OperationArgument argument, int index, bool supportBodyArguments)
    {
        sb.AppendLine($"        {argument.Type}? arg{index} = null;");
        sb.AppendLine();

        sb.AppendLine($"        if (request.Query.TryGetValue({CompilationUtil.GetSafeString(argument.Name)}, out var queryArg{index}))");
        sb.AppendLine("        {");
        sb.AppendQueryArgumentAssignment(argument, index, "query");
        sb.AppendLine("        }");

        if (supportBodyArguments)
        {
            sb.AppendLine($"        else if (bodyArgs?.TryGetValue({CompilationUtil.GetSafeString(argument.Name)}, out var bodyArg{index})");
            sb.AppendLine("        {");
            sb.AppendQueryArgumentAssignment(argument, index, "body");
            sb.AppendLine("        }");
        }

        // todo body args
    }

    private static void AppendQueryArgumentAssignment(this StringBuilder sb, OperationArgument argument, int index, string readFrom)
    {
        var sourceName = $"{readFrom}Arg{index}";

        // todo: high performance support for int.TryParse etc.

        if (argument.Type == typeof(string))
        {
            sb.AppendLine($"            arg{index} = {sourceName};");
        }
        else
        {
            // todo: try and bad format exception
            sb.AppendLine($"            arg{index} = ({argument.Type}?)registry.Formatting.Read({sourceName}, typeof({argument.Type}));");
        }
    }


}
