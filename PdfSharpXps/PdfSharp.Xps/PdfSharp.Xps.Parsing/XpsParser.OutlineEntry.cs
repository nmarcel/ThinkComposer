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
    /// Parses a OutlineEntry element.
    /// </summary>
    OutlineEntry ParseOutlineEntry()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      OutlineEntry outlineEntry = new OutlineEntry();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "OutlineLevel":
            outlineEntry.OutlineLevel = int.Parse(this.reader.Value);
            break;

          case "OutlineTarget":
            outlineEntry.OutlineTarget = this.reader.Value;
            break;

          case "Description":
            outlineEntry.Description = this.reader.Value;
            break;

          case "xml:lang":
            outlineEntry.lang = this.reader.Value;
            break;
          
          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return outlineEntry;
    }
  }
}