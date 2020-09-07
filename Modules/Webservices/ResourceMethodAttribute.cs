using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceMethodAttribute : MethodAttribute
    {

        #region Get-/Setters

        /// <summary>
        /// The path this method is availabe at.
        /// </summary>
        public string? Path { get; set; }

        #endregion

        #region Initialization

        /// <summary>
        /// Marks the method as a webservice method.
        /// </summary>
        /// <param name="requestMethod">The HTTP verb used to invoke the method</param>
        /// <param name="path">The path the method should be available at</param>
        public ResourceMethodAttribute(RequestMethod requestMethod = RequestMethod.GET, string? path = null) : base(requestMethod)
        {
            Path = path;
        }

        /// <summary>
        /// Configures the method to be invoked via GET at the given path.
        /// </summary>
        /// <param name="path">The path the method should be available at</param>
        public ResourceMethodAttribute(string path) : this(RequestMethod.GET, path)
        {

        }

        #endregion

    }

}
