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
  class XPImageBrush : XPTilingBrush
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="XPObjectBase"/> class.
    /// </summary>
    XPImageBrush(ImageBrush brush)
      : base(brush)
    {
    }

    public static XPImageBrush Create(DocumentRenderingContext context, ImageBrush brush)
    {
      XPImageBrush xpImageBrush = new XPImageBrush(brush);
      xpImageBrush.Construct(context, brush);
      return xpImageBrush;
    }

    void Construct(DocumentRenderingContext context, ImageBrush brush)
    {
      FixedPage fpage = brush.GetParent<FixedPage>();
      if (fpage == null)
        Debug.Assert(false);

      FixedPayload payload = fpage.Document.Payload;  // TODO: find better way to get FixedPayload
      //Debug.Assert(Object.Equals(payload, Context.


      this.form = XFormBuilder.FromImageBrush(context, brush);

      // Get the font object.
      // Important: font.PdfFont is not yet defined here on the first call
      //string uriString = brush.ImageSource;
      //BitmapSource bitmapSource = payload.GetImage(uriString);

      //XPImage xpImage = new XPImage(bitmapSource);
    }

    XForm Form
    {
      get { return this.form; }
    }
    XForm form;

    PdfFormXObject FormXObject
    {
      get { return this.formXObject; }
    }
    PdfFormXObject formXObject = null;

    PdfTilingPattern Pattern
    {
      get { return this.pattern; }
    }
    PdfTilingPattern pattern = null;
  }
}
