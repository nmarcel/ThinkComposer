using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Contains a set of <PathFigure> elements.
  /// </summary>
  class PathGeometry : XpsElement
  {
    //PathGeometry()
    //{
    //  FillRule = FillRule.EvenOdd;
    //}

    /// <summary>
    /// Gets the smallest rectangle that completely contains all figures of the path.
    /// </summary>
    public XRect GetBoundingBox()
    {
      XRect rect = XRect.Empty;
      foreach (PathFigure figure in Figures)
        rect.Union(figure.GetBoundingBox());
      return rect;
    }

    /// <summary>
    /// Describes the geometry of the path.
    /// </summary>
    public PathFigureCollection Figures = new PathFigureCollection();

    /// <summary>
    /// Specifies how the intersecting areas of geometric shapes are combined to form a region.
    /// Valid values are EvenOdd and NonZero. 
    /// </summary>
    public FillRule FillRule { get; set; }

    /// <summary>
    /// Specifies the local matrix transformation that is applied to all child and descendant elements 
    /// of the path geometry before it is used for filling, clipping, or stroking.
    /// </summary>
    public MatrixTransform Transform { get; set; }

    /// <summary>
    /// Specifies a name for a resource in a resource dictionary. x:Key MUST be present when the
    /// current element is defined in a resource dictionary. x:Key MUST NOT be specified outside of
    /// a resource dictionary [M4.2]. 
    /// </summary>
    // x:Key
    public string Key { get; set; }
  }
}