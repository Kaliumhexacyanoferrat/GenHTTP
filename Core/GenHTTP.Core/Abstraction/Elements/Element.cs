using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Utilities;

namespace GenHTTP.Abstraction.Elements
{

    /// <summary>
    /// The base class for all elements of the document
    /// object framework.
    /// </summary>
    public abstract class Element
    {
        private string _Name;
        private string _ID;
        private string _InternalId;
        private string _Title;
        private string _OnClick, _OnDblClick, _OnMouseDown, _OnMouseUp, _OnMouseOver, _OnMouseMove;
        private string _OnMouseOut, _OnKeyPress, _OnKeyDown, _OnKeyUp;
        private Internationalization _Internationalization;
        private DocumentType _Type = DocumentType.XHtml_1_1_Strict;

        #region Constructors

        /// <summary>
        /// Create a new element.
        /// </summary>
        public Element()
        {
            _Internationalization = new Internationalization();
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// Code, which will be executed if the user releases a key.
        /// </summary>
        public string OnKeyUp
        {
            get { return _OnKeyUp; }
            set { _OnKeyUp = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user presses a key.
        /// </summary>
        public string OnKeyDown
        {
            get { return _OnKeyDown; }
            set { _OnKeyDown = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user presses a key.
        /// </summary>
        public string OnKeyPress
        {
            get { return _OnKeyPress; }
            set { _OnKeyPress = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user leaves the element with the mouse cursor.
        /// </summary>
        public string OnMouseOut
        {
            get { return _OnMouseOut; }
            set { _OnMouseOut = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user moves the cursor over the element.
        /// </summary>
        public string OnMouseMove
        {
            get { return _OnMouseMove; }
            set { _OnMouseMove = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user hovers over the element.
        /// </summary>
        public string OnMouseOver
        {
            get { return _OnMouseOver; }
            set { _OnMouseOver = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user releases a mouse key.
        /// </summary>
        public string OnMouseUp
        {
            get { return _OnMouseUp; }
            set { _OnMouseUp = value; }
        }

        /// <summary>
        /// Code, which will be executed if the user presses a mouse key.
        /// </summary>
        public string OnMouseDown
        {
            get { return _OnMouseDown; }
            set { _OnMouseDown = value; }
        }

        /// <summary>
        /// Code, which will be executed on click.
        /// </summary>
        public string OnClick
        {
            get { return _OnClick; }
            set { _OnClick = value; }
        }

        /// <summary>
        /// Code, which will be executed on double click.
        /// </summary>
        public string OnDblClick
        {
            get { return _OnDblClick; }
            set { _OnDblClick = value; }
        }

        /// <summary>
        /// The name of the element.
        /// </summary>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// The ID of the element.
        /// </summary>
        public string ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        /// <summary>
        /// The internal ID of the element.
        /// </summary>
        /// <remarks>
        /// This ID will not get serialized.
        /// </remarks>
        public string InternalId
        {
            get { return _InternalId; }
            set { _InternalId = value; }
        }

        /// <summary>
        /// The internationalization of this element.
        /// </summary>
        public Internationalization Internationalization
        {
            get { return _Internationalization; }
            set { _Internationalization = value; }
        }

        /// <summary>
        /// The title of the element.
        /// </summary>
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        /// <summary>
        /// Specifies the serialization output format.
        /// </summary>
        public DocumentType RenderingType
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// XHTML output?
        /// </summary>
        public bool IsXHtml
        {
            get { return (_Type == DocumentType.XHtml_1_1_Strict || _Type == DocumentType.XHtml_1_1_Transitional); }
        }

        /// <summary>
        /// Strict output?
        /// </summary>
        public bool IsStrict
        {
            get { return (_Type == DocumentType.XHtml_1_1_Strict || _Type == DocumentType.Html_4_01_Strict); }
        }

        #endregion

        #region ID handling

        /// <summary>
        /// Retrieve an element or child element of this element
        /// with the given ID.
        /// </summary>
        /// <param name="id">The ID of the element to find</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public virtual Element GetElementById(string id)
        {
            if (_ID == id) return this;
            return null;
        }

        /// <summary>
        /// Retrieve an element or child element of this element with
        /// the given ID or internal ID.
        /// </summary>
        /// <param name="id">The ID or internal ID of the element to find</param>
        /// <param name="isInternal">Defines, whether the (X)HTML or the internal ID should be checked</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public virtual Element GetElementById(string id, bool isInternal)
        {
            if (isInternal && _InternalId == id) return this;
            if (!isInternal && _ID == id) return this;
            return null;
        }

        /// <summary>
        /// Retrieve an element or child element of this element
        /// with the given ID.
        /// </summary>
        /// <param name="id">The ID of the element to find</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public virtual Element this[string id]
        {
            get { return GetElementById(id); }
        }

        /// <summary>
        /// Retrieve an element or child element of this element with
        /// the given ID or internal ID.
        /// </summary>
        /// <param name="id">The ID or internal ID of the element to find</param>
        /// <param name="isInternal">Defines, whether the (X)HTML or the internal ID should be checked</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public virtual Element this[string id, bool isInternal]
        {
            get { return GetElementById(id, isInternal); }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Convert the attributes of this element into (X)HTML code.
        /// </summary>
        /// <param name="isXHtml">Define, whether the ouput should be in XHTML</param>
        /// <returns>The (X)HTML representation of this element</returns>
        internal string ToXHtml(bool isXHtml)
        {
            string ret = "";
            if (_Name != null && _Name.Length > 0) ret += " name=\"" + _Name + "\"";
            if (_ID != null && _ID.Length > 0) ret += " id=\"" + _ID + "\"";
            if (_Title != null && _Title.Length > 0) ret += " title=\"" + _Title + "\"";
            ret += _Internationalization.Serialize(isXHtml);
            // events
            if (_OnClick != null && _OnClick.Length > 0) ret += " onclick=\"" + _OnClick + "\"";
            if (_OnDblClick != null && _OnDblClick.Length > 0) ret += " ondblclick=\"" + _OnDblClick + "\"";
            if (_OnMouseDown != null && _OnMouseDown.Length > 0) ret += " onmousedown=\"" + _OnMouseDown + "\"";
            if (_OnMouseUp != null && _OnMouseUp.Length > 0) ret += " onmouseup=\"" + _OnMouseUp + "\"";
            if (_OnMouseOver != null && _OnMouseOver.Length > 0) ret += " onmouseover=\"" + _OnMouseOver + "\"";
            if (_OnMouseMove != null && _OnMouseMove.Length > 0) ret += " onmousemove=\"" + _OnMouseMove + "\"";
            if (_OnMouseOut != null && _OnMouseOut.Length > 0) ret += " onmouseout=\"" + _OnMouseOut + "\"";
            if (_OnKeyPress != null && _OnKeyPress.Length > 0) ret += " onkeypress=\"" + _OnKeyPress + "\"";
            if (_OnKeyUp != null && _OnKeyUp.Length > 0) ret += " onkeyup=\"" + _OnKeyUp + "\"";
            if (_OnKeyDown != null && _OnKeyDown.Length > 0) ret += " onkeydown=\"" + _OnKeyDown + "\"";
            return ret;
        }

        /// <summary>
        /// Serialize the element to a string builder.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">Specifies the output type</param>
        public abstract void Serialize(StringBuilder b, DocumentType type);

        /// <summary>
        /// Serialize the element.
        /// </summary>
        /// <param name="type">The output type</param>
        /// <returns>The serialized element</returns>
        public string Serialize(DocumentType type)
        {
            StringBuilder b = new StringBuilder();
            Serialize(b, type);
            return b.ToString();
        }

        /// <summary>
        /// Check, whether a string contains something or not.
        /// </summary>
        /// <param name="str">The string to check</param>
        /// <returns>true, if the given string is not null or empty</returns>
        protected bool StringTest(string str)
        {
            return (str != null && str.Length > 0);
        }

        #endregion

    }

}
