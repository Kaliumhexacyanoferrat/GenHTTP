using GenHTTP.Engine.Rocket;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using URocket.Engine.Configs;

var resource = Resource.FromString("Hello World!");
var content = Content.From(resource);
var app = Layout.Create()
    .Add("route", content);

await Host.Create(new EngineOptions
    {
        Port = 8080,
        ReactorCount = 12
    })
      .Handler(app)
      .Defaults()
      .Console()
      .RunAsync();
