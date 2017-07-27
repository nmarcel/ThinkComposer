using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Internal;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents the width and height of an object.
  /// </summary>
  [DebuggerDisplay("X={X}, Y={Y}, Width={Width}, Height={Height}")]
  public struct Rect
  {
    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public Point TopLeft
    {
      get { return new Point(X, Y); }
      set { X = value.X; Y = value.Y; }
    }

    public Size Size
    {
      get { return new Size(Width, Height); }
      set { Width = value.Width; Height = value.Height; }
    }

    internal void Union(Rect rect)
    {
      double minX = Math.Min(X, rect.X);
      double minY = Math.Min(Y, rect.Y);
      double maxWidth = Math.Max(X + Width, rect.X + rect.Width);
      double maxHeight = Math.Max(Y + Height, rect.Y + Height);
      X = minX;
      Y = minY;
      Width = Math.Max(maxWidth - minX, 0.0);
      Height = Math.Max(maxHeight - minY, 0.0);
    }

    internal void Union(Point point)
    {
      double minX = Math.Min(X, point.X);
      double minY = Math.Min(Y, point.Y);
      double maxWidth = Math.Max(X + Width, point.X);
      double maxHeight = Math.Max(Y + Height, point.Y);
      X = minX;
      Y = minY;
      Width = Math.Max(maxWidth - minX, 0.0);
      Height = Math.Max(maxHeight - minY, 0.0);
    }

    /// <summary>
    /// Parses the specified value.
    /// </summary>
    public static Rect Parse(string value)
    {
      Rect rect = new Rect();
      IFormatProvider formatProvider = CultureInfo.InvariantCulture;
      TokenizerHelper helper = new TokenizerHelper(value, formatProvider);
      rect.X = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      rect.Y = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      rect.Width = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      rect.Height = Convert.ToDouble(helper.NextTokenRequired(), formatProvider);
      return rect;
    }
  }
}
