using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// A neutral container element.
    /// </summary>
    /// <remarks>
    /// You can use this as a placeholder, providing the object modell
    /// without any additional (X)HTML code (like span or div would cause).
    /// 
    /// No information appended to this element will show any effect.
    /// </remarks>
    public class NeutralElement : StyledContainerElement
    {

        #region Constructors

        /// <summary>
        /// Create a new, empty neutral element.
        /// </summary>
        public NeutralElement()
        {

        }

        /// <summary>
        /// Create a new neutral element with some content.
        /// </summary>
        /// <param name="text">The text to add</param>
        public NeutralElement(string text)
        {
            AddText(text);
        }

        /// <summary>
        /// Create a new neutral element with some content.
        /// </summary>
        /// <param name="element">The element to add</param>
        public NeutralElement(Element element)
        {
            Add(element);
        }

        /// <summary>
        /// Create a new neutral element with some content.
        /// </summary>
        /// <param name="elements">The elements to add</param>
        public NeutralElement(IEnumerable<Element> elements)
        {
            foreach (Element e in elements) Add(e);
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
            SerializeChildren(b, type);
        }

        #endregion

    }

}
