using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Modules.Core;

using GenHTTP.Examples.Examples.Infrastructure.Exceptions;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public static class InfrastructureExamples
    {

        public static IRouterBuilder Create()
        {
            return Layout.Create()
                         .Add("exceptions", ExceptionExample.Create());
        }

    }

}
