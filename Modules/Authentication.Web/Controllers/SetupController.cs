using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

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

    }

}
