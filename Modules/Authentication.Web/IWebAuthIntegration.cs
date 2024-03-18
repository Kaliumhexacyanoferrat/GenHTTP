using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Authentication.Web
{
    
    public interface IWebAuthIntegration
    {

        bool AllowAnonymous { get => false; }

        IHandlerBuilder SetupHandler { get; }

        string SetupRoute { get => "setup"; }

        IHandlerBuilder LoginHandler { get; }

        string LoginRoute { get => "login"; }

        ValueTask<bool> CheckSetupRequired(IRequest request);

        ValueTask<IUser?> VerifyTokenAsync(string sessionToken);

        ValueTask<string> StartSessionAsync(IRequest request, IUser user);

    }

}
