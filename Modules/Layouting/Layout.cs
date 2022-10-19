using GenHTTP.Modules.Layouting.Provider;

namespace GenHTTP.Modules.Layouting
{

    public static class Layout
    {

        /// <summary>
        /// Creates a new layout that can be used to route requests.
        /// </summary>
        /// <returns>The newly created layout builder</returns>
        public static LayoutBuilder Create()
        {
            return new LayoutBuilder();
        }

    }

}
