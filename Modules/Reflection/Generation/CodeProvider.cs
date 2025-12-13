using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProvider
{

    public static string Generate(Operation operation)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("using GenHTTP.Api.Protocol;");
        sb.AppendLine();
        sb.AppendLine("using GenHTTP.Modules.Reflection;");
        sb.AppendLine("using GenHTTP.Modules.IO;");
        sb.AppendLine();

        sb.AppendLine("public static class Invoker");
        sb.AppendLine("{");
        
        sb.AppendLine("    public static ValueTask<IResponse?> Invoke(object instance, IRequest request, MethodRegistry registry)");
        sb.AppendLine("    {");
        
        sb.AppendInstanceCreation(operation);

        sb.AppendInvocation(operation);
        
        sb.AppendResultConversion(operation);
        
        sb.AppendLine("    }");
        
        sb.AppendLine("}");
        
        var str = sb.ToString();
        
        return sb.ToString();
    }

    private static void AppendInstanceCreation(this StringBuilder sb, Operation operation)
    {
        // todo: what if we have null here? inline?
        var typeName = operation.Method.DeclaringType!.Name;

        sb.AppendLine($"        var typedInstance = ({typeName})instance;");
    }

    private static void AppendInvocation(this StringBuilder sb, Operation operation)
    {
        // todo: arguments
        var methodName = operation.Method.Name;
        
        sb.AppendLine($"        var result = typedInstance.{methodName}();");
    }

    private static void AppendResultConversion(this StringBuilder sb, Operation operation)
    {
        switch (operation.Result.Sink)
        {
            case OperationResultSink.Formatter:
                {
                    sb.AppendFormattedResult(operation);
                    break;
                }
            default: throw new NotSupportedException();
        }

        sb.AppendLine("        return new(response);");
    }

    private static void AppendFormattedResult(this StringBuilder sb, Operation operation)
    {
        var type = operation.Result.Type;

        if (type == typeof(string))
        {
            sb.AppendStringResult();
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    private static void AppendStringResult(this StringBuilder sb)
    {
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(result)");
        sb.AppendLine("                              .Build();");
    }
    
}
