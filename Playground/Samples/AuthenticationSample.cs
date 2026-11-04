using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Playground.Samples;

public static class AuthenticationSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * A notes API secured with API keys. Every user only sees their own notes,
         * while administrators may see the notes of all users.
         *
         * Clients send their key via the "X-API-Key" header: "alice-key" for a user,
         * "bob-key" for an administrator.
         *
         * Basic, bearer (JWT) and client certificate authentication work the same way.
         *
         * See https://genhttp.org/documentation/content/concerns/authentication/
         *
         */

        var auth = ApiKeyAuthentication.Create()
                                       .Authenticator(AuthenticateAsync);

        // allows the service methods to declare the user as a parameter
        var injection = Injection.Default()
                                 .Add(new UserInjector<NoteUser>());

        return Layout.Create()
                     .AddService<NoteService>("notes", injectors: injection)
                     .Authentication(auth);
    }

    private static ValueTask<IUser?> AuthenticateAsync(IRequest request, string key)
    {
        // this is where you would look up the key in your database
        IUser? user = key switch
        {
            "alice-key" => new NoteUser("alice", "USER"),
            "bob-key" => new NoteUser("bob", "USER", "ADMIN"),
            _ => null // unknown key, access denied
        };

        return new(user);
    }

    public record NoteUser(string Name, params string[] Roles) : IUser
    {

        public string DisplayName => Name;

    }

    public record Note(string Owner, string Text);

    public class NoteService
    {
        private static readonly List<Note> Notes = [];

        // GET http://localhost:8080/authentication/notes/
        [ResourceMethod]
        public List<Note> Mine(NoteUser user) => Notes.Where(n => n.Owner == user.Name).ToList();

        // PUT http://localhost:8080/authentication/notes/ with the text as body
        [ResourceMethod(Method.Put)]
        public Note Add(NoteUser user, [FromBody] string text)
        {
            var note = new Note(user.Name, text);

            Notes.Add(note);

            return note;
        }

        // GET http://localhost:8080/authentication/notes/all
        [ResourceMethod("all")]
        [RequireRole("ADMIN")]
        public List<Note> All() => Notes;

    }

}
