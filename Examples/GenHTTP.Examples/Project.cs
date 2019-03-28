using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;

using GenHTTP.Modules.Core;

namespace GenHTTP.Examples
{

    public static class Project
    {

        public static IRouterBuilder Build()
        {
            var layout = Layout.Create()
                               .Add("index", Page.From("Hello World!").Title("GenHTTP Examples"), true);

            return layout;
        }

    }

}
