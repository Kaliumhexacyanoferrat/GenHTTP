using GenHTTP.Modules.Pages.Combined;

namespace GenHTTP.Modules.Pages
{

    public static class CombinedPage
    {

        /// <summary>
        /// Allows to assemble a web page from different kind of
        /// parts, probably using different kind of technology
        /// and rendering engines.
        /// </summary>
        /// <returns>The newly created builder</returns>
        public static CombinedPageBuilder Create() => new();

    }

}
