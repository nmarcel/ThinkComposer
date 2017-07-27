using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using PdfSharp.Xps.XpsModel;

namespace PdfSharp.Xps.Parsing
{
  partial class XpsParser
  {
    /// <summary>
    /// Parses a Path element.
    /// </summary>
    Path ParsePath()
    {
      Debug.Assert(this.reader.Name == "Path");
      bool isEmptyElement = this.reader.IsEmptyElement;
      Path path = new Path();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Data":
            path.Data = ParsePathGeometry(this.reader.Value);
            break;

          case "Fill":
            path.Fill = ParseBrush(this.reader.Value); 
            break;

          case "RenderTransform":
            path.RenderTransform = ParseMatrixTransform(this.reader.Value);
            break;

          case "Clip":
            path.Clip = ParsePathGeometry(this.reader.Value);
            break;

          case "Opacity":
            path.Opacity = ParseDouble(this.reader.Value);
            break;

          case "OpacityMask":
            path.OpacityMask = ParseBrush(this.reader.Value);
            break;

          case "Stroke":
            path.Stroke = ParseBrush(this.reader.Value);
            break;

          case "StrokeDashArray":
            path.StrokeDashArray = reader.Value;
            break;

          case "StrokeDashCap":
            path.StrokeDashCap = reader.Value;
            break;

          case "StrokeDashOffset":
            path.StrokeDashOffset = ParseDouble(reader.Value);
            break;

          case "StrokeEndLineCap":
            path.StrokeEndLineCap = ParseEnum<LineCap>(this.reader.Value);
            break;

          case "StrokeStartLineCap":
            path.StrokeStartLineCap = ParseEnum<LineCap>(this.reader.Value);
            break;

          case "StrokeLineJoin":
            path.StrokeLineJoin = ParseEnum<LineJoin>(this.reader.Value);
            break;

          case "StrokeMiterLimit":
            path.StrokeMiterLimit = ParseDouble(this.reader.Value);
            break;

          case "StrokeThickness":
            path.StrokeThickness = ParseDouble(this.reader.Value);
            break;

          case "Name":
            path.Name = this.reader.Value;
            break;

          case "FixedPage_NavigateUri":
          case "FixedPage.NavigateUri":
            path.FixedPage_NavigateUri = reader.Value;
            break;

          case "AutomationProperties_Name":
            path.AutomationProperties_Name = this.reader.Value;
            break;

          case "AutomationProperties.HelpText":
            path.AutomationProperties_HelpText = this.reader.Value;
            break;

          case "SnapsToDevicePixels":
            path.SnapsToDevicePixels = ParseBool(this.reader.Value);
            break;

          case "xml:lang":
            path.lang = this.reader.Value;
            break;

          case "x:Key":
            path.Key = this.reader.Value;
            break;

          case "xml:id":
            break;

          case "xml:space":
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
            case "Path.RenderTransform":
              MoveToNextElement();
              path.RenderTransform = ParseMatrixTransform();
              break;

            case "Path.Clip":
              MoveToNextElement();
              path.Clip = ParsePathGeometry();
              MoveToNextElement();
              break;

            case "Path.OpacityMask":
              MoveToNextElement();
              path.OpacityMask = ParseBrush();
              path.OpacityMask.Parent = path;
              MoveToNextElement();
              break;

            case "Path.Fill":
              MoveToNextElement();
              path.Fill = ParseBrush();
              path.Fill.Parent = path;
              MoveToNextElement();
              break;

            case "Path.Stroke":
              MoveToNextElement();
              path.Fill = ParseBrush();
              path.Fill.Parent = path;
              MoveToNextElement();
              break;

            case "Path.Data":
              MoveToNextElement();
              path.Data = ParsePathGeometry();
              MoveToNextElement();
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return path;
    }
  }
}