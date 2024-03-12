using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication.Web.Concern;
using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;
using System;
using System.Threading.Tasks;

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

        private const string SETUP_CONFIG = "webAuth_setupConfig";

        public static SetupConfig BuiltIn(Func<IRequest, ValueTask<bool>> setupRequired, Func<IRequest, string, string, ValueTask<SetupResult>> performSetup, string route = "setup")
        {
            return new SetupConfig(Controller.From<SetupController>(), route, setupRequired, performSetup);
        }

        public static SetupConfig Custom(Func<IRequest, ValueTask<bool>> setupRequired, IHandlerBuilder handler, string route = "setup")
        {
            return new SetupConfig(handler, route, setupRequired, null);
        }

        public static SetupConfig GetConfig(IRequest request) => (SetupConfig)request.Properties[SETUP_CONFIG];

        public static void SetConfig(IRequest request,  SetupConfig config) => request.Properties[SETUP_CONFIG] = config;

    }

}
