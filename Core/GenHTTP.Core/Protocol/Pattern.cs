using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Core
{

    /// <summary>
    /// Regular expressions used by this server.
    /// </summary>
    internal static class Pattern
    {

        public static readonly Regex HTTP = new Regex(@"^HTTP/((?:1\.0)|(?:1\.1))\r\n");

        public static readonly Regex METHOD = new Regex("^(GET|POST|HEAD)");

        public static readonly Regex URL = new Regex("^([^ ]+) ");

        public static readonly Regex NEW_LINE = new Regex(@"^(\r\n)");

        public static readonly Regex WHITESPACE = new Regex(@"^([ \t]+)");

        public static readonly Regex CONTENT = new Regex("^(.+)");

        public static readonly Regex GET_PARAMETER = new Regex("([^&=]+)=([^&]*)");

        public static readonly Regex HEADER_DEFINITION = new Regex(@"^([^: \n]+): ");

        public static readonly Regex HEADER_CONTENT = new Regex(@"^([^\n]+)\r\n");
        
    }

}
