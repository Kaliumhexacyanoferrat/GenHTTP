using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// This class desribes an element with a specifid style
    /// and child elements.
    /// </summary>
    public abstract class StyledElementWithChildren : StyledElement
    {
        /// <summary>
        /// The children of this element.
        /// </summary>
        protected List<Element> _Children;

        #region Constructors

        /// <summary>
        /// Create a new, styled element with children.
        /// </summary>
        public StyledElementWithChildren()
        {
            _Children = new List<Element>();
        }

        #endregion

        #region Element search

        /// <summary>
        /// Retrieve an element or child element of this element
        /// with the given ID.
        /// </summary>
        /// <param name="id">The ID of the element to find</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public override Element GetElementById(string id)
        {
            Element e = base.GetElementById(id);
            if (e != null) return e;
            foreach (Element child in _Children)
            {
                if (child == null) continue;
                e = base.GetElementById(id);
                if (e != null) return e;
            }
            return null;
        }

        /// <summary>
        /// Retrieve an element or child element of this element
        /// with the given ID.
        /// </summary>
        /// <param name="id">The ID of the element to find</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public override Element this[string id]
        {
            get
            {
                return GetElementById(id);
            }
        }

        /// <summary>
        /// Retrieve an element or child element of this element with
        /// the given ID or internal ID.
        /// </summary>
        /// <param name="id">The ID or internal ID of the element to find</param>
        /// <param name="isInternal">Defines, whether the (X)HTML or the internal ID should be checked</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public override Element GetElementById(string id, bool isInternal)
        {
            Element e = base.GetElementById(id, isInternal);
            if (e != null) return e;
            foreach (Element child in _Children)
            {
                if (child == null) continue;
                e = base.GetElementById(id, isInternal);
                if (e != null) return e;
            }
            return null;
        }

        /// <summary>
        /// Retrieve an element or child element of this element with
        /// the given ID or internal ID.
        /// </summary>
        /// <param name="id">The ID or internal ID of the element to find</param>
        /// <param name="isInternal">Defines, whether the (X)HTML or the internal ID should be checked</param>
        /// <returns>The requested element or null, if it could not be found</returns>
        public override Element this[string id, bool isInternal]
        {
            get
            {
                return GetElementById(id, isInternal);
            }
        }

        #endregion

        #region Children access

        /// <summary>
        /// Retrieve an enumerator to iterate over the child elements in this collection.
        /// </summary>
        /// <returns>The requested enumerator</returns>
        public virtual IEnumerator<Element> GetEnumerator()
        {
            return _Children.GetEnumerator();
        }

        /// <summary>
        /// The number of child elements.
        /// </summary>
        public virtual int Count
        {
            get { return _Children.Count; }
        }

        /// <summary>
        /// Retrieve a child element by it's logical number.
        /// </summary>
        /// <param name="nr">The number of the element in the collection</param>
        /// <returns>The requested element</returns>
        public virtual Element this[int nr]
        {
            get
            {
                if (nr >= 0 && nr < Count) return _Children[nr];
                return null;
            }
        }

        /// <summary>
        /// Add an element to this container.
        /// </summary>
        /// <param name="element">The element to add</param>
        public virtual void Add(Element element)
        {
            _Children.Add(element);
        }

        /// <summary>
        /// Add a single space.
        /// </summary>
        public virtual void AddSpace()
        {
            Add(new Text("&nbsp;", false));
        }

        /// <summary>
        /// Add some spaces.
        /// </summary>
        /// <param name="count">The number of spaces to add</param>
        public virtual void AddSpace(ushort count)
        {
            string str = "";
            for (int i = 0; i < count; i++) str += "&nbsp;";
            Add(new Text(str, false));
        }

        /// <summary>
        /// Add a new line.
        /// </summary>
        /// <param name="type">The type of the related document</param>
        public virtual void AddNewLine(DocumentType type)
        {
            string str = (type == DocumentType.XHtml_1_1_Strict || type == DocumentType.XHtml_1_1_Transitional) ? "<br />" : "<br>";
            Add(new Text("\r\n" + str + "\r\n", false));
        }

        /// <summary>
        /// Add some new lines.
        /// </summary>
        /// <param name="type">The type of the related document</param>
        /// <param name="count">The number of new lines</param>
        public virtual void AddNewLine(DocumentType type, ushort count)
        {
            string str = "";
            for (int i = 0; i < count; i++) str += (type == DocumentType.XHtml_1_1_Strict || type == DocumentType.XHtml_1_1_Transitional) ? "<br />" : "<br>";
            Add(new Text("\r\n" + str + "\r\n", false));
        }

        /// <summary>
        /// Remove an element from the children list.
        /// </summary>
        /// <param name="element">The element to remove</param>
        public virtual void Remove(Element element)
        {
            if (_Children.Contains(element)) _Children.Remove(element);
        }

        /// <summary>
        /// Remove an element from the children list.
        /// </summary>
        /// <param name="nr">The logical number of the element to remove</param>
        public virtual void Remove(int nr)
        {
            if (nr >= 0 && nr < Count) _Children.RemoveAt(nr);
        }

        #endregion

        #region Static include

        /// <summary>
        /// Include a file as a static part.
        /// </summary>
        /// <param name="file">The file to include</param>
        /// <returns>The added text object</returns>
        public virtual Text Include(string file)
        {
            StreamReader r = new StreamReader(file);
            Text txt = new Text(r.ReadToEnd());
            txt.EscapeEntities = false;
            Add(txt);
            r.Close();
            return txt;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Serialize the element to a string builder.
        /// </summary>
        /// <param name="b">The string builder to write to</param>
        /// <param name="type">Specifies the output code type</param>
        internal virtual void SerializeChildren(StringBuilder b, DocumentType type)
        {
            foreach (Element child in _Children)
            {
                if (child != null) child.Serialize(b, type);
            }
        }

        #endregion

    }

}
