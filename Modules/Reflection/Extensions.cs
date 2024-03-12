using System;
using System.Reflection;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Reflection
{

    public static class Extensions
    {

        /// <summary>
        /// Checks, whether the given parameter can be passed via the URL.
        /// </summary>
        /// <param name="info">The parameter to be analyzes</param>
        /// <returns><c>true</c>, if the given parameter can be passed via the URL</returns>
        public static bool CanFormat(this ParameterInfo info, FormatterRegistry formatters)
        {
            return info.CheckNullable() || formatters.CanHandle(info.ParameterType);
        }

        /// <summary>
        /// Checks, whether the given parameter is a nullable value type.
        /// </summary>
        /// <param name="info">The parameter to be analyzes</param>
        /// <returns><c>true</c>, if the given parameter is a nullable value type</returns>
        public static bool CheckNullable(this ParameterInfo info)
        {
            return Nullable.GetUnderlyingType(info.ParameterType) is not null;
        }

        /// <summary>
        /// Creates a regular expression that will match the name of the
        /// given parameter.
        /// </summary>
        /// <param name="name">The name to generate the expression from</param>
        /// <returns>The newly created expression</returns>
        public static string ToParameter(this string name)
        {
            return @$"(?<{name}>[^/]+)";
        }

    }

}
