using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultIOExtensions
{

    public static void AppendIOSupportedResult(this StringBuilder sb, Operation operation)
    {
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(result)");
        sb.AppendResultModifications(operation, "                              ");
        sb.AppendLine("                              .Build();");
        sb.AppendLine();
    }

}
