using System;
using System.Text;
using System.Collections.Generic;

using GenHTTP.Api.Compilation;
using GenHTTP.Api.Abstraction;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Core
{

    /// <summary>
    /// The base template of the default server page.
    /// </summary>
    public class ServerPageBase : ITemplateBase
    {
        private long _ContentLength;
        private Encoding _Encoding;
        private List<byte[]> _Parts;
        private DocumentType _Type = DocumentType.XHtml_1_1_Strict;

        /// <summary>
        /// Create a new ServerPage base.
        /// </summary>
        internal ServerPageBase()
        {
            _Encoding = Encoding.GetEncoding("utf-8");
            _Parts = new List<byte[]>(4);
            _Parts.Add(_Encoding.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"\r\n\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n\r\n<head>\r\n  <title>GenHTTP Webserver</title>\r\n  <link rel=\"icon\" type=\"image/vnd.microsoft.icon\" href=\"/favicon.ico\" />\r\n</head>\r\n\r\n<body style=\"font-family: Tahoma; font-size: 10pt; color: #000000; background-color: #C8C8C8;\">\r\n<br /><br />\r\n\r\n\r\n<div style=\"padding: 5px; margin-left: 10%; margin-right: 10%; color: white; background-color: #279717; font-weight: bold; border: 1px solid #279717;\">\r\n"));
            _Parts.Add(_Encoding.GetBytes("\r\n</div>\r\n\r\n\r\n\r\n<div style=\"padding: 5px; margin-left: 10%; margin-right: 10%; background-color: white; border: 1px solid #279717;\">\r\n"));
            _Parts.Add(_Encoding.GetBytes("\r\n</div>\r\n\r\n\r\n\r\n<div style=\"font-size: 8pt; padding: 5px; margin-left: 10%; margin-right: 10%; color: white; background-color: #279717; text-align: right; border: 1px solid #279717;\">\r\nGenHTTP Webserver v"));
            _Parts.Add(_Encoding.GetBytes("\r\n</div>\r\n\r\n\r\n<br /><br />\r\n\r\n\r\n<div style=\"padding: 5px; margin-left: 10%; margin-right: 10%; text-align: center;\">\r\n<img src=\"/valid_xhtml.png\" alt=\"Valid XHTML\" /><img src=\"/valid_css.gif\" alt=\"Valid CSS\" />\r\n</div>\r\n\r\n</body>\r\n\r\n</html>"));
            _ContentLength = 1177;
        }

        /// <summary>
        /// The encoding used for this page.
        /// </summary>
        public Encoding Encoding
        {
            get { return _Encoding; }
        }

        /// <summary>
        /// The parts of this page.
        /// </summary>
        /// <param name="nr">The number of the page to retrieve</param>
        /// <returns>The requested page</returns>
        public byte[] this[int nr]
        {
            get { return _Parts[nr]; }
        }

        /// <summary>
        /// The length of the base content.
        /// </summary>
        public long ContentLength
        {
            get { return _ContentLength; }
        }

        /// <summary>
        /// The ouput type.
        /// </summary>
        public DocumentType Type
        {
            get { return _Type; }
        }

    }

    /// <summary>
    /// The default server page.
    /// </summary>
    public class ServerPage : IServerPage
    {
        private ITemplateBase _Base;
        private List<byte[]> _Content;
        private System.String _Title;
        private System.String _Value;
        private System.String _ServerVersion;

        /// <summary>
        /// Create a new server page.
        /// </summary>
        /// <param name="baseClass">The base class of this template</param>
        public ServerPage(ITemplateBase baseClass)
        {
            _Content = new List<byte[]>(3);
            _Base = baseClass;
        }

        /// <summary>
        /// The base class of this template
        /// </summary>
        public ITemplateBase Base
        {
            get { return _Base; }
        }

        /// <summary>
        /// The title of the page.
        /// </summary>
        public System.String Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        /// <summary>
        /// The content of the main box.
        /// </summary>
        public System.String Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// The version of the server.
        /// </summary>
        public System.String ServerVersion
        {
            get { return _ServerVersion; }
            set { _ServerVersion = value; }
        }

        /// <summary>
        /// Generate this page.
        /// </summary>
        /// <returns>The generated page</returns>
        public byte[] ToByteArray()
        {
            _Content.Add(_Base.Encoding.GetBytes(_Title));
            _Content.Add(_Base.Encoding.GetBytes(_Value));
            _Content.Add(_Base.Encoding.GetBytes(_ServerVersion));
            long contentLength = 1177 + _Content[0].Length + _Content[1].Length + _Content[2].Length;
            byte[] ret = new byte[contentLength];
            int nextPos = 0;
            // Title
            System.Buffer.BlockCopy(_Base[0], 0, ret, nextPos, 599);
            nextPos += _Base[0].Length;
            System.Buffer.BlockCopy(_Content[0], 0, ret, nextPos, _Content[0].Length);
            nextPos += _Content[0].Length;
            // Value
            System.Buffer.BlockCopy(_Base[1], 0, ret, nextPos, 134);
            nextPos += _Base[1].Length;
            System.Buffer.BlockCopy(_Content[1], 0, ret, nextPos, _Content[1].Length);
            nextPos += _Content[1].Length;
            // ServerVersion
            System.Buffer.BlockCopy(_Base[2], 0, ret, nextPos, 204);
            nextPos += _Base[2].Length;
            System.Buffer.BlockCopy(_Content[2], 0, ret, nextPos, _Content[2].Length);
            nextPos += _Content[2].Length;
            System.Buffer.BlockCopy(_Base[3], 0, ret, nextPos, _Base[3].Length);
            return ret;
        }

    }

}

