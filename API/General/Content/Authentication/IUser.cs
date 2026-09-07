namespace GenHTTP.Api.Content.Authentication;

/// <summary>
/// Information about a user that is associated with the currently handled request.
/// </summary>
public interface IUser
{

    /// <summary>
    /// The name of the user as it should be shown on the UI or in log files.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// The roles of this user.
    /// </summary>
    string[]? Roles => null;

}
