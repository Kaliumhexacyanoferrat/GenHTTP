using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

using GenHTTP.Patterns;

namespace GenHTTP.Utilities {
  
  /// <summary>
  /// Available tokens for scanning.
  /// </summary>
  internal enum XmlToken {
    XmlHeader,
    XmlComment,
    XmlElementBegin,
    XmlAttribute,
    XmlAttributeValue,
    XmlElementValue,
    XmlElementEnd,
    XmlShortElementEnd,
    XmlNormalElementEnd,
    XmlEnd,
    Unknown
  }

  /// <summary>
  /// Scans a XML file.
  /// </summary>
  internal class SettingScanner {
    private string _ToScan;
    private XmlToken _Current;
    private string _Value;
    private bool _EnableValue;
    private bool _EnableAttributeValue;

    // regular expressions used to scan
    private static Regex _RegExHeader;
    private static Regex _RegExComment;
    private static Regex _RegExElementBegin;
    private static Regex _RegExAttribute;
    private static Regex _RegExAttributeValue;
    private static Regex _RegExElementValue;
    private static Regex _RegExElementEnd;
    private static Regex _RegExShortElementEnd;
    private static Regex _RegExNormalElementEnd;
    private static Regex _RegExWhitespace;

    /// <summary>
    /// Create a new scanner instance using a file as input data.
    /// </summary>
    /// <param name="file">The file to read</param>
    internal SettingScanner(string file) {
      // read the file
      if (!File.Exists(file)) throw new FileNotFoundException("The SettingScanner could not find the file to read: '" + file + "'", file);
      StreamReader reader = new StreamReader(file);
      _ToScan = reader.ReadToEnd();
      reader.Close();
      // init regular expressions if they are not yet initialized
      if (_RegExHeader == null) {
        _RegExHeader = new PatternXmlHeader();
        _RegExComment = new PatternXmlComment();
        _RegExElementBegin = new PatternXmlElementBegin();
        _RegExAttribute = new PatternXmlAttribute();
        _RegExAttributeValue = new PatternXmlAttributeValue();
        _RegExElementValue = new PatternXmlElementValue();
        _RegExElementEnd = new PatternXmlElementEnd();
        _RegExShortElementEnd = new PatternXmlShortElementEnd();
        _RegExWhitespace = new PatternXmlWhitespace();
        _RegExNormalElementEnd = new PatternXmlNormalElementEnd();
      }
    }

    #region get-/setters

    public XmlToken Current {
      get { return _Current; }
    }

    public string Value {
      get { return _Value; }
    }

    #endregion

    /// <summary>
    /// Retrieve the next token.
    /// </summary>
    /// <returns>The next token</returns>
    public XmlToken NextToken() {
      return _Current = Scan();
    }

    private XmlToken Scan() {
      // we're at the end of the file
      if (_ToScan == "") return XmlToken.XmlEnd;
      // remove whitespace
      IsMatch(_RegExWhitespace);
      if (IsMatch(_RegExHeader)) return XmlToken.XmlHeader;
      if (IsMatch(_RegExComment)) return XmlToken.XmlComment;
      if (IsMatch(_RegExElementBegin)) return XmlToken.XmlElementBegin;
      if (IsMatch(_RegExElementEnd)) return XmlToken.XmlElementEnd;
      if (IsMatch(_RegExShortElementEnd)) { _EnableValue = true; return XmlToken.XmlShortElementEnd; }
      if (IsMatch(_RegExNormalElementEnd)) { _EnableValue = true; return XmlToken.XmlNormalElementEnd; }
      if (IsMatch(_RegExAttribute)) { _EnableAttributeValue = true; return XmlToken.XmlAttribute; }
      if (_EnableAttributeValue && IsMatch(_RegExAttributeValue)) { _EnableAttributeValue = false; return XmlToken.XmlAttributeValue; }
      if (_EnableValue && IsMatch(_RegExElementValue)) { _EnableValue = false; return XmlToken.XmlElementValue; }
      // what to do with this syntactic crap?
      return XmlToken.Unknown;
    }

    private bool IsMatch(Regex re) {
      Match m = re.Match(_ToScan);
      if (!m.Success) return false;
      _Value = m.Groups[1].Value;
      _ToScan = _ToScan.Substring(m.Length);
      return true;
    }

  }

}
