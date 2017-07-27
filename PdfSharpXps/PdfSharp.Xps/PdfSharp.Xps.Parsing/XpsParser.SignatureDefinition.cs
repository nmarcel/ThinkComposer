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
    /// Parses a SignatureDefinition element.
    /// </summary>
    SignatureDefinition ParseSignatureDefinition()
    {
      Debug.Assert(this.reader.Name == "");
      bool isEmptyElement = this.reader.IsEmptyElement;
      SignatureDefinition signatureDefinition = new SignatureDefinition();
      while (MoveToNextAttribute())
      {
        switch (this.reader.Name)
        {
          case "SpotID":
            signatureDefinition.SpotID = this.reader.Value;
            break;

          case "SignerName":
            signatureDefinition.SignerName = this.reader.Value;
            break;

          case "xml:lang":
            signatureDefinition.lang = this.reader.Value;
            break;
          
          default:
            UnexpectedAttribute(this.reader.Name);
            break;
        }
      }
      MoveToNextElement();
      return signatureDefinition;
    }
  }
}