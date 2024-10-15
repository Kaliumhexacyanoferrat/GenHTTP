using GenHTTP.Engine;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;

// todo bug: Inline.Get("/") => wirft fehler!

// todo: formale parameterbeschreibung plus parameter-typ (müsste sich überall durchziehen ...)

var description = ApiDescription.Create()
                                .Discovery(ApiDiscovery.Default());

var app = Inline.Create()
                .Get(() => 42)
                .Get("/users", () => new List<string>() { "a" })
                .Get("/users/:name", (string name) => name)
                .Add(description);

Host.Create()
    .Handler(app)
    .Defaults()
    .Development()
    .Console()
    .Run();
