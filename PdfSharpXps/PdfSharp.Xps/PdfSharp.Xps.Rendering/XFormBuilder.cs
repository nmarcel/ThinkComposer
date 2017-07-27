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
  /// TODO: Rename to XFormBuilder
  /// </summary>
  class XFormBuilder : BuilderBase
  {
    XFormBuilder(DocumentRenderingContext context)
      : base(context)
    {
    }

    /// <summary>
    /// Creates an XForm from an image brush.
    /// </summary>
    public static XForm FromImageBrush(DocumentRenderingContext context, ImageBrush brush)
    {
      XPImage xpImage = ImageBuilder.FromImageBrush(context, brush);
      XImage ximage = xpImage.XImage;
      double width = ximage.PixelWidth;
      double height = ximage.PixelHeight;

      // view box in point
      // XRect box = new XRect(brush.Viewbox.X * 0.75, brush.Viewbox.Y * 0.75, brush.Viewbox.Width * 0.75, brush.Viewbox.Height * 0.75);
      XRect box = new XRect(0, 0, width, height);
      XForm xform = new XForm(context.PdfDocument, box);

      PdfContentWriter formWriter = new PdfContentWriter(context, xform, RenderMode.Default);

      Debug.Assert(ximage != null);

      PdfFormXObject pdfForm = xform.PdfForm;
      pdfForm.Elements.SetMatrix(PdfFormXObject.Keys.Matrix, new XMatrix());

      //formWriter.Size = brush.Viewport.Size;
      formWriter.BeginContentRaw();

      string imageID = formWriter.Resources.AddImage(new PdfImage(context.PdfDocument, ximage));
      XMatrix matrix = new XMatrix();
      double scaleX = brush.Viewport.Width / brush.Viewbox.Width * 4 / 3 * ximage.PointWidth;
      double scaleY = brush.Viewport.Height / brush.Viewbox.Height * 4 / 3 * ximage.PointHeight;
      matrix.TranslatePrepend(-brush.Viewbox.X, -brush.Viewbox.Y);
      matrix.ScalePrepend(scaleX, scaleY);
      matrix.TranslatePrepend(brush.Viewport.X / scaleX, brush.Viewport.Y / scaleY);

      matrix = new XMatrix(width, 0, 0, -height, 0, height);
      formWriter.WriteLiteral("q\n");
      // TODO:WriteClip(path.Data);
      //formWriter.WriteLiteral("{0:0.###} 0 0 -{1:0.###} {2:0.###} {3:0.###} cm 100 Tz {4} Do Q\n",
      //  matrix.M11, matrix.M22, matrix.OffsetX, matrix.OffsetY + brush.Viewport.Height, imageID);
      formWriter.WriteMatrix(matrix);
      formWriter.WriteLiteral(imageID + " Do Q\n");

      formWriter.EndContent();

      return xform;
    }

    /// <summary>
    /// Creates an XForm from a visual brush.
    /// </summary>
    public static XForm FromVisualBrush(DocumentRenderingContext context, VisualBrush brush)
    {
      //XPImage xpImage = ImageBuilder.FromImageBrush(context, brush);
      //XImage ximage = xpImage.XImage;
      double width = brush.Viewport.Width;
      double height = brush.Viewport.Height;

      // view box in point
      // XRect box = new XRect(brush.Viewbox.X * 0.75, brush.Viewbox.Y * 0.75, brush.Viewbox.Width * 0.75, brush.Viewbox.Height * 0.75);
      XRect box = new XRect(0, 0, width, height);
      XForm xform = new XForm(context.PdfDocument, box);

      PdfContentWriter formWriter = new PdfContentWriter(context, xform, RenderMode.Default);
      PdfFormXObject pdfForm = xform.PdfForm;
      pdfForm.Elements.SetMatrix(PdfFormXObject.Keys.Matrix, new XMatrix());

      //formWriter.Size = brush.Viewport.Size;
      formWriter.BeginContent(false);
      formWriter.WriteElement(brush.Visual);
      formWriter.EndContent();

      return xform;
    }
  }
}
