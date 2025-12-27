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


var test = Inline.Create().Get(async () =>
                                   {
                                       var stream = new MemoryStream();

                                       await stream.WriteAsync("42"u8.ToArray());

                                       stream.Seek(0, SeekOrigin.Begin);

                                       return stream;
                                   })
                                   .ExecutionMode(ExecutionMode.Auto);

var app = Layout.Create()
               // .Add("handler", handler)
               // .Add("codegen", withCodeGen)
               // .Add("args", withArgs)
               // .Add("reflection", withReflection)
                .Add("test", test);

await Host.Create()
          .Handler(app)
          .Defaults()
          .Console()
          .RunAsync(); // or StartAsync() for non-blocking
