using System.Diagnostics.CodeAnalysis;
using GenHTTP.Api.Content;

using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Functional.CodeGen;

public static class HandlerRegistry
{
    private static readonly Dictionary<string, Func<MethodRegistry, IHandler>> Factories = [];

    public static void Add(string identifier, Func<MethodRegistry, IHandler> factory)
    {
        Factories.Add(identifier, factory);
    }

    public static bool TryGet(string identifier, MethodRegistry registry, [MaybeNullWhen(returnValue: false)] out IHandler handler)
    {
        if (Factories.TryGetValue(identifier, out var factory))
        {
            handler = factory(registry);
            return true;
        }

        handler = null;
        return false;
    }
    
}
