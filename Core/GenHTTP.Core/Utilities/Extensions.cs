using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Core.Utilities
{

    internal static class Extensions
    {

        internal static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

    }

}
