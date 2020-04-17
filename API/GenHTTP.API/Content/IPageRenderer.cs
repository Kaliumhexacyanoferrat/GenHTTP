using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Content
{

    public interface IPageRenderer
    {

        IResponseBuilder Render(TemplateModel model);

    }

}
