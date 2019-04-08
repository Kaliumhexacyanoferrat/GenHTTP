using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public class ExceptionProvider : IContentProvider
    {

        public IResponseBuilder Handle(IRequest request)
        {
            throw new Exception("Something went utterly wrong!");
        }

    }

}
