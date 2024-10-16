using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.Functional;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.OpenApi;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Webservices;

// todo bug: Inline.Get("/") => wirft fehler!

// todo: formale parameterbeschreibung plus parameter-typ (müsste sich überall durchziehen ...)

var description = ApiDescription.Create()
                                                  .Title("My API")
                                                  .Version("1.0.0");

var api = Layout.Create()
                .AddService<UserService>("users")
                .Add(description);

Host.Create()
    .Handler(api)
    .Defaults()
    .Development()
    .Console()
    .Run();

public record User(int ID, string Name);

public class UserService
{

    //[ResourceMethod]
    //public List<User> GetUsers() { return []; }

    //[ResourceMethod(RequestMethod.Get, ":id")]
    //public User GetUser(int id) { return new(id, "Me"); }

    [ResourceMethod(RequestMethod.Put)]
    public void PutUser(User user) { }

}
