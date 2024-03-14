using GenHTTP.Engine;
using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var setupDone = false;

var setup = Setup.BuiltIn
(
    setupRequired: (req) => new(!setupDone),
    performSetup: (req, u, p) => { setupDone = true; return new(SetupResult.Success); }
);

var sessionHandling = SessionHandling.BuiltIn
(
    verifyToken: (token) => new()
);

var auth = WebAuthentication.Create()
                            .SessionHandling(sessionHandling)
                            .EnableSetup(setup);

Host.Create()
    .Handler(Content.From(Resource.FromString("Hello World")).Add(auth))
    .Defaults()
    .Development()
    .Console()
    .Run();
