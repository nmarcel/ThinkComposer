using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Pseudo element that represents an XML comment in an XPS file.
  /// </summary>
  class Comment : XpsElement
  {
    /// <summary>
    /// Gets or sets the comment text.
    /// </summary>
    public string Text { get; set; }
  }
}