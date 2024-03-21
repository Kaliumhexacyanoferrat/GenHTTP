using System.Threading.Tasks;

using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.Web
{
    
    public interface ISimpleWebAuthIntegration
    {

        bool AllowAnonymous { get => false; }

        string SetupRoute { get => "setup"; }

        string LoginRoute { get => "login"; }

        string ResourceRoute { get => "auth-resources"; }

        ValueTask<bool> CheckSetupRequired(IRequest request);

        ValueTask PerformSetup(IRequest request, string username, string password);

        ValueTask<IUser?> VerifyTokenAsync(string sessionToken);

        ValueTask<string> StartSessionAsync(IRequest request, IUser user);

        ValueTask<IUser?> PerformLogin(IRequest request, string username, string password);

    }

}
