using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.Basic;

public record BasicAuthenticationUser(string Name) : IUser
{

    public string DisplayName => Name;
}
