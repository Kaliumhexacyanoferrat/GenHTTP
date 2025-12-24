using GenHTTP.Engine.Internal;

using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Reflection;

var handler = Content.From(Resource.FromString("Hello World!"));

var withCodeGen = Inline.Create().Get(() => "Hello World!").ExecutionMode(ExecutionMode.Auto);

var withArgs = Inline.Create().Get((string x) => x).ExecutionMode(ExecutionMode.Auto);

var withReflection = Inline.Create().Get(() => "Hello World!").ExecutionMode(ExecutionMode.Reflection);


var test = Inline.Create()
                 .Get((SomeClass.MyEnum input) => input)
                 .ExecutionMode(ExecutionMode.Auto);

var app = Layout.Create()
               // .Add("handler", handler)
               // .Add("codegen", withCodeGen)
               // .Add("args", withArgs)
               // .Add("reflection", withReflection)
                .Add("test", test);

await Host.Create()
<<<<<<< HEAD
<<<<<<< HEAD
          .Handler(content)
          .Defaults()
          .Console()
          .RunAsync(); // or StartAsync() for non-blocking
=======
    .Handler(service)
    .Defaults()
    .RunAsync(); // or StartAsync() for non-blocking

public class MyService
{

    [ResourceMethod]
    public MyData Hello() => new("Hello World!");

}
<<<<<<< HEAD
>>>>>>> 9f59196e (Add support for compiled service methods)
=======

public record MyData(string Data);
>>>>>>> 2e9361a1 (WIP)
=======
          .Handler(app)
          .Defaults()
          .Development()
          .RunAsync();
<<<<<<< HEAD
<<<<<<< HEAD
>>>>>>> 5c3a20cf (Code gen improvements)
=======
>>>>>>> 524a8632 (Add first argument handling)
=======

public class SomeClass {
public enum MyEnum
{
    A,
    B
}
}
>>>>>>> f34d114c (Add support for nested type names)
