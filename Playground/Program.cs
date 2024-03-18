using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;

using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;

var auth = WebAuthentication.Simple<MyIntegration>();

Host.Create()
    .Handler(Content.From(Resource.FromString("Hello World")).Add(auth))
    .Defaults()
    .Development()
    .Console()
    .Run();

public class MyIntegration : ISimpleWebAuthIntegration
{
    private List<MyUser> _Users = new();

    private Dictionary<string, MyUser> _Sessions = new();

    public ValueTask<bool> CheckSetupRequired(IRequest request) => new(_Users.Count == 0);

    public ValueTask PerformSetup(IRequest request, string username, string password)
    {
        _Users.Add(new(username, password));
        return new();
    }

    public ValueTask<IUser?> PerformLogin(IRequest request, string username, string password)
    {
        return new(_Users.FirstOrDefault(e => e.Name == username && e.Password == password));
    }

    public ValueTask<string> StartSessionAsync(IRequest request, IUser user)
    {
        var token = Guid.NewGuid().ToString();
        _Sessions.Add(token, (MyUser)user);
        return new(token);
    }

    public ValueTask<IUser?> VerifyTokenAsync(string sessionToken)
    {
        if (_Sessions.TryGetValue(sessionToken, out var user))
        {
            return new(user);
        }

        return new();
    }

}

public class MyUser : IUser
{

    public string Name { get; }

    public string Password { get; }

    public string DisplayName => Name;

    public MyUser(string name, string password) { Name = name; Password = password; }

}
