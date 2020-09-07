using GenHTTP.Api.Routing;
using System.Text.RegularExpressions;

namespace GenHTTP.Modules.Reflection
{

    public class MethodRouting
    {

        #region Get-/Setters

        /// <summary>
        /// The path of the method, with placeholders for
        /// path variables (":var").
        /// </summary>
        public WebPath Path { get; }

        /// <summary>
        /// The path of the method, converted into a regular
        /// expression to be evaluated at runtime.
        /// </summary>
        public Regex ParsedPath { get; }

        /// <summary>
        /// The first segment of the raw path, if any.
        /// </summary>
        public string? Segment { get; }

        #endregion

        #region Initialization

        public MethodRouting(string path, string pathExpression, string? segment)
        {
            Path = new PathBuilder(path).Build();
            ParsedPath = new Regex(pathExpression, RegexOptions.Compiled);
            
            Segment = segment;
        }

        #endregion

    }

}
