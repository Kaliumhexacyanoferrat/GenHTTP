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

    public sealed class SetupController
    {

        public IHandlerBuilder Index()
        {
            return ModRazor.Page(Resource.FromAssembly("EnterAccount.cshtml"), (r, h) => new BasicModel(r, h))
                           .Title("Setup");
        }

        [ControllerAction(RequestMethod.POST)]
        public async Task<IHandlerBuilder> Index(string user, string password, IRequest request)
        {
            var setupConfig = Setup.GetConfig(request);

            var result = await setupConfig.PerformSetup!(request, user, password);

            return Redirect.To("{web-auth}/", true);
        }

    }

}
