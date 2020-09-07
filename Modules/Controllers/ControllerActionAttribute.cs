using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Controllers
{

    public class ControllerActionAttribute : MethodAttribute
    {

        /// <summary>
        /// Configures the action to accept requests of the given kind.
        /// </summary>
        /// <param name="methods">The request methods which are supported by this action</param>
        public ControllerActionAttribute(params RequestMethod[] methods) : base(methods) { }

        /// <summary>
        /// Configures the action to accept requests of the given kind.
        /// </summary>
        /// <param name="methods">The request methods which are supported by this action</param>
        public ControllerActionAttribute(params FlexibleRequestMethod[] methods) : base(methods) { }

    }

}
