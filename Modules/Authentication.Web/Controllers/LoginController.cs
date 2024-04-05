using System;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Controllers;
using GenHTTP.Modules.Placeholders;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{

    public class LoginController 
    {

        private Func<IRequest, string, string, ValueTask<IUser?>> PerformLogin { get; }

        public LoginController(Func<IRequest, string, string, ValueTask<IUser?>> performLogin)
        {
            PerformLogin = performLogin;
        }

        public IHandlerBuilder Index(IRequest request)
        {
            var user = request.GetUser<IUser>();

            if (user != null)
            {
                return Page.From("Login", "You are already logged in.");
            }

            return RenderLogin(status: ResponseStatus.Unauthorized);
        }

        [ControllerAction(RequestMethod.POST)]
        public async Task<IHandlerBuilder> Index(string user, string password, IRequest request)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                return RenderLogin(user, "Please enter username and password.", status: ResponseStatus.BadRequest);
            }

            var authenticatedUser = await PerformLogin(request, user, password);

            if (authenticatedUser != null)
            {
                request.SetUser(authenticatedUser);

                return Redirect.To("{web-auth}/", true);
            }
            else
            {
                return RenderLogin(user, "Invalid username or password.", status: ResponseStatus.Forbidden);
            }
        }

        private static IHandlerBuilder RenderLogin(string? username = null, string? errorMessage = null, ResponseStatus? status = null)
            => View.RenderAccountEntry("Login", "Login", username, errorMessage, status);

    }

}
