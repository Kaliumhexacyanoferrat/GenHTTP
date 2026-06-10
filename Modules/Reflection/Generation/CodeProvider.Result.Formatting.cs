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
            sb.AppendLine("        var resultContent = new StringContent(result);");
        }
        else if (IsFormattable(type))
        {
            sb.AppendLine("        var resultContent = new FormattableContent(result);");
        }
        else
        {
            sb.AppendLine("        var resultContent = registry.Formatting.GetContent(result);");
        }

        sb.AppendLine();
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(resultContent)");
        sb.AppendResultModifications(operation, "                              ");
        sb.AppendLine("                              .Build();");
        sb.AppendLine();
    }

    private static bool IsFormattable(Type type)
        => type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(uint) ||
           type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal) || type == typeof(Guid);

}
