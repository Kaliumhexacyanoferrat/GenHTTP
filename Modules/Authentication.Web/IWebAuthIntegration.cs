using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web.Controllers;

namespace GenHTTP.Modules.Authentication.Web
{

    /// <summary>
    /// Authentication and authorization logic to be used by the
    /// web authentication concern.
    /// </summary>
    /// <typeparam name="TUser">The type of user managed by this integration</typeparam>
    /// <remarks>
    /// Use this kind of integration if you would like to have
    /// fine grained control over your login flow and to render
    /// custom login UIs. 
    /// 
    /// If you would like to quickly add authentication to your application,
    /// use <see cref="ISimpleWebAuthIntegration{TUser}" /> instead.
    /// </remarks>
    public interface IWebAuthIntegration<TUser> where TUser : IUser
    {

        /// <summary>
        /// False if you would like to force non logged in users
        /// to log in. 
        /// </summary>
        bool AllowAnonymous { get => false; }

        /// <summary>
        /// The handler invoked to render the setup pages.
        /// </summary>
        /// <remarks>
        /// This handler should:
        /// 
        /// 1. Render an UI that allows an user to configure the app on first run
        /// 2. Actually perform the configuration when submitted by the user
        /// 3. Redirect to the login page after the setup has been completed
        /// 
        /// For reference <see cref="SetupController" />.
        /// </remarks>
        IHandlerBuilder SetupHandler { get; }

        /// <summary>
        /// The route the setup functionality will be available
        /// from (defaults to "/setup/").
        /// </summary>
        string SetupRoute { get => "setup"; }

        /// <summary>
        /// The handler invoked to render the login pages.
        /// </summary>
        /// <remarks>
        /// This handler should:
        /// 
        /// 1. Render an UI that allows your users to login to the app
        /// 2. Set the authenticated user record via
        /// 3. Redirect the user back to the root of your app
        /// 
        /// For reference <see cref="LoginController{TUser}"/>.
        /// </remarks>
        IHandlerBuilder LoginHandler { get; }

        /// <summary>
        /// The route the login page will be available from
        /// (defaults to "/login/").
        /// </summary>
        string LoginRoute { get => "login"; }

        /// <summary>
        /// The handler invoked to render the logout page.
        /// </summary>
        /// <remarks>
        /// This handler should:
        /// 
        /// 1. Clear the user property from the requests property bag
        /// 
        ///  For reference <see cref="LogoutController" />.
        /// </remarks>
        IHandlerBuilder LogoutHandler { get; }

        /// <summary>
        /// The route the logout page will be available from
        /// (defaults to "/logout/").
        /// </summary>
        string LogoutRoute { get => "logout"; }

        /// <summary>
        /// The handler invoked to fetch additional resources referenced
        /// by your setup, login or logout handlers (such as images, scripts
        /// or additional stylesheets).
        /// </summary>
        IHandlerBuilder ResourceHandler { get; }

        /// <summary>
        /// The route used to serve additional resources required
        /// by your controllers.
        /// </summary>
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

    }

}
