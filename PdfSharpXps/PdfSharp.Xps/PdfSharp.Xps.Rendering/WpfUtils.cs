using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PdfSharp.Xps.Rendering
{
  /// <summary>
  /// Some temporary stuff.
  /// </summary>
  static class WpfUtils
  {
    /// <summary>
    /// Converts a PolyQuadraticBezierSegment into a PolyLineSegment because I currently have no muse to calculate
    /// the correct Bézier curves.
    /// </summary>
    public static PdfSharp.Xps.XpsModel.PolyLineSegment FlattenSegment(PdfSharp.Xps.XpsModel.Point startPoint,
      PdfSharp.Xps.XpsModel.PolyQuadraticBezierSegment seg)
    {
      PathGeometry geo = new PathGeometry();
      PathFigure fig = new PathFigure();
      geo.Figures.Add(fig);
      fig.StartPoint = new Point(startPoint.X, startPoint.Y);
      int count = seg.Points.Count;
      Point[] points = new Point[count];
      for (int idx = 0; idx < count - 1; idx += 2)
      {
        QuadraticBezierSegment qbseg = new QuadraticBezierSegment(
          new Point(seg.Points[idx].X, seg.Points[idx].Y), new Point(seg.Points[idx + 1].X, seg.Points[idx + 1].Y), seg.IsStroked);
        fig.Segments.Add(qbseg);
      }
      geo = geo.GetFlattenedPathGeometry();
      fig = geo.Figures[0];
      PolyLineSegment lineSeg = (PolyLineSegment)fig.Segments[0];
      PdfSharp.Xps.XpsModel.PolyLineSegment resultSeg = new PdfSharp.Xps.XpsModel.PolyLineSegment();
      foreach (Point point in lineSeg.Points)
        resultSeg.Points.Add(new PdfSharp.Xps.XpsModel.Point(point.X, point.Y));
      return resultSeg;
    }

#if true
    /// <summary>
    /// Converts an ArcSegment into a PolyLineSegment because I currently have no muse to calculate
    /// the correct Bézier curves.
    /// </summary>
    public static PdfSharp.Xps.XpsModel.PolyLineSegment FlattenSegment(PdfSharp.Xps.XpsModel.Point startPoint,
      PdfSharp.Xps.XpsModel.ArcSegment seg)
    {
      PathGeometry geo = new PathGeometry();
      PathFigure fig = new PathFigure();
      geo.Figures.Add(fig);
      fig.StartPoint = new Point(startPoint.X, startPoint.Y);
      ArcSegment aseg = new ArcSegment(new Point(seg.Point.X, seg.Point.Y), new Size(seg.Size.Width, seg.Size.Height), seg.RotationAngle,
        seg.IsLargeArc, (SweepDirection)seg.SweepDirection, seg.IsStroked);
      fig.Segments.Add(aseg);
      geo = geo.GetFlattenedPathGeometry();
      fig = geo.Figures[0];
      //PolyLineSegment lineSeg = (PolyLineSegment)fig.Segments[0];
      PdfSharp.Xps.XpsModel.PolyLineSegment resultSeg = new PdfSharp.Xps.XpsModel.PolyLineSegment();
      int count = fig.Segments.Count;
      for (int idx = 0; idx < count; idx++)
      {
        PathSegment pathSeg = fig.Segments[idx];
        if (pathSeg is PolyLineSegment)
        {
          PolyLineSegment plseg = (PolyLineSegment)pathSeg;
          foreach (Point point in plseg.Points)
            resultSeg.Points.Add(new PdfSharp.Xps.XpsModel.Point(point.X, point.Y));
        }
        else if (pathSeg is LineSegment)
        {
          LineSegment lseg = (LineSegment)pathSeg;
          resultSeg.Points.Add(new PdfSharp.Xps.XpsModel.Point(lseg.Point.X, lseg.Point.Y));
        }
        else
        {
          Debugger.Break();
        }
      }
      return resultSeg;
    }
#endif
  }
}