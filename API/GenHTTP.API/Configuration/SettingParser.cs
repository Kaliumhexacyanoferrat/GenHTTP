using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Configuration
{

    internal class SettingParser
    {
        private SettingScanner _Scanner;

        internal SettingParser(string file)
        {
            _Scanner = new SettingScanner(file);
        }

        internal Setting Fill()
        {
            // XML header
            if (_Scanner.NextToken() != XmlToken.XmlHeader) throw new XmlParserException("XML header expected");
            // node
            _Scanner.NextToken();
            return RuleNode();
        }

        /// <summary>
        /// Build node structure.
        /// </summary>
        private Setting RuleNode()
        {
            // ignore comments
            RuleCommentNode();
            // there must be a node!
            if (_Scanner.Current != XmlToken.XmlElementBegin) throw new XmlParserException("XML node expected");
            Setting s = new Setting(_Scanner.Value);
            _Scanner.NextToken();
            // are there any attributes?
            while (_Scanner.Current == XmlToken.XmlAttribute)
            {
                string attribName = _Scanner.Value;
                if (_Scanner.NextToken() != XmlToken.XmlAttributeValue) throw new XmlParserException("XML attribute value expected");
                s.Attributes[attribName] = _Scanner.Value;
                _Scanner.NextToken();
            }
            // short end of the open tag
            if (_Scanner.Current == XmlToken.XmlShortElementEnd) { _Scanner.NextToken(); return s; }
            // normal end of the open tag
            if (_Scanner.Current != XmlToken.XmlNormalElementEnd) throw new XmlParserException("XML normal or short end of tag expected");
            _Scanner.NextToken();
            // value or more nodes?
            if (_Scanner.Current == XmlToken.XmlElementValue)
            {
                s.Value = _Scanner.Value;
                _Scanner.NextToken();
            }
            else
            {
                RuleNodeCollection(s);
            }
            // and close the tag
            if (_Scanner.Current != XmlToken.XmlElementEnd) throw new XmlParserException("XML end of element expected");
            _Scanner.NextToken();
            // return the newly created setting
            return s;
        }

        /// <summary>
        /// Various nodes in a list.
        /// </summary>
        private void RuleNodeCollection(Setting s)
        {
            RuleCommentNode();
            if (_Scanner.Current == XmlToken.XmlElementBegin)
            {
                Setting n = RuleNode();
                s.Children.Add(n);
                if (_Scanner.Current == XmlToken.XmlElementBegin || _Scanner.Current == XmlToken.XmlComment) RuleNodeCollection(s);
            }
        }

        /// <summary>
        /// Searches for comments.
        /// </summary>
        private void RuleCommentNode()
        {
            if (_Scanner.Current == XmlToken.XmlComment)
            {
                _Scanner.NextToken();
                if (_Scanner.Current == XmlToken.XmlComment) RuleCommentNode();
            }
        }


    }

    /// <summary>
    /// Will be thrown if the <see cref="SettingParser" /> fails to parse a XML file.
    /// </summary>
    public class XmlParserException : Exception
    {

        /// <summary>
        /// Create a new parser exception.
        /// </summary>
        /// <param name="message">The message of this exception</param>
        public XmlParserException(string message)
          : base(message)
        {

        }

    }

}
