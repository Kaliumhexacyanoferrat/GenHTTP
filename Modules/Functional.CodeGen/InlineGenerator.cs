using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using GenHTTP.Modules.Functional.CodeGen.Model;

namespace GenHTTP.Modules.Functional.CodeGen;

[Generator]
public class InlineHandlerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Compilation provider
        var compilationProvider = context.CompilationProvider;

        // 2. Syntax provider: suche nur InvocationExpressions
        var invocationProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax,
                transform: static (ctx, _) => TransformIfBuildAs(ctx)
            )
            .Where(m => m != null);

        // 3. Combine Compilation + Matches
        var combined = compilationProvider.Combine(invocationProvider.Collect());

        // 4. SourceOutput
        context.RegisterSourceOutput(combined, (spc, pair) =>
        {
            var (compilation, matches) = pair;
            if (matches.IsDefaultOrEmpty) return;

            var handlers = new List<GeneratedHandler>();

            foreach (var match in matches.OfType<InlineBuildAsMatch>())
            {
                var identifier = match.Identifier;
                if (!string.IsNullOrEmpty(identifier))
                {
                    var typeName = $"GenHTTP_Inline_{Guid.NewGuid():N}";
                    handlers.Add(new GeneratedHandler(typeName, identifier));
                }
            }

            if (handlers.Count == 0) return;

            var source = new GeneratedSource("GenHTTP.Modules.Functional.CodeGen", "10.3.0", handlers);
            var code = EntryPoint.Emit(source);
            spc.AddSource("GenHTTP.InlineBuilder.Generated.g.cs", SourceText.From(code, Encoding.UTF8));
        });
    }

    private static InlineBuildAsMatch? TransformIfBuildAs(GeneratorSyntaxContext ctx)
    {
        if (ctx.Node is not InvocationExpressionSyntax invocation) return null;

        if (invocation.Expression is not MemberAccessExpressionSyntax ma) return null;
        if (ma.Name.Identifier.Text != "BuildAs") return null;

        var args = invocation.ArgumentList.Arguments;
        if (args.Count != 1) return null;

        if (args[0].Expression is not LiteralExpressionSyntax lit) return null;
        if (!lit.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression)) return null;

        var identifier = lit.Token.ValueText;

        return new InlineBuildAsMatch(invocation, identifier);
    }
}

// Modellklasse
public class InlineBuildAsMatch
{
    public InvocationExpressionSyntax Invocation;
    public string Identifier;

    public InlineBuildAsMatch(InvocationExpressionSyntax invocation, string identifier)
    {
        Invocation = invocation;
        Identifier = identifier;
    }
}
