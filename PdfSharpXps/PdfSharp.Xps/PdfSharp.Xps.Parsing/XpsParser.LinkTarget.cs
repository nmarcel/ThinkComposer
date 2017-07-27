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
    /// Parses a LinkTarget element.
    /// </summary>
    LinkTarget ParseLinkTarget()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      LinkTarget linkTarget = new LinkTarget();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Name":
            linkTarget.Name = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return linkTarget;
    }
  }
}