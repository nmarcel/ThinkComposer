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
    /// Parses a GradientStop element.
    /// </summary>
    GradientStop ParseGradientStop()
    {
      GradientStop gs = new GradientStop();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Color":
            gs.Color = Color.Parse(this.reader.Value);
            break;

          case "Offset":
            gs.Offset = ParseDouble(this.reader.Value);
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveBeyondThisElement();
      return gs;
    }
  }
}