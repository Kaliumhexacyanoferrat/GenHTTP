using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderInterceptionExtensions
{

    public static void AppendInterception(this StringBuilder sb, Operation operation, StringBuilder declarations)
    {
        if (operation.Interceptors.Count > 0)
        {
            sb.AppendLine($"        var interceptionArgs = new Dictionary<ByteString, object?>({operation.Arguments.Count})");
            sb.AppendLine("        {");

            int i = 1;

            foreach (var arg in operation.Arguments)
            {
                declarations.AppendLine($"    private static readonly ByteString InterceptorArg{i}Name = new({CompilationUtil.GetSafeString(arg.Value.Name)});");
                
                sb.Append($"            {{ InterceptorArg{i}Name, arg{i++} }}");

                if (i < operation.Arguments.Count - 1)
                {
                    sb.Append(',');
                }

                sb.AppendLine();
            }

            sb.AppendLine("        };");
            sb.AppendLine();

            sb.AppendLine("        var interceptionResult = await interception(request, interceptionArgs, accepted);");
            sb.AppendLine();

            sb.AppendLine("        if (interceptionResult != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            return interceptionResult;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
    }

}
