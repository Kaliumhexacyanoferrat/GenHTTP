using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.Basic;

public record BasicAuthenticationUser : IUser
{

    public string Name { get; }

    public string DisplayName => Name;

    public string[] Roles { get; }

    public BasicAuthenticationUser(string name, params string[] roles)
    {
        Name = name;
        Roles = roles;
    }

}
