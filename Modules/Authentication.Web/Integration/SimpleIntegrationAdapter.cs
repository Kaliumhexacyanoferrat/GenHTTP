using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Authentication.Web.Integration
{

    public class SimpleIntegrationAdapter : IWebAuthIntegration
    {

        bool AllowAnonymous { get; }

        string SetupRoute { get;}

        string LoginRoute { get; }

        public IHandlerBuilder SetupHandler { get; private set; }

        public IHandlerBuilder LoginHandler { get; private set; }

        private ISimpleWebAuthIntegration SimpleIntegration { get; }

        public SimpleIntegrationAdapter(ISimpleWebAuthIntegration simpleIntegration)
        {
            SimpleIntegration = simpleIntegration;

            AllowAnonymous = simpleIntegration.AllowAnonymous;

            SetupRoute = simpleIntegration.SetupRoute;
            LoginRoute = simpleIntegration.LoginRoute;

            SetupHandler = Controller.From(new SetupController((r, u, p) => simpleIntegration.PerformSetup(r, u, p)));
            LoginHandler = Controller.From(new LoginController((r, u, p) => simpleIntegration.PerformLogin(r, u, p)));
        }

        public ValueTask<bool> CheckSetupRequired(IRequest request) => SimpleIntegration.CheckSetupRequired(request);

        public ValueTask<string> StartSessionAsync(IRequest request, IUser user) => SimpleIntegration.StartSessionAsync(request, user);

        public ValueTask<IUser?> VerifyTokenAsync(string sessionToken) => SimpleIntegration.VerifyTokenAsync(sessionToken);

    }

}
