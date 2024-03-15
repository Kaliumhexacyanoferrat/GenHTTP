using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;

namespace GenHTTP.Modules.Authentication.Web
{

    public enum SetupResult
    {
        Success
    }

    public record class SetupConfig
    (

        IHandlerBuilder Handler,

        string Route,

        Func<IRequest, ValueTask<bool>> SetupRequired,

        Func<IRequest, string, string, ValueTask<SetupResult>>? PerformSetup

    );

    public static class Setup
    {
        private const string CONFIG_KEY = "__AUTH_WEB_SETUPCONFIG";

        public static SetupConfig BuiltIn(Func<IRequest, ValueTask<bool>> setupRequired, Func<IRequest, string, string, ValueTask<SetupResult>> performSetup, string route = "setup")
        {
            return new SetupConfig(Controller.From<SetupController>(), route, setupRequired, performSetup);
        }

        public static SetupConfig Custom(Func<IRequest, ValueTask<bool>> setupRequired, IHandlerBuilder handler, string route = "setup")
        {
            return new SetupConfig(handler, route, setupRequired, null);
        }

        public static SetupConfig GetConfig(IRequest request) => (SetupConfig)request.Properties[CONFIG_KEY];

        public static void SetConfig(IRequest request,  SetupConfig config) => request.Properties[CONFIG_KEY] = config;

    }

}
