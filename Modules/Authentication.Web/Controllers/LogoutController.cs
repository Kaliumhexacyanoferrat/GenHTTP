using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Placeholders;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{
    
    public class LogoutController
    {

        public LogoutController()
        {

        }

        public IHandlerBuilder Index(IRequest request)
        {
            var user = request.GetUser<IUser>();

            if (user == null)
            {
                return Page.From("Logout", "You are already logged out.");
            }
            else
            {
                request.ClearUser();
                return Page.From("Logout", "You have been successfully logged out.");
            }
        }

    }

}
