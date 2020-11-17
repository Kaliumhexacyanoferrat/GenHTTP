using System.Text.RegularExpressions;

namespace GenHTTP.Engine
{

    /// <summary>
    /// Regular expressions used by this server.
    /// </summary>
    internal static class Pattern
    {
        
        public static readonly Regex GET_PARAMETER = new("([^&=]+)=([^&]*)", RegexOptions.Compiled);
        
    }

}
