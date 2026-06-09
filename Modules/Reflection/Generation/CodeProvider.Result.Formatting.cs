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
            sb.AppendLine("        var content = new StringContent(result);");
        }
        else if (type.IsAssignableTo(typeof(IUtf8SpanFormattable)))
        {
            sb.AppendLine("        var content = new FormattableContent(result);");
        }
        else
        {
            sb.AppendLine("        var content = registry.Formatting.GetContent(result);");
        }

        sb.AppendLine();
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(content)");
        sb.AppendResultModifications(operation, "                              ");
        sb.AppendLine("                              .Build();");
        sb.AppendLine();
    }

}
