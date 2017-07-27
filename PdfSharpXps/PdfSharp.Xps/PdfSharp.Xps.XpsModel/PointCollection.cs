using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents a collection of Point objecs.
  /// </summary>
  class PointStopCollection : List<Point>
  {
    // Currently just a placeholder of a generic list.

    /// <summary>
    /// Gets the smallest rectangle that completely contains all points of the collection.
    /// </summary>
    internal XRect GetBoundingBox()
    {
      XRect rect = XRect.Empty;
      foreach (Point point in this)
        rect.Union(new XPoint(point.X, point.Y));
      return rect;
    }
  }
}