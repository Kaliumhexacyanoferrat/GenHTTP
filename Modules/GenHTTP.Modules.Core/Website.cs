using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Modules.Core.Websites;

namespace GenHTTP.Modules.Core
{
    
    public static class Website
    {

        public static WebsiteBuilder Create() => new WebsiteBuilder();

    }

}
