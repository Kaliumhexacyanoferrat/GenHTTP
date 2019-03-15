using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Style;
using GenHTTP.Api.Abstraction.Elements.Containers;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// Allows you to add span elements to a container.
    /// </summary>
    public class SpanCollection : ISpanContainer
    {
        private AddElement _Delegate;

        #region Constructors

        /// <summary>
        /// Create a new span collection.
        /// </summary>
        /// <param name="d">The method used to add elements to the underlying container</param>
        public SpanCollection(AddElement d)
        {
            _Delegate = d;
        }

        #endregion

        #region ISpanContainer Members

        /// <summary>
        /// Add a new, empty span.
        /// </summary>
        /// <returns>The created object</returns>
        public Span AddSpan()
        {
            Span span = new Span();
            _Delegate(span);
            return span;
        }

        /// <summary>
        /// Add a new span.
        /// </summary>
        /// <param name="text">The content of the span</param>
        /// <param name="decoration">The text decoration to use</param>
        /// <returns>The created object</returns>
        public Span AddSpan(string text, ElementTextDecoration decoration)
        {
            Span span = new Span(text, decoration);
            _Delegate(span);
            return span;
        }

        /// <summary>
        /// Add a new span.
        /// </summary>
        /// <param name="text">The content of the span</param>
        /// <param name="fontWeight">The font weight to use</param>
        /// <returns>The created object</returns>
        public Span AddSpan(string text, ElementFontWeight fontWeight)
        {
            Span span = new Span(text, fontWeight);
            _Delegate(span);
            return span;
        }

        #endregion

    }

}
