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
    /// Parses a LinearGradientBrush element.
    /// </summary>
    LinearGradientBrush ParseLinearGradientBrush()
    {
      Debug.Assert(this.reader.Name == "LinearGradientBrush");
      bool isEmptyElement = this.reader.IsEmptyElement;
      LinearGradientBrush brush = new LinearGradientBrush();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Opacity":
            brush.Opacity = ParseDouble(this.reader.Value);
            break;

          case "Transform":
            brush.Transform = ParseMatrixTransform(this.reader.Value);
            break;

          case "ColorInterpolationMode":
            brush.ColorInterpolationMode = ParseEnum<ClrIntMode>(this.reader.Value);
            break;

          case "SpreadMethod":
            brush.SpreadMethod = ParseEnum<SpreadMethod>(this.reader.Value);
            break;

          case "MappingMode":
            brush.MappingMode = ParseEnum<MappingMode>(this.reader.Value);
            break;

          case "StartPoint":
            brush.StartPoint = Point.Parse(this.reader.Value);
            break;

          case "EndPoint":
            brush.EndPoint = Point.Parse(this.reader.Value);
            break;

          case "x:Key":
            brush.Key = this.reader.Value;
            break;

          default:
            Debugger.Break();
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
            case "LinearGradientBrush.Transform":
              MoveToNextElement();
              brush.Transform = ParseMatrixTransform();
              MoveToNextElement();
              break;

            case "LinearGradientBrush.GradientStops":
              // do not MoveToNextElement();
              brush.GradientStops = ParseGradientStops();
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return brush;
    }
  }
}