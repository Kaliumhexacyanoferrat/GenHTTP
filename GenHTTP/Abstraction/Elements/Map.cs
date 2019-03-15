using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using GenHTTP.Abstraction.Elements.Containers;
using GenHTTP.Abstraction.Elements.Collections;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// A map for an image.
  /// </summary>
  public class Map : StyledElementWithChildren, IAreaContainer {
    private AreaCollection _AreaElements;

    #region Constructors

    /// <summary>
    /// Create a new map.
    /// </summary>
    /// <param name="name">The name of the map</param>
    public Map(string name) {
      if (name == null || name.Length == 0) throw new ArgumentException("You need to specify a name for the map");
      Name = name;
      _AreaElements = new AreaCollection(new AddElement(Add));
    }

    #endregion

    #region Element management

    /// <summary>
    /// Add an element to this map.
    /// </summary>
    /// <param name="element">An area element</param>
    public override void Add(Element element) {
      if (!(element is Area)) throw new ArgumentException("You can only add areas to a map");
      base.Add(element);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("\r\n\r\n<map" + ToClassString() + ToXHtml(IsXHtml) + ToCss() + ">\r\n");
      SerializeChildren(b, type);
      b.Append("</map>\r\n\r\n");
    }

    #endregion

    #region IAreaContainer Members

    /// <summary>
    /// Add a new, empty area.
    /// </summary>
    /// <returns>The created object</returns>
    public Area AddArea() {
      return _AreaElements.AddArea();
    }

    /// <summary>
    /// Add a clickable rectangle.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="rectangle">The coordinates of the area</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, Rectangle rectangle) {
      return _AreaElements.AddArea(link, alternativeDescription, rectangle);
    }

    /// <summary>
    /// Add a clickable circle.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="point">The middle point of the circle</param>
    /// <param name="radius">The radius of the circle</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, Point point, ushort radius) {
      return _AreaElements.AddArea(link, alternativeDescription, point, radius);
    }

    /// <summary>
    /// Add a clickable polygon.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="points">The coordinates of the polygon</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, Point[] points) {
      return _AreaElements.AddArea(link, alternativeDescription, points);
    }

    /// <summary>
    /// Add a clickable polygon.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="points">The coordinates of the polygon</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, IEnumerable<Point> points) {
      return _AreaElements.AddArea(link, alternativeDescription, points);
    }

    #endregion

  }
}
