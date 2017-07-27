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
    /// Parses a PathFigure element.
    /// </summary>
    PathFigure ParsePathFigure()
    {
      Debug.Assert(this.reader.Name == "PathFigure");
      bool isEmptyElement = this.reader.IsEmptyElement;
      PathFigure fig = new PathFigure();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "IsClosed":
            fig.IsClosed = ParseBool(this.reader.Value);
            break;

          case "StartPoint":
            fig.StartPoint = Point.Parse(this.reader.Value);
            break;

          case "IsFilled":
            fig.IsFilled = ParseBool(this.reader.Value);
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      if (!isEmptyElement)
      {
        MoveToNextElement();
        while (this.reader.IsStartElement())
        {
          switch (this.reader.Name)
          {
            case "PolyLineSegment":
              fig.Segments.Add(ParsePolyLineSegment());
              break;

            case "PolyBezierSegment":
              fig.Segments.Add(ParsePolyBezierSegment());
              break;

            case "ArcSegment":
              fig.Segments.Add(ParseArcSegment());
              break;

            case "PolyQuadraticBezierSegment":
              fig.Segments.Add(ParsePolyQuadraticBezierSegment());
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return fig;
    }
  }
}