using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Functional.CodeGen;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Functional;

[GeneratedCode("GenHTTP.Modules.Functional.CodeGen", "10.3.0")]
public sealed class MyHandler1(MethodRegistry registry) : IHandler
{

    [ModuleInitializer]
    public static void Register()
    {
        HandlerRegistry.Add("...", (registry) => new MyHandler1(registry));
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var x = registry.Formatting;
        return default;
    }
    
}
