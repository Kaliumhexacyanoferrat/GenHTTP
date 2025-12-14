using System.Text;

using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProvider
{

    public static string Generate(Operation operation)
    {
        var isAsync = CheckAsync(operation);

        var sb = new StringBuilder();

        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine();
        sb.AppendLine("using GenHTTP.Api.Protocol;");
        sb.AppendLine("using GenHTTP.Api.Content;");
        sb.AppendLine();
        sb.AppendLine("using GenHTTP.Modules.Reflection;");
        sb.AppendLine("using GenHTTP.Modules.IO;");
        sb.AppendLine();

        sb.AppendLine("public static class Invoker");
        sb.AppendLine("{");

        if (operation.Delegate != null)
        {
            sb.AppendLine($"    public static {(isAsync ? "async" : string.Empty)} ValueTask<IResponse?> Invoke(Delegate logic, IRequest request, MethodRegistry registry)");
        }
        else
        {
            sb.AppendLine($"    public static {(isAsync ? "async" : string.Empty)} ValueTask<IResponse?> Invoke(object instance, IRequest request, MethodRegistry registry)");
        }

        sb.AppendLine("    {");

        sb.AppendInvocation(operation);

        sb.AppendResultConversion(operation, isAsync);

        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static bool CheckAsync(Operation operation)
    {
        return operation.Result.Sink == OperationResultSink.Serializer;
    }
    
}
