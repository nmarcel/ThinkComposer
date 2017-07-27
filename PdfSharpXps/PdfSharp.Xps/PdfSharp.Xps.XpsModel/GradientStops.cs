using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Indicates a location and range of color progression for rendering a gradien
  /// </summary>
  class GradientStop : XpsElement
  {
    /// <summary>
    /// Specifies the gradient stop color. An sRGB color value specified as a 6-digit hexadecimal number
    /// (#RRGGBB) or an extended color. 
    /// </summary>
    public Color Color { get; set; }

    /// <summary>
    /// Specifies the gradient offset. The offset indicates a point along the progression of the gradient
    /// at which a color is specified. Colors between gradient offsets in the progression are interpolated.
    /// </summary>
    public double Offset { get; set; }
  }
}