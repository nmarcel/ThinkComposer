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
    /// Parses a NamedElement element.
    /// </summary>
    NamedElement ParseNamedElement()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      NamedElement namedElement = new NamedElement();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "NameReference":
            namedElement.NameReference = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return namedElement;
    }
  }
}