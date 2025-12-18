using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CompilationUtil
{

    public static string GetSafeString(string input)
        => SyntaxFactory.Literal(input).ToFullString();

    public static string GetQualifiedName(Type type)
        => type.FullName!.Replace('+', '.');

}
