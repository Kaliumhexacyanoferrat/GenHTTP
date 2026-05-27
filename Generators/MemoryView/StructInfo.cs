using Microsoft.CodeAnalysis;

namespace GenHTTP.Generators.MemoryView;

internal sealed record StructInfo(
    string Namespace,
    string TypeName,
    string FieldName,
    string PropertyName,
    bool GenerateToString
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

        var fieldName = "_value";
        if (attribute.ConstructorArguments.Length > 0
            && attribute.ConstructorArguments[0].Value is string fn
            && fn.Length > 0)
        {
            fieldName = fn;
        }

        var propertyName = "Value";
        var generateToString = true;

        foreach (var arg in attribute.NamedArguments)
        {
            switch (arg.Key)
            {
                case "PropertyName":
                    if (arg.Value.Value is string pn && pn.Length > 0) propertyName = pn;
                    break;
                case "GenerateToString":
                    if (arg.Value.Value is bool gts) generateToString = gts;
                    break;
            }
        }

        var ns = symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();

        return new StructInfo(ns, symbol.Name, fieldName, propertyName, generateToString);
    }

    internal string HintName =>
        string.IsNullOrEmpty(Namespace)
            ? $"{TypeName}.MemoryView.g.cs"
            : $"{Namespace}.{TypeName}.MemoryView.g.cs";
}
