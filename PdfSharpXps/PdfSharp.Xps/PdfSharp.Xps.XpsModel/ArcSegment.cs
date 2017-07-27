using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents an elliptical arc between two points.
  /// </summary>
  class ArcSegment : PathSegment
  {
    /// <summary>
    /// Gets the smallest rectangle that completely contains all points of the segments.
    /// </summary>
    public override XRect GetBoundingBox()
    {
      System.Windows.Media.ArcSegment arc = new System.Windows.Media.ArcSegment
      (new System.Windows.Point(Point.X, Point.Y), new System.Windows.Size(Size.Width, Size.Height), RotationAngle, IsLargeArc,
      (System.Windows.Media.SweepDirection)SweepDirection, IsStroked);
      System.Windows.Media.PathFigure figure = new System.Windows.Media.PathFigure();
      System.Windows.Media.PathGeometry geo = new System.Windows.Media.PathGeometry();
      System.Windows.Rect bounds = geo.Bounds;

      // TODO: incorrect result, just a hack
      XRect rect = new XRect(Point.X - Size.Width, Point.Y - Size.Height, 2 * Size.Width, 2 * Size.Height);
      return rect;
    }

    /// <summary>
    /// Specifies the endpoint of the elliptical arc.
    /// </summary>
    public Point Point { get; set; }

    /// <summary>
    /// Specifies the x and y radius of the elliptical arc as an x,y pair. 
    /// </summary>
    public Size Size { get; set; }

    /// <summary>
    /// Indicates how the ellipse is rotated relative to the current coordinate system. 
    /// </summary>
    public double RotationAngle { get; set; }

    /// <summary>
    /// Determines whether the arc is drawn with a sweep of 180 or greater. Can be true or false. 
    /// </summary>
    public bool IsLargeArc { get; set; }

    /// <summary>
    /// Specifies the direction in which the arc is drawn. Valid values are Clockwise and Counterclockwise.
    /// </summary>
    public SweepDirection SweepDirection { get; set; }

    /// <summary>
    /// Specifies whether the stroke for this segment of the path is drawn. Can be true or false. 
    /// </summary>
    public bool IsStroked { get; set; }
  }
}