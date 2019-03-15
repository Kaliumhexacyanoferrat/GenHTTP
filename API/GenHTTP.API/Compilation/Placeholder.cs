using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Compilation
{

    /// <summary>
    /// A placeholder, used in compiled documents.
    /// </summary>
    /// <remarks>
    /// The object framework of the GenHTTP webserver is quite
    /// slow and requires a lot of memory and CPU time. Therefore,
    /// it is recommended to compile documents so their static
    /// content is available in byte form. You can't access
    /// the document via the object framework anymore, but you
    /// can insert placeholders before you begin to compile the
    /// document. This placeholders are type-safe and can
    /// use the object framework again.
    /// </remarks>
    public class Placeholder
    {
        private string _Name;
        private Type _Type;

        #region constructors

        /// <summary>
        /// Create a new placeholder with an explicit name.
        /// </summary>
        /// <param name="type">The type of content this placeholder stands for</param>
        /// <param name="name">The unique name of the placeholder</param>
        public Placeholder(Type type, string name)
        {
            _Name = name;
            _Type = type;
        }

        #endregion

        #region get-/setters

        /// <summary>
        /// The name of this placeholder.
        /// </summary>
        public string Name
        {
            get { return _Name; }
        }

        /// <summary>
        /// The type content this placeholder stands for.
        /// </summary>
        public Type Type
        {
            get { return _Type; }
        }

        #endregion

        /// <summary>
        /// Allows you to use this placeholder in all string
        /// fields of the document object framework.
        /// </summary>
        /// <remarks>
        /// For example, if you want to set a part of the title of
        /// a document later, you could use this code:
        /// <code>
        /// doc.Header.Title = new Placeholder(PlaceholderType.EntityString, "Title") + " - GenHTTP";
        /// </code>
        /// </remarks>
        /// <returns>The string representation of this placeholder</returns>
        public override string ToString()
        {
            return "%GENHTTP PLACEHOLDER TYPE: " + _Type.ToString() + " NAME: " + _Name + "%";
        }

    }

}
