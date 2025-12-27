using System.Text;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultVoidExtensions
{

    public static void AppendVoidResult(this StringBuilder sb)
    {
        sb.AppendLine("        var response = request.Respond()");
        sb.AppendLine("                              .Status(ResponseStatus.NoContent)");
        sb.AppendLine("                              .Build();");
    }

}
