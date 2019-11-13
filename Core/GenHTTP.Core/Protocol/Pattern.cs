using System.Text.RegularExpressions;

namespace GenHTTP.Core
{

    /// <summary>
    /// Regular expressions used by this server.
    /// </summary>
    internal static class Pattern
    {

        public static readonly Regex HTTP = new Regex(@"^HTTP/((?:1\.0)|(?:1\.1))\r\n", RegexOptions.Compiled);

        public static readonly Regex METHOD = new Regex("^([A-Z]+) ", RegexOptions.Compiled);

        public static readonly Regex URL = new Regex("^([^ ]+) ", RegexOptions.Compiled);

        public static readonly Regex NEW_LINE = new Regex(@"^(\r\n)", RegexOptions.Compiled);

        public static readonly Regex WHITESPACE = new Regex(@"^([ \t]+)", RegexOptions.Compiled);

        public static readonly Regex CONTENT = new Regex("^(.+)", RegexOptions.Compiled);

        public static readonly Regex GET_PARAMETER = new Regex("([^&=]+)=([^&]*)", RegexOptions.Compiled);

        public static readonly Regex HEADER_DEFINITION = new Regex(@"^([^: \n]+): ", RegexOptions.Compiled);

        public static readonly Regex HEADER_CONTENT = new Regex(@"^([^\n]+)\r\n", RegexOptions.Compiled);

    }

}
