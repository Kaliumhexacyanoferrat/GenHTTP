namespace GenHTTP.Modules.Authentication.Web.ViewModels
{
    
    public record EnterAccountModel
    (

        string ButtonCaption,

        string? Username = null,

        string? ErrorMessage = null

    );
    
}
