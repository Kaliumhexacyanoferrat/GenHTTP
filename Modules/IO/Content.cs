using System.IO;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Content.IO;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    // todo: unterscheidung von content und download (z.B. download nur get/head, content egal, disposition)
    // todo: alle stellen ändern, von content anstatt download sinn ergibt 

    public static class Content
    {

        public static ContentProviderBuilder FromString(string content) => From(Resource.FromString(content).Type(ContentType.TextPlain));

        public static ContentProviderBuilder FromFile(string file) => FromFile(new FileInfo(file));

        public static ContentProviderBuilder FromFile(FileInfo file) => From(Resource.FromFile(file));

        public static ContentProviderBuilder FromResource(string fileName) => FromResource(Assembly.GetCallingAssembly(), fileName);

        public static ContentProviderBuilder FromResource(Assembly source, string fileName) => From(Resource.FromAssembly(source, fileName).Name(fileName));

        public static ContentProviderBuilder From(IBuilder<IResource> resource) => From(resource.Build());

        public static ContentProviderBuilder From(IResource resource) => new ContentProviderBuilder().Resource(resource);

    }

}
