namespace GenHTTP.Api.Content.Authentication
{
    
    /// <summary>
    /// Information about an user that is associated with
    /// the currently handled request.
    /// </summary>
    public interface IUser
    {

        /// <summary>
        /// The name of the user as it should be shown on
        /// the UI (e.g. a rendered, themed page).
        /// </summary>
        string DisplayName { get; }

    }

}
