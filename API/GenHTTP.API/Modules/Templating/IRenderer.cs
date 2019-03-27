using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Modules.Templating
{

    public interface IRenderer<T> where T : IBaseModel
    {

        string Render(T model);

    }

}
