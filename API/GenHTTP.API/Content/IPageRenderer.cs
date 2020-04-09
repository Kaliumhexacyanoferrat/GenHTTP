using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Content
{
    
    public interface IPageRenderer
    {

        IResponseBuilder Render(TemplateModel model);

    }

}
