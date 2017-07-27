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
    /// Parses a RadialGradientBrush element.
    /// </summary>
    RadialGradientBrush ParseRadialGradientBrush()
    {
      Debug.Assert(this.reader.Name == "RadialGradientBrush");
      bool isEmptyElement = this.reader.IsEmptyElement;
      RadialGradientBrush brush = new RadialGradientBrush();
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
            brush.ColorInterpolationMode= ParseEnum<ClrIntMode>(this.reader.Value);
            break;

          case "SpreadMethod":
            brush.SpreadMethod = ParseEnum<SpreadMethod>(this.reader.Value);
            break;

          case "MappingMode ":
            brush.MappingMode = ParseEnum<MappingMode>(this.reader.Value);
            break;

          case "Center":
            brush.Center = Point.Parse(this.reader.Value);
            break;

          case "GradientOrigin":
            brush.GradientOrigin = Point.Parse(this.reader.Value);
            break;

          case "RadiusX":
            brush.RadiusX =ParseDouble(this.reader.Value);
            break;

          case "RadiusY":
            brush.RadiusY = ParseDouble(this.reader.Value);
            break;

          case "x:Key":
            brush.Key = this.reader.Value;
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
            case "RadialGradientBrush.Transform":
              MoveToNextElement();
              brush.Transform = ParseMatrixTransform();
              MoveToNextElement();
              break;

            case "RadialGradientBrush.GradientStops":
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