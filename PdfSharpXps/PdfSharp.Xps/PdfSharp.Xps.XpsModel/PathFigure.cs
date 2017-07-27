using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Specifies a set of one or more segment elements defining a closed region.
  /// </summary>
  class PathFigure : XpsElement
  {
    /// <summary>
    /// Gets the smallest rectangle that completely contains all segments of the figure.
    /// </summary>
    public XRect GetBoundingBox()
    {
      XRect rect = new XRect(StartPoint.X, StartPoint.Y, 0, 0);
      foreach (PathSegment segment in Segments)
        rect.Union(segment.GetBoundingBox());
      return rect;
    }

    /// <summary>
    /// Specifies whether the path is closed. If set to true, the stroke is drawn "closed," that is,
    /// the last point in the last segment of the path figure is connected with the point specified
    /// in the StartPoint attribute, otherwise the stroke is drawn "open," and the last point is not
    /// connected to the start point. Only applicable if the path figure is used in a <Path> element
    /// that specifies a stroke.
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// Specifies the starting point for the first segment of the path figure.
    /// </summary>
    public Point StartPoint { get; set; }

    /// <summary>
    /// Specifies whether the path figure is used in computing the area of the containing path geometry.
    /// Can be true or false. When set to false, the path figure is considered only for stroking. 
    /// </summary>
    public bool IsFilled { get; set; }

    public PathSegmentCollection Segments = new PathSegmentCollection();
  }
}