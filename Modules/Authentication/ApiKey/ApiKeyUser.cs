using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.ApiKey;

public class ApiKeyUser : IUser
{

    #region Initialization

    public ApiKeyUser(string key, params string[] roles)
    {
        Key = key;
        Roles = roles;
    }

    #endregion

    #region Get-/Setters

    public string DisplayName => Key;

    public string Key { get; }

    public string[] Roles { get; }

    #endregion

}
