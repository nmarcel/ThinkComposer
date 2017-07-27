using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Represents an index to a specific location in the document.
  /// </summary>
  class OutlineEntry : XpsElement
  {
    /// <summary>
    /// A description of the level where the outline entry exists in the hierarchy.
    /// A value of 1 is the root.
    /// </summary>
    public int OutlineLevel { get; set; }

    /// <summary>
    /// The URI to which the outline entry is linked.
    /// This may be a URI to a named element within the document or an external URI,
    /// such as a website. It can be used as a hyperlink destination.
    /// </summary>
    public string OutlineTarget { get; set; }

    /// <summary>
    /// The friendly text associated with this outline entry.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// This attribute specifies the default language used for any child element contained
    /// within the current element or nested child elements. The language is specified
    /// according to IETF RFC 3066.
    /// </summary>
    public string lang { get; set; }
  }
}