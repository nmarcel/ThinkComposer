using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Identifies the StoryFragments part where this individual story fragment is defined.
  /// </summary>
  class StoryFragmentReference : XpsElement
  {
    /// <summary>
    /// Used to distingush between multiple story fragments from the same story on a single page.
    /// </summary>
    public string FragmentName { get; set; }

    /// <summary>
    /// Identifies the page number of the document that the story fragment is related to.
    /// Page numbers start at 1 and correspond to the order of <PageContent> elements
    /// in the FixedDocument part.
    /// </summary>
    public int Page { get; set; }
  }
}