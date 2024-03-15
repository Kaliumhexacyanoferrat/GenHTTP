using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;

using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

// in-memory user and session storage for demonstration
var users = new List<MyUser>();

var sessions = new Dictionary<string, MyUser>();

// first run wizard
var setup = Setup.BuiltIn
(
    setupRequired: (req) => new(users.Count == 0),
    performSetup: (req, u, p) => 
    {
        users.Add(new MyUser(u, p));
        return new(SetupResult.Success); 
    }
);

// session generation and verification
var sessionHandling = SessionHandling.BuiltIn
(
    verifyToken: (token) =>
    {
        if (sessions.TryGetValue(token , out var user))
        {
            return new(user);
        }

        return new();
    },
    startSession: (req, user) => 
    {
        var token = Guid.NewGuid().ToString();
        sessions.Add(token, (MyUser)user);
        return new(token);
    }
);

// actual login
var login = Login.BuiltIn
(
    performLogin: (req, u, p) =>
    {
        var user = users.Where(e => e.Name == u && e.Password == p);

        if (user.Any())
        {
            return new(new LoginResult(LoginStatus.Success, user.First()));
        }

        throw new ProviderException(ResponseStatus.InternalServerError, "Ups");
    }
);

// wire everything
var auth = WebAuthentication.Create()
                            .SessionHandling(sessionHandling)
                            .Login(login)
                            .EnableSetup(setup);

Host.Create()
    .Handler(Content.From(Resource.FromString("Hello World")).Add(auth))
    .Defaults()
    .Development()
    .Console()
    .Run();

// custom user structure
public class MyUser : IUser
{

    public string Name { get; }

    public string Password { get; }

    public string DisplayName => Name;

    public MyUser(string name, string password) { Name = name; Password = password; }

}
