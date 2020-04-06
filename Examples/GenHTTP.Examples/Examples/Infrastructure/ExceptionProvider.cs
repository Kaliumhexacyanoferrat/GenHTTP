using System;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public class ExceptionProvider : IContentProvider
    {

        public FlexibleContentType? ContentType => null;

        public string? Title => null;

        public IResponseBuilder Handle(IRequest request)
        {
            throw new Exception("Something went utterly wrong!");
        }

    }

}
