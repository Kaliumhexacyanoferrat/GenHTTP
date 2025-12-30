using System.Runtime.InteropServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.Reflection.Routing;

namespace GenHTTP.Modules.Reflection.Generation;

internal static class OptimizedDelegate
{

    /// <summary>
    /// Specifies, whether code generation is supported on this system.
    /// </summary>
    internal static bool Supported { get; } = IsRuntimeCompilationSupported();

    /// <summary>
    /// Compiles the given operation into an invokable delegate.
    /// </summary>
    /// <param name="operation">The operation to be compiled</param>
    /// <typeparam name="T">Either an object for instance based services or a delegate for functional services</typeparam>
    /// <returns>The delegate or null, if code generation is not supported on this system</returns>
    /// <exception cref="CodeGenerationException">Thrown, if the compilation failed for some reason</exception>
    internal static Func<T, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>? Compile<T>(Operation operation)
    {
        if (!Supported) return null;

        String? code = null;

        try
        {
            code = CodeProvider.Generate(operation);

            return DelegateProvider.Compile<T>(code);
        }
        catch (Exception e)
        {
            throw new CodeGenerationException(code, e);
        }
    }
    
    private static bool IsRuntimeCompilationSupported()
    {
        if (RuntimeInformation.ProcessArchitecture is Architecture.Arm or Architecture.Arm64)
        {
            return false;
        }
        
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
