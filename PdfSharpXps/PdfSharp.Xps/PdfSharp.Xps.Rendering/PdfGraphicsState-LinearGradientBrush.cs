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

#pragma warning disable 414, 169 // incomplete code state

namespace PdfSharp.Xps.Rendering
{
  partial class PdfGraphicsState
  {
    [Obsolete]
    void RealizeLinearGradientBrush(LinearGradientBrush brush, XForm xform)
    {
      XMatrix matrix = this.currentTransform;
      PdfShadingPattern pattern = new PdfShadingPattern(this.writer.Owner);
      pattern.Elements[PdfShadingPattern.Keys.PatternType] = new PdfInteger(2); // shading pattern

      // Setup shading
      PdfShading shading = new PdfShading(this.writer.Owner);
      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;

      PdfDictionary function = BuildShadingFunction(brush.GradientStops, colorMode);
      function.Elements.SetString("/@", "This is the shading function of a LinearGradientBrush");
      shading.Elements[PdfShading.Keys.Function] = function;

      shading.Elements[PdfShading.Keys.ShadingType] = new PdfInteger(2); // Axial shading
      //if (colorMode != PdfColorMode.Cmyk)
      shading.Elements[PdfShading.Keys.ColorSpace] = new PdfName("/DeviceRGB");
      //else
      //shading.Elements[Keys.ColorSpace] = new PdfName("/DeviceCMYK");

      //double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
      double x1 = brush.StartPoint.X;
      double y1 = brush.StartPoint.Y;
      double x2 = brush.EndPoint.X;
      double y2 = brush.EndPoint.Y;

      shading.Elements[PdfShading.Keys.Coords] = new PdfLiteral("[{0:0.###} {1:0.###} {2:0.###} {3:0.###}]", x1, y1, x2, y2);

      // old: Elements[Keys.Background] = new PdfRawItem("[0 1 1]");
      // old: Elements[Keys.Domain] = 
      shading.Elements[PdfShading.Keys.Extend] = new PdfLiteral("[true true]");

      // Setup pattern
      pattern.Elements[PdfShadingPattern.Keys.Shading] = shading;
      pattern.Elements[PdfShadingPattern.Keys.Matrix] = PdfLiteral.FromMatrix(matrix); // new PdfLiteral("[" + PdfEncoders.ToString(matrix) + "]");

      string name = this.writer.Resources.AddPattern(pattern);
      this.writer.WriteLiteral("/Pattern cs\n", name);
      this.writer.WriteLiteral("{0} scn\n", name);

      double alpha = brush.Opacity * brush.GradientStops.GetAverageAlpha();
      if (alpha < 1 && this.writer.renderMode == RenderMode.Default)
      {
#if true
        PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(alpha);
        string gs = this.writer.Resources.AddExtGState(extGState);
        this.writer.WriteLiteral("{0} gs\n", gs);
#else
#if true
        if (xform == null)
        {
          PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(alpha);
          string gs = this.writer.Resources.AddExtGState(extGState);
          this.writer.WriteGraphicState(extGState);
        }
        else
        {
          //PdfFormXObject pdfForm = this.writer.Owner.FormTable.GetForm(form);
          PdfFormXObject pdfForm = xform.pdfForm;
          pdfForm.Elements.SetString("/@", "This is the Form XObject of the soft mask");

          string formName = this.writer.Resources.AddForm(pdfForm);

          PdfTransparencyGroupAttributes tgAttributes = new PdfTransparencyGroupAttributes(this.writer.Owner);
          //this.writer.Owner.Internals.AddObject(tgAttributes);
          tgAttributes.Elements.SetName(PdfTransparencyGroupAttributes.Keys.CS, "/DeviceRGB");

          // Set reference to transparency group attributes
          pdfForm.Elements.SetObject(PdfFormXObject.Keys.Group, tgAttributes);
          pdfForm.Elements[PdfFormXObject.Keys.Matrix] = new PdfLiteral("[1.001 0 0 1.001 0.001 0.001]");


          PdfSoftMask softmask = new PdfSoftMask(this.writer.Owner);
          this.writer.Owner.Internals.AddObject(softmask);
          softmask.Elements.SetString("/@", "This is the soft mask");
          softmask.Elements.SetName(PdfSoftMask.Keys.S, "/Luminosity");
          softmask.Elements.SetReference(PdfSoftMask.Keys.G, pdfForm);
          //pdfForm.Elements.SetName(PdfFormXObject.Keys.Type, "Group");
          //pdfForm.Elements.SetName(PdfFormXObject.Keys.ss.Ss.Type, "Group");

          PdfExtGState extGState = new PdfExtGState(this.writer.Owner);
          this.writer.Owner.Internals.AddObject(extGState);
          extGState.Elements.SetReference(PdfExtGState.Keys.SMask, softmask);
          this.writer.WriteGraphicState(extGState);
        }

#else
        XForm form = new XForm(this.writer.Owner, 220, 140);
        XGraphics formGfx = XGraphics.FromForm(form);

        // draw something
        //XSolidBrush xbrush = new XSolidBrush(XColor.FromArgb(128, 128, 128));
        XLinearGradientBrush xbrush = new XLinearGradientBrush(new XPoint(0, 0), new XPoint(220,0), XColors.White, XColors.Black);
        formGfx.DrawRectangle(xbrush, -10000, -10000, 20000, 20000);
        //formGfx.DrawString("Text, Graphics, Images, and Forms", new XFont("Verdana", 10, XFontStyle.Regular), XBrushes.Navy, 3, 0, XStringFormat.TopLeft);
        formGfx.Dispose();

        // Close form
        form.Finish();
        PdfFormXObject pdfForm = this.writer.Owner.FormTable.GetForm(form);
        string formName = this.writer.Resources.AddForm(pdfForm);

        //double x = 20, y = 20;
        //double cx = 1;
        //double cy = 1;

        //this.writer.AppendFormat("q {2:0.###} 0 0 -{3:0.###} {0:0.###} {4:0.###} cm 100 Tz {5} Do Q\n",
        //  x, y, cx, cy, y + 0, formName);

        //this.writer.AppendFormat("q {2:0.###} 0 0 -{3:0.###} {0:0.###} {4:0.###} cm 100 Tz {5} Do Q\n",
        //  x, y, cx, cy, y + 220/1.5, formName);

        PdfTransparencyGroupAttributes tgAttributes = new PdfTransparencyGroupAttributes(this.writer.Owner);
        this.writer.Owner.Internals.AddObject(tgAttributes);

        tgAttributes.Elements.SetName(PdfTransparencyGroupAttributes.Keys.CS, "/DeviceRGB");


        // Set reference to transparency group attributes
        pdfForm.Elements.SetReference(PdfFormXObject.Keys.Group, tgAttributes);


        PdfSoftMask softmask = new PdfSoftMask(this.writer.Owner);
        this.writer.Owner.Internals.AddObject(softmask);
        softmask.Elements.SetName(PdfSoftMask.Keys.S, "/Luminosity");
        softmask.Elements.SetReference(PdfSoftMask.Keys.G, pdfForm);
        //pdfForm.Elements.SetName(PdfFormXObject.Keys.Type, "Group");
        //pdfForm.Elements.SetName(PdfFormXObject.Keys.ss.Ss.Type, "Group");

        PdfExtGState extGState = new PdfExtGState(this.writer.Owner);
        this.writer.Owner.Internals.AddObject(extGState);
        extGState.Elements.SetReference(PdfExtGState.Keys.SMask, softmask);
        this.writer.WriteGraphicState(extGState);
#endif
#endif
      }
    }

    [Obsolete]
    void RealizeRadialGradientBrush(RadialGradientBrush brush, XForm xform)
    {
      XMatrix matrix = new XMatrix(); // this.renderer.defaultViewMatrix;
      //matrix.MultiplyPrepend(this.Transform);
      matrix = this.currentTransform;
      //matrix.MultiplyPrepend(XMatrix.CreateScaling(1.3, 1));
      PdfShadingPattern pattern = new PdfShadingPattern(this.writer.Owner);
      pattern.Elements[PdfShadingPattern.Keys.PatternType] = new PdfInteger(2); // shading pattern

      // Setup shading
      PdfShading shading = new PdfShading(this.writer.Owner);

      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;

      PdfDictionary function = BuildShadingFunction(brush.GradientStops, colorMode);

      shading.Elements[PdfShading.Keys.ShadingType] = new PdfInteger(3); // Radial shading
      //if (colorMode != PdfColorMode.Cmyk)
      shading.Elements[PdfShading.Keys.ColorSpace] = new PdfName("/DeviceRGB");
      //else
      //shading.Elements[Keys.ColorSpace] = new PdfName("/DeviceCMYK");

      double x0 = brush.GradientOrigin.X;
      double y0 = brush.GradientOrigin.Y;
      double r0 = 0;
      double x1 = brush.Center.X;
      double y1 = brush.Center.Y;
      double r1 = brush.RadiusX;

      shading.Elements[PdfShading.Keys.Coords] =
        new PdfLiteral("[{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###}]", x0, y0, r0, x1, y1, r1);

      // old: Elements[Keys.Background] = new PdfRawItem("[0 1 1]");
      // old: Elements[Keys.Domain] = 
      shading.Elements[PdfShading.Keys.Function] = function;
      shading.Elements[PdfShading.Keys.Extend] = new PdfLiteral("[true true]");

      // Setup pattern
      pattern.Elements[PdfShadingPattern.Keys.Shading] = shading;
      pattern.Elements[PdfShadingPattern.Keys.Matrix] = PdfLiteral.FromMatrix(matrix); // new PdfLiteral("[" + PdfEncoders.ToString(matrix) + "]");

      string name = this.writer.Resources.AddPattern(pattern);
      this.writer.WriteLiteral("/Pattern cs\n", name);
      this.writer.WriteLiteral("{0} scn\n", name);

      double alpha = brush.Opacity * brush.GradientStops.GetAverageAlpha();
      if (alpha < 1)
      {
#if true
        PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(alpha);
        string gs = this.writer.Resources.AddExtGState(extGState);
        this.writer.WriteLiteral("{0} gs\n", gs);
#else
        if (xform == null)
        {
          PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(alpha);
          string gs = this.writer.Resources.AddExtGState(extGState);
          this.writer.WriteGraphicState(extGState);
        }
        else
        {
          //PdfFormXObject pdfForm = this.writer.Owner.FormTable.GetForm(form);
          PdfFormXObject pdfForm = xform.pdfForm;
          pdfForm.Elements.SetString("/@", "This is the Form XObject of the soft mask");

          string formName = this.writer.Resources.AddForm(pdfForm);

          PdfTransparencyGroupAttributes tgAttributes = new PdfTransparencyGroupAttributes(this.writer.Owner);
          //this.writer.Owner.Internals.AddObject(tgAttributes);
          tgAttributes.Elements.SetName(PdfTransparencyGroupAttributes.Keys.CS, "/DeviceRGB");

          // Set reference to transparency group attributes
          pdfForm.Elements.SetObject(PdfFormXObject.Keys.Group, tgAttributes);
          pdfForm.Elements[PdfFormXObject.Keys.Matrix] = new PdfLiteral("[1.001 0 0 1.001 0.001 0.001]");


          PdfSoftMask softmask = new PdfSoftMask(this.writer.Owner);
          this.writer.Owner.Internals.AddObject(softmask);
          softmask.Elements.SetString("/@", "This is the soft mask");
          softmask.Elements.SetName(PdfSoftMask.Keys.S, "/Luminosity");
          softmask.Elements.SetReference(PdfSoftMask.Keys.G, pdfForm);
          //pdfForm.Elements.SetName(PdfFormXObject.Keys.Type, "Group");
          //pdfForm.Elements.SetName(PdfFormXObject.Keys.ss.Ss.Type, "Group");

          PdfExtGState extGState = new PdfExtGState(this.writer.Owner);
          this.writer.Owner.Internals.AddObject(extGState);
          extGState.Elements.SetReference(PdfExtGState.Keys.SMask, softmask);
          this.writer.WriteGraphicState(extGState);
        }
#endif
      }
    }

    //void RealizeImageBrush(ImageBrush brush, XForm xform)
    //{
    //}

    //void RealizeVisualBrush(VisualBrush brush, XForm xform)
    //{
    //}

    /// <summary>
    /// Builds the shading function of the specified gradient stop collection.
    /// </summary>
    PdfDictionary BuildShadingFunction(GradientStopCollection gradients, PdfColorMode colorMode)
    {
      bool softMask = this.writer.renderMode == RenderMode.SoftMask;
      PdfDictionary func = new PdfDictionary();
      int count = gradients.Count;
      Debug.Assert(count >= 2);
      if (count == 2)
      {
        // Build one Type 2 function
        func.Elements["/FunctionType"] = new PdfInteger(2); // Type 2 - Exponential Interpolation Function
        Color clr0 = gradients[0].Color;
        Color clr1 = gradients[1].Color;
        if (softMask)
        {
          clr0 = Utils.AlphaToGray(clr0);
          clr1 = Utils.AlphaToGray(clr1);
        }
        func.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0, colorMode) + "]");
        func.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1, colorMode) + "]");
        func.Elements["/Domain"] = new PdfLiteral("[0 1]");
        func.Elements["/N"] = new PdfInteger(1); // be linear
      }
      else
      {
        // Build a Type 3 function with an array of n-1 Type 2 functions
        func.Elements["/FunctionType"] = new PdfInteger(3); // Type 3 - Stitching Function
        func.Elements["/Domain"] = new PdfLiteral("[0 1]");
        PdfArray fnarray = new PdfArray();
        func.Elements["/Functions"] = fnarray;

        StringBuilder bounds = new StringBuilder("[");
        StringBuilder encode = new StringBuilder("[");

        for (int idx = 1; idx < count; idx++)
        {
          PdfDictionary fn2 = new PdfDictionary();
          fn2.Elements["/FunctionType"] = new PdfInteger(2);
          Color clr0 = gradients[idx - 1].Color;
          Color clr1 = gradients[idx].Color;
          if (softMask)
          {
            clr0 = Utils.AlphaToGray(clr0);
            clr1 = Utils.AlphaToGray(clr1);
          }
          fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0, colorMode) + "]");
          fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1, colorMode) + "]");
          fn2.Elements["/Domain"] = new PdfLiteral("[0 1]");
          fn2.Elements["/N"] = new PdfInteger(1);
          //this.renderer.Owner.Internals.AddObject(fn2);
          //fnarray.Elements.Add(fn2.Reference);
          fnarray.Elements.Add(fn2);
          if (idx > 1)
          {
            bounds.Append(' ');
            encode.Append(' ');
          }
          if (idx < count - 1)
            bounds.Append(PdfEncoders.ToString(gradients[idx].Offset));
          encode.Append("0 1");
        }
        bounds.Append(']');
        //encode.Append(" 0 1");
        encode.Append(']');
        func.Elements["/Bounds"] = new PdfLiteral(bounds.ToString());
        func.Elements["/Encode"] = new PdfLiteral(encode.ToString());
      }
      return func;
    }
  }
}