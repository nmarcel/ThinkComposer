using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Base class of a path segment classes.
  /// </summary>
  abstract class PathSegment : XpsElement
  {
    /// <summary>
    /// When overridden in a derived class, gets the smallest rectangle that completely contains all points of the segments.
    /// </summary>
    public abstract XRect GetBoundingBox();
  }
}