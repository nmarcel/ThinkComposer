using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Specifies where a consumer should place a signature spot.
  /// </summary>
  class SpotLocation : XpsElement
  {
    /// <summary>
    /// Specifies the page on which the signature spot should be displayed.
    /// </summary>
    public string PageURI { get; set; }

    /// <summary>
    /// Specifies the x coordinate of the origin point (upper-left corner) on the page where
    /// the signature spot should be displayed.
    /// </summary>
    public double StartX { get; set; }

    /// <summary>
    /// Specifies the y coordinate of the origin point (upper-left corner) on the page where
    /// the signature spot should be displayed.
    /// </summary>
    public double StartY { get; set; }
  }
}