using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderInterceptionExtensions
{

    public static void AppendInterception(this StringBuilder sb, Operation operation)
    {
        if (operation.Interceptors.Count > 0)
        {
            sb.AppendLine($"        var interceptionArgs = new Dictionary<string, object?>({operation.Arguments.Count})");
            sb.AppendLine("        {");

            int i = 1;
            
            foreach (var arg in operation.Arguments)
            {
                sb.Append($"            {{ {CompilationUtil.GetSafeString(arg.Key)}, arg{i++} }}");

                if (i < operation.Arguments.Count - 1)
                {
                    sb.Append(',');
                }

                sb.AppendLine();
            }
            
            sb.AppendLine("        };");
            sb.AppendLine();
            
            sb.AppendLine("        var interceptionResult = await interception(request, interceptionArgs);");
            sb.AppendLine();
            
            sb.AppendLine("        if (interceptionResult != null)");
            sb.AppendLine("        {");
            sb.AppendLine("            return interceptionResult;");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
    }
    
}
