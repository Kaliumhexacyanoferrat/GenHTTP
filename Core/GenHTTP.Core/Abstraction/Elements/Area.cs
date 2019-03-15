/*

Updated: 2009/10/22

2009/10/22  Andreas Nägeli        Initial version of this file.


LICENSE: This file is part of the GenHTTP webserver.

GenHTTP is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GenHTTP.Abstraction.Elements {
  
  /// <summary>
  /// Describes an area of a map.
  /// </summary>
  public class Area : Element {
    private Shape _Shape;
    private string _Coords;
    private string _Link;
    private string _AlternativeDescription;
    private bool _NoLink;

    #region Constructors

    /// <summary>
    /// Create a new, empty area.
    /// </summary>
    /// <remarks>
    /// You need to set the coords and the alternative description of this area before you can serialize it.
    /// </remarks>
    public Area() {

    }

    /// <summary>
    /// Create a new area.
    /// </summary>
    /// <param name="link">The link of the area</param>
    /// <param name="alternativeDescription">The description of the area</param>
    /// <param name="rectangle">The coordinates of the area</param>
    public Area(string link, string alternativeDescription, Rectangle rectangle) {
      Link = link;
      AlternativeDescription = alternativeDescription;
      Coords = rectangle.X + "," + rectangle.Y + "," + (rectangle.X + rectangle.Width) + "," + (rectangle.Y + rectangle.Height);
    }

    /// <summary>
    /// Create a new area.
    /// </summary>
    /// <param name="link">The link of the area</param>
    /// <param name="alternativeDescription">The description of the area</param>
    /// <param name="point">The middle point of the circle</param>
    /// <param name="radius">The radius of the circle</param>
    public Area(string link, string alternativeDescription, Point point, ushort radius) {
      Link = link;
      AlternativeDescription = alternativeDescription;
      Coords = point.X + "," + point.Y + "," + radius;
    }

    /// <summary>
    /// Create a new area.
    /// </summary>
    /// <param name="link">The link of the area</param>
    /// <param name="alternativeDescription">The description of the area</param>
    /// <param name="points">The points of the polygon</param>
    public Area(string link, string alternativeDescription, Point[] points) {
      Link = link;
      AlternativeDescription = alternativeDescription;
      string coords = "";
      foreach (Point p in points) {
        coords += "," + p.X + "," + p.Y;
      }
      if (coords.Length > 0) coords = coords.Substring(1);
      Coords = coords;
    }

    /// <summary>
    /// Create a new area.
    /// </summary>
    /// <param name="link">The link of this area</param>
    /// <param name="alternativeDescription">The description of this area</param>
    /// <param name="points">The points of the polygon</param>
    public Area(string link, string alternativeDescription, IEnumerable<Point> points) {
      Link = link;
      AlternativeDescription = alternativeDescription;
      string coords = "";
      foreach (Point p in points) {
        coords += "," + p.X + "," + p.Y;
      }
      if (coords.Length > 0) coords = coords.Substring(1);
      Coords = coords;
    }

    #endregion

    #region get-/setters

    /// <summary>
    /// The shape of this area.
    /// </summary>
    public Shape Shape {
      get { return _Shape; }
      set { _Shape = value; }
    }

    /// <summary>
    /// The coordinates of the area.
    /// </summary>
    public string Coords {
      get { return _Coords; }
      set {
        if (value == null || value.Length == 0) throw new ArgumentException("The coordinates of an area cannot be empty or null");
        _Coords = value;
      }
    }

    /// <summary>
    /// The link which is opened on click on the area.
    /// </summary>
    public string Link {
      get { return _Link; }
      set { _Link = value; }
    }

    /// <summary>
    /// An alternative description (required).
    /// </summary>
    public string AlternativeDescription {
      get { return _AlternativeDescription; }
      set {
        if (value == null || value.Length == 0) throw new ArgumentException("The alternative description is required and can therefor not be null or empty");
        _AlternativeDescription = value;
      }
    }

    /// <summary>
    /// Specifies, whether something should happen if the user clicks on the area.
    /// </summary>
    /// <remarks>
    /// By default, this value is set to true.
    /// </remarks>
    public bool NoLink {
      get { return _NoLink; }
      set { _NoLink = value; }
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize this element.
    /// </summary>
    /// <param name="b">The string builder to write to</param>
    /// <param name="type">The output type</param>
    public override void Serialize(StringBuilder b, DocumentType type) {
      b.Append("  <area" + ToXHtml(IsXHtml));
      b.Append(" shape=\"" + GetShape(_Shape) + "\"");
      b.Append(" coords=\"" + _Coords + "\"");
      b.Append(" alt=\"" + _AlternativeDescription + "\"");
      if (_Link != null && _Link.Length > 0) b.Append(" href=\"" + _Link + "\"");
      if (_NoLink) b.Append(" nohref=\"nohref\"");
      if (IsXHtml) b.Append(" />"); else b.Append(">");
      b.Append("\r\n");
    }

    /// <summary>
    /// Retrieve the (X)HTML representation of a shape.
    /// </summary>
    /// <param name="shape">The type of a shape to convert</param>
    /// <returns>The (X)HTML representation of this shape type</returns>
    public static string GetShape(Shape shape) {
      if (shape == Shape.Rectangle) return "rect";
      if (shape == Shape.Polygon) return "poly";
      return "circle";
    }

    #endregion

  }

}
