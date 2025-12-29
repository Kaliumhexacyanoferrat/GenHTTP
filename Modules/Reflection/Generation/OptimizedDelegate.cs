using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

internal static class OptimizedDelegate
{
    private static readonly bool CompilationSupported = IsRuntimeCompilationSupported();

    internal static bool Supported => CompilationSupported;
    
    internal static Func<T, Operation, IRequest, IHandler, MethodRegistry, RequestInterception, ValueTask<IResponse?>>? Compile<T>(Operation operation)
    {
        if (!CompilationSupported) return null;
     
        // todo: constraints when we cannot compile (e.g. non-public types)
        
        var code = CodeProvider.Generate(operation);

        return DelegateProvider.Compile<T>(code);
    }
    
    private static bool IsRuntimeCompilationSupported()
    {
        var roslynType = Type.GetType("Microsoft.CodeAnalysis.CSharp.CSharpCompilation, Microsoft.CodeAnalysis.CSharp");
        
        if (roslynType == null)
        {
            return false;
        }

        var asmBuilderType = Type.GetType("System.Reflection.Emit.AssemblyBuilder");
        
        if (asmBuilderType == null)
        {
            return false;
        }

        var defineMethod = asmBuilderType.GetMethod("DefineDynamicAssembly",
        [
            typeof(System.Reflection.AssemblyName), typeof(System.Reflection.Emit.AssemblyBuilderAccess)
        ]);

        if (defineMethod == null)
        {
            return false;
        }
        
        var assemblyLoadMethod = typeof(System.Reflection.Assembly).GetMethod("Load", new[] { typeof(byte[]) });
        
        if (assemblyLoadMethod == null)
        {
            return false;
        }

        return true;
    }
    
}
