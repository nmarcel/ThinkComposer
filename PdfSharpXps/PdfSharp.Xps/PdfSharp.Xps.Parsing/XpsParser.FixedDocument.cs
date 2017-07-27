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
    /// Parses a FixedDocument element.
    /// </summary>
    FixedDocument ParseFixedDocument()
    {
      Debug.Assert(this.reader.Name == "FixedDocument");
      bool isEmptyElement = this.reader.IsEmptyElement;
      FixedDocument fdoc = new FixedDocument();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          default:
            //UnexpectedAttribute();
            break;
        }
      }
      if (!isEmptyElement)
      {
        MoveToNextElement();
        while (this.reader.IsStartElement())
        {
          switch (this.reader.Name)
          {
            case "PageContent":
              {
                isEmptyElement = this.reader.IsEmptyElement;
                while (MoveToNextAttribute())
                {
                  switch (this.reader.Name)
                  {
                    case "Source":
                      fdoc.PageContentUriStrings.Add(this.reader.Value);
                      break;

                    case "Width":
                      // TODO: preview width
                      break;

                    case "Height":
                      // TODO: preview height
                      break;

                    default:
                      UnexpectedAttribute(this.reader.Name);
                      break;
                  }
                }
                if (!isEmptyElement)
                {
                  MoveToNextElement();
                  // Move beyond PageContent.LinkTargets
                  if (this.reader.IsStartElement())
                    MoveBeyondThisElement();
                }
              }
              MoveToNextElement();
              break;

            case "LinkTarget":
              Debug.Assert(false);
              MoveBeyondThisElement();
              break;

            default:
              Debugger.Break();
              break;
          }
        }
      }
      MoveToNextElement();
      return fdoc;
    }
  }
}