using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Modules.Core;

namespace GenHTTP.Examples.Examples.Infrastructure.Exceptions
{

    public class ExceptionExample
    {

        public static IRouterBuilder Create()
        {
            return Layout.Create()
                         .Add("index", new ExceptionProvider(), true);
        }

    }

}

