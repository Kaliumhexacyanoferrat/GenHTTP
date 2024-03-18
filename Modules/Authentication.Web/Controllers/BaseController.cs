using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Razor;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{

    public class BaseController
    {

        protected IHandlerBuilder RenderAccountEntry(string title)
        {
            return ModRazor.Page(Resource.FromAssembly("EnterAccount.cshtml"), (r, h) => new BasicModel(r, h))
                           .AddStyle("{web-auth-resources}/style.css")
                           .Title(title);
        }

    }

}
