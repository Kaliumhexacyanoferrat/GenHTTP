using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Patterns;

namespace GenHTTP {

  /// <summary>
  /// All possible Tokens in a HTTP-Request
  /// </summary>
  internal enum HttpToken {
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
  internal class HttpScanner {
    private string _Value;
    private HttpToken _Current = HttpToken.Unknown;
    private bool _LastTokenMethod = false;
    private bool _UseContentPattern = false;
    private StringBuilder _ToScan;

    private static PatternWhitespace _PatternWhitespace;
    private static PatternHttp _PatternHttp;
    private static PatternMethod _PatternMethod;
    private static PatternUrl _PatternUrl;
    private static PatternHeaderDefinition _PatternHeaderDefinition;
    private static PatternHeaderContent _PatternHeaderContent;
    private static PatternNewLine _PatternNewLine;

    private string _PatternContent = @"^(.+)";

    internal HttpScanner() {
      // initialize reg ex objects on first use
      if (_PatternWhitespace == null) {
        _PatternWhitespace = new PatternWhitespace();
        _PatternHttp = new PatternHttp();
        _PatternMethod = new PatternMethod();
        _PatternUrl = new PatternUrl();
        _PatternHeaderDefinition = new PatternHeaderDefinition();
        _PatternHeaderContent = new PatternHeaderContent();
        _PatternNewLine = new PatternNewLine();
      }
      // init string builder
      _ToScan = new StringBuilder();
    }

    internal bool UseContentPattern {
      get { return _UseContentPattern; }
      set { _UseContentPattern = value; }
    }

    internal void SetContentLength(long length) {
      _PatternContent = @"^((?:.|\n){" + length + "})";
    }

    internal HttpToken NextToken() {
      if (!_UseContentPattern) {
        IsMatch(_PatternWhitespace);
        if (IsMatch(_PatternHttp)) return _Current = HttpToken.Http;
        if (IsMatch(_PatternMethod)) { _LastTokenMethod = true; return _Current = HttpToken.Method; }
        if (_LastTokenMethod) {
          if (IsMatch(_PatternUrl)) { _LastTokenMethod = false; return _Current = HttpToken.Url; }
        }
        if (IsMatch(_PatternHeaderDefinition)) return _Current = HttpToken.HeaderDefinition;
        if (IsMatch(_PatternHeaderContent)) return _Current = HttpToken.HeaderContent;
        if (IsMatch(_PatternNewLine)) return _Current = HttpToken.NewLine;
      }
      else {
        if (IsMatch(_PatternContent)) { _UseContentPattern = false; return _Current = HttpToken.Content; }
      }
      return _Current = HttpToken.Unknown;
    }

    private bool IsMatch(string pattern) {
      Regex re = new Regex(pattern);
      return IsMatch(re);
    }

    private bool IsMatch(Regex re) {
      string content = _ToScan.ToString();
      if (re.IsMatch(content)) {
        Match m = re.Match(content);
        _Value = m.Groups[1].Value;
        _ToScan.Remove(0, m.Length);
        return true;
      }
      return false;
    }

    internal void AddToScan(string toScan) {
      _ToScan.Append(toScan);
    }

    internal HttpToken Current {
      get { return _Current; }
    }

    internal string Value {
      get { return _Value; }
    }

    public string CurrentToken {
      get {
        return Enum.GetName(typeof(HttpToken), _Current);
      }
    }


  }

}
