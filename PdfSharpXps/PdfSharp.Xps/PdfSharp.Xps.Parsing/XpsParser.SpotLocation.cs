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
    /// Parses a SpotLocation element.
    /// </summary>
    SpotLocation ParseSpotLocation()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      SpotLocation spotLocation = new SpotLocation();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "StartX":
            spotLocation.StartX = double.Parse(this.reader.Value);
            break;

          case "StartY":
            spotLocation.StartY = double.Parse(this.reader.Value);
            break;

          case "PageURI":
            spotLocation.PageURI = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return spotLocation;
    }
  }
}