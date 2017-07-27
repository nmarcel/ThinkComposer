using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Specifies a list of meaningful indices into the XPS Document, similar to a table of contents,
  /// or to external URIs, such as web addresses.
  /// </summary>
  class DocumentOutline : XpsElement
  {
    /// <summary>
    /// This attribute specifies the default language used for any child element contained within
    /// the current element or nested child elements. The language is specified according
    /// to IETF RFC 3066.
    /// </summary>
    //xml:lang
    public string lang { get; set; }
  }
}