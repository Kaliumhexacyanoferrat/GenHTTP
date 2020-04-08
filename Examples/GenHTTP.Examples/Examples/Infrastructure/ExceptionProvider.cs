using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Examples.Examples.Infrastructure
{

    public class ExceptionProvider : IHandler
    {

        public FlexibleContentType? ContentType => null;

        public string? Title => null;

        public IHandler Parent { get; }

        public ExceptionProvider(IHandler parent)
        {
            Parent = parent;
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            throw new NotImplementedException();
        }

        public IResponse? Handle(IRequest request)
        {
            throw new Exception("Something went utterly wrong!");
        }

    }

}
