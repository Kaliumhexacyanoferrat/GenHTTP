using System.Reflection;

using GenHTTP.Api.Protocol;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Modules.Reflection.Generation;

public static class DelegateProvider
{

    public static Func<T, IRequest, MethodRegistry, ValueTask<IResponse?>> Compile<T>(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var assemblyName = Path.GetRandomFileName();
        
        var references = AppDomain.CurrentDomain.GetAssemblies()
                                  .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                                  .Select(a => MetadataReference.CreateFromFile(a.Location))
                                  .Cast<MetadataReference>();

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
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

        return (Func<T, IRequest, MethodRegistry, ValueTask<IResponse?>>)Delegate.CreateDelegate(
            typeof(Func<T, IRequest, MethodRegistry, ValueTask<IResponse?>>), method
        );
    }
    
}
