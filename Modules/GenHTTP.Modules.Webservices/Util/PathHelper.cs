using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Modules.Webservices.Util
{

    internal static class PathHelper
    {

        private static readonly Regex EMPTY = new Regex("^(/|)$", RegexOptions.Compiled);

        private static readonly Regex VAR_PATTERN = new Regex(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static Regex Parse(string? path)
        {
            if (path != null)
            {
                var builder = new StringBuilder(path);

                // convert parameters of the format ":var" into appropriate groups
                foreach (Match match in VAR_PATTERN.Matches(path))
                {
                    builder.Replace(match.Value, @$"(?<{match.Groups[1].Value}>[a-z0-9]+)");
                }

                return new Regex($"^/{builder.ToString()}$");
            }

            return EMPTY;
        }

    }

}
