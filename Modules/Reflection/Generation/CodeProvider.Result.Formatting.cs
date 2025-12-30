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
            sb.AppendIOSupportedResult(operation);
        }
        else if (type.IsEnum || type == typeof(Guid) || IsSimpleNumber(type))
        {
            sb.AppendLine("        var formattedResult = result.ToString() ?? string.Empty;");
            sb.AppendLine();

            sb.AppendFormattedString(operation);
        }
        else if (type.IsPrimitive && type != typeof(bool))
        {
            sb.AppendLine("        var formattedResult = Convert.ChangeType(result, typeof(string), CultureInfo.InvariantCulture) as string ?? string.Empty;");
            sb.AppendLine();

            sb.AppendFormattedString(operation);
        }
        else
        {
            sb.AppendLine("        var formattedResult = registry.Formatting.Write(result, result.GetType()) ?? string.Empty;");
            sb.AppendLine();

            sb.AppendFormattedString(operation);
        }
    }

    private static void AppendFormattedString(this StringBuilder sb, Operation operation)
    {
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(formattedResult)");
        sb.AppendResultModifications(operation, "                              ");
        sb.AppendLine("                              .Build();");
        sb.AppendLine();
    }

    private static bool IsSimpleNumber(Type type)
    {
        return type == typeof(int) || type == typeof(byte) || type == typeof(long)
            || type == typeof(uint) || type == typeof(ulong)
            || type == typeof(short) || type == typeof(ushort);
    }

}
