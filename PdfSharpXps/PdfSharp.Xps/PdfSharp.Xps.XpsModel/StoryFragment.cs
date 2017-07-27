using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Specifies the document structural markup that appears on the current page for a single story block.
  /// </summary>
  class StoryFragment : XpsElement
  {
    /// <summary>
    /// Identifies the story that this story fragment belongs to.
    /// If omitted, the story fragment is not associated with any story.
    /// </summary>
    public string StoryName { get; set; }

    /// <summary>
    /// Used to uniquely identify the story fragment.
    /// </summary>
    public string FragmentName { get; set; }

    /// <summary>
    /// Specifies the type of content included in the story fragment. Valid values are Content,
    /// Header, and Footer.
    /// </summary>
    public string FragmentType { get; set; }
  }
}