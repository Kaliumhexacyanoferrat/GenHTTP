using GenHTTP.Engine;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

Host.Create()
    .Handler(Content.From(Resource.FromString("Hello World")))
    //.Defaults()
    //.Development()
    //.Console()
    .Run();

