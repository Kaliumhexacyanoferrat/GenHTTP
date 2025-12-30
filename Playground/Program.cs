using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Reflection;

var handler = Handler.From((IRequest request) =>
{
    var i = int.Parse(request.Target.Current?.Value ?? throw new InvalidOperationException());

    var j = int.Parse(request.Query["j"]);

    return request.Respond()
                  .Content((i + j).ToString())
                  .Type(ContentType.TextPlain)
                  .Build();
});

var withCodeGen = Inline.Create().Get(":i", (int i, int j) => i + j).ExecutionMode(ExecutionMode.Auto);

var withReflection = Inline.Create().Get(":i", (int i, int j) => i + j).ExecutionMode(ExecutionMode.Reflection);


var test = Inline.Create()
                 .Get(() => Task.CompletedTask)
                 .ExecutionMode(ExecutionMode.Auto);

var app = Layout.Create()
                 .Add("handler", handler)
                 .Add("codegen", withCodeGen)
                 .Add("reflection", withReflection);

await Host.Create()
          .Handler(app)
          .RunAsync(); // or StartAsync() for non-blocking
