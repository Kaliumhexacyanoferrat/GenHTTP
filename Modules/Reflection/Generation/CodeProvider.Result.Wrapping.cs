using System.Text;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeBuilderResultWrappingExtensions
{

    public static void AppendResultModifications(this StringBuilder sb, Operation operation, string prefix)
    {
        if (CompilationUtil.HasWrappedResult(operation))
        {
            sb.AppendLine($"{prefix}.Apply(b => wrapped.Apply(b))");
        }
    }

}
