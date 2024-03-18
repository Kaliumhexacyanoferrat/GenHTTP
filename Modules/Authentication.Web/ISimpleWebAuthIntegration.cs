using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Authentication.Web
{
    
    public interface ISimpleWebAuthIntegration
    {

        bool AllowAnonymous { get => false; }

        string SetupRoute { get => "setup"; }

        string LoginRoute { get => "login"; }

        ValueTask<bool> CheckSetupRequired(IRequest request);

        ValueTask PerformSetup(IRequest request, string username, string password);

        ValueTask<IUser?> VerifyTokenAsync(string sessionToken);

        ValueTask<string> StartSessionAsync(IRequest request, IUser user);

        ValueTask<IUser?> PerformLogin(IRequest request, string username, string password);

    }

}
