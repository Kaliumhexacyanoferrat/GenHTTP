using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// Represents a button.
    /// </summary>
    public class Button : StyledElementWithChildren
    {
        private string _Value;
        private ButtonType _Type = ButtonType.Submit;
        private bool _Disabled;
        private string _OnFocus;
        private string _OnBlur;
        private char _AccessKey;
        private ushort _TabIndex;

        #region Constructors

        /// <summary>
        /// Create a new, empty button.
        /// </summary>
        public Button()
        {

        }

        /// <summary>
        /// Create a new button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        public Button(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Create a new button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="type">The type of the button</param>
        public Button(string name, ButtonType type)
        {
            Name = name;
            _Type = type;
        }

        /// <summary>
        /// Create a new button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        public Button(string name, string value)
        {
            Name = name;
            _Value = value;
        }

        /// <summary>
        /// Create a new button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="content">The inner value of the button</param>
        public Button(string name, string value, string content)
        {
            Name = name;
            _Value = value;
            Add(new Text(content));
        }

        /// <summary>
        /// Create a new button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="type">The type of the button</param>
        public Button(string name, string value, ButtonType type)
        {
            Name = name;
            _Value = value;
            _Type = type;
        }

        /// <summary>
        /// Create a new button.
        /// </summary>
        /// <param name="name">The name of the button</param>
        /// <param name="value">The value of the button</param>
        /// <param name="type">The type of the button</param>
        /// <param name="content">The inner value of the button</param>
        public Button(string name, string value, string content, ButtonType type)
        {
            Name = name;
            _Value = value;
            _Type = type;
            Add(new Text(content));
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The value of the button.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// The type of the button.
        /// </summary>
        public ButtonType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// Specifies, whether this button is disabled.
        /// </summary>
        public bool Disabled
        {
            get { return _Disabled; }
            set { _Disabled = value; }
        }

        /// <summary>
        /// Code, which will be executed if the button gets the focus.
        /// </summary>
        public string OnFocus
        {
            get { return _OnFocus; }
            set { _OnFocus = value; }
        }

        /// <summary>
        /// Code which will be executed if the button loses the focus.
        /// </summary>
        public string OnBlur
        {
            get { return _OnBlur; }
            set { _OnBlur = value; }
        }

        /// <summary>
        /// The shortcut of this button.
        /// </summary>
        public char AccessKey
        {
            get { return _AccessKey; }
            set { _AccessKey = value; }
        }

        /// <summary>
        /// The tab index of this button.
        /// </summary>
        public ushort TabIndex
        {
            get { return _TabIndex; }
            set { _TabIndex = value; }
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
            b.Append("<button" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
            b.Append(" type=\"" + _Type.ToString().ToLower() + "\"");
            if (_Value != null && _Value.Length > 0) b.Append(" value=\"" + _Value + "\"");
            if (_Disabled) b.Append(" disabled=\"disabled\"");
            if (_OnFocus != null && _OnFocus.Length > 0) b.Append(" onfocus=\"" + _OnFocus + "\"");
            if (_OnBlur != null && _OnBlur.Length > 0) b.Append(" onblur=\"" + _OnBlur + "\"");
            if (_AccessKey != 0) b.Append(" accesskey=\"" + _AccessKey + "\"");
            if (_TabIndex > 0) b.Append(" tabindex=\"" + _TabIndex + "\"");
            b.Append(">");
            SerializeChildren(b, type);
            b.Append("</button>");
        }

        #endregion

    }
}
