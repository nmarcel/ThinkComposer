using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;

namespace PdfSharp.Xps.Rendering
{
  /// <summary>
  /// Base class for all builder classes.
  /// </summary>
  class BuilderBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BuilderBase"/> class.
    /// </summary>
    protected BuilderBase(DocumentRenderingContext context)
    {
      this.context = context;
    }

    /// <summary>
    /// Gets the document rendering context this builder is associated with.
    /// </summary>
    protected DocumentRenderingContext Context
    {
      get { return this.context; }
    }
    DocumentRenderingContext context;
  }

  /// <summary>
  /// Base class for TilingPatternBuilder and ShadingBuilder.
  /// </summary>
  class PatternOrShadingBuilder : BuilderBase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BuilderBase"/> class.
    /// </summary>
    protected PatternOrShadingBuilder(DocumentRenderingContext context)
      : base(context)
    { }

    protected bool CanOptimizeForTwoColors(GradientStopCollection gradients)
    {
      return gradients.Count == 2 && gradients[0].Offset == 0 && gradients[1].Offset == 1;
    }

  }
}
