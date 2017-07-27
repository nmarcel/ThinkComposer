using System;
using System.Collections.Generic;
using System.Text;

namespace PdfSharp.Xps.XpsModel
{
  /// <summary>
  /// Identifies a resource that can be safely discarded by a resource-constrained consumer.
  /// </summary>
  class Discard : XpsElement
  {
    /// <summary>
    /// The first fixed page that no longer needs the identified resource in order to be processed.
    /// </summary>
    public string SentinelPage { get; set; }

    /// <summary>
    /// The resource that can be safely discarded.
    /// </summary>
    public string Target { get; set; }
  }
}