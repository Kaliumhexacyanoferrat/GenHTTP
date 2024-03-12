using GenHTTP.Api.Protocol;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Authentication.Web
{
    
    public interface IWebAuthenticationBackend
    {

        ValueTask<bool> CheckSetupRequired(IRequest request);

    }

}
