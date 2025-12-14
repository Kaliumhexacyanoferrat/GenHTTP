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

        if (operation.Delegate == null)
        {
            sb.AppendInstanceCreation(operation);
        }

        sb.AppendInvocation(operation);

        sb.AppendResultConversion(operation, isAsync);

        sb.AppendLine("    }");

        sb.AppendLine("}");

        var str = sb.ToString();

        return sb.ToString();
    }

    private static bool CheckAsync(Operation operation)
    {
        return operation.Result.Sink == OperationResultSink.Serializer;
    }

    private static void AppendInstanceCreation(this StringBuilder sb, Operation operation)
    {
        // todo: what if we have null here? inline?
        var typeName = operation.Method.DeclaringType!.Name;

        sb.AppendLine($"        var typedInstance = ({typeName})instance;");
        sb.AppendLine();
    }

    private static void AppendInvocation(this StringBuilder sb, Operation operation)
    {
        var arguments = string.Join(", ", operation.Arguments.Select(x => x.Key));
        
        if (operation.Delegate != null)
        {
            var type = (operation.Result.Sink == OperationResultSink.None) ? "Action" : "Func";
            
            var argumentTypes = new List<Type>(operation.Arguments.Select(x => x.Value.Type));

            if (operation.Result.Sink != OperationResultSink.None)
            {
                argumentTypes.Add(operation.Result.Type);
            }

            var stringTypes = string.Join(", ", argumentTypes);
            
            sb.AppendLine($"        var typedLogic = ({type}<{stringTypes}>)logic;");
            sb.AppendLine();
            
            sb.AppendLine($"        var result = typedLogic({arguments});");
            
            sb.AppendLine();
        }
        else
        {
            var methodName = operation.Method.Name;

            sb.AppendLine($"        var result = typedInstance.{methodName}({arguments});");

            sb.AppendLine();
        }
    }

    private static void AppendResultConversion(this StringBuilder sb, Operation operation, bool isAsync)
    {
        switch (operation.Result.Sink)
        {
            case OperationResultSink.Formatter:
                {
                    sb.AppendFormattedResult(operation);
                    break;
                }
            case OperationResultSink.Serializer:
                {
                    sb.AppendSerializedResult();
                    break;
                }
            default: throw new NotSupportedException();
        }

        sb.AppendLine();

        if (isAsync)
        {
            sb.AppendLine("        return response;");
        }
        else
        {
            sb.AppendLine("        return new(response);");
        }
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
            sb.AppendLine("        var response = request.Respond()");
            sb.AppendLine("                              .Content(registry.Formatting.Write(result, result.GetType()) ?? string.Empty)");
            sb.AppendLine("                              .Build();");
        }
    }

    private static void AppendStringResult(this StringBuilder sb)
    {
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(result)");
        sb.AppendLine("                              .Build();");
    }

}
