using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Modules.Core.Listing;

namespace GenHTTP.Modules.Core
{
    
    public static class DirectoryListing
    {

        public static ListingRouterBuilder From(DirectoryInfo directory) => From(directory.FullName);

        public static ListingRouterBuilder From(string directory) => new ListingRouterBuilder().Directory(directory);

    }

}
