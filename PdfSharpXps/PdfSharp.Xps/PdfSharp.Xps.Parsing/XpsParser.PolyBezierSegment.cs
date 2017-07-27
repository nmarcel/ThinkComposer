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
    PolyBezierSegment ParsePolyBezierSegment()
    {
      Debug.Assert(this.reader.Name == "PolyBezierSegment");
      PolyBezierSegment seg = new PolyBezierSegment();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "IsStroked":
            seg.IsStroked = ParseBool(this.reader.Value);
            break;

          case "Points":
            seg.Points = Point.ParsePoints(this.reader.Value);
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