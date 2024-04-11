using GenHTTP.Modules.Authentication.Web.Concern;
using GenHTTP.Modules.Authentication.Web.Integration;

namespace GenHTTP.Modules.Authentication.Web
{

    /// <summary>
    /// Allows 
    /// </summary>
    public static class WebAuthentication
    {

        public static WebAuthenticationBuilder Simple<T>() where T : ISimpleWebAuthIntegration, new() => Simple(new T());

        public static WebAuthenticationBuilder Simple(ISimpleWebAuthIntegration integration) => Advanced(new SimpleIntegrationAdapter(integration));

        public static WebAuthenticationBuilder Advanced<T>() where T : IWebAuthIntegration, new() => Advanced(new T());

        public static WebAuthenticationBuilder Advanced(IWebAuthIntegration integration)
        {
            return new WebAuthenticationBuilder().Integration(integration);
        }

    }

}
