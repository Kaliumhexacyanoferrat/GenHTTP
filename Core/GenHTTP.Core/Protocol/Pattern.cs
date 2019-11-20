using System.Text.RegularExpressions;

namespace GenHTTP.Core
{

    /// <summary>
    /// Regular expressions used by this server.
    /// </summary>
    internal static class Pattern
    {
        
        public static readonly Regex GET_PARAMETER = new Regex("([^&=]+)=([^&]*)", RegexOptions.Compiled);
        
    }

}
