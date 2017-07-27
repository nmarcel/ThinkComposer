using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Specifies a sequence of fixed documents.
  /// </summary>
  class FixedDocumentSequence : XpsElement
  {
    /// <summary>
    /// A collection of document references.
    /// </summary>
    public XpsElementCollection<DocumentReference> DocumentReferences = new XpsElementCollection<DocumentReference>();
  }
}