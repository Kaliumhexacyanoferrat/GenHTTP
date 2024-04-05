using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication.Web.ViewModels;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Razor;

namespace GenHTTP.Modules.Authentication.Web.Controllers
{
    
    public static class View
    {

        public static IHandlerBuilder RenderAccountEntry(string title, string buttonCaption, string? username = null, string? errorMessage = null, ResponseStatus? status = null)
        {
            var response = ModRazor.Page(Resource.FromAssembly("EnterAccount.cshtml"), (r, h) => new ViewModel<EnterAccountModel>(r, h, new(buttonCaption, username, errorMessage)))
                                   .AddStyle("{web-auth-resources}/style.css")
                                   .Title(title);

            if (status != null)
            {
                response.Status(status.Value);
            }

            return response;
        }

    }

}
