using System.Text;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultDynamicExtensions
{

    public static void AppendDynamicResult(this StringBuilder sb, Operation operation)
    {
        var resultType = operation.Result.Type;

        if (typeof(IResponse).IsAssignableFrom(resultType))
        {
            sb.AppendLine("        var response = result;");
        }
        else if (typeof(IResponseBuilder).IsAssignableFrom(resultType))
        {
            sb.AppendLine("        var response = result.Build();");
        }
        else if (typeof(IHandler).IsAssignableFrom(resultType))
        {
            sb.AppendLine("        var response = await result.HandleAsync(request);");
        }
        else if (typeof(IHandlerBuilder).IsAssignableFrom(resultType))
        {
            sb.AppendLine("        var response = await result.Build().HandleAsync(request);");
        }
        else
        {
            throw new NotSupportedException($"Dynamic result of type '{resultType}' is not supported");
        }
    }

}
