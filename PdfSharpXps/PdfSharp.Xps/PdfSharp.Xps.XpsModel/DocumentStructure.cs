using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// The root element of the DocumentStructure part.
  /// </summary>
  class DocumentStructure : XpsElement
  {
    /// <summary>
    /// Contains a structured document outline that provides a list of links into the document
    /// contents or external sites.
    /// </summary>
    public DocumentOutline Outline { get; set; }
  }
}