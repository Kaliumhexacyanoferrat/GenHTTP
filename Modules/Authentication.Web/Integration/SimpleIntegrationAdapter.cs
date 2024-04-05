using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Authentication.Web.Integration
{

    public class SimpleIntegrationAdapter : IWebAuthIntegration
    {
        private readonly bool _AllowAnonymous;

        private readonly string _SetupRoute, _LoginRoute, _ResourceRoute;

        bool IWebAuthIntegration.AllowAnonymous { get => _AllowAnonymous; }

        string IWebAuthIntegration.SetupRoute { get => _SetupRoute; }

        string IWebAuthIntegration.LoginRoute { get => _LoginRoute; }

        string IWebAuthIntegration.ResourceRoute { get => _ResourceRoute; }

        public IHandlerBuilder SetupHandler { get; private set; }

        public IHandlerBuilder LoginHandler { get; private set; }

        public IHandlerBuilder ResourceHandler { get; private set; }

        private ISimpleWebAuthIntegration SimpleIntegration { get; }

        public SimpleIntegrationAdapter(ISimpleWebAuthIntegration simpleIntegration)
        {
            SimpleIntegration = simpleIntegration;

            _AllowAnonymous = simpleIntegration.AllowAnonymous;

            _SetupRoute = simpleIntegration.SetupRoute;
            _LoginRoute = simpleIntegration.LoginRoute;
            _ResourceRoute = simpleIntegration.ResourceRoute;

            SetupHandler = Controller.From(new SetupController((r, u, p) => simpleIntegration.PerformSetup(r, u, p)));
            LoginHandler = Controller.From(new LoginController((r, u, p) => simpleIntegration.PerformLogin(r, u, p)));

            ResourceHandler = Resources.From(ResourceTree.FromAssembly("Resources"));
        }

        public ValueTask<bool> CheckSetupRequired(IRequest request) => SimpleIntegration.CheckSetupRequired(request);

        public ValueTask<string> StartSessionAsync(IRequest request, IUser user) => SimpleIntegration.StartSessionAsync(request, user);

        public ValueTask<IUser?> VerifyTokenAsync(string sessionToken) => SimpleIntegration.VerifyTokenAsync(sessionToken);

    }

}
