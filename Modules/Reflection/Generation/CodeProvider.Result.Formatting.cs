using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultFormattingExtensions
{
    
    public static void AppendFormattedResult(this StringBuilder sb, Operation operation)
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
