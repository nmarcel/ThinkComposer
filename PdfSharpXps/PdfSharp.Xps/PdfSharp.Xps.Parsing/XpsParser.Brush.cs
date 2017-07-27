using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.IO;
using PdfSharp.Xps.XpsModel;

namespace PdfSharp.Xps.Parsing
{
  partial class XpsParser
  {
    /// <summary>
    /// Parses a Brush element.
    /// </summary>
    Brush ParseBrush()
    {
      Brush brush = null;
      switch (this.reader.Name)
      {
        case "ImageBrush":
          brush = ParseImageBrush();
          break;

        case "SolidColorBrush":
          brush = ParseSolidColorBrush();
          break;

        case "LinearGradientBrush":
          brush = ParseLinearGradientBrush();
          break;

        case "RadialGradientBrush":
          brush = ParseRadialGradientBrush();
          break;

        case "VisualBrush":
          brush = ParseVisualBrush();
          break;

        default:
          Debugger.Break();
          break;
      }
      return brush;
    }

    /// <summary>
    /// Parses a Brush attribute.
    /// </summary>
    Brush ParseBrush(string value)
    {
      Brush brush = TryParseStaticResource<Brush>(value);
      if (brush != null)
        return brush;
      return Brush.Parse(value);
    }
  }
}