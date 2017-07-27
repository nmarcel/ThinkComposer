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
    /// Parses a DocumentReference element.
    /// </summary>
    DocumentReference ParseDocumentReference()
    {
      Debug.Assert(this.reader.Name == "DocumentReference");
      bool isEmptyElement = this.reader.IsEmptyElement;
      DocumentReference documentReference = new DocumentReference();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "Source":
            documentReference.Source = this.reader.Value;
            break;

          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return documentReference;
    }
  }
}