using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultExtensions
{

    public static void AppendResultConversion(this StringBuilder sb, Operation operation, bool isAsync)
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
            case OperationResultSink.Binary:
                {
                    sb.AppendIOSupportedResult();
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

}
