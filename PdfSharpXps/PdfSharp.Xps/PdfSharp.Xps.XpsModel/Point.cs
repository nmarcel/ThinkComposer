using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Internal;
using PdfSharp.Xps.Parsing;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents the width and height of an object.
  /// </summary>
  public struct Point
  {
    public Point(double x, double y)
      : this()
    {
      X = x;
      Y = y;
    }

    public double X { get; set; }

    public double Y { get; set; }

    public static bool operator ==(Point point1, Point point2)
    {
      return point1.X == point2.X && point1.Y == point2.Y;
    }

    public static bool operator !=(Point point1, Point point2)
    {
      return !(point1 == point2);
    }

    public static bool Equals(Point point1, Point point2)
    {
      return point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y);
    }

    public override bool Equals(object o)
    {
      if (o == null || !(o is Point))
        return false;
      Point point = (Point)o;
      return Equals(this, point);
    }

    public bool Equals(Point value)
    {
      return Equals(this, value);
    }

    public override int GetHashCode()
    {
      return this.X.GetHashCode() ^ this.Y.GetHashCode();
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="PdfSharp.Xps.XpsModel.Point"/> to <see cref="PdfSharp.Drawing.XPoint"/>.
    /// </summary>
    public static implicit operator XPoint(Point point)
    {
      return new XPoint(point.X, point.Y);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="PdfSharp.Drawing.XPoint"/> to <see cref="PdfSharp.Xps.XpsModel.Point"/>.
    /// </summary>
    public static implicit operator Point(XPoint point)
    {
      return new Point(point.X, point.Y);
    }

    /// <summary>
    /// Parses the specified value.
    /// </summary>
    public static Point Parse(string value)
    {
      Point point = new Point();
      TokenizerHelper tokenizer = new TokenizerHelper(value);
      point.X = ParserHelper.ParseDouble(tokenizer.NextTokenRequired());
      point.Y = ParserHelper.ParseDouble(tokenizer.NextTokenRequired());
      return point;
    }

    /// <summary>
    /// Parses a series of points.
    /// </summary>
    internal static PointStopCollection ParsePoints(string value)
    {
      PointStopCollection points = new PointStopCollection();
      TokenizerHelper tokenizer = new TokenizerHelper(value);
      while (tokenizer.NextToken())
      {
        Point point = new Point(ParserHelper.ParseDouble(tokenizer.GetCurrentToken()), ParserHelper.ParseDouble(tokenizer.NextTokenRequired()));
        points.Add(point);
      }
      return points;
    }
  }
}