using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using GenHTTP.Abstraction.Elements.Containers;

namespace GenHTTP.Abstraction.Elements.Collections {
  
  /// <summary>
  /// Allows you to add areas to a container.
  /// </summary>
  public class AreaCollection : IAreaContainer {
    private AddElement _Delegate;

    #region Constructors

    /// <summary>
    /// Create a new area collection.
    /// </summary>
    /// <param name="d">The method used to add elements to the underlying container</param>
    public AreaCollection(AddElement d) {
      _Delegate = d;
    }

    #endregion

    #region IAreaContainer Members

    /// <summary>
    /// Add a new, empty area.
    /// </summary>
    /// <returns>The created object</returns>
    public Area AddArea() {
      Area area = new Area();
      _Delegate(area);
      return area;
    }

    /// <summary>
    /// Add a clickable rectangle.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="rectangle">The coordinates of the area</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, Rectangle rectangle) {
      Area area = new Area(link, alternativeDescription, rectangle);
      _Delegate(area);
      return area;
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
      Area area = new Area(link, alternativeDescription, point, radius);
      _Delegate(area);
      return area;
    }

    /// <summary>
    /// Add a clickable polygon.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="points">The coordinates of the polygon</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, Point[] points) {
      Area area = new Area(link, alternativeDescription, points);
      _Delegate(area);
      return area;
    }

    /// <summary>
    /// Add a clickable polygon.
    /// </summary>
    /// <param name="link">The URL of the document to link</param>
    /// <param name="alternativeDescription">The alternative description of the document</param>
    /// <param name="points">The coordinates of the polygon</param>
    /// <returns>The created object</returns>
    public Area AddArea(string link, string alternativeDescription, IEnumerable<Point> points) {
      Area area = new Area(link, alternativeDescription, points);
      _Delegate(area);
      return area;
    }

    #endregion

  }

}
