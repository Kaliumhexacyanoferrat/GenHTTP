using System.Text.RegularExpressions;

using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Reflection
{

    public sealed class MethodRouting
    {
        private readonly string _PathExpression;

        private Regex? _ParsedPath;

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
        public Regex ParsedPath => _ParsedPath ??= new(_PathExpression, RegexOptions.Compiled);

        /// <summary>
        /// The first segment of the raw path, if any.
        /// </summary>
        public string? Segment { get; }

        /// <summary>
        /// True, if this route matches the index of the
        /// scoped segment.
        /// </summary>
        public bool IsIndex { get; }

        #endregion

        #region Initialization

        public MethodRouting(string path, string pathExpression, string? segment, bool isIndex)
        {
            Path = new PathBuilder(path).Build();

            _PathExpression = pathExpression;
            
            Segment = segment;
            IsIndex = isIndex;
        }

        #endregion

    }

}
