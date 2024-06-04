using GenHTTP.Engine;

using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;

// Content.From(Resource.FromString("Hello World"))

Host.Create()
    .Handler(Layout.Create())
    .Defaults()
    .Development()
    .Console()
    .Run();

