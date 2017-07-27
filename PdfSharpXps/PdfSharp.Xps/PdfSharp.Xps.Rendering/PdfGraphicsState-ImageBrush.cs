using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Xps.XpsModel;
using System.Windows.Media.Imaging;

#pragma warning disable 414, 169 // incomplete code state

namespace PdfSharp.Xps.Rendering
{
  partial class PdfGraphicsState
  {
    [Obsolete]
    void RealizeImageBrush(ImageBrush brush, ref XForm xform, ref XImage xImage)
    {
      //Debug.Assert(xform != null);

      //xform = new XForm(this.writer.Owner, new XRect(brush.Viewbox.X * 3 / 4, brush.Viewbox.Y * 3 / 4, brush.Viewbox.Width * 3 / 4, brush.Viewbox.Height * 3 / 4));

      //string imageUri = brush.ImageSource;


      FixedPage fpage = brush.GetParent<FixedPage>();
      if (fpage == null)
        Debug.Assert(false);

      FixedPayload payload = fpage.Document.Payload;

      // Get the font object.
      // Important: font.PdfFont is not yet defined here on the first call
      string uriString = brush.ImageSource;
      BitmapSource image = payload.GetImage(uriString);

      //      int factor = 1;
      //int width = (int)(100 * factor);
      //int height = (int)(100 * factor);
      //DrawingVisual visual = new DrawingVisual();
      //DrawingContext vdc = visual.RenderOpen();
      //vdc.DrawRectangle(Brushes.GreenYellow, null, new Rect(0, 0, width, height));
      //vdc.DrawLine(new Pen(Brushes.Red, 3), new Point(0, 0), new Point(100, 100));
      //vdc.DrawLine(new Pen(Brushes.Red, 3), new Point(100, 0), new Point(0, 100));
      //vdc.Close();
      //RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 72 * factor, 72 * factor, PixelFormats.Default);
      //rtb.Render(visual);

      //BitmapImage bitmapImage = new BitmapImage(

      xImage = XImage.FromBitmapSource(image);



      //XGraphics gfx = XGraphics.FromForm(xform);
      //gfx.DrawRectangle(XBrushes.Firebrick, new XRect(0, 0, 100, 100));


      //PdfContentWriter formWriter = new PdfContentWriter(xform, RenderMode.Default);

      ////formWriter.Size = brush.Viewport.Size;
      //formWriter.BeginContent(false);

      ////formWriter.WriteElement(visual);
      //formWriter.EndContent();
    }
  }
}