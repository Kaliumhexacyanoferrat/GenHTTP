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
        private string _Value;
        private HttpToken _Current = HttpToken.Unknown;
        private bool _LastTokenMethod = false;
        private bool _UseContentPattern = false;
        private StringBuilder _ToScan;

        private string _PatternContent = @"^(.+)";

        internal HttpScanner()
        {
            _ToScan = new StringBuilder();
        }

        internal bool UseContentPattern
        {
            get { return _UseContentPattern; }
            set { _UseContentPattern = value; }
        }

        internal void SetContentLength(long length)
        {
            _PatternContent = @"^((?:.|\n){" + length + "})";
        }

        internal HttpToken NextToken()
        {
            if (!_UseContentPattern)
            {
                IsMatch(Pattern.WHITESPACE);

                if (IsMatch(Pattern.HTTP))
                {
                    return _Current = HttpToken.Http;
                }

                if (IsMatch(Pattern.METHOD))
                {
                    _LastTokenMethod = true;
                    return _Current = HttpToken.Method;
                }

                if (_LastTokenMethod)
                {
                    if (IsMatch(Pattern.URL))
                    {
                        _LastTokenMethod = false;
                        return _Current = HttpToken.Url;
                    }
                }

                if (IsMatch(Pattern.HEADER_DEFINITION))
                {
                    return _Current = HttpToken.HeaderDefinition;
                }

                if (IsMatch(Pattern.HEADER_CONTENT))
                {
                    return _Current = HttpToken.HeaderContent;
                }

                if (IsMatch(Pattern.NEW_LINE))
                {
                    return _Current = HttpToken.NewLine;
                }
            }
            else
            {
                if (IsMatch(_PatternContent))
                {
                    _UseContentPattern = false;
                    return _Current = HttpToken.Content;
                }
            }

            return _Current = HttpToken.Unknown;
        }

        private bool IsMatch(string pattern)
        {
            Regex re = new Regex(pattern);
            return IsMatch(re);
        }

        private bool IsMatch(Regex re)
        {
            string content = _ToScan.ToString();
            
            if (re.IsMatch(content))
            {
                Match m = re.Match(content);
                _Value = m.Groups[1].Value;
                _ToScan.Remove(0, m.Length);
                return true;
            }

            return false;
        }

        internal void AddToScan(string toScan)
        {
            _ToScan.Append(toScan);
        }

        internal HttpToken Current
        {
            get { return _Current; }
        }

        internal string Value
        {
            get { return _Value; }
        }

        public string CurrentToken
        {
            get
            {
                return Enum.GetName(typeof(HttpToken), _Current);
            }
        }


    }

}
