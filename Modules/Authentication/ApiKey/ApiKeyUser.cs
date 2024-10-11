using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.ApiKey;

public class ApiKeyUser : IUser
{

    #region Initialization

    public ApiKeyUser(string key)
    {
        Key = key;
    }

    #endregion

    #region Get-/Setters

    public string DisplayName => Key;

    public string Key { get; }

    #endregion

}
