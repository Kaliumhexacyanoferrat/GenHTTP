using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Websites.Menues;

namespace GenHTTP.Modules.Websites
{

    public static class Menu
    {

        public static GeneratedMenuBuilder From(string route)
        {
            IAsyncEnumerable<ContentElement> provider(IRequest request, IHandler handler)
            {
                var root = request.Server.Handler;

                foreach (var resolver in handler.FindParents<IHandlerResolver>(root))
                {
                    var responsible = resolver.Find(route);

                    if (responsible is not null)
                    {
                        return responsible.GetContentAsync(request);
                    }
                }

                return AsyncEnumerable.Empty<ContentElement>();
            }

            return Create(provider);
        }

        public static GeneratedMenuBuilder Create(Func<IRequest, IHandler, IAsyncEnumerable<ContentElement>> provider)
        {
            return new GeneratedMenuBuilder().Provider(provider);
        }

        public static StaticMenuBuilder Empty() => new();

    }

}
