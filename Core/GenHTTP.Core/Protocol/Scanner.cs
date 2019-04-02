using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Core.Protocol;

namespace GenHTTP.Core
{

    internal enum Token
    {
        Protocol,
        Method,
        Url,
        HeaderDefinition,
        HeaderContent,
        NewLine,
        Unknown
    }

    internal class Scanner
    {

        #region Get-/Setters

        public Token Current { get; protected set; }

        public string Value { get; protected set; }

        public RequestBuffer Buffer { get; }

        public bool StatusLine { get; set; }

        #endregion

        #region Initialization

        internal Scanner(RequestBuffer buffer)
        {
            Buffer = buffer;

            StatusLine = true;

            Current = Token.Unknown;
            Value = string.Empty;
        }

        #endregion

        #region Functionality

        internal Token NextToken()
        {
            IsMatch(Pattern.WHITESPACE);

            if (IsMatch(Pattern.NEW_LINE))
            {
                return Current = Token.NewLine;
            }

            if (StatusLine)
            {
                if (Current != Token.Method)
                {
                    if (IsMatch(Pattern.METHOD))
                    {
                        return Current = Token.Method;
                    }
                }
                
                if (Current != Token.Url)
                {
                    if (IsMatch(Pattern.URL))
                    {
                        return Current = Token.Url;
                    }
                }

                if (IsMatch(Pattern.HTTP))
                {
                    StatusLine = false;
                    return Current = Token.Protocol;
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

            return Current = Token.Unknown;
        }

        private bool IsMatch(Regex re)
        {
            var content = Buffer.GetString();

            var match = re.Match(content);

            if (match.Success)
            {
                Value = match.Groups[1].Value;
                Buffer.Advance((ushort)match.Length);

                return true;
            }

            return false;
        }

        #endregion

    }

}
