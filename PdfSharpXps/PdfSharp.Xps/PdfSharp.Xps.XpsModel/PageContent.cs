using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Defines a reference from a fixed document to a part that contains a <FixedPage> element.
  /// </summary>
  class PageContent : XpsElement
  {
    /// <summary>
    /// Specifies a URI that refers to the page content, held in a distinct part within the package.
    /// The content identified MUST be a FixedPage part within the XPS Document [M3.5].
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Typical width of pages contained in the page content.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Typical height of pages contained in the page content.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Contains a collection of <LinkTarget> elements, each of which is addressable via hyperlink.
    /// </summary>
    public int LinkTargets { get; set; }
  }
}