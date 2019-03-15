using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Stores <see cref="Text" /> elements.
    /// </summary>
    public class TextCollection : ITextContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new text collection object.
        /// </summary>
        /// <param name="d">The delegate used to add elements to the related collection</param>
        public TextCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region ITextContainer Members

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        public void Print(string text)
        {
            _Delegate(new Text(text));
        }

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
        public void Print(string text, bool escapeEntities)
        {
            _Delegate(new Text(text, escapeEntities));
        }

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <returns>The newly created object</returns>
        public Text AddText(string text)
        {
            Text txt = new Text(text);
            _Delegate(txt);
            return txt;
        }

        /// <summary>
        /// Print some text to the element.
        /// </summary>
        /// <param name="text">The text to print</param>
        /// <param name="escapeEntities">Specify, whether to escape entities or not</param>
        /// <returns>The newly created object</returns>
        public Text AddText(string text, bool escapeEntities)
        {
            Text txt = new Text(text, escapeEntities);
            _Delegate(txt);
            return txt;
        }

        /// <summary>
        /// Add an empty text element to the element.
        /// </summary>
        /// <returns>The newly created object</returns>
        public Text AddText()
        {
            Text txt = new Text();
            _Delegate(txt);
            return txt;
        }

        #endregion

    }

}
