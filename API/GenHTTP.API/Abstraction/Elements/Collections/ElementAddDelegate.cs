using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Api.Abstraction.Elements.Collections
{

    /// <summary>
    /// The signature of the method used to add an element
    /// to a <see cref="StyledContainerElement"/>.
    /// </summary>
    /// <param name="e">The element to add</param>
    public delegate void AddElement(Element e);

}
