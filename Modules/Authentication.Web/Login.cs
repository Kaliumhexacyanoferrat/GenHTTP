using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;

namespace GenHTTP.Modules.Authentication.Web
{

    public enum LoginStatus
    {
        Success
    }

    public record class LoginResult
    (

        LoginStatus Status,

        IUser? AuthenticatedUser

    );

    public record class LoginConfig
    (

        IHandlerBuilder Handler,

        string Route,

        Func<IRequest, string, string, ValueTask<LoginResult>>? PerformLogin

    );

    public static class Login
    {
        private const string CONFIG_KEY = "__AUTH_WEB_LOGINCONFIG";

        public static LoginConfig BuiltIn(Func<IRequest, string, string, ValueTask<LoginResult>> performLogin, string route = "login")
        {
            return new LoginConfig(Controller.From<LoginController>(), route, performLogin);
        }

        public static LoginConfig Custom(IHandlerBuilder handler, string route = "login")
        {
            return new LoginConfig(handler, route, null);
        }

        public static LoginConfig GetConfig(IRequest request) => (LoginConfig)request.Properties[CONFIG_KEY];

        public static void SetConfig(IRequest request, LoginConfig config) => request.Properties[CONFIG_KEY] = config;

    }

}
