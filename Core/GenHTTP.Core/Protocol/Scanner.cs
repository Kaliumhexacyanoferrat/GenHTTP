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
    internal enum Token
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
    /// Scans the stream for the <see cref="Parser" />.
    /// </summary>
    internal class Scanner
    {

        #region Get-/Setters

        public Token Current { get; protected set; }

        public string Value { get; protected set; }

        public bool UseContentPattern { get; set; }

        protected string PatternContent { get; set; }

        protected StringBuilder Buffer { get; }
        
        protected bool LastTokenMethod { get; set; }

        #endregion

        #region Initialization

        internal Scanner()
        {
            Buffer = new StringBuilder();

            LastTokenMethod = false;
            PatternContent = @"^(.+)";

            Current = Token.Unknown;
            Value = string.Empty;
        }

        #endregion

        #region Functionality

        internal Token NextToken()
        {
            if (!UseContentPattern)
            {
                IsMatch(Pattern.WHITESPACE);

                if (IsMatch(Pattern.HTTP))
                {
                    return Current = Token.Http;
                }

                if (IsMatch(Pattern.METHOD))
                {
                    LastTokenMethod = true;
                    return Current = Token.Method;
                }

                if (LastTokenMethod)
                {
                    if (IsMatch(Pattern.URL))
                    {
                        LastTokenMethod = false;
                        return Current = Token.Url;
                    }
                }

                if (IsMatch(Pattern.HEADER_DEFINITION))
                {
                    return Current = Token.HeaderDefinition;
                }

                if (IsMatch(Pattern.HEADER_CONTENT))
                {
                    return Current = Token.HeaderContent;
                }

                if (IsMatch(Pattern.NEW_LINE))
                {
                    return Current = Token.NewLine;
                }
            }
            else
            {
                if (IsMatch(PatternContent))
                {
                    UseContentPattern = false;
                    return Current = Token.Content;
                }
            }

            return Current = Token.Unknown;
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
