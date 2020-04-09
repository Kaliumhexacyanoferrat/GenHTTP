using GenHTTP.Api.Content.Templating;

namespace GenHTTP.Api.Content
{
    
    public interface IErrorHandler
    {

        TemplateModel Render(ErrorModel error);

    }

}
