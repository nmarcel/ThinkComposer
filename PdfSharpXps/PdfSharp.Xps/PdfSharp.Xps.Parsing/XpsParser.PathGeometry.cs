using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.IO;
using PdfSharp.Internal;
using PdfSharp.Xps.XpsModel;

namespace PdfSharp.Xps.Parsing
{
  partial class XpsParser
  {
    /// <summary>
    /// Parses a PathGeometry element.
    /// </summary>
    PathGeometry ParsePathGeometry()
    {
      Debug.Assert(this.reader.Name == "PathGeometry");
      bool isEmptyElement = this.reader.IsEmptyElement;
      PathGeometry geo = new PathGeometry();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Transform":
            geo.Transform = ParseMatrixTransform(this.reader.Value);
            break;

          case "Figures":
            geo.Figures = ParsePathGeometry(this.reader.Value).Figures;
            break;

          case "FillRule":
            geo.FillRule = ParseEnum<FillRule>(this.reader.Value);
            break;

          case "x:Key":
            geo.Key = this.reader.Value;
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
            case "PathGeometry.RenderTransform":
              MoveToNextElement();
              geo.Transform = ParseMatrixTransform();
              MoveToNextElement();
              break;

            case "PathFigure":
              geo.Figures.Add(ParsePathFigure());
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return geo;
    }

    /// <summary>
    /// Parses a PathGeometry from a data string element.
    /// </summary>
    PathGeometry ParsePathGeometry(string data)
    {
#if DEBUG_
      // XPS = M 20,100 C 45,50 70,150 95,100 S 145,150 170,100 220,150 245,100 C 220,50 195,150 170,100 S 120,150 95,100 45,150 20,100
      // XXX = M20,100C45,50 70,150 95,100 120,50 145,150 170,100 195,50 220,150 245,100 220,50 195,150 170,100 145,50 120,150 95,100 70,50 45,150 20,100
      if (data.StartsWith("M 20,100 C 45,50 70,150 95,100 S 145,"))
        Debugger.Break();
#endif
      PathGeometry geo = TryParseStaticResource<PathGeometry>(data);
      if (geo != null)
        return geo;

      data = FixHack(data);
      // From the algorithm on page 365 in XPS 1.0 specs
      // See Petzold page 813
      geo = new PathGeometry();
      Point point = new Point();
      PathFigure figure = null;
      TokenizerHelper helper = new TokenizerHelper(data);
      helper.NextTokenRequired();
      do
      {
        string token = helper.GetCurrentToken();
        switch (token[0])
        {
          // FillRule
          case 'F':
            geo.FillRule = helper.NextTokenRequired() == "1" ? FillRule.NonZero : FillRule.EvenOdd;
            break;

          // Move 
          case 'M':
            {
              figure = new PathFigure();
              geo.Figures.Add(figure);
              point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
              figure.StartPoint = point;
            }
            break;

          // Move 
          case 'm':
            {
              figure = new PathFigure();
              geo.Figures.Add(figure);
              point = new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired()));
              figure.StartPoint = point;
            }
            break;

          // Line 
          case 'L':
            {
              PolyLineSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyLineSegment) != null)
              { }
              else
              {
                seg = new PolyLineSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Line 
          case 'l':
            {
              PolyLineSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyLineSegment) != null)
              { }
              else
              {
                seg = new PolyLineSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Horizontal Line 
          case 'H':
            {
              PolyLineSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyLineSegment) != null)
              { }
              else
              {
                seg = new PolyLineSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                point.X = ParseDouble(helper.NextTokenRequired());
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Horizontal Line 
          case 'h':
            {
              PolyLineSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyLineSegment) != null)
              { }
              else
              {
                seg = new PolyLineSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                point.X += ParseDouble(helper.NextTokenRequired());
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Vertical Line 
          case 'V':
            {
              PolyLineSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyLineSegment) != null)
              { }
              else
              {
                seg = new PolyLineSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                point.Y = ParseDouble(helper.NextTokenRequired());
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Vertical Line 
          case 'v':
            {
              PolyLineSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyLineSegment) != null)
              { }
              else
              {
                seg = new PolyLineSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                point.Y += ParseDouble(helper.NextTokenRequired());
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Elliptical Arc
          case 'A':
            do
            {
              // I cannot believe it: "A70.1,50.1 1,34 0 0 170.1,30.1"
              // The rotation angle "1,34" uses a ',' instead of a '.' in my German Windows Vista!
              //A70.1,50.1    1,34   0   0   170.1,30.1
              ArcSegment seg = new ArcSegment();
              figure.Segments.Add(seg);
              seg.Size = new Size(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
              seg.RotationAngle = ParseDouble(helper.NextTokenRequired());
              seg.IsLargeArc = helper.NextTokenRequired() == "1";
              seg.SweepDirection = helper.NextTokenRequired() == "1" ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
              point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
              seg.Point = point;
            } while (!Char.IsLetter(helper.PeekNextCharacter()));
            break;

          // Elliptical Arc
          case 'a':
            do
            {
              ArcSegment seg = new ArcSegment();
              figure.Segments.Add(seg);
              seg.Size = new Size(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
              seg.RotationAngle = ParseDouble(helper.NextTokenRequired());
              seg.IsLargeArc = helper.NextTokenRequired() == "1";
              seg.SweepDirection = helper.NextTokenRequired() == "1" ? SweepDirection.Clockwise : SweepDirection.Counterclockwise;
              point = new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired()));
              seg.Point = point;
            } while (!Char.IsLetter(helper.PeekNextCharacter()));
            break;

          // Cubic Bézier Curve
          case 'C':
            {
              PolyBezierSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyBezierSegment) != null)
              { }
              else
              {
                seg = new PolyBezierSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                seg.Points.Add(new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired())));
                seg.Points.Add(new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired())));
                point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Cubic Bézier Curve
          case 'c':
            {
              PolyBezierSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyBezierSegment) != null)
              { }
              else
              {
                seg = new PolyBezierSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                seg.Points.Add(new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired())));
                seg.Points.Add(new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired())));
                point = new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Smooth Cubic Bézier Curve
          case 'S':
            {
              PolyBezierSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyBezierSegment) != null)
              { }
              else
              {
                seg = new PolyBezierSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                Point pt = new Point();
                int count = seg.Points.Count;
                segCount = figure.Segments.Count;
                if (count > 0)
                {
                  Point lastCtrlPoint = seg.Points[count - 2];
                  pt.X = 2 * point.X - lastCtrlPoint.X;
                  pt.Y = 2 * point.Y - lastCtrlPoint.Y;
                }
                else if (segCount > 1 && figure.Segments[count - 2] is PolyBezierSegment)
                {
                  PolyBezierSegment lastSeg = (PolyBezierSegment)figure.Segments[count - 2];
                  count = lastSeg.Points.Count;
                  Point lastCtrlPoint = lastSeg.Points[count - 2];
                  pt.X = 2 * point.X - lastCtrlPoint.X;
                  pt.Y = 2 * point.Y - lastCtrlPoint.Y;
                }
                else
                {
                  pt = point;
                }
                seg.Points.Add(pt);
                seg.Points.Add(new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired())));
                point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Smooth Cubic Bézier Curve
          case 's':
            {
              PolyBezierSegment seg;
              int segCount = figure.Segments.Count;
              if (segCount > 0 && (seg = figure.Segments[segCount - 1] as PolyBezierSegment) != null)
              { }
              else
              {
                seg = new PolyBezierSegment();
                figure.Segments.Add(seg);
              }
              do
              {
                Point pt = new Point();
                int count = seg.Points.Count;
                segCount = figure.Segments.Count;
                if (count > 0)
                {
                  Point lastCtrlPoint = seg.Points[count - 2];
                  pt.X = 2 * point.X - lastCtrlPoint.X;
                  pt.Y = 2 * point.Y - lastCtrlPoint.Y;
                }
                else if (segCount > 1 && figure.Segments[count - 2] is PolyBezierSegment)
                {
                  PolyBezierSegment lastSeg = (PolyBezierSegment)figure.Segments[count - 2];
                  count = lastSeg.Points.Count;
                  Point lastCtrlPoint = lastSeg.Points[count - 2];
                  pt.X = 2 * point.X - lastCtrlPoint.X;
                  pt.Y = 2 * point.Y - lastCtrlPoint.Y;
                }
                else
                {
                  pt = point;
                }
                seg.Points.Add(pt);
                seg.Points.Add(new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired())));
                point = new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Quadratic Bézier Curve
          case 'Q':
            {
              PolyQuadraticBezierSegment seg = new PolyQuadraticBezierSegment();
              figure.Segments.Add(seg);
              do
              {
                seg.Points.Add(new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired())));
                point = new Point(ParseDouble(helper.NextTokenRequired()), ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Quadratic Bézier Curve
          case 'q':
            {
              PolyQuadraticBezierSegment seg = new PolyQuadraticBezierSegment();
              figure.Segments.Add(seg);
              do
              {
                seg.Points.Add(new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired())));
                point = new Point(point.X + ParseDouble(helper.NextTokenRequired()), point.Y + ParseDouble(helper.NextTokenRequired()));
                seg.Points.Add(point);
              } while (!Char.IsLetter(helper.PeekNextCharacter()));
            }
            break;

          // Close
          case 'Z':
          case 'z':
            {
              figure.IsClosed = true;
              if (figure.Segments.Count > 0)
              {
                PathSegment seg = figure.Segments[0];
              }
              point = figure.StartPoint;
              figure = null;
            }
            break;

          default:
            Debug.Assert(false);
            break;
        }
      } while (helper.NextToken());
      return geo;
    }

    /// <summary>
    /// Hack to insert blanks because I do not like to fix Tokenizer at this moment.
    /// </summary>
    string FixHack(string data)
    {
      int length = data.Length;
      StringBuilder s = new StringBuilder(length * 2);

      for (int idx = 0; idx < length; idx++)
      {
        char ch = data[idx];
        // Skip exponent 'E'
        if (Char.IsLetter(ch) && ch != 'E' && ch != 'e')
        {
          s.Append(' ');
          s.Append(ch);
          s.Append(' ');
        }
        else
          s.Append(ch);
      }

      //A  70.1,50.1    1,34   0   0   170.1,30.1

      data = s.ToString();

      // Fix this: "A  70.1,50.1    1,34   0   0   170.1,30.1"
      for (int idx = 0; idx < length; idx++)
      {
        if (data[idx] == 'A')
        {
          int blank1 = data.IndexOf(' ', idx + 3);
          int blank2 = data.IndexOf(' ', blank1 + 1);
          //string test = data.Substring(blank1, blank2 - blank1);
          //test.GetType();
          int comma = data.IndexOf(',', blank1);
          if (comma > 0 && comma < blank2)
            s[comma] = '.';
        }
      }
      data = s.ToString();
      return data;
    }

  }
}