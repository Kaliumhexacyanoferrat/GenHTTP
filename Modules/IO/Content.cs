using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

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
