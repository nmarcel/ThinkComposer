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
    /// Parses a Visual element.
    /// </summary>
    Visual ParseVisual()
    {
      bool isEmptyElement = this.reader.IsEmptyElement;
      Visual visual = new Visual();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
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
          XpsElement element = null;
          switch (this.reader.Name)
          {
            case "Canvas":
              element = ParseCanvas();
              visual.Content.Add(element);
              element.Parent = visual;
              break;

            case "Path":
              element = ParsePath();
              visual.Content.Add(element);
              element.Parent = visual;
              break;

            case "Glyphs":
              element = ParseGlyphs();
              visual.Content.Add(element);
              element.Parent = visual;
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return visual;
    }
  }
}