using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenHTTP.Modules.Functional.CodeGen;

// Equivalent to InlineFactoryMatch record
internal sealed class InlineFactoryMatch
{
    public InvocationExpressionSyntax FactoryCall { get; }

    public ITypeSymbol BuilderType { get; }

    public InlineFactoryMatch(InvocationExpressionSyntax factoryCall, ITypeSymbol builderType)
    {
        FactoryCall = factoryCall;
        BuilderType = builderType;
    }
}

// Equivalent to InlineRoute record
internal sealed class InlineRoute
{
    public string Verb { get; }

    public string? Path { get; }

    public ExpressionSyntax DelegateExpression { get; }

    public bool IsSafe { get; }

    public string DelegateSource { get; }

    public InlineRoute(string verb, string? path, ExpressionSyntax delegateExpression, bool isSafe, string delegateSource)
    {
        Verb = verb;
        Path = path;
        DelegateExpression = delegateExpression;
        IsSafe = isSafe;
        DelegateSource = delegateSource;
    }
}

// Equivalent to InlineBuilderGroup record
internal sealed class InlineBuilderGroup
{
    public string Key { get; }

    public InvocationExpressionSyntax FactoryCall { get; }

    public ITypeSymbol BuilderType { get; }

    public List<InlineRoute> Routes { get; }

    // Primary constructor
    public InlineBuilderGroup(string key, InvocationExpressionSyntax factoryCall, ITypeSymbol builderType, List<InlineRoute> routes)
    {
        Key = key;
        FactoryCall = factoryCall;
        BuilderType = builderType;
        Routes = routes;
    }

    // Auxiliary constructor to mimic original record constructor
    public InlineBuilderGroup(string key, InvocationExpressionSyntax factoryCall, ITypeSymbol builderType, List<InlineRoute> routes, bool dummy)
        : this(key, factoryCall, builderType, routes)
    {
    }
    
}
