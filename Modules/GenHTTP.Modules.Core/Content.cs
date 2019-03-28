using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core
{

    public static class Content
    {

        public static StringProviderBuilder From(string content)
        {
            return new StringProviderBuilder().Data(content).Type(ContentType.TextPlain);
        }
        
    }

}
