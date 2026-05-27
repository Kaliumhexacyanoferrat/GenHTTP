using Microsoft.CodeAnalysis;

namespace GenHTTP.Generators.MemoryView;

internal sealed record StructInfo(
    string Namespace,
    string TypeName
)
{
    private const string AttributeFullName = "GenHTTP.Api.MemoryViewAttribute";

    internal static StructInfo? From(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol symbol)
            return null;

        AttributeData? attribute = null;
        foreach (var a in ctx.Attributes)
        {
            if (a.AttributeClass?.ToDisplayString() == AttributeFullName)
            {
                attribute = a;
                break;
            }
        }
        if (attribute is null) return null;

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        return new StructInfo(ns, symbol.Name);
    }

    internal string HintName =>
        string.IsNullOrEmpty(Namespace)
            ? $"{TypeName}.MemoryView.g.cs"
            : $"{Namespace}.{TypeName}.MemoryView.g.cs";

}
