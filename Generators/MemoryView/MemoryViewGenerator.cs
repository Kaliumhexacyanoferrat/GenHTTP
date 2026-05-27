using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenHTTP.Generators.MemoryView;

[Generator]
public sealed class MemoryViewGenerator : IIncrementalGenerator
{
    private const string AttributeFullName = "GenHTTP.Api.MemoryViewAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var structs = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AttributeFullName,
                predicate: static (node, _) => node is StructDeclarationSyntax,
                transform: static (ctx, _) => StructInfo.From(ctx))
            .Where(static info => info is not null);

        context.RegisterSourceOutput(structs, static (ctx, info) =>
            ctx.AddSource(info!.HintName, CodeEmitter.Emit(info!)));
    }
}
