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
    /// Parses an ImageBrush element.
    /// </summary>
    ImageBrush ParseImageBrush()
    {
      Debug.Assert(this.reader.Name == "ImageBrush");
      bool isEmptyElement = this.reader.IsEmptyElement;
      ImageBrush brush = new ImageBrush();
      brush.Opacity = 1;
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          //case "Name":
          //  //brush.Name = this.rdr.Value;
          //  break;

          case "Opacity":
            brush.Opacity = ParseDouble(this.reader.Value);
            break;

          case "Transform":
            brush.Transform = ParseMatrixTransform(this.reader.Value);
            break;

          case "ViewboxUnits":
            brush.ViewboxUnits = ParseEnum<ViewUnits>(this.reader.Value);
            break;

          case "ViewportUnits":
            brush.ViewportUnits = ParseEnum<ViewUnits>(this.reader.Value);
            break;

          case "TileMode":
            brush.TileMode = ParseEnum<TileMode>(this.reader.Value);
            break;

          case "Viewbox":
            brush.Viewbox = Rect.Parse(this.reader.Value);
            break;

          case "Viewport":
            brush.Viewport = Rect.Parse(this.reader.Value);
            break;

          case "ImageSource":
            brush.ImageSource = this.reader.Value;
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
            case "ImageBrush.Transform":
              MoveToNextElement();
              brush.Transform = ParseMatrixTransform();
              MoveToNextElement();
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