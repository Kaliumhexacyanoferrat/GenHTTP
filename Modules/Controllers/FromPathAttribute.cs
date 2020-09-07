using System;

namespace GenHTTP.Modules.Controllers
{

    /// <summary>
    /// Marking an argument of a controller method with this attribute causes
    /// the value of the argument to be read from the request path.
    /// </summary>
    /// <remarks>
    /// Specifying arguments annotated with this attribute will alter the path
    /// the action matches (e.g. /action/argument/). You may specify as many
    /// path arguments as you like, but they must not be optional.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromPathAttribute : Attribute
    {

    }

}
