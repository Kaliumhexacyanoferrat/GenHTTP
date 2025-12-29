using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultExtensions
{

    public static void AppendResultConversion(this StringBuilder sb, Operation operation, bool isAsync)
    {
        sb.AppendNullReturn(operation, isAsync);

        switch (operation.Result.Sink)
        {
            case OperationResultSink.Formatter:
                {
                    sb.AppendFormattedResult(operation);
                    break;
                }
            case OperationResultSink.Serializer:
                {
                    sb.AppendSerializedResult(operation);
                    break;
                }
            case OperationResultSink.Binary:
                {
                    sb.AppendIOSupportedResult(operation);
                    break;
                }
            case OperationResultSink.Dynamic:
                {
                    sb.AppendDynamicResult(operation);
                    break;
                }
            case OperationResultSink.None:
                {
                    sb.AppendVoidResult();
                    break;
                }
            default: throw new NotSupportedException();
        }
        
        if (isAsync)
        {
            sb.AppendLine("        return response;");
        }
        else
        {
            sb.AppendLine("        return new(response);");
        }
    }

    private static void AppendNullReturn(this StringBuilder sb, Operation operation, bool isAsync)
    {
        if (CompilationUtil.CanHoldNull(operation.Result.Type))
        {
            sb.AppendLine("        if (result == null)");
            sb.AppendLine("        {");
            sb.AppendLine("            var noContent = request.Respond().Status(ResponseStatus.NoContent);");
            sb.AppendLine();
            
            if (CompilationUtil.HasWrappedResult(operation))
            {
                sb.AppendLine("        wrapped.Apply(noContent);");
                sb.AppendLine();
            }
            
            if (isAsync)
            {
                sb.AppendLine("            return noContent.Build();");
            }
            else
            {
                sb.AppendLine("            return new(noContent.Build());");
            }
            
            sb.AppendLine("        }");
            sb.AppendLine();
        }
    }

}
