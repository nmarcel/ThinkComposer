using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// All document structure is related to the fixed page markup using this element.
  /// The <NamedElement> points to a single markup element contained in the fixed page markup.
  /// </summary>
  class NamedElement : XpsElement
  {
    /// <summary>
    /// Identifies the named element in the FixedPage part markup that is referenced
    /// by the document structure markup.
    /// </summary>
    public string NameReference { get; set; }
  }
}