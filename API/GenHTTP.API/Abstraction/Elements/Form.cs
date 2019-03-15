using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// A formular used to transmit data to the server.
    /// </summary>
    public class Form : StyledContainerElement
    {
        private string _Action;
        private FormMethod _Method = FormMethod.Post;
        private bool _CanSendFiles;

        #region Construcors

        /// <summary>
        /// Create a new form.
        /// </summary>
        /// <param name="action">The URL of the target file</param>
        public Form(string action)
        {
            _Action = action;
        }

        /// <summary>
        /// Create a new form.
        /// </summary>
        /// <param name="action">The URL of the target file</param>
        /// <param name="method">The HTTP method to use</param>
        public Form(string action, FormMethod method)
        {
            _Action = action;
            _Method = method;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The URL of the target file.
        /// </summary>
        public string Action
        {
            get { return _Action; }
            set
            {
                if (value == null || value.Length == 0) throw new ArgumentException("The action of a form cannot be null or empty");
                _Action = value;
            }
        }

        /// <summary>
        /// The HTTP method to use.
        /// </summary>
        public FormMethod Method
        {
            get { return _Method; }
            set { _Method = value; }
        }

        /// <summary>
        /// Specifies, whether this form contains a file input box.
        /// </summary>
        public bool CanSendFiles
        {
            get { return _CanSendFiles; }
            set { _CanSendFiles = value; }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize this element.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">The output type</param>
        public override void Serialize(StringBuilder b, DocumentType type)
        {
            b.Append("\r\n<form" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + " action=\"" + _Action + "\" method=\"" + _Method.ToString().ToLower() + "\"" + ((_CanSendFiles) ? " enctype=\"multipart/form-data\"" : "") + ">\r\n");
            SerializeChildren(b, type);
            b.Append("\r\n</form>\r\n");
        }

        #endregion

    }

}
