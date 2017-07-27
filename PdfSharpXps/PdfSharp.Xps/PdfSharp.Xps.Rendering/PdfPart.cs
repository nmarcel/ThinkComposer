using System;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Pdf.Advanced;
using XpsModel = PdfSharp.Xps.XpsModel;

namespace PdfSharp.Xps.Rendering
{
#if true_
  /// <summary>
  /// Contains one or more PDF objects or other PDF elements that represents XPS element.
  /// </summary>
  abstract class PdfPart
  {
    protected PdfPart()
    { }

    protected PdfPart(DocumentRenderingContext drc)
    {
      this.drc = drc;
    }

    protected DocumentRenderingContext drc;
  }

  sealed class ImageBrushPart : PdfPart
  {
    ImageBrushPart(DocumentRenderingContext drc)
      : base(drc)
    {
    }

    public PdfExtGState GraphicsState;
    public PdfTilingPattern Pattern;
    public PdfFormXObject Form;
    public PdfImage Image;
    public PdfImage SoftMask;

  }

  sealed class LinearGradientBrushPart : PdfPart
  {
  }

  sealed class RadialGradientBrushPart : PdfPart
  {
  }

  sealed class VisualBrushPart : PdfPart
  {
  }

  sealed class SolidColorBrushPart : PdfPart
  {

    public PdfExtGState gs;

    public XpsModel.Color color;
  }

  sealed class MatrixTransformPart : PdfPart
  {
  }

  sealed class PathGeometryPart : PdfPart
  {
  }

  sealed class PathPart : PdfPart
  {
  }

  sealed class GlyphPart : PdfPart
  {
  }

  sealed class CanvasPart : PdfPart
  {
  }

#endif
}
