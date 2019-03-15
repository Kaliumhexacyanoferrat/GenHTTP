using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// Defines an input field for a formular.
    /// </summary>
    public class Input : StyledElement
    {
        private InputType _Type;
        private string _Value;
        private bool _Checked;
        private bool _Disabled;
        private bool _Readonly;
        private ushort _Size;
        private ushort _MaxLength;
        private string _Source;
        private string _AlternativeDescription;
        private string _UseMap;
        private string _OnSelect;
        private string _OnChange;
        private string _OnFocus;
        private string _OnBlur;
        private ushort _TabIndex;
        private char _AccessKey;
        private DocumentEncoding _Accept;

        #region Constructors

        /// <summary>
        /// Create a new, empty input element.
        /// </summary>
        public Input()
        {

        }

        /// <summary>
        /// Create a new input element.
        /// </summary>
        /// <param name="type">The type of the element</param>
        /// <param name="name">The name of the control</param>
        /// <param name="id">The ID of the control</param>
        public Input(InputType type, string name, string id)
        {
            _Type = type;
            Name = name;
            ID = id;
        }

        /// <summary>
        /// Create a new input element.
        /// </summary>
        /// <param name="type">The type of the element</param>
        /// <param name="name">The name of the control</param>
        /// <param name="id">The ID of the control</param>
        /// <param name="value">The value of the element</param>
        public Input(InputType type, string name, string id, string value)
        {
            _Type = type;
            Name = name;
            ID = id;
            _Value = value;
        }

        /// <summary>
        /// Create a new checkbox.
        /// </summary>
        /// <param name="name">The name of the checkbox</param>
        /// <param name="check">Specify, whether this checkbox is checked</param>
        public Input(string name, bool check)
        {
            _Type = InputType.Checkbox;
            Name = name;
            _Checked = check;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The type of the input element.
        /// </summary>
        public InputType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// The value of the input element.
        /// </summary>
        public string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        /// <summary>
        /// If this element is a checkbox, this property specifies,
        /// whether the checkbox is checked or not.
        /// </summary>
        public bool Checked
        {
            get { return _Checked; }
            set { _Checked = value; }
        }

        /// <summary>
        /// Specifies, whether this control is disabled or not.
        /// </summary>
        public bool Disabled
        {
            get { return _Disabled; }
            set { _Disabled = value; }
        }

        /// <summary>
        /// Specifies, whether this control is read-only or not.
        /// </summary>
        public bool ReadOnly
        {
            get { return _Readonly; }
            set { _Readonly = value; }
        }

        /// <summary>
        /// The size of the element.
        /// </summary>
        public ushort Size
        {
            get { return _Size; }
            set { _Size = value; }
        }

        /// <summary>
        /// The maximum length of the input data.
        /// </summary>
        public ushort MaxLength
        {
            get { return _MaxLength; }
            set { _MaxLength = value; }
        }

        /// <summary>
        /// The source of this element.
        /// </summary>
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

        /// <summary>
        /// An alternative description.
        /// </summary>
        public string AlternativeDescription
        {
            get { return _AlternativeDescription; }
            set { _AlternativeDescription = value; }
        }

        /// <summary>
        /// If this element should act as a <see cref="Map"/>,
        /// this property defines the location of the map.
        /// </summary>
        public string UseMap
        {
            get { return _UseMap; }
            set { _UseMap = value; }
        }

        /// <summary>
        /// Code which will be executed, if the content of the
        /// input element changes.
        /// </summary>
        public string OnChange
        {
            get { return _OnChange; }
            set { _OnChange = value; }
        }

        /// <summary>
        /// Code which will be executed if the element is selected.
        /// </summary>
        public string OnSelect
        {
            get { return _OnSelect; }
            set { _OnSelect = value; }
        }

        /// <summary>
        /// Specifies for a file input box, which file types are accepted
        /// by this dialog.
        /// </summary>
        public DocumentEncoding AcceptCharset
        {
            get { return _Accept; }
            set { _Accept = value; }
        }

        /// <summary>
        /// Code which will be executed if the element gets the focus.
        /// </summary>
        public string OnFocus
        {
            get { return _OnFocus; }
            set { _OnFocus = value; }
        }

        /// <summary>
        /// Code which will be executed if the element loses the focus.
        /// </summary>
        public string OnBlur
        {
            get { return _OnBlur; }
            set { _OnBlur = value; }
        }

        /// <summary>
        /// The shortcut of this element.
        /// </summary>
        public char AccessKey
        {
            get { return _AccessKey; }
            set { _AccessKey = value; }
        }

        /// <summary>
        /// The tabindex of this element.
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
            b.Append("\r\n<input type=\"" + _Type.ToString().ToLower() + "\"" + ToClassString() + ToXHtml(IsXHtml) + ToCss());
            if (_Value != null && _Value.Length > 0) b.Append(" value=\"" + _Value + "\"");
            if (_Checked) b.Append(" checked=\"checked\"");
            if (_Disabled) b.Append(" disabled=\"disabled\"");
            if (_Readonly) b.Append(" readonly=\"readonly\"");
            if (_Size > 0) b.Append(" size=\"" + _Size + "\"");
            if (_MaxLength > 0) b.Append(" maxlength=\"" + _MaxLength + "\"");
            if (_Source != null && _Source.Length > 0) b.Append(" src=\"" + _Source + "\"");
            if (_AlternativeDescription != null && _AlternativeDescription.Length > 0) b.Append(" alt=\"" + _AlternativeDescription + "\"");
            if (_UseMap != null && _UseMap.Length > 0) b.Append(" usemap=\"" + _UseMap + "\"");
            if (_OnSelect != null && _OnSelect.Length > 0) b.Append(" onselect=\"" + _OnSelect + "\"");
            if (_OnChange != null && _OnChange.Length > 0) b.Append(" onchange=\"" + _OnChange + "\"");
            if (_OnFocus != null && _OnFocus.Length > 0) b.Append(" onfocus=\"" + _OnFocus + "\"");
            if (_OnBlur != null && _OnBlur.Length > 0) b.Append(" onblur=\"" + _OnBlur + "\"");
            if (_TabIndex > 0) b.Append(" tabindex=\"" + _TabIndex + "\"");
            if (_AccessKey != 0) b.Append(" accesskey=\"" + _AccessKey + "\"");
            if (_Accept != null) b.Append(" accept=\"" + _Accept.Name + "\"");
            if (IsXHtml) b.Append(" />"); else b.Append(">");
            b.Append("\r\n");
        }

        #endregion

    }

}
