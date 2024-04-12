using GenHTTP.Api.Content.Authentication;

using GenHTTP.Modules.Authentication.Web.Concern;
using GenHTTP.Modules.Authentication.Web.Integration;

namespace GenHTTP.Modules.Authentication.Web
{

    /// <summary>
    /// Secures content by adding a form based flow that requires
    /// users to login before they are allowed to access.
    /// </summary>
    public static class WebAuthentication
    {

        /// <summary>
        /// Creates a simple workflow that uses the given class
        /// to actually perform login operations.
        /// </summary>
        /// <typeparam name="T">The integration to be used to perform authentication</typeparam>
        /// <typeparam name="TUser">The type of user record used by the integration</typeparam>
        /// <returns>The newly created authentication builder</returns>
        public static WebAuthenticationBuilder<TUser> Simple<T, TUser>() where T : ISimpleWebAuthIntegration<TUser>, new() where TUser : class, IUser => Simple(new T());

        /// <summary>
        /// Creates a simple workflow that uses the given object
        /// to actually perform login operations.
        /// </summary>
        /// <typeparam name="TUser">The type of user record used by the integration</typeparam>
        /// <returns>The newly created authentication builder</returns>
        public static WebAuthenticationBuilder<TUser> Simple<TUser>(ISimpleWebAuthIntegration<TUser> integration) where TUser : class, IUser  => Advanced(new SimpleIntegrationAdapter<TUser>(integration));

        /// <summary>
        /// Creates a workflow that uses the given configuration
        /// to handle requests.
        /// </summary>
        /// <typeparam name="T">The type to use for authentication</typeparam>
        /// <typeparam name="TUser">The type of user record used by the integration</typeparam>
        /// <returns>The newly created authentication builder</returns>
        public static WebAuthenticationBuilder<TUser> Advanced<T, TUser>() where T : IWebAuthIntegration<TUser>, new() where TUser : class, IUser => Advanced(new T());

        /// <summary>
        /// Creates a workflow that uses the given configuration
        /// to handle requests.
        /// </summary>
        /// <typeparam name="TUser">The type of user record used by the integration</typeparam>
        /// <returns>The newly created authentication builder</returns>
        public static WebAuthenticationBuilder<TUser> Advanced<TUser>(IWebAuthIntegration<TUser> integration) where TUser : class, IUser
        {
            return new WebAuthenticationBuilder<TUser>(integration).SessionHandling<CookieSessionHandling>();
        }

    }

}
