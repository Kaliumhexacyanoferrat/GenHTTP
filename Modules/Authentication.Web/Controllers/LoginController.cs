using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Razor;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{
    
    public class LoginController
    {

        private Func<IRequest, string, string, ValueTask<IUser?>> PerformLogin { get; }

        public LoginController(Func<IRequest, string, string, ValueTask<IUser?>> performLogin) 
        {
            PerformLogin = performLogin;
        }

        public IHandlerBuilder Index()
        {
            // ToDo: already logged in
            return RenderLogin();
        }

        [ControllerAction(RequestMethod.POST)]
        public async Task<IHandlerBuilder> Index(string user, string password, IRequest request)
        {
            var authenticatedUser = await PerformLogin(request, user, password);

            if (authenticatedUser != null)
            {
                request.SetUser(authenticatedUser);

                return Redirect.To("{web-auth}/", true);
            }
            else
            {
                return RenderLogin();
            }
        }

        private static IHandlerBuilder RenderLogin()
        {
            return ModRazor.Page(Resource.FromAssembly("EnterAccount.cshtml"), (r, h) => new BasicModel(r, h))
                           .Title("Login");
        }

    }

}
