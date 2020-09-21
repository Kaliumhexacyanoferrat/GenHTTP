using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO
{

    public static class Content
    {

        public static StringProviderBuilder From(string content)
        {
            return new StringProviderBuilder().Data(content).Type(ContentType.TextPlain);
        }

    }

}
