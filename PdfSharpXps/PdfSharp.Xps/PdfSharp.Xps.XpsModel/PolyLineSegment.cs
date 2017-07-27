using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Specifies a set of points between which lines are drawn.
  /// </summary>
  class PolyLineSegment : PathSegment
  {
    /// <summary>
    /// Gets the smallest rectangle that completely contains all points of the segments.
    /// </summary>
    public override XRect GetBoundingBox()
    {
      return Points.GetBoundingBox();
    }

    /// <summary>
    /// Specifies whether the stroke for this segment of the path is drawn. Can be true or false. 
    /// </summary>
    public bool IsStroked { get; set; }

    /// <summary>
    /// Specifies a set of coordinates for the multiple segments that define the poly line segment.
    /// Coordinate values within each pair are comma-separated and additional whitespace may appear.
    /// Coordinate pairs are separated from other coordinate pairs by whitespace. 
    /// </summary>
    public PointStopCollection Points = new PointStopCollection();
  }
}