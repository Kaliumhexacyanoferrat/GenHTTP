using GenHTTP.Api.Content.Authentication;

namespace GenHTTP.Modules.Authentication.Basic
{

    public class BasicAuthenticationUser : IUser
    {

        #region Get-/Setters

        public string DisplayName => Name;

        public string Name { get; }

        #endregion

        #region Initialization

        public BasicAuthenticationUser(string username)
        {
            Name = username;
        }

        #endregion

    }

}
