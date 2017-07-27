using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Fills defined geometric regions with a solid color.
  /// </summary>
  class SolidColorBrush : Brush
  {
    public SolidColorBrush()
    {
      Opacity = 1;
    }

    /// <summary>
    /// Defines the uniform transparency of the brush fill. Values range from 0 (fully transparent)
    /// to 1 (fully opaque), inclusive. Values outside of this range are invalid. 
    /// </summary>
    public double Opacity { get; set; }

    /// <summary>
    /// Specifies a name for a resource in a resource dictionary. x:Key MUST be present when the
    /// current element is defined in a resource dictionary. x:Key MUST NOT be specified outside of
    /// a resource dictionary [M6.1]. 
    /// </summary>
    // x:Key
    public string Key { get; set; }    

    /// <summary>
    /// Specifies the color for filled elements. An sRGB color value specified as a 6-digit
    /// hexadecimal number (#RRGGBB) or an extended color. 
    /// </summary>
    public Color Color { get; set; }
  }
}