using System.Reflection;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.Reflection.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Modules.Reflection.Generation;

internal static class DelegateProvider
{

    internal static Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>> Compile<T>(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var assemblyName = Path.GetRandomFileName();

        var references = AppDomain.CurrentDomain.GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location))
                                  .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(assemblyName, [syntaxTree], references,
                                                   new CSharpCompilationOptions(
                                                       OutputKind.DynamicallyLinkedLibrary,
                                                       optimizationLevel: OptimizationLevel.Release,
                                                       concurrentBuild: true,
                                                       deterministic: false
                                                   )
        );

        using var ms = new MemoryStream();

        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = string.Join(Environment.NewLine, result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            throw new InvalidOperationException("Failed to compile handler:" + Environment.NewLine + Environment.NewLine + errors);
        }

        ms.Seek(0, SeekOrigin.Begin);

        var assembly = Assembly.Load(ms.ToArray());

        var type = assembly.GetType("Invoker");

        var method = type!.GetMethod("Invoke")!;

        // warm up the JIT
        RuntimeHelpers.PrepareMethod(method.MethodHandle);

        return (Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>)Delegate.CreateDelegate(
            typeof(Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>), method
        );
    }

}
