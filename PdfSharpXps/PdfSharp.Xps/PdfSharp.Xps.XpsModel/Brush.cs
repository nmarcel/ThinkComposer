using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Base class of all five brush types
  /// </summary>
  class Brush : XpsElement
  {
    internal static Brush Parse(string value)
    {
      SolidColorBrush brush = new SolidColorBrush();
      brush.Color = Color.Parse(value);
      return brush;
    }
  }
}