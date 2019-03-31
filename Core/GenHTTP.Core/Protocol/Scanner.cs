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

        protected bool LastTokenMethod { get; set; }

        #endregion

        #region Initialization

        internal Scanner(RequestBuffer buffer)
        {
            Buffer = buffer;

            LastTokenMethod = false;

            Current = Token.Unknown;
            Value = string.Empty;
        }

        #endregion

        #region Functionality

        internal async Task<Token> NextToken()
        {
            await IsMatch(Pattern.WHITESPACE);

            if (await IsMatch(Pattern.NEW_LINE))
            {
                return Current = Token.NewLine;
            }

            if (await IsMatch(Pattern.HTTP))
            {
                return Current = Token.Protocol;
            }

            if (await IsMatch(Pattern.METHOD))
            {
                LastTokenMethod = true;
                return Current = Token.Method;
            }

            if (LastTokenMethod)
            {
                if (await IsMatch(Pattern.URL))
                {
                    LastTokenMethod = false;
                    return Current = Token.Url;
                }
            }

            if (await IsMatch(Pattern.HEADER_DEFINITION))
            {
                return Current = Token.HeaderDefinition;
            }

            if (await IsMatch(Pattern.HEADER_CONTENT))
            {
                return Current = Token.HeaderContent;
            }
            
            return Current = Token.Unknown;
        }

        private async Task<bool> IsMatch(Regex re)
        {
            var content = await Buffer.GetString();

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
