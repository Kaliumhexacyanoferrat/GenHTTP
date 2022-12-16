using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;

namespace GenHTTP.Modules.AutoLayout.Scanning
{

    public static class HandlerResolver
    {

        public static IHandlerBuilder Resolve(IResource resource)
        {
            var type = resource.ContentType ?? FlexibleContentType.Get(resource.Name?.GuessContentType() ?? ContentType.ApplicationForceDownload);

            switch (type)
            {
                default: return Content.From(resource);
            }
        }

    }

}
