using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{
    
    public interface IWebAuthIntegration
    {

        bool AllowAnonymous { get => false; }

        IHandlerBuilder SetupHandler { get; }

        string SetupRoute { get => "setup"; }

        IHandlerBuilder LoginHandler { get; }

        string LoginRoute { get => "login"; }

        IHandlerBuilder LogoutHandler { get; }

        string LogoutRoute { get => "logout"; }

        IHandlerBuilder ResourceHandler { get; }

        string ResourceRoute { get => "auth-resources"; }

        ValueTask<bool> CheckSetupRequired(IRequest request);

        ValueTask<IUser?> VerifyTokenAsync(string sessionToken);

        ValueTask<string> StartSessionAsync(IRequest request, IUser user);

    }

}
