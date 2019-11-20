using System.Text.RegularExpressions;

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
        NoData,
        Unknown
    }

    internal class Scanner
    {
        private const string NEW_LINE = "\r\n";

        #region Get-/Setters

        public Token Current { get; private set; }

        public string Value { get; private set; }

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
            var content = Buffer.GetString();

            if (Current == Token.Unknown)
            {
                return Match(content, " ", Token.Method);
            }
            else if (Current == Token.Method)
            {
                return Match(content, " ", Token.Url);
            }
            else if (Current == Token.Url)
            {
                if (TryMatch(content, NEW_LINE, out var protocol))
                {
                    Value = protocol.Substring(5);
                    return Current = Token.Protocol;
                }
            }
            else if (Current == Token.Protocol || Current == Token.HeaderContent)
            {
                if (content.StartsWith(NEW_LINE))
                {
                    Buffer.Advance(2);
                    return Current = Token.NewLine;
                }
                else
                {
                    return Match(content, ":", Token.HeaderDefinition);
                }
            }
            else if (Current == Token.HeaderDefinition)
            {
                return Match(content, NEW_LINE, Token.HeaderContent);
            }

            Value = string.Empty;
            return Token.Unknown;
        }
        
        private Token Match(string content, string separator, Token newToken)
        {
            if (TryMatch(content, separator, out var value))
            {
                Value = value;
                return Current = newToken;
            }

            Value = string.Empty;
            return Token.Unknown;
        }

        private bool TryMatch(string content, string separator, out string value)
        {
            var index = content.IndexOf(separator);

            if (index > -1)
            {
                value = content.Substring(0, index).Trim();
                Buffer.Advance((ushort)(index + separator.Length));

                return true;
            }

            value = string.Empty;
            return false;
        }

        #endregion

    }

}
