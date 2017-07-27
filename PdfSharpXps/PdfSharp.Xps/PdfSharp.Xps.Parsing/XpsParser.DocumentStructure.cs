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
    /// Parses a DocumentStructure element.
    /// </summary>
    DocumentStructure ParseDocumentStructure()
    {
      Debug.Assert(this.reader.Name == "DocumentStructure");
      bool isEmptyElement = this.reader.IsEmptyElement;
      DocumentStructure documentStructure = new DocumentStructure();
      if (!isEmptyElement)
      {
        MoveToNextElement();
        while (this.reader.IsStartElement())
        {
          switch (this.reader.Name)
          {
            case "Outline":
              documentStructure.Outline = ParseDocumentOutline();
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return documentStructure;
    }
  }
}