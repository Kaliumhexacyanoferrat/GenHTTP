using GenHTTP.Api.Protocol;
using GenHTTP.Engine;
using GenHTTP.Modules.Authentication.Web;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Practices;
using System.Threading.Tasks;

var auth = WebAuthentication.Create()
                            .Backend<MyBackend>()
                            .EnableSetup();

Host.Create()
    .Handler(Content.From(Resource.FromString("Hello World")).Add(auth))
    .Defaults()
    .Development()
    .Console()
    .Run();

public class MyBackend : IWebAuthenticationBackend
{
    
    public ValueTask<bool> CheckSetupRequired(IRequest request)
    {
        return new(true);
    }

}