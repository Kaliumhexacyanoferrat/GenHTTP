using GenHTTP.Engine;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;

// todo bug: Inline.Get("/") => wirft fehler!

// todo: formale parameterbeschreibung plus parameter-typ (müsste sich überall durchziehen ...)

var description = ApiDescription.Create()
                                                  .Title("My API")
                                                  .Version("1.0.0");

var app = Inline.Create()
                .Get("/users/:id", (int id) => id)
                .Add(description);

Host.Create()
    .Handler(app)
    .Defaults()
    .Development()
    .Console()
    .Run();
