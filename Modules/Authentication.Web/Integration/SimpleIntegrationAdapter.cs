using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web.Controllers;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.Authentication.Web.Integration
{

    public class SimpleIntegrationAdapter<TUser> : IWebAuthIntegration<TUser> where TUser : IUser
    {
        private readonly bool _AllowAnonymous;

        private readonly string _SetupRoute, _LoginRoute, _ResourceRoute, _LogoutRoute;

        bool IWebAuthIntegration<TUser>.AllowAnonymous { get => _AllowAnonymous; }

        string IWebAuthIntegration<TUser>.SetupRoute { get => _SetupRoute; }

        string IWebAuthIntegration<TUser>.LoginRoute { get => _LoginRoute; }

        string IWebAuthIntegration<TUser>.LogoutRoute { get => _LogoutRoute; }

        string IWebAuthIntegration<TUser>.ResourceRoute { get => _ResourceRoute; }

        public IHandlerBuilder SetupHandler { get; private set; }

        public IHandlerBuilder LoginHandler { get; private set; }

        public IHandlerBuilder LogoutHandler { get; private set; }

        public IHandlerBuilder ResourceHandler { get; private set; }

        private ISimpleWebAuthIntegration<TUser> SimpleIntegration { get; }

        public SimpleIntegrationAdapter(ISimpleWebAuthIntegration<TUser> simpleIntegration)
        {
            SimpleIntegration = simpleIntegration;

            _AllowAnonymous = simpleIntegration.AllowAnonymous;

            _SetupRoute = simpleIntegration.SetupRoute;
            _LoginRoute = simpleIntegration.LoginRoute;
            _LogoutRoute = simpleIntegration.LogoutRoute;

            _ResourceRoute = simpleIntegration.ResourceRoute;

            SetupHandler = Controller.From(new SetupController((r, u, p) => simpleIntegration.PerformSetup(r, u, p)));
            LoginHandler = Controller.From(new LoginController<TUser>((r, u, p) => simpleIntegration.PerformLoginAsync(r, u, p)));
            LogoutHandler = Controller.From(new LogoutController());

            ResourceHandler = Resources.From(ResourceTree.FromAssembly("Resources"));
        }

        public ValueTask<bool> CheckSetupRequired(IRequest request) => SimpleIntegration.CheckSetupRequired(request);

        public ValueTask<string> StartSessionAsync(IRequest request, TUser user) => SimpleIntegration.StartSessionAsync(request, user);

        public ValueTask<TUser?> VerifyTokenAsync(IRequest request, string sessionToken) => SimpleIntegration.VerifyTokenAsync(request, sessionToken);

    }

}
