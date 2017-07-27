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
    /// Parses a PageContent element.
    /// </summary>
    PageContent ParsePageContent()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      PageContent pageContent = new PageContent();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Width":
            pageContent.Width = int.Parse(this.reader.Value);
            break;

          case "Height":
            pageContent.Height = int.Parse(this.reader.Value);
            break;

          case "LinkTargets":
            pageContent.LinkTargets = int.Parse(this.reader.Value);
            break;

          case "Source":
            pageContent.Source = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return pageContent;
    }
  }
}