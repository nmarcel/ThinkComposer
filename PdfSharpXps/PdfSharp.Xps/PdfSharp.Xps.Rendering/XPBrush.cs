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
  class XPBrush : XPObject
  {
    protected XPBrush(Brush brush)
    {
      this.brush = brush;
    }

    Brush Brush
    {
      get { return this.brush; }
    }
    Brush brush;
  }
}
