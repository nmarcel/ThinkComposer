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
    /// Parses a VisualBrush element.
    /// </summary>
    VisualBrush ParseVisualBrush()
    {
      bool isEmptyElement = this.reader.IsEmptyElement;
      VisualBrush brush = new VisualBrush();
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

          case "Viewbox":
            brush.Viewbox = Rect.Parse(this.reader.Value);
            break;

          case "Viewport":
            brush.Viewport = Rect.Parse(this.reader.Value);
            break;

          case "TileMode":
            brush.TileMode = ParseEnum<TileMode>(this.reader.Value);
            break;

          case "ViewboxUnits":
            brush.ViewboxUnits = ParseEnum<ViewUnits>(this.reader.Value);
            break;

          case "ViewportUnits":
            brush.ViewportUnits = ParseEnum<ViewUnits>(this.reader.Value);
            break;

          case "Visual":
            brush.Visual = ParseVisual();
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
            case "VisualBrush.Transform":
              MoveToNextElement();
              brush.Transform = ParseMatrixTransform();
              break;

            case "VisualBrush.GradientStops":
              MoveToNextElement();
              //brush.GradientStops= ParseGradientStops();
              break;

            case "VisualBrush.Visual":
              //MoveToNextElement();
              brush.Visual = ParseVisual();
              brush.Visual.Parent = brush;
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