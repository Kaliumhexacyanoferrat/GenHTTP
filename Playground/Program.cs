using GenHTTP.Engine;

using GenHTTP.Modules.IO;

Host.Create()
    .Handler(Content.From(Resource.FromString("Hello World")))
    .Run();
