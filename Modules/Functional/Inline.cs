using GenHTTP.Modules.Functional.Provider;

namespace GenHTTP.Modules.Functional
{

    public static class Inline
    {

        /// <summary>
        /// Creates a functional handler that accepts delegates
        /// which are executed to respond to incoming requests.
        /// </summary>
        public static InlineBuilder Create() => new();

    }

}
