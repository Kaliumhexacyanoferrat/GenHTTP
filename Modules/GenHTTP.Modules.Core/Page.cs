using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core
{

    public static class Page
    {

        public static PageProviderBuilder From(string content)
        {
            return new PageProviderBuilder().Content(content);
        }

    }

}
