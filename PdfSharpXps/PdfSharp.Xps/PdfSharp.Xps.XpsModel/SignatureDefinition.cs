using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// A single signature definition.
  /// </summary>
  class SignatureDefinition : XpsElement
  {
    /// <summary>
    /// A globally unique identifier for this signature spot.
    /// </summary>
    public string SpotID { get; set; }

    /// <summary>
    /// A string representing the identity of the individual who is requested to sign the XPS Document,
    /// or the name of the individual who has signed the XPS Document.
    /// </summary>
    public string SignerName { get; set; }

    /// <summary>
    /// Specifies the language used for the current element and its descendants.
    /// The language is specified according to RFC 3066.
    /// </summary>
    public string lang { get; set; }
  }
}