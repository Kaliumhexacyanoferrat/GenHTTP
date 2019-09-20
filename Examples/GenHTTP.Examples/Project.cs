using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core;
using GenHTTP.Modules.Scriban;

using GenHTTP.Examples.Examples.Infrastructure;
using GenHTTP.Examples.Examples.Webservices;

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
                         .Add("webservice", WebserviceExamples.Create())
                         .Add("index", index, true);
        }

    }

}
