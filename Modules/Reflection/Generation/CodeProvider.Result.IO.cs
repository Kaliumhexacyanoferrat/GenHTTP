using System.Text;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultIOExtensions
{

    public static void AppendIOSupportedResult(this StringBuilder sb)
    {
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Content(result)");
        sb.AppendLine("                              .Build();");
    }

}
