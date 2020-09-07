using GenHTTP.Modules.Controllers.Provider;

namespace GenHTTP.Modules.Controllers
{

    public static class Controller
    {

        /// <summary>
        /// Creates a handler that will use the given controller class to generate responses.
        /// </summary>
        /// <typeparam name="T">The type of the controller to be used</typeparam>
        /// <returns>The newly created request handler</returns>
        public static ControllerBuilder<T> From<T>() where T : new() => new ControllerBuilder<T>();

    }

}
