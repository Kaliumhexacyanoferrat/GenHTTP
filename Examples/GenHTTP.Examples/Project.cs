using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Routing;
using GenHTTP.Examples.Examples.Infrastructure;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Scriban;

namespace GenHTTP.Examples
{

    public static class Project
    {

        public static IRouterBuilder Build()
        {
            var index = ModScriban.Page(Data.FromResource("Index.html"))
                                  .Title("Examples");

            return Layout.Create()
                         .Add("infrastructure", InfrastructureExamples.Create())
                         .Add("index", index, true);
        }

    }

}
