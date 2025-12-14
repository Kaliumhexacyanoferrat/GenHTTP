using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderResultSerializationExtensions
{
    
    public static void AppendSerializedResult(this StringBuilder sb)
    {
        sb.AppendLine("        var serializer = registry.Serialization.GetSerialization(request);");
        sb.AppendLine();
        sb.AppendLine("        if (serializer is null)");
        sb.AppendLine("        {");
        sb.AppendLine("            throw new ProviderException(ResponseStatus.UnsupportedMediaType, \"Requested format is not supported\");");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine("        var serializedResult = await serializer.SerializeAsync(request, result);");
        sb.AppendLine();
        sb.AppendLine("        var response = serializedResult.Build();");
    }
    
}
