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
  class LinearShadingBuilder : PatternOrShadingBuilder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LinearShadingBuilder"/> class.
    /// </summary>
    LinearShadingBuilder(DocumentRenderingContext context) :
      base(context)
    { }

    /// <summary>
    /// Builds a shading from a linear gradient brush.
    /// </summary>
    public static PdfShading BuildShadingFromLinearGradientBrush(DocumentRenderingContext context, LinearGradientBrush brush)
    {
      LinearShadingBuilder builder = new LinearShadingBuilder(context);
      PdfShading shading = builder.BuildShading(brush);
      return shading;
    }

    /// <summary>
    /// Builds a pattern from a linear gradient brush.
    /// </summary>
    public static PdfShadingPattern BuildPatternFromLinearGradientBrush(DocumentRenderingContext context, LinearGradientBrush brush, XMatrix transform)
    {
      LinearShadingBuilder builder = new LinearShadingBuilder(context);
      PdfShadingPattern pattern = builder.BuildShadingPattern(brush, transform);
      return pattern;
    }

    /// <summary>
    /// Builds a form XObject from a linear gradient brush that uses transparency.
    /// </summary>
    public static PdfFormXObject BuildFormFromLinearGradientBrush(DocumentRenderingContext context, LinearGradientBrush brush, PathGeometry geometry)
    {
      LinearShadingBuilder builder = new LinearShadingBuilder(context);
      PdfFormXObject pdfForm = builder.BuildForm(brush, geometry);
      return pdfForm;
    }

    /// <summary>
    /// Builds a PdfShading from the specified brush. If a gradient contains transparency, a soft mask is created an added to the 
    /// specified graphic state.
    /// </summary>
    PdfShading BuildShading(LinearGradientBrush brush)
    {
      // Setup shading
      PdfShading shading = new PdfShading(Context.PdfDocument);
#if DEBUG
      if (DevHelper.RenderComments)
        shading.Elements.SetString("/@comment", "This is the shading of a LinearGradientBrush");
#endif
      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;

      PdfDictionary function = BuildShadingFunction(brush.GradientStops, false, colorMode);
#if DEBUG
      if (DevHelper.RenderComments)
        function.Elements.SetString("/@comment", "This is the shading function of a LinearGradientBrush");
#endif
      shading.Elements.SetBoolean(PdfShading.Keys.AntiAlias, false);
      shading.Elements[PdfShading.Keys.Function] = function;
      shading.Elements.SetInteger(PdfShading.Keys.ShadingType, 2); // Axial shading
      shading.Elements.SetName(PdfShading.Keys.ColorSpace, "/DeviceRGB"); // TODO: respect ColorMode

      double x1 = brush.StartPoint.X;
      double y1 = brush.StartPoint.Y;
      double x2 = brush.EndPoint.X;
      double y2 = brush.EndPoint.Y;
      shading.Elements[PdfShading.Keys.Coords] = new PdfLiteral("[{0:0.####} {1:0.####} {2:0.####} {3:0.####}]", x1, y1, x2, y2);

      shading.Elements[PdfShading.Keys.Domain] = new PdfLiteral("[0 1]");
      if (brush.SpreadMethod == SpreadMethod.Pad)
        shading.Elements[PdfShading.Keys.Extend] = new PdfLiteral("[true true]");
      else
      {
        DevHelper.NotImplemented("SpreadMethod." + brush.SpreadMethod.ToString() + " Background painted in green.");
#if DEBUG
        // Note from PDF reference: The background color is applied only when the shading is used as part of
        // a shading pattern, not when it is painted directly with the sh operator.
        shading.Elements[PdfShading.Keys.Background] = new PdfLiteral("[0 1 0]"); // TODO: respect ColorMode
#else
        // Best we can currently do
        shading.Elements[PdfShading.Keys.Extend] = new PdfLiteral("[true true]");
#endif
      }
      return shading;
    }

    /// <summary>
    /// Builds a PdfShadingPattern from the specified brush.
    /// </summary>
    PdfShadingPattern BuildShadingPattern(LinearGradientBrush brush, XMatrix transform)
    {
      //<<
      //  /Matrix [0.75 0 0 -0.75 8 470.4]
      //  /PatternType 2
      //  /Shading 22 0 R
      //  /Type /Pattern
      //>>
      XMatrix matrix = transform;
      PdfShadingPattern pattern = Context.PdfDocument.Internals.CreateIndirectObject<PdfShadingPattern>();
      pattern.Elements.SetInteger(PdfShadingPattern.Keys.PatternType, 2);  // Shading
      pattern.Elements.SetMatrix(PdfShadingPattern.Keys.Matrix, matrix);

      PdfShading shading = BuildShading(brush);
      Context.PdfDocument.Internals.AddObject(shading);
      pattern.Elements.SetReference(PdfShadingPattern.Keys.Shading, shading);

      return pattern;
    }

    /// <summary>
    /// Builds a PdfFormXObject from the specified brush. 
    /// // If a gradient contains transparency, a soft mask is created an added to the specified graphic state.
    /// </summary>
    PdfFormXObject BuildForm(LinearGradientBrush brush, PathGeometry geometry)
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
      PdfShading shading = BuildShading(brush);

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
    PdfSoftMask BuildSoftMask(LinearGradientBrush brush)
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
    PdfShading BuildShadingForSoftMask(LinearGradientBrush brush)
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

      double x1 = brush.StartPoint.X;
      double y1 = brush.StartPoint.Y;
      double x2 = brush.EndPoint.X;
      double y2 = brush.EndPoint.Y;
      shading.Elements.SetValue(PdfShading.Keys.Coords, new PdfLiteral("[{0:0.####} {1:0.####} {2:0.####} {3:0.####}]", x1, y1, x2, y2));

      shading.Elements.SetValue(PdfShading.Keys.Domain, new PdfLiteral("[0 1]"));
      shading.Elements.SetValue(PdfShading.Keys.Extend, new PdfLiteral("[true true]"));

      PdfColorMode colorMode = PdfColorMode.Rgb; //this.document.Options.ColorMode;
      PdfDictionary function = BuildShadingFunction(brush.GradientStops, true, colorMode);
#if true
      shading.Elements.SetValue(PdfShading.Keys.Function, function);
#else
      Context.PdfDocument.Internals.AddObject(function);
      shading.Elements.SetReference(PdfShading.Keys.Function, function);
#endif

      return shading;
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
      PdfDictionary function = BuildShadingFunction(brush.GradientStops, true, colorMode);
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
    protected PdfDictionary BuildShadingFunction(GradientStopCollection gradients, bool softMask, PdfColorMode colorMode)
    {
      PdfDictionary func = new PdfDictionary();
      int count = gradients.Count;
      Debug.Assert(count >= 2);
      if (count == 2 && gradients[0].Offset == 0 && gradients[1].Offset == 1)
      {
        // Build a Type 2 function
        func.Elements["/FunctionType"] = new PdfInteger(2);  // Type 2 - Exponential Interpolation Function
        Color clr0 = gradients[0].Color;
        Color clr1 = gradients[1].Color;
        if (softMask)
        {
          func.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0.ScA) + "]");
          func.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1.ScA) + "]");
          func.Elements["/Range"] = new PdfLiteral("[0 1]");
        }
        else
        {
          func.Elements["/C0"] = new PdfLiteral("[" + PdfEncoders.ToString(clr0, colorMode) + "]");
          func.Elements["/C1"] = new PdfLiteral("[" + PdfEncoders.ToString(clr1, colorMode) + "]");
          func.Elements["/Range"] = new PdfLiteral("[0 1 0 1 0 1]");
        }
        func.Elements["/Domain"] = new PdfLiteral("[0 1]");
        func.Elements["/N"] = new PdfInteger(1);  // be linear
      }
      else
      {
        // Build a Type 3 function with an array of n-1 Type 2 functions
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
        func.Elements["/Bounds"] = new PdfLiteral(bounds.ToString());
        func.Elements["/Encode"] = new PdfLiteral(encode.ToString());
      }
      return func;
    }
  }
}