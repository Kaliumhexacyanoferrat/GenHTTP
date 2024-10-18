using GenHTTP.Engine.Internal;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var app = Content.From(Resource.FromString("Hello World"));

Host.Create()
    .Handler(app)
    .Defaults()
    .Development()
    .Console()
    .Run();
