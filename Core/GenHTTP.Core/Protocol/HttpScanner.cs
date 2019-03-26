using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GenHTTP.Core
{

    /// <summary>
    /// All possible Tokens in a HTTP-Request
    /// </summary>
    internal enum HttpToken
    {
        Http,
        Method,
        Url,
        HeaderDefinition,
        HeaderContent,
        NewLine,
        Content,
        Unknown
    }

    /// <summary>
    /// Scans the stream for the <see cref="HttpParser" />.
    /// </summary>
    internal class HttpScanner
    {

        #region Get-/Setters

        public HttpToken Current { get; protected set; }

        public string Value { get; protected set; }

        public bool UseContentPattern { get; set; }

        protected string PatternContent { get; set; }

        protected StringBuilder Buffer { get; }
        
        protected bool LastTokenMethod { get; set; }

        #endregion

        #region Initialization

        internal HttpScanner()
        {
            Buffer = new StringBuilder();

            LastTokenMethod = false;
            PatternContent = @"^(.+)";

            Current = HttpToken.Unknown;
            Value = string.Empty;
        }

        #endregion

        #region Functionality

        internal HttpToken NextToken()
        {
            if (!UseContentPattern)
            {
                IsMatch(Pattern.WHITESPACE);

                if (IsMatch(Pattern.HTTP))
                {
                    return Current = HttpToken.Http;
                }

                if (IsMatch(Pattern.METHOD))
                {
                    LastTokenMethod = true;
                    return Current = HttpToken.Method;
                }

                if (LastTokenMethod)
                {
                    if (IsMatch(Pattern.URL))
                    {
                        LastTokenMethod = false;
                        return Current = HttpToken.Url;
                    }
                }

                if (IsMatch(Pattern.HEADER_DEFINITION))
                {
                    return Current = HttpToken.HeaderDefinition;
                }

                if (IsMatch(Pattern.HEADER_CONTENT))
                {
                    return Current = HttpToken.HeaderContent;
                }

                if (IsMatch(Pattern.NEW_LINE))
                {
                    return Current = HttpToken.NewLine;
                }
            }
            else
            {
                if (IsMatch(PatternContent))
                {
                    UseContentPattern = false;
                    return Current = HttpToken.Content;
                }
            }

            return Current = HttpToken.Unknown;
        }

        internal void SetContentLength(long length)
        {
            PatternContent = @"^((?:.|\n){" + length + "})";
        }

        internal void Append(string value)
        {
            Buffer.Append(value);
        }

        private bool IsMatch(string pattern)
        {
            return IsMatch(new Regex(pattern));
        }

        private bool IsMatch(Regex re)
        {
            var content = Buffer.ToString();

            var match = re.Match(content);

            if (match.Success)
            {
                Value = match.Groups[1].Value;
                Buffer.Remove(0, match.Length);

                return true;
            }

            return false;
        }

        #endregion

    }

}
