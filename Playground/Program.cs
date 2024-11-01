using GenHTTP.Engine;

using GenHTTP.Modules.Inspection;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var app = Content.From(Resource.FromString("Hello World")).AddInspector();

Host.Create()
    .Handler(app)
    .Defaults()
    .Development()
    .Console()
    .Run();
