using GenHTTP.Engine;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;

// todo bug: Inline.Get("/") => wirft fehler!

var description = ApiDescription.Create()
                                .Discovery(ApiDiscovery.Default());

var app = Inline.Create()
                .Get(() => 42)
                .Get("/users", () => new List<string>() { "a" })
                .Add(description);

Host.Create()
    .Handler(app)
    .Defaults()
    .Development()
    .Console()
    .Run();
