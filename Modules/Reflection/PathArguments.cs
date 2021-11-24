using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Modules.Reflection
{

    public static class PathArguments
    {
        private static readonly MethodRouting EMPTY = new("/", "^(/|)$", null, true);

        private static readonly Regex VAR_PATTERN = new(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Parses the given path and returns a routing structure
        /// expected by the method provider to check which logic
        /// to be executed on request.
        /// </summary>
        /// <param name="path">The path to be analyzed</param>
        /// <returns>The routing information to be used by the method provider</returns>
        public static MethodRouting Route(string? path)
        {
            if (path is not null)
            {
                var builder = new StringBuilder(path);

                // convert parameters of the format ":var" into appropriate groups
                foreach (Match match in VAR_PATTERN.Matches(path))
                {
                    builder.Replace(match.Value, match.Groups[1].Value.ToParameter());
                }

                var splitted = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                return new MethodRouting(path, $"^/{builder}$", (splitted.Length > 0) ? splitted[0] : null, false);
            }

            return EMPTY;
        }

    }

}
