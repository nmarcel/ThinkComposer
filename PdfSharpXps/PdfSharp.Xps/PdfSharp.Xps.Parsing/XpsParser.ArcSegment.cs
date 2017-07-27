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
    /// Parses a PolyBezierSegment element.
    /// </summary>
    ArcSegment ParseArcSegment()
    {
      Debug.Assert(this.reader.Name == "ArcSegment");
      ArcSegment seg = new ArcSegment();
      seg.IsStroked = true;
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Point":
            seg.Point = Point.Parse(this.reader.Value);
            break;

          case "Size":
            seg.Size = Size.Parse(this.reader.Value);
            break;

          case "RotationAngle":
            seg.RotationAngle = ParseDouble(this.reader.Value);
            break;

          case "IsLargeArc":
            seg.IsLargeArc = ParseBool(this.reader.Value);
            break;

          case "SweepDirection":
            seg.SweepDirection = ParseEnum<SweepDirection>(this.reader.Value);
            break;

          case "IsStroked":
            seg.IsStroked = ParseBool(this.reader.Value);
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveBeyondThisElement();
      return seg;
    }
  }
}