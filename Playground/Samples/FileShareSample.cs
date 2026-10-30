using GenHTTP.Api.Content;

using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.DirectoryBrowsing;
using GenHTTP.Modules.IO;

namespace GenHTTP.Playground.Samples;

public static class FileShareSample
{

    public static IHandlerBuilder Create()
    {
        /*
         *
         * Shares a folder with friends: they log in with their browser, browse
         * through the files and download them. Downloads of large files can be
         * paused and resumed, as range requests are supported.
         *
         * Open http://localhost:8080/file-share/ and log in as "friend" / "secret".
         *
         * See https://genhttp.org/documentation/content/handlers/listing/
         * and https://genhttp.org/documentation/content/concerns/range-support/
         *
         */

        var files = ResourceTree.FromDirectory("./");

        var auth = BasicAuthentication.Create()
                                      .Add("friend", "secret");

        return Listing.From(files)
                      .AddRangeSupport()
                      .Authentication(auth);
    }

}
