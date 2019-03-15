using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GenHTTP.Api.Abstraction.Elements.Containers;
using GenHTTP.Api.Abstraction.Elements.Collections;

namespace GenHTTP.Api.Abstraction.Elements
{

    /// <summary>
    /// Defines a table section.
    /// </summary>
    public class TableSection : StyledElementWithChildren, ITableLineContainer
    {
        private TableLineCollection _Lines;
        private TableSectionType _Type;

        #region Constructors

        /// <summary>
        /// Create a new table section.
        /// </summary>
        /// <param name="type">The type of the section</param>
        public TableSection(TableSectionType type)
        {
            _Type = type;
            _Lines = new TableLineCollection(Add);
        }

        #endregion

        #region Element management

        /// <summary>
        /// Add an child element to the table section.
        /// </summary>
        /// <param name="element">A table line</param>
        public override void Add(Element element)
        {
            if (!(element is TableLine)) throw new ArgumentException("A table section can only contain table lines", "element");
            _Children.Add(element);
        }

        #endregion

        #region ITableLineContainer Members

        /// <summary>
        /// Add a new, empty table line.
        /// </summary>
        /// <returns>The created object</returns>
        public TableLine AddTableLine()
        {
            return _Lines.AddTableLine();
        }

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        public TableLine AddTableLine(string[] cells)
        {
            return _Lines.AddTableLine(cells);
        }

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        public TableLine AddTableLine(IEnumerable<string> cells)
        {
            return _Lines.AddTableLine(cells);
        }

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        public TableLine AddTableLine(Element[] cells)
        {
            return _Lines.AddTableLine(cells);
        }

        /// <summary>
        /// Add a table line.
        /// </summary>
        /// <param name="cells">The content of this line</param>
        /// <returns>The created and filled object</returns>
        public TableLine AddTableLine(IEnumerable<Element> cells)
        {
            return _Lines.AddTableLine(cells);
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
            string tag = "t" + _Type.ToString().ToLower();
            b.Append(" <" + tag + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
            SerializeChildren(b, type);
            b.Append(" </" + tag + ">\r\n");
        }

        #endregion

    }

}
