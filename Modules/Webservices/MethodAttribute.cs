using System;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Attribute indicating that this method can be invoked
    /// via a webservice call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodAttribute : Attribute
    {

        #region Get-/Setters

        /// <summary>
        /// The HTTP verb used to invoke this method.
        /// </summary>
        public FlexibleRequestMethod RequestMethod { get; set; }

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
        public MethodAttribute(RequestMethod requestMethod = Api.Protocol.RequestMethod.GET, string? path = null)
        {
            RequestMethod = new FlexibleRequestMethod(requestMethod);
            Path = path;
        }

        /// <summary>
        /// Configures the method to be invoked via GET at the given path.
        /// </summary>
        /// <param name="path">The path the method should be available at</param>
        public MethodAttribute(string path) : this(Api.Protocol.RequestMethod.GET, path)
        {

        }

        #endregion

    }

}
