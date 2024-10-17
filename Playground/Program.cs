using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

// todo bug: Inline.Get("/") => wirft fehler!

var description = ApiDescription.Create()
                                .Title("My API")
                                .Version("1.0.0");

var inline = Inline.Create()
                   .Put("file", (Stream stream) => true)
                   .Add(description);

Host.Create()
    .Handler(inline)
    .Defaults()
    .Development()
    .Console()
    .Run();

public record User(int ID, string Name);

public class UserService
{

    [ResourceMethod]
    public Stream Avatar(DateTime cannot, short s, byte b, bool b2) => new MemoryStream();
}

public class DeviceController
{

    // [ControllerAction(RequestMethod.Post)]
    public void Register(int id)
    {

    }

    [ControllerAction(RequestMethod.Get)]
    public IHandlerBuilder Wildcard() => Redirect.To("https://google.de");

    [Obsolete]
    [ControllerAction(RequestMethod.Get)]
    public ValueTask<User?> GetUserAsync() => new();
}
