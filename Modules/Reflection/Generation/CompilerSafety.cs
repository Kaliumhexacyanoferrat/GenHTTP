using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CompilerSafety
{

    public static string GetString(string input)
        => SyntaxFactory.Literal(input).ToFullString();

}
