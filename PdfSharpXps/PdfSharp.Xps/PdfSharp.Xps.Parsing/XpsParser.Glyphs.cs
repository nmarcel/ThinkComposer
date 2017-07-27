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
    /// Parses a Glyphs element.
    /// </summary>
    Glyphs ParseGlyphs()
    {
      Debug.Assert(this.reader.Name == "Glyphs");
      Glyphs glyphs = new Glyphs();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "BidiLevel":
            glyphs.BidiLevel = Int32.Parse(this.reader.Value);
            break;

          case "CaretStops":
            glyphs.CaretStops = this.reader.Value;
            break;

          case "DeviceFontName":
            glyphs.DeviceFontName = this.reader.Value;
            break;

          case "Fill":
            glyphs.Fill = ParseBrush(this.reader.Value); 
            break;

          case "FontRenderingEmSize":
            glyphs.FontRenderingEmSize = ParseDouble(this.reader.Value);
            break;

          case "FontUri":
            glyphs.FontUri = this.reader.Value;
            break;

          case "OriginX":
            glyphs.OriginX = ParseDouble(this.reader.Value);
            break;

          case "OriginY":
            glyphs.OriginY = ParseDouble(this.reader.Value);
            break;

          case "IsSideways":
            glyphs.IsSideways = ParseBool(this.reader.Value);
            break;

          case "Indices":
            glyphs.Indices = new GlyphIndices(this.reader.Value);
            break;

          case "UnicodeString":
            glyphs.UnicodeString = this.reader.Value;
            break;

          case "StyleSimulations":
            glyphs.StyleSimulations = ParseEnum<StyleSimulations>(this.reader.Value);
            break;

          case "RenderTransform":
            glyphs.RenderTransform = ParseMatrixTransform(this.reader.Value);
            break;

          case "Clip":
            glyphs.Clip = ParsePathGeometry(this.reader.Value);
            break;

          case "Opacity":
            glyphs.Opacity = ParseDouble(this.reader.Value);
            break;

          case "Name":
            glyphs.Name = this.reader.Value;
            break;

          case "FixedPage.NavigateUri":
            glyphs.FixedPage_NavigateUri = this.reader.Value;
            break;

          case "xml:lang":
            glyphs.lang = this.reader.Value;
            break;

          case "x:Key":
            glyphs.Key = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      // TODO Glyphs
      MoveBeyondThisElement();
      return glyphs;
    }
  }
}