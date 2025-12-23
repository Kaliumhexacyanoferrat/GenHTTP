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
            sb.AppendIOSupportedResult();
        }
        else
        {
            sb.AppendLine("        var response = request.Respond()");
            sb.AppendLine("                              .Content(registry.Formatting.Write(result, result.GetType()) ?? string.Empty)");
            sb.AppendLine("                              .Build();");
        }
    }

}
