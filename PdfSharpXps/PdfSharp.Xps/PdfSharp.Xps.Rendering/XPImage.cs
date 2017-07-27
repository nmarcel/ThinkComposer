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

using BitmapSource = System.Windows.Media.Imaging.BitmapSource;

namespace PdfSharp.Xps.Rendering
{
  class XPImage : XPObject
  {
    public XPImage(BitmapSource bitmapSource)
    {
      this.xImage = XImage.FromBitmapSource(bitmapSource);
    }

    public XImage XImage
    {
      get { return this.xImage; }
    }
    XImage xImage;
  }
}
