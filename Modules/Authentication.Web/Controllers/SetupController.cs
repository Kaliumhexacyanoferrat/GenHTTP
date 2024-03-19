using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{

    public sealed class SetupController : BaseController
    {

        private Func<IRequest, string, string, ValueTask> PerformSetup { get; }

        public SetupController(Func<IRequest, string, string, ValueTask> performSetup)
        {
            PerformSetup = performSetup;
        }

        public IHandlerBuilder Index()
        {
            return RenderSetup();
        }

        [ControllerAction(RequestMethod.POST)]
        public async Task<IHandlerBuilder> Index(string user, string password, IRequest request)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                return RenderSetup(user, "Please enter username and password.");
            }

            await PerformSetup(request, user, password);

            return Redirect.To("{web-auth}/", true);
        }

        private IHandlerBuilder RenderSetup(string? username = null, string? errorMessage = null) => RenderAccountEntry("Setup", "Create Account", username, errorMessage);

    }

}
