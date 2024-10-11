using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.ApiKey;

public class ApiKeyUser : IUser
{

    #region Get-/Setters

    public string DisplayName => Key;

    public string Key { get; }

    #endregion

    #region Initialization

    public ApiKeyUser(string key)
    {
            Key = key;
        }

    #endregion

}
