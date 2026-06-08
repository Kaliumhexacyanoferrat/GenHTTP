using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.Reflection.Routing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Modules.Reflection.Generation;

internal static class DelegateProvider
{
    private static readonly ConcurrentDictionary<string, MetadataReference> References = [];

    private static readonly CSharpCompilationOptions CompilationOptions = new(
        OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Release,
        concurrentBuild: false,
        deterministic: false
    );
    
    /// <summary>
    /// Compiles the given source code into an invocable delegate.
    /// </summary>
    /// <param name="code">The source code to be compiled</param>
    /// <typeparam name="T">Either object for a webservice instance or a delegate for functional invocations</typeparam>
    /// <returns>The compiled delegate</returns>
    /// <exception cref="InvalidOperationException">Thrown if the compilation failed for some reason</exception>
    internal static Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>> Compile<T>(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var assemblyName = Path.GetRandomFileName();

        var compilation = CSharpCompilation.Create(assemblyName, [syntaxTree], GetReferences(), CompilationOptions);

        using var ms = new MemoryStream();

        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = string.Join(Environment.NewLine, result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            throw new InvalidOperationException("Failed to compile handler:" + Environment.NewLine + Environment.NewLine + errors);
        }

        ms.Seek(0, SeekOrigin.Begin);

        var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
        
        var type = assembly.GetType("Invoker");

        var method = type!.GetMethod("Invoke")!;
        
        // warm up the JIT
        RuntimeHelpers.PrepareMethod(method.MethodHandle);

        return (Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>)Delegate.CreateDelegate(
            typeof(Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>), method
        );
    }

    private static MetadataReference[] GetReferences()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                continue;
            }

            References.GetOrAdd(assembly.Location, (s) => MetadataReference.CreateFromFile(s));
        }

        return References.Values.ToArray();
    }
    
}
