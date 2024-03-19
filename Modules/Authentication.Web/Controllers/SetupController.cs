using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Razor;

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
            return RenderAccountEntry("Setup", "Create Account");
        }

        [ControllerAction(RequestMethod.POST)]
        public async Task<IHandlerBuilder> Index(string user, string password, IRequest request)
        {
            await PerformSetup(request, user, password);

            return Redirect.To("{web-auth}/", true);
        }

    }

}
