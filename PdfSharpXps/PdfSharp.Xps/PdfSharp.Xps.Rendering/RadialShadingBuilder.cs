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
  /// Constructs a PdfShading from LinearGradientBrush or RadialGradientBrush.
  /// </summary>
  class RadialShadingBuilder : PatternOrShadingBuilder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="RadialShadingBuilder"/> class.
    /// </summary>
    RadialShadingBuilder(DocumentRenderingContext context) :
      base(context)
    { }

    /// <summary>
    /// Builds a shading pattern from an radial gradient brush.
    /// </summary>
    public static PdfShadingPattern BuildFromRadialGradientBrush(DocumentRenderingContext context, RadialGradientBrush brush, XRect boundingBox, XMatrix transform)
    {
      RadialShadingBuilder builder = new RadialShadingBuilder(context);
      PdfShadingPattern pattern = builder.BuildPattern(brush, boundingBox, transform);
      return pattern;
    }

    PdfShadingPattern BuildPattern(RadialGradientBrush brush, XRect boundingBox, XMatrix transform)
    {
      //<<
      //  /BBox [0 0 600 760]
      //  /Length 123
      //  /Matrix [0.75 0 0 -0.75 0 480]
      //  /PaintType 1
      //  /PatternType 1
      //  /Resources
      //  <<
      //    /ColorSpace
      //    <<
      //      /CS0 12 0 R
      //      /CS1 12 0 R
      //    >>
      //    /ExtGState
      //    <<
      //      /GS0 10 0 R
      //      /GS1 16 0 R
      //    >>
      //    /Shading
      //    <<
      //      /Sh0 15 0 R
      //    >>
      //  >>
      //  /TilingType 3
      //  /Type /Pattern
      //  /XStep 600
      //  /YStep 1520
      //>>
      //stream
      //  /CS0 cs 1 0 0  scn
      //  1 i 
      //  /GS0 gs
      //  0 0 600 759.999 re
      //  f
      //  q
      //  0 0 600 760 re
      //  W n
      //  q
      //  0 g
      //  /GS1 gs
      //  1 0 0 0.5 0 0 cm
      //  BX /Sh0 sh EX Q
      //  Q
      //endstream
      XRect bbox = new XRect(-600,-700, 1200, 1520); // HACK
      XMatrix matrix = transform;
      //matrix.Prepend(brush.Transform.Matrix);

      double xStep = 600;
      double yStep = 1520; // HACK

      PdfShadingPattern pattern = Context.PdfDocument.Internals.CreateIndirectObject<PdfShadingPattern>();
      pattern.Elements.SetInteger(PdfTilingPattern.Keys.PatternType, 1);  // Tiling
      pattern.Elements.SetInteger(PdfTilingPattern.Keys.PaintType, 1);  // Color
      pattern.Elements.SetInteger(PdfTilingPattern.Keys.TilingType, 3);  // Constant spacing and faster tiling
      pattern.Elements.SetMatrix(PdfTilingPattern.Keys.Matrix, matrix);
      pattern.Elements.SetRectangle(PdfTilingPattern.Keys.BBox, new PdfRectangle(bbox));
      pattern.Elements.SetReal(PdfTilingPattern.Keys.XStep, xStep);
      pattern.Elements.SetReal(PdfTilingPattern.Keys.YStep, yStep);

      double dx = 2 * brush.RadiusX;
      double dy = 2 * brush.RadiusY;
      XRect brushBox = new XRect(brush.Center.X - brush.RadiusX, brush.Center.Y - brush.RadiusY, dx, dy);
      Debug.Assert(dx * dy > 0, "Radius is 0.");
      double scaleX, scaleY;
      if (dx > dy)
      {
        scaleX = 1;
        scaleY = dy / dx;
      }
      else
      {
        scaleX = dx / dy;
        scaleY = 1;
      }

      PdfColorMode colorMode = PdfColorMode.Rgb;
      PdfDictionary funcReflected;
      PdfDictionary funcRegular = BuildShadingFunction(brush.GradientStops, false, colorMode, true, out funcReflected);
      if (brush.SpreadMethod != SpreadMethod.Pad)
      {
        if (CanOptimizeForTwoColors(brush.GradientStops))
        {
          PdfDictionary dummy;
          funcReflected = BuildShadingFunction(brush.GradientStops, false, colorMode, false, out dummy);
        }
        else
        {
          Context.PdfDocument.Internals.AddObject(funcRegular);
        }
      }

      //PdfShading shading = BuildShading(brush, scaleX, scaleY);

      int shadingCount = 1;
      if (brush.SpreadMethod != SpreadMethod.Pad)
      {
        // TODO: Calculate number of required shadings
        shadingCount = Convert.ToInt32(Math.Max(boundingBox.width / (2 * brush.RadiusX), boundingBox.height / (2 * brush.RadiusY)) + 1);

        // HACK: Rule of thumb, better than nothing
        shadingCount *= 2;
      }
      PdfShading[] shadings = new PdfShading[shadingCount];

      // Create the shadings
      for (int idx = 0; idx < shadingCount; idx++)
      {
        PdfShading shading = BuildShading2(brush, scaleX, scaleY, idx,
          brush.SpreadMethod == SpreadMethod.Reflect && idx % 2 == 1 ? funcReflected : funcRegular);
        shadings[idx] = shading;
      }

      PdfContentWriter writer = new PdfContentWriter(Context, pattern);
      writer.BeginContentRaw();

      // Fill background (even if spread method is not pad)
      writer.WriteLiteral("q /DeviceRGB cs\n");
      //writer.WriteLiteral(PdfEncoders.ToString(clr0, colorMode) + " rg 1 i\n");
      writer.WriteLiteral(PdfEncoders.ToString(brush.GradientStops[brush.GradientStops.Count - 1].Color, PdfColorMode.Rgb) + " rg 1 i\n");
      writer.WriteLiteral("1 i\n");
      PdfExtGState gs = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
      gs.SetDefault1();
      writer.WriteGraphicsState(gs);
      writer.WriteLiteral("0 0 600 760 re f\n");
      XMatrix mat = brush.Transform.Matrix;
      if (!mat.IsIdentity)
        writer.WriteMatrix(mat);

      // Just work: silly loop thru shadings
      for (int idx = shadingCount - 1; idx >= 0; idx--)
      {
        writer.WriteLiteral("q 0 0 600 760 re W n\n");
        writer.WriteLiteral("q 0 g\n");
        gs = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
        gs.SetDefault2();
        writer.WriteGraphicsState(gs);

        XMatrix transformation = new XMatrix(scaleX, 0, 0, scaleY, 0, 0);
        writer.WriteMatrix(transformation);

        string shName = writer.Resources.AddShading(shadings[idx]);
        writer.WriteLiteral("BX " + shName + " sh EX Q Q\n");
      }
      writer.WriteLiteral("Q\n");
      writer.EndContent();

      return pattern;
    }

    PdfShading BuildShading(RadialGradientBrush brush, double scaleX, double scaleY)
    {
      // Setup shading
      //<<
      //  /AntiAlias false
      //  /ColorSpace 12 0 R
      //  /Coords [120 120 120 120 120 0]
      //  /Domain [0 1]
      //  /Extend [false false]
      //  /Function 14 0 R
      //  /ShadingType 3
      //>>
      PdfShading shading = Context.PdfDocument.Internals.CreateIndirectObject<PdfShading>();
      shading.Elements.SetInteger(PdfShading.Keys.ShadingType, 3); // Radial shading
      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;
      if (colorMode != PdfColorMode.Cmyk)
        shading.Elements[PdfShading.Keys.ColorSpace] = new PdfName("/DeviceRGB");
      else
        shading.Elements[PdfShading.Keys.ColorSpace] = new PdfName("/DeviceCMYK");

      //bool invers = true;

      PdfDictionary function = null; // BuildShadingFunction(brush.GradientStops, false, colorMode);

#if DEBUG_
      if (DevHelper.RenderComments)
        funcRegular.Elements.SetString("/@comment", "This is the shading function of a RadialGradientBrushPattern");
#endif
      shading.Elements[PdfShading.Keys.Function] = function;
      shading.Elements[PdfShading.Keys.Extend] = new PdfLiteral("[false false]");

      double x0 = brush.Center.X / scaleX;
      double y0 = brush.Center.Y / scaleY;
      double r0 = Math.Max(brush.RadiusX, brush.RadiusY);
      double x1 = brush.GradientOrigin.X / scaleX;
      double y1 = brush.GradientOrigin.Y / scaleY;
      double r1 = 0;

      shading.Elements[PdfShading.Keys.Coords] =
        new PdfLiteral("[{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###}]", x0, y0, r0, x1, y1, r1);

      return shading;
    }

    PdfShading BuildShading2(RadialGradientBrush brush, double scaleX, double scaleY, int repeatcount, PdfObject function)
    {
      // Setup shading
      //<<
      //  /AntiAlias false
      //  /ColorSpace 12 0 R
      //  /Coords [120 120 120 120 120 0]
      //  /Domain [0 1]
      //  /Extend [false false]
      //  /Function 14 0 R
      //  /ShadingType 3
      //>>
      PdfShading shading = Context.PdfDocument.Internals.CreateIndirectObject<PdfShading>();

      shading.Elements.SetInteger(PdfShading.Keys.ShadingType, 3); // Radial shading
      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;
      if (colorMode != PdfColorMode.Cmyk)
        shading.Elements[PdfShading.Keys.ColorSpace] = new PdfName("/DeviceRGB");
      else
        shading.Elements[PdfShading.Keys.ColorSpace] = new PdfName("/DeviceCMYK");

#if DEBUG_
      if (DevHelper.RenderComments)
        function.Elements.SetString("/@comment", "This is the shading function of a RadialGradientBrushPattern");
#endif
      shading.Elements[PdfShading.Keys.Function] = function;
      shading.Elements[PdfShading.Keys.AntiAlias] = new PdfLiteral("false"); // redundant
      shading.Elements[PdfShading.Keys.Extend] = new PdfLiteral("[false false]");

      double r = Math.Max(brush.RadiusX, brush.RadiusY);
      double x0 = brush.Center.X / scaleX;
      double y0 = brush.Center.Y / scaleY;
      double r0 = r * (repeatcount + 1);
      double x1 = brush.GradientOrigin.X / scaleX;
      double y1 = brush.GradientOrigin.Y / scaleY;
      double r1 = r * repeatcount;

      shading.Elements[PdfShading.Keys.Coords] =
        new PdfLiteral("[{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###}]", x0, y0, r0, x1, y1, r1);

      return shading;
    }

    /// <summary>
    /// Builds a PdfFormXObject from the specified brush. 
    /// // If a gradient contains transparency, a soft mask is created an added to the specified graphic state.
    /// </summary>
    PdfFormXObject BuildForm(RadialGradientBrush brush, PathGeometry geometry)
    {
      PdfFormXObject pdfForm = Context.PdfDocument.Internals.CreateIndirectObject<PdfFormXObject>();

      // HACK
      pdfForm.Elements.SetRectangle(PdfFormXObject.Keys.BBox, new PdfRectangle(0, 640, 480, 0));

      // Transparency group of the form
      //<<
      //  /I true
      //  /K false
      //  /S /Transparency
      //  /Type /Group
      //>>
      PdfTransparencyGroupAttributes tgPrimaryForm = Context.PdfDocument.Internals.CreateIndirectObject<PdfTransparencyGroupAttributes>();
      // not set by Acrobat: tgAttributes.Elements.SetName(PdfTransparencyGroupAttributes.Keys.CS, "/DeviceRGB");
      tgPrimaryForm.Elements.SetBoolean(PdfTransparencyGroupAttributes.Keys.I, true);
      tgPrimaryForm.Elements.SetBoolean(PdfTransparencyGroupAttributes.Keys.K, false);
      pdfForm.Elements.SetReference(PdfFormXObject.Keys.Group, tgPrimaryForm);

      // Shading
      PdfShading shading = BuildShading(brush, 1, 1);

      // ExtGState 
      //<<
      //  /AIS false
      //  /BM /Normal
      //  /ca 1
      //  /CA 1
      //  /op false
      //  /OP false
      //  /OPM 1
      //  /SA true
      //  /SMask 22 0 R
      //  /Type /ExtGState
      //>>
      PdfExtGState pdfExtGState = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
      pdfExtGState.SetDefault1();

      // Soft mask
      PdfSoftMask softmask = BuildSoftMask(brush);
      pdfExtGState.Elements.SetReference(PdfExtGState.Keys.SMask, softmask);

      // PdfFormXObject
      //<<
      //  /BBox [200.118 369.142 582.795 -141.094]
      //  /Group 11 0 R
      //  /Length 117
      //  /Matrix [1 0 0 1 0 0]
      //  /Resources
      //  <<
      //    /ColorSpace
      //    <<
      //      /CS0 8 0 R
      //    >>
      //    /ExtGState
      //    <<
      //      /GS0 23 0 R
      //    >>
      //    /Shading
      //    <<
      //      /Sh0 14 0 R
      //    >>
      //  >>
      //  /Subtype /Form
      //>>
      //stream
      //q
      //203.868 365.392 157.5 -97.5 re
      //W* n
      //q
      //0 g
      //1 i 
      ///GS0 gs
      //0.75 0 0 -0.75 200.1181183 369.1417236 cm
      //BX /Sh0 sh EX Q
      //Q
      //endstream
      PdfContentWriter writer = new PdfContentWriter(Context, pdfForm);
      writer.BeginContentRaw();
      // Acrobat 8 clips to bounding box, so we should do
      writer.WriteClip(geometry);
      //writer.WriteGraphicsState(extGState);
      //writer.WriteLiteral("0 g\n");
      writer.WriteLiteral(writer.Resources.AddExtGState(pdfExtGState) + " gs\n");

      XMatrix transform = new XMatrix(); //(brush.Viewport.Width / viewBoxForm.width, 0, 0, brush.Viewport.Height / viewBoxForm.height, 0, 0);
      writer.WriteMatrix(transform);
      writer.WriteLiteral("BX " + writer.Resources.AddShading(shading) + " sh EX\n");
      writer.EndContent();

      return pdfForm;
    }

    /// <summary>
    /// Builds the soft mask.
    /// </summary>
    PdfSoftMask BuildSoftMask(RadialGradientBrush brush)
    {
      Debug.Assert(brush.GradientStops.HasTransparency);

      XRect viewBox = new XRect(0, 0, 360, 480); // HACK
      //XForm xform = new XForm(Context.PdfDocument, viewBox);

      PdfFormXObject form = Context.PdfDocument.Internals.CreateIndirectObject<PdfFormXObject>();
#if DEBUG
      if (DevHelper.RenderComments)
        form.Elements.SetString("/@comment", "This is the Form XObject of the soft mask");
#endif
      form.Elements.SetRectangle(PdfFormXObject.Keys.BBox, new PdfRectangle(viewBox));


      // Transparency group of mask form
      //<<
      //  /CS /DeviceGray
      //  /I false
      //  /K false
      //  /S /Transparency
      //  /Type /Group
      //>>
      PdfTransparencyGroupAttributes tgAttributes = Context.PdfDocument.Internals.CreateIndirectObject<PdfTransparencyGroupAttributes>();
      tgAttributes.Elements.SetName(PdfTransparencyGroupAttributes.Keys.CS, "/DeviceGray");
      tgAttributes.Elements.SetBoolean(PdfTransparencyGroupAttributes.Keys.I, false);
      tgAttributes.Elements.SetBoolean(PdfTransparencyGroupAttributes.Keys.K, false);

      // ExtGState of mask form
      //<<
      //  /AIS false
      //  /BM /Normal
      //  /ca 1
      //  /CA 1
      //  /op false
      //  /OP false
      //  /OPM 1
      //  /SA true
      //  /SMask /None
      //  /Type /ExtGState
      //>>
      PdfExtGState pdfStateMaskFrom = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
      pdfStateMaskFrom.SetDefault1();

      // Shading of mask form
      PdfShading shadingFrom = BuildShadingForSoftMask(brush);

      ////// Set reference to transparency group attributes
      ////pdfForm.Elements.SetObject(PdfFormXObject.Keys.Group, tgAttributes);
      ////pdfForm.Elements[PdfFormXObject.Keys.Matrix] = new PdfLiteral("[1.001 0 0 1.001 0.001 0.001]");

      // Soft mask
      //<<
      //  /G 21 0 R   % form
      //  /S /Luminosity
      //  /Type /Mask
      //>>
      PdfSoftMask softmask = Context.PdfDocument.Internals.CreateIndirectObject<PdfSoftMask>();  // new PdfSoftMask(this.writer.Owner);
      //extGState.Elements.SetReference(PdfExtGState.Keys.SMask, softmask);
      //this.writer.Owner.Internals.AddObject(softmask);
#if DEBUG
      if (DevHelper.RenderComments)
        softmask.Elements.SetString("/@comment", "This is the soft mask");
#endif
      softmask.Elements.SetName(PdfSoftMask.Keys.S, "/Luminosity");
      softmask.Elements.SetReference(PdfSoftMask.Keys.G, form);

      // Create content of mask form
      //<<
      //  /BBox [200.118 369.142 582.795 -141.094]
      //  /Group 16 0 R
      //  /Length 121
      //  /Matrix [1 0 0 1 0 0]
      //  /Resources
      //  <<
      //    /ExtGState
      //    <<
      //      /GS0 20 0 R
      //    >>
      //    /Shading
      //    <<
      //      /Sh0 19 0 R
      //    >>
      //  >>
      //  /Subtype /Form
      //>>
      //stream
      //  q
      //    200.118 369.142 382.677 -510.236 re
      //    W n
      //  q
      //    0 g
      //    1 i 
      //    GS0 gs
      //    0.75 0 0 -0.75 200.1181183 369.1417236 cm
      //   BX /Sh0 sh EX Q
      //  Q
      //endstream
      form.Elements.SetReference(PdfFormXObject.Keys.Group, tgAttributes);
      PdfContentWriter writer = new PdfContentWriter(Context, form);
      writer.BeginContentRaw();
      // Acrobat 8 clips to bounding box, so we should do
      // why   0 480 360 -480 re ??
      //writer.WriteClip(bbox);
      //writer.WriteGraphicsState(extGState);
      writer.WriteLiteral("1 i 0 g\n");
      writer.WriteLiteral(writer.Resources.AddExtGState(pdfStateMaskFrom) + " gs\n");

      XMatrix transform = new XMatrix(); //(brush.Viewport.Width / viewBoxForm.width, 0, 0, brush.Viewport.Height / viewBoxForm.height, 0, 0);
      writer.WriteMatrix(transform);
      writer.WriteLiteral("BX " + writer.Resources.AddShading(shadingFrom) + " sh EX\n");
      writer.EndContent();

      return softmask;
    }

    /// <summary>
    /// Builds a monochrome shading for a form XObject of a soft mask.
    /// </summary>
    PdfShading BuildShadingForSoftMask(RadialGradientBrush brush)
    {
      // Setup shading
      //<<
      //  /ShadingType 2
      //  /AntiAlias false
      //  /BBox [0 0 510.236 680.315]
      //  /ColorSpace /DeviceGray
      //  /Coords [5 5 153.492 -86.924]
      //  /Domain [0 1]
      //  /Extend [true true]
      //  /Function 18 0 R
      //>>
      PdfShading shading = Context.PdfDocument.Internals.CreateIndirectObject<PdfShading>();
#if DEBUG
      if (DevHelper.RenderComments)
        shading.Elements.SetString("/@comment", "This is the shading function of a soft mask");
#endif
      shading.Elements.SetInteger(PdfShading.Keys.ShadingType, 2); // Axial shading
      shading.Elements.SetBoolean(PdfShading.Keys.AntiAlias, false);
      // TODO: BBox full page
      //shading.Elements.SetValue(PdfShading.Keys.BBox, new PdfLiteral("[0 0 480 640]"));
      shading.Elements.SetName(PdfShading.Keys.ColorSpace, "/DeviceGray");

      ////double x1 = brush.StartPoint.X;
      ////double y1 = brush.StartPoint.Y;
      ////double x2 = brush.EndPoint.X;
      ////double y2 = brush.EndPoint.Y;
      ////shading.Elements.SetValue(PdfShading.Keys.Coords, new PdfLiteral("[{0:0.####} {1:0.####} {2:0.####} {3:0.####}]", x1, y1, x2, y2));

      shading.Elements.SetValue(PdfShading.Keys.Domain, new PdfLiteral("[0 1]"));
      shading.Elements.SetValue(PdfShading.Keys.Extend, new PdfLiteral("[true true]"));

      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;
      PdfDictionary func;
      PdfDictionary function = BuildShadingFunction(brush.GradientStops, true, colorMode, true, out func);
#if true
      shading.Elements.SetValue(PdfShading.Keys.Function, function);
#else
      Context.PdfDocument.Internals.AddObject(function);
      shading.Elements.SetReference(PdfShading.Keys.Function, function);
#endif

      return shading;
    }

    /// <summary>
    /// Builds the shading function of the specified gradient stop collection.
    /// </summary>
    protected PdfDictionary BuildShadingFunction(GradientStopCollection gradients, bool softMask, PdfColorMode colorMode, bool reverse, out PdfDictionary funcReverse)
    {
      PdfDictionary func = new PdfDictionary();
      int count = gradients.Count;
      Debug.Assert(count >= 2);

      if (CanOptimizeForTwoColors(gradients))
      {
        funcReverse = null;

        // Build a Type 3 function with an array of 2 Type 2 functions
        func.Elements["/FunctionType"] = new PdfInteger(3);  // Type 3 - Stitching Function
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
            fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0.ScA) + "]");
            fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1.ScA) + "]");
            fn2.Elements["/Range"] = new PdfLiteral("[0 1]");
          }
          else
          {
            fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0, colorMode) + "]");
            fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1, colorMode) + "]");
            fn2.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
          }
          fn2.Elements["/Domain"] = new PdfLiteral("[0 1]");
          fn2.Elements["/N"] = new PdfInteger(1);
          fnarray.Elements.Add(fn2);
          if (idx > 1)
          {
            bounds.Append(' ');
            encode.Append(' ');
          }
          if (idx < count - 1)
            bounds.Append(PdfEncoders.ToString(gradients[idx].Offset));
          encode.Append(reverse ? "1 0" : "0 1");
        }
        bounds.Append(']');
        encode.Append(']');
        func.Elements["/Bounds"] = new PdfLiteral(bounds.ToString());
        func.Elements["/Encode"] = new PdfLiteral(encode.ToString());
      }
      else
      {
#if true
        funcReverse = BuildShadingFunction3(gradients, softMask, colorMode);
        Context.PdfDocument.Internals.AddObject(funcReverse);

        func.Elements["/FunctionType"] = new PdfInteger(3);  // Type 3 - Stitching Function
        func.Elements["/Domain"] = new PdfLiteral("[0 1]");
        func.Elements["/Encode"] = new PdfLiteral("[1 0]");
        func.Elements["/Bounds"] = new PdfLiteral("[]");
        func.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
        PdfArray fnarray0 = new PdfArray();
        fnarray0.Elements.Add(funcReverse);
        func.Elements["/Functions"] = fnarray0;

#else
        //        // Build a Type 3 function with an array of n-1 Type 2 functions

        PdfDictionary fn1 = new PdfDictionary();


        func.Elements["/FunctionType"] = new PdfInteger(3);  // Type 3 - Stitching Function
        func.Elements["/Domain"] = new PdfLiteral("[0 1]");
        func.Elements["/Encode"] = new PdfLiteral("[1 0]");
        func.Elements["/Bounds"] = new PdfLiteral("[]");
        func.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
        PdfArray fnarray0 = new PdfArray();
        fnarray0.Elements.Add(fn1);
        func.Elements["/Functions"] = fnarray0;





        fn1.Elements["/FunctionType"] = new PdfInteger(3);  // Type 3 - Stitching Function
        fn1.Elements["/Domain"] = new PdfLiteral("[0 1]");
        fn1.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
        PdfArray fnarray = new PdfArray();
        fn1.Elements["/Functions"] = fnarray;

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
            fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0.ScA) + "]");
            fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1.ScA) + "]");
            fn2.Elements["/Range"] = new PdfLiteral("[0 1]");
          }
          else
          {
            fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0, colorMode) + "]");
            fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1, colorMode) + "]");
            fn2.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
          }
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
        encode.Append(']');
        fn1.Elements["/Bounds"] = new PdfLiteral(bounds.ToString());
        fn1.Elements["/Encode"] = new PdfLiteral(encode.ToString());
#endif
      }
      return func;
    }

    PdfDictionary BuildShadingFunction3(GradientStopCollection gradients, bool softMask, PdfColorMode colorMode)
    {
      int count = gradients.Count;
      Debug.Assert(count >= 2);

      //        // Build a Type 3 function with an array of n-1 Type 2 functions

      PdfDictionary fn1 = new PdfDictionary();


      fn1.Elements["/FunctionType"] = new PdfInteger(3);  // Type 3 - Stitching Function
      fn1.Elements["/Domain"] = new PdfLiteral("[0 1]");
      fn1.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
      PdfArray fnarray = new PdfArray();
      fn1.Elements["/Functions"] = fnarray;

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
          fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0.ScA) + "]");
          fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1.ScA) + "]");
          fn2.Elements["/Range"] = new PdfLiteral("[0 1]");
        }
        else
        {
          fn2.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0, colorMode) + "]");
          fn2.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1, colorMode) + "]");
          fn2.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
        }
        fn2.Elements["/Domain"] = new PdfLiteral("[0 1]");
        fn2.Elements["/N"] = new PdfInteger(1);
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
      encode.Append(']');
      fn1.Elements["/Bounds"] = new PdfLiteral(bounds.ToString());
      fn1.Elements["/Encode"] = new PdfLiteral(encode.ToString());

      return fn1;
    }
  }
}