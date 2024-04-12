using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{
    
    /// <summary>
    /// Authentication and authorization logic to be used by the
    /// web authentication concern.
    /// </summary>
    /// <typeparam name="TUser">The type of user managed by this integration</typeparam>
    /// <remarks>
    /// Use this kind of integration if you would like to quickly
    /// add login forms to your application. This integration will
    /// use a default set of controllers that render very simple
    /// UIs to you app users. 
    /// 
    /// If you would like to customize the authentication workflow,
    /// use <see cref="IWebAuthIntegration{TUser}" /> instead.
    /// </remarks>
    public interface ISimpleWebAuthIntegration<TUser> where TUser : IUser
    {

        /// <summary>
        /// False if you would like to force non logged in users
        /// to log in. 
        /// </summary>
        bool AllowAnonymous { get => false; }

        /// <summary>
        /// The route the setup functionality will be available
        /// from (defaults to "/setup/").
        /// </summary>
        string SetupRoute { get => "setup"; }

        /// <summary>
        /// The route the login page will be available from
        /// (defaults to "/login/").
        /// </summary>
        string LoginRoute { get => "login"; }

        /// <summary>
        /// The route the logout page will be available from
        /// (defaults to "/logout/").
        /// </summary>
        string LogoutRoute { get => "logout"; }

        /// <summary>
        /// The route used to serve additional resources required
        /// by the default controllers.
        /// </summary>
        /// <remarks>
        /// This is used by the default controllers which implement
        /// the simple integration flow to serve additional style sheets
        /// to style the login page.
        /// </remarks>
        string ResourceRoute { get => "auth-resources"; }

        /// <summary>
        /// Return true to redirect users to a setup page that allows
        /// an administrator setting up your application to initially
        /// create an accont with.
        /// </summary>
        /// <param name="request">The currently handled request</param>
        /// <returns>true if the application needs to be set up</returns>
        /// <remarks>
        /// This feature allows you to provision your application without
        /// the need of using fixed user accounts which would compromise
        /// the security of your deployments.
        /// 
        /// Typically you want to return true while there are no users
        /// yet and false as soon as there are some.
        /// 
        /// To disable the feature completely, just return false here.
        /// </remarks>
        ValueTask<bool> CheckSetupRequired(IRequest request);

        /// <summary>
        /// Called by the setup controller to initialize your application
        /// for the given admin user. 
        /// </summary>
        /// <param name="request">The currently handled request</param>
        /// <param name="username">The name entered by the user</param>
        /// <param name="password">The password entered by the user</param>
        /// <remarks>
        /// After this call, <see cref="CheckSetupRequired(IRequest)" /> is
        /// expected to return false on subsequent calls.
        /// </remarks>
        ValueTask PerformSetup(IRequest request, string username, string password);

        /// <summary>
        /// Invoked with the session token read from the client connection
        /// to actually load and check the session.
        /// </summary>
        /// <param name="request">The currently handled request</param>
        /// <param name="sessionToken">The token read from the client connection</param>
        /// <returns>The user record the session belongs to or null, if the session is not valid anymore</returns>
        /// <remarks>
        /// In this method you will need to verify the session specified by the client
        /// against some session storage, e.g. a Redis or database server.
        /// </remarks>
        ValueTask<TUser?> VerifyTokenAsync(IRequest request, string sessionToken);

        /// <summary>
        /// Invoked to generate a new session token for the authenticated user.
        /// </summary>
        /// <param name="request">The currently handled request</param>
        /// <param name="user">The user which just performed a login</param>
        /// <returns>The newly created (or re-used) session token</returns>
        ValueTask<string> StartSessionAsync(IRequest request, TUser user);

        /// <summary>
        /// Invoked to actually authenticate an user who entered their
        /// credentials in the login form.
        /// </summary>
        /// <param name="request">The currently handled request</param>
        /// <param name="username">The name of the user</param>
        /// <param name="password">The password of the user</param>
        /// <returns>The matching user record if the credentials are valid</returns>
        /// <remarks>
        /// If the user account does not exist or the credentials are incorrect,
        /// return null to cause the controller to render an error message.
        /// </remarks>
        ValueTask<TUser?> PerformLoginAsync(IRequest request, string username, string password);

    }

}
