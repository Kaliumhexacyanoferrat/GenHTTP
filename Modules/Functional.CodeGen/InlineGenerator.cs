using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GenHTTP.Modules.Functional.CodeGen;

[Generator]
public class InlineHandlerGenerator : IIncrementalGenerator
{
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Compilation provider
        var compilationProvider = context.CompilationProvider;

        // Syntax provider: filter only InvocationExpressions (Inline.Create())
        var candidateProvider = context.SyntaxProvider
                                       .CreateSyntaxProvider(
                                           predicate: static (node, _) => node is InvocationExpressionSyntax,
                                           transform: static (ctx, _) => TransformIfInlineCreate(ctx)
                                       )
                                       .Where(static m => m != null);

        // Combine compilation + candidates
        var combined = compilationProvider.Combine(candidateProvider.Collect());

        // Register SourceOutput with access to compilation
        context.RegisterSourceOutput(combined, (spc, pair) =>
        {
            var (compilation, matches) = pair;

            if (matches.IsDefaultOrEmpty)
                return;

            /*if (!Debugger.IsAttached)
            {
                Debugger.Launch();
                Debugger.Break();
            }*/

            var groups = new List<InlineBuilderGroup>();

            foreach (var match in matches.OfType<InlineFactoryMatch>())
            {
                var semanticModel = compilation.GetSemanticModel(match.FactoryCall.SyntaxTree);
                groups.AddRange(CollectBuilderUsages(match, semanticModel));
            }

            if (groups.Count == 0) return;

            var generatedSource = GenerateFile(groups);
            
            spc.AddSource("GenHTTP.InlineBuilder.Generated.g.cs", SourceText.From(generatedSource, Encoding.UTF8));
        });
    }

    private static InlineFactoryMatch? TransformIfInlineCreate(GeneratorSyntaxContext ctx)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;

        // Example: Inline.Create().Get(...)
        // invocation.Expression = (MemberAccessExpressionSyntax) "Inline.Create().Get"
        if (invocation.Expression is not MemberAccessExpressionSyntax member)
            return null;

        // Try to find an inner invocation: Inline.Create()
        var potentialCreateCall = member.Expression as InvocationExpressionSyntax;
        if (potentialCreateCall == null)
            return null;

        // Get the method name from the inner invocation
        if (potentialCreateCall.Expression is not MemberAccessExpressionSyntax innerMember)
            return null;

        if (innerMember.Name.Identifier.Text != "Create")
            return null;

        // Resolve the symbol of the inner Create() call
        var symbol = ctx.SemanticModel.GetSymbolInfo(innerMember).Symbol as IMethodSymbol;
        if (symbol == null)
            return null;

        if (symbol.ContainingType.ToDisplayString() != "GenHTTP.Modules.Functional.Inline")
            return null;

        // Success: we matched Inline.Create()
        return new InlineFactoryMatch(potentialCreateCall, symbol.ReturnType);
    }

    private static IEnumerable<InlineBuilderGroup> CollectBuilderUsages(
    InlineFactoryMatch factoryMatch,
    SemanticModel model)
    {
        var invocation = factoryMatch.FactoryCall;

        // Check if factory invocation is assigned to a variable
        var declarator = invocation.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
        if (declarator != null && declarator.Initializer?.Value == invocation)
        {
            // Classic variable assignment: var b = Inline.Create();
            string varName = declarator.Identifier.Text;
            var block = invocation.FirstAncestorOrSelf<BlockSyntax>();
            if (block != null)
            {
                var calls = block.DescendantNodes()
                                 .OfType<InvocationExpressionSyntax>()
                                 .Where(inv => IsCallOnVariable(inv, varName, model))
                                 .ToList();

                if (calls.Count > 0)
                {
                    var grp = BuildGroupFromCalls(invocation, calls, model, factoryMatch.BuilderType);
                    if (grp != null) yield return grp;
                }
            }
        }
        else
        {
            // Inline usage or part of a chain: Inline.Create().Get(...).Post(...)
            var root = invocation.SyntaxTree.GetRoot();
            var candidates = root.DescendantNodes()
                                 .OfType<InvocationExpressionSyntax>()
                                 .Where(inv => IsChainedFromFactory(inv, invocation))
                                 .ToList();

            if (candidates.Count > 0)
            {
                var grp = BuildGroupFromCalls(invocation, candidates, model, factoryMatch.BuilderType);
                if (grp != null) yield return grp;
            }
        }
    }

    /// <summary>
    /// Checks if an invocation is called on a variable (e.g., 'b.Get(...)' where b is InlineBuilder)
    /// </summary>
    private static bool IsCallOnVariable(InvocationExpressionSyntax invocation, string varName, SemanticModel model)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax ma)
            return false;

        var receiverText = ma.Expression.ToString();
        if (!string.Equals(receiverText, varName, StringComparison.Ordinal))
            return false;

        var msym = model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
        if (msym == null) return false;

        return msym.ContainingType.ToDisplayString() == "GenHTTP.Modules.Functional.Provider.InlineBuilder";
    }

    /// <summary>
    /// Checks if an invocation is chained from the factory (Inline.Create())
    /// </summary>
    private static bool IsChainedFromFactory(InvocationExpressionSyntax invocation, InvocationExpressionSyntax factory)
    {
        ExpressionSyntax? expr = invocation.Expression;
        if (expr is not MemberAccessExpressionSyntax ma)
            return false;

        while (true)
        {
            var inner = ma.Expression;

            if (ReferenceEquals(inner, factory))
                return true;

            if (inner is InvocationExpressionSyntax innerInv)
            {
                if (ReferenceEquals(innerInv, factory))
                    return true;

                if (innerInv.Expression is MemberAccessExpressionSyntax innerMa)
                {
                    ma = innerMa;
                    continue;
                }
            }

            break;
        }

        return false;
    }

    private static InlineBuilderGroup? BuildGroupFromCalls(InvocationExpressionSyntax factoryCall, List<InvocationExpressionSyntax> calls, SemanticModel model, ITypeSymbol builderType)
    {
        var routes = new List<InlineRoute>();
        foreach (var inv in calls)
        {
            var methodSym = model.GetSymbolInfo(inv).Symbol as IMethodSymbol;
            if (methodSym == null) continue;

            var name = methodSym.Name;
            if (name is not ("Get" or "Post" or "Put" or "Delete" or "Any" or "On" or "Head")) continue;

            var args = inv.ArgumentList.Arguments;
            string? pathLiteral = null;
            ExpressionSyntax? delegateExpr = null;

            if (args.Count == 1)
            {
                delegateExpr = args[0].Expression;
            }
            else if (args.Count == 2)
            {
                if (args[0].Expression is LiteralExpressionSyntax lit)
                    pathLiteral = lit.Token.ValueText;
                else
                    continue;

                delegateExpr = args[1].Expression;
            }
            else continue;

            bool safe = IsDelegateExpressionSafe(delegateExpr, model, out string delegateSource);
            routes.Add(new InlineRoute(name, pathLiteral, delegateExpr!, safe, delegateSource));
        }

        if (routes.Count == 0) return null;
        if (routes.Any(r => !r.IsSafe)) return null;

        var key = $"builder_{factoryCall.GetLocation().GetLineSpan().StartLinePosition.Line}_{factoryCall.GetLocation().GetLineSpan().StartLinePosition.Character}";
        return new InlineBuilderGroup(key, factoryCall, builderType, routes);
    }

    private static bool IsDelegateExpressionSafe(ExpressionSyntax? expr, SemanticModel model, out string sourceText)
    {
        sourceText = expr?.ToString() ?? "null";
        if (expr == null) return false;

        if (expr is IdentifierNameSyntax || expr is MemberAccessExpressionSyntax)
        {
            var info = model.GetSymbolInfo(expr).Symbol;
            if (info is IMethodSymbol ms && ms.IsStatic)
            {
                sourceText = ms.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return true;
            }
            return false;
        }

        if (expr is LambdaExpressionSyntax lambda)
        {
            var data = model.AnalyzeDataFlow(lambda);
            if (!data.Captured.Any())
            {
                sourceText = lambda.ToString();
                return true;
            }
            return false;
        }

        if (expr is AnonymousMethodExpressionSyntax anon)
        {
            var data = model.AnalyzeDataFlow(anon);
            if (!data.Captured.Any())
            {
                sourceText = anon.ToString();
                return true;
            }
            return false;
        }

        return false;
    }

    private static string GenerateFile(List<InlineBuilderGroup> groups)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using System;");
        sb.AppendLine("using GenHTTP.Modules.Functional.Provider;");
        sb.AppendLine("using GenHTTP.Api.Protocol;");
        sb.AppendLine();
        sb.AppendLine("namespace GenHTTP.Modules.Functional.Provider");
        sb.AppendLine("{");
        sb.AppendLine("    public partial class InlineBuilder");
        sb.AppendLine("    {");
        sb.AppendLine("        internal partial IHandler? BuildGenerated()");
        sb.AppendLine("        {");
        sb.AppendLine("            var functions = new System.Collections.Generic.List<InlineFunction>();");

        foreach (var g in groups)
        {
            foreach (var r in g.Routes)
            {
                var pathExpr = r.Path is null ? "null" : $"\"{EscapeString(r.Path)}\"";
                var methodConfigExpr = GenerateMethodConfigurationExpression(r.Verb);
                var delegateExpr = r.DelegateSource;

                sb.AppendLine("            functions.Add(new InlineFunction(");
                sb.AppendLine($"                {pathExpr},");
                sb.AppendLine($"                {methodConfigExpr},");
                sb.AppendLine($"                {delegateExpr}");
                sb.AppendLine("            ));");
            }
        }

        sb.AppendLine();
        sb.AppendLine("            var extensions = new MethodRegistry(Serialization.Default().Build(), Injection.Default().Build(), Formatting.Default().Build());");
        sb.AppendLine("            return Concerns.Chain(new System.Collections.Generic.List<IConcern>(), new InlineHandler(functions, extensions));");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateMethodConfigurationExpression(string verb)
    {
        var v = verb switch
        {
            "Get" => "RequestMethod.Get",
            "Post" => "RequestMethod.Post",
            "Put" => "RequestMethod.Put",
            "Delete" => "RequestMethod.Delete",
            "Head" => "RequestMethod.Head",
            "Any" => null,
            "On" => null,
            _ => null
        };

        if (v == null)
        {
            return "new MethodConfiguration(new System.Collections.Generic.HashSet<FlexibleRequestMethod>(Enum.GetValues(typeof(RequestMethod)).Cast<RequestMethod>().Select(FlexibleRequestMethod.Get)))";
        }
        else
        {
            return $"new MethodConfiguration(new System.Collections.Generic.HashSet<FlexibleRequestMethod>(new [] {{ FlexibleRequestMethod.Get({v}) }}))";
        }
    }

    private static string EscapeString(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    
}
