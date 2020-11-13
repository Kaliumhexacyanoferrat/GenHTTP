using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GenHTTP.Modules.ReverseProxy.Provider
{

    public static class BrokenCookieHeaderParser
    {
        private static readonly Regex EXPIRATION_FIX = new("expires=[A-Za-z]{3},", RegexOptions.Compiled);
        private static readonly Regex EXPIRATION_RE_FIX = new("expires=[A-Za-z]{3}-", RegexOptions.Compiled);

        public static List<string> GetCookies(string brokenLine)
        {
            var line = EXPIRATION_FIX.Replace(brokenLine, m => m.Value.Substring(0, m.Value.Length - 1) + "-");

            var parts = line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var result = new List<string>();

            foreach (var part in parts)
            {
                result.Add(EXPIRATION_RE_FIX.Replace(part, m => m.Value.Substring(0, m.Value.Length - 1) + ",").Trim());
            }

            return result;
        }

    }

}