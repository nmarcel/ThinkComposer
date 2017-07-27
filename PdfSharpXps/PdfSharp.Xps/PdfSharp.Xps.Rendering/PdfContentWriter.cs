using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using PdfSharp.Xps.XpsModel;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.Internal;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Pdf;
using PdfSharp.Fonts.OpenType;

#pragma warning disable 414, 169, 649 // incomplete code state

namespace PdfSharp.Xps.Rendering
{
  /// <summary>
  /// Provides the funtionality to write a PDF content stream for a PdfPage or an XForm.
  /// </summary>
  partial class PdfContentWriter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PdfContentWriter"/> class
    /// for creating a content stream of the specified page.
    /// </summary>
    public PdfContentWriter(DocumentRenderingContext context, PdfPage page) // , XGraphics gfx, XGraphicsPdfPageOptions options)
    {
      this.context = context;
      this.page = page;
      this.contentStreamDictionary = page;
      //this.colorMode = page.document.Options.ColorMode;
      //this.options = options;
      this.content = new StringBuilder();
      this.graphicsState = new PdfGraphicsState(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfContentWriter"/> class
    /// for creating a content stream of the specified form.
    /// </summary>
    public PdfContentWriter(DocumentRenderingContext context, XForm form, RenderMode renderMode) // , XGraphics gfx, XGraphicsPdfPageOptions options)
    {
      this.context = context;
      this.form = form;
      this.contentStreamDictionary = form;
      this.renderMode = renderMode;
      //this.colorMode = page.document.Options.ColorMode;
      //this.options = options;
      this.content = new StringBuilder();
      this.graphicsState = new PdfGraphicsState(this);
    }

    /// <summary>
    /// </summary>
    public PdfContentWriter(DocumentRenderingContext context, PdfDictionary contentDictionary)
    {
      if (!(contentDictionary is IContentStream))
        throw new ArgumentException("contentDictionary must implement IContentStream.");
      this.context = context;
      this.contentDictionary = contentDictionary;
      this.contentStreamDictionary = (IContentStream)contentDictionary;
      this.renderMode = RenderMode.Default;
      //this.colorMode = page.document.Options.ColorMode;
      //this.options = options;
      this.content = new StringBuilder();
      this.graphicsState = new PdfGraphicsState(this);
    }

    internal PdfPage page;
    internal XForm form;
    internal PdfDictionary contentDictionary;
    internal IContentStream contentStreamDictionary;
    internal RenderMode renderMode;


    /// <summary>
    /// Gets the document rendering context this PDF contentwriter belongs to.
    /// </summary>
    public DocumentRenderingContext Context
    {
      get { return this.context; }
    }
    DocumentRenderingContext context;

    internal void CreateDefaultTransparencyGroup() // HACK
    {
      if (this.page != null)
        this.page.transparencyUsed = true;
    }

    /// <summary>
    /// Writes all elements of the collection to the content stream.
    /// </summary>
    public void WriteElements(XpsElementCollection elements)
    {
      //elements.ForEach(new Action<XpsElement>(writee));
      foreach (XpsElement element in elements)
        WriteElement(element);
    }

    /// <summary>
    /// Writes the specified element to the content stream.
    /// </summary>
    public void WriteElement(XpsElement element)
    {
      Canvas canvas;
      Path path;
      Glyphs glyphs;
      Visual visual;
      Comment comment;
      if ((canvas = element as Canvas) != null)
      {
        BeginGraphic();
        WriteCanvas(canvas);
      }
      else if ((path = element as Path) != null)
      {
        BeginGraphic();
        WritePath(path);
      }
      else if ((glyphs = element as Glyphs) != null)
      {
        //BeginText();
        WriteGlyphs(glyphs);
      }
      else if ((visual = element as Visual) != null)
      {
        WriteVisual(visual);
        //foreach (XpsElement visualElement in visual.Content)
        //  WriteElement(visualElement);
      }
      else if ((comment = element as Comment) != null)
      {
        //DaSt : Comment?
        //WriteGlyphs(glyphs);
      }
      else
        throw new InvalidOperationException("Invalid element type.");
    }

    /// <summary>
    /// Writes a Visual to the content stream.
    /// </summary>
    internal void WriteVisual(Visual visual) // Is internal for VisualBrush
    {
      WriteSaveState("begin Visual", null);
      WriteElements(visual.Content);
      WriteRestoreState("end Visual", null);
    }

    /// <summary>
    /// Writes a Canvas to the content stream.
    /// </summary>
    private void WriteCanvas(Canvas canvas)
    {
      WriteSaveState("begin Canvas", canvas.Name);

      // Transform also affects clipping and opacity mask
      bool transformed = canvas.RenderTransform != null;
      if (transformed)
      {
        MultiplyTransform(canvas.RenderTransform);
        WriteRenderTransform(canvas.RenderTransform);
      }

      bool clipped = canvas.Clip != null;
      if (clipped)
        WriteClip(canvas.Clip);

      if (canvas.Opacity < 1)
        MultiplyOpacity(canvas.Opacity);

      if (canvas.OpacityMask != null)
        WriteOpacityMask(canvas.OpacityMask);

      WriteElements(canvas.Content);
      // Must leave text mode at end of canvas
      BeginGraphic();
      WriteRestoreState("end Canvas", canvas.Name);
    }

#if in_other_file
    /// <summary>
    /// Writes a Glyphs to the content stream.
    /// </summary>
    private void WriteGlyphs(Glyphs glyphs)
    {
      WriteSaveState("begin Glyphs", glyphs.Name);

      // Transform also affects clipping and opacity mask
      bool transformed = glyphs.RenderTransform != null;
      if (transformed)
        WriteRenderTransform(glyphs.RenderTransform);

      bool clipped = glyphs.Clip != null;
      if (clipped)
        WriteClip(glyphs.Clip);

      if (glyphs.Opacity < 1)
        MultiplyOpacity(glyphs.Opacity);

      if (glyphs.OpacityMask != null)
        WriteOpacityMask(glyphs.OpacityMask);

      XForm xform = null;
      XImage ximage = null;
      RealizeFill(glyphs.Fill, ref xform, ref ximage);
      RealizeFont(glyphs);

      double x = glyphs.OriginX;
      double y = glyphs.OriginY;


      //switch (format.Alignment)
      //{
      //  case XStringAlignment.Near:
      //    // nothing to do
      //    break;

      //  case XStringAlignment.Center:
      //    x += (rect.Width - width) / 2;
      //    break;

      //  case XStringAlignment.Far:
      //    x += rect.Width - width;
      //    break;
      //}

      PdfFont realizedFont = this.graphicsState.realizedFont;
      Debug.Assert(realizedFont != null);
      realizedFont.AddChars(glyphs.UnicodeString);

      TrueTypeDescriptor descriptor = realizedFont.FontDescriptor.descriptor;

      //if (bold && !descriptor.IsBoldFace)
      //{
      //  // TODO: emulate bold by thicker outline
      //}

      //if (italic && !descriptor.IsBoldFace)
      //{
      //  // TODO: emulate italic by shearing transformation
      //}

      string s2 = "";
      string s = glyphs.UnicodeString;
      if (!String.IsNullOrEmpty(s))
      {
        int length = s.Length;
        for (int idx = 0; idx < length; idx++)
        {
          char ch = s[idx];
          int glyphID = 0;
          if (descriptor.fontData.cmap.symbol)
          {
            glyphID = (int)ch + (descriptor.fontData.os2.usFirstCharIndex & 0xFF00);
            glyphID = descriptor.CharCodeToGlyphIndex((char)glyphID);
          }
          else
            glyphID = descriptor.CharCodeToGlyphIndex(ch);
          s2 += (char)glyphID;
        }
      }
      s = s2;

      byte[] bytes = PdfEncoders.RawUnicodeEncoding.GetBytes(s);
      bytes = PdfEncoders.FormatStringLiteral(bytes, true, false, true, null);
      string text = PdfEncoders.RawEncoding.GetString(bytes);
      if (glyphs.IsSideways)
      {
        XMatrix textMatrix = new XMatrix();
        textMatrix.RotateAtPrepend(-90, new XPoint(x, y));
        XPoint pos = new XPoint(x, y);
        AdjustTextMatrix(ref pos);
        WriteTextTransform(textMatrix);
        WriteLiteral("{0} Tj\n", text);
      }
      else
      {
        XPoint pos = new XPoint(x, y);
        AdjustTextMatrix(ref pos);
        WriteLiteral("{0:0.###} {1:0.###} Td {2} Tj\n", pos.x, pos.y, text);
        //PdfEncoders.ToStringLiteral(s, PdfStringEncoding.RawEncoding, null));
      }
      WriteRestoreState("end Glyphs", glyphs.Name);
    }

    // ...on the way to handle Indices...
    private void WriteGlyphs_Indices1(Glyphs glyphs, string text)
    {
      GlyphIndicesComplexity complexity = glyphs.Indices.Complexity;
      complexity = GlyphIndicesComplexity.ClusterMapping;
      switch (complexity)
      {
        case GlyphIndicesComplexity.None:
          break;

        case GlyphIndicesComplexity.DistanceOnly:
          break;

        case GlyphIndicesComplexity.GlyphIndicesAndDistanceOnly:
          break;

        case GlyphIndicesComplexity.ClusterMapping:
          WriteGlyphs_ClusterMapping(glyphs, text);
          break;
      }
    }

    private void WriteGlyphs_None(Glyphs glyphs, string text)
    {
      // TODO:
    }

    private void WriteGlyphs_DistanceOnly(Glyphs glyphs, string text)
    {
      // TODO:
    }

    private void WriteGlyphs_GlyphIndicesAndDistanceOnly(Glyphs glyphs, string text)
    {
      // TODO:
    }

    private void WriteGlyphs_ClusterMapping(Glyphs glyphs, string text)
    {
      GlyphIndices indices = glyphs.Indices;
      int chCount = text.Length;
      //int chidx = 0;
      int glCont = indices.Count;
      int glidx = 0;
      bool stop = false;
      do
      {
        GlyphIndices.GlyphMapping mapping = indices[glidx];

//        if (mapping.HasClusterCount
      }
      while (!stop);
    }
#endif

    /// <summary>
    /// Writes a Path to the content stream.
    /// </summary>
    private void WritePath(Path path)
    {
      if (WriteSingleLineStrokeWithSpecialCaps(path))
        return;

      WriteSaveState("begin Path", path.Name);

      // Transform also affects clipping and opacity mask
      if (path.RenderTransform != null && this.renderMode == RenderMode.Default)
      {
        MultiplyTransform(path.RenderTransform);
        WriteRenderTransform(path.RenderTransform);
      }

      if (path.Clip != null && this.renderMode == RenderMode.Default)
        WriteClip(path.Clip);

      if (path.Opacity < 1)
        MultiplyOpacity(path.Opacity);

      if (path.OpacityMask != null)
        WriteOpacityMask(path.OpacityMask);

      if (path.Fill == null)
      {
        if (path.Stroke != null)
        {
#if true
          WriteStrokeGeometry(path);
#else
          // Just stroke the path
          RealizeStroke(path);
          WriteGeometry(path.Data);
          WritePathFillStroke(path);
#endif
        }
        else
          Debug.Assert(false, "??? Path with neither Stroke nor Fill encountered.");
      }
      else
      {
        SolidColorBrush sBrush;
        LinearGradientBrush lgBrush;
        RadialGradientBrush rgBrush;
        ImageBrush iBrush;
        VisualBrush vBrush;
        if ((sBrush = path.Fill as SolidColorBrush) != null)
        {
          Color color = sBrush.Color;
          double opacity = Opacity * color.ScA;
          if (opacity < 1)
            RealizeFillOpacity(opacity);

          WriteRgb(color, " rg\n");

          if (path.Stroke != null)
            RealizeStroke(path);

          WriteGeometry(path.Data);
          WritePathFillStroke(path);
        }
        else if ((lgBrush = path.Fill as LinearGradientBrush) != null)
        {
          // TODO: For better visual compatibility use a Shading Pattern if Opacity is not 1 and
          // the Stroke Style is not solid. Acrobat 8 ignores this case.

          PdfExtGState xgState = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
          xgState.SetDefault1();

          double opacity = Opacity * lgBrush.Opacity; ;
          if (opacity < 1)
          {
            xgState.StrokeAlpha = opacity;
            xgState.NonStrokeAlpha = opacity;
          }
          RealizeExtGState(xgState);

          // 1st draw fill
          if (lgBrush.GradientStops.HasTransparency)
          {
            // Create a FormXObject with a soft mask
            PdfFormXObject form = LinearShadingBuilder.BuildFormFromLinearGradientBrush(Context, lgBrush, path.Data);
            string foName = Resources.AddForm(form);
            WriteLiteral(foName + " Do\n");
          }
          else
          {
            // Create just a shading
            PdfShading shading = LinearShadingBuilder.BuildShadingFromLinearGradientBrush(Context, lgBrush);
            string shName = Resources.AddShading(shading);
            WriteLiteral("q\n");
            WriteClip(path.Data);
            WriteLiteral("BX " + shName + " sh EX Q\n");
          }

          // 2nd draw stroke
          if (path.Stroke != null)
            WriteStrokeGeometry(path);
        }
        else if ((rgBrush = path.Fill as RadialGradientBrush) != null)
        {
          PdfExtGState xgState = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
          xgState.SetDefault1();

          double avGradientOpacity = rgBrush.GradientStops.GetAverageAlpha(); // HACK
          double opacity = Opacity * rgBrush.Opacity * avGradientOpacity;
          if (opacity < 1)
          {
            xgState.StrokeAlpha = opacity;
            xgState.NonStrokeAlpha = opacity;
          }
          //RealizeExtGState(xgState);

#if true
          XRect boundingBox = path.Data.GetBoundingBox();
          // Always creates a pattern, because the background must be filled
          PdfShadingPattern pattern = RadialShadingBuilder.BuildFromRadialGradientBrush(Context, rgBrush, boundingBox, Transform);
          string paName = Resources.AddPattern(pattern);

          // stream
          // /CS0 cs /P0 scn
          // /GS0 gs
          // 0 480 180 -90 re
          // f*
          // endstream
          // endobj
          WriteLiteral("/Pattern cs " + paName + " scn\n");
          // move to here: RealizeExtGState(xgState);
          RealizeExtGState(xgState);
          WriteGeometry(path.Data);
          if (path.Data.FillRule == FillRule.NonZero) // NonZero means Winding
            WriteLiteral("f\n");
          else
            WriteLiteral("f*\n");
#else
#if true
          // 1st draw fill
          if (rgBrush.GradientStops.HasTransparentColors)
          {
            // Create a FormXObject with a soft mask
            PdfFormXObject form = ShadingBuilder.BuildFormFromRadialGradientBrush(Context, rgBrush, path.Data);
            string foName = Resources.AddForm(form);
            WriteLiteral(foName + " Do\n");
          }
          else
          {
            // Create just a shading
            PdfShading shading = ShadingBuilder.BuildShadingFromRadialGradientBrush(Context, rgBrush);
            string shName = Resources.AddShading(shading);
            WriteLiteral("q\n");
            WriteClip(path.Data);
            WriteLiteral("BX " + shName + " sh EX Q\n");
          }
#else
          // Establish graphic state dictionary
          PdfExtGState extGState = new PdfExtGState(Context.PdfDocument);
          Context.PdfDocument.Internals.AddObject(extGState);
          string gsName = Resources.AddExtGState(extGState);
          WriteLiteral(gsName + " gs\n");

          // 1st draw fill
          PdfShading shading = ShadingBuilder.BuildFormFromRadialGradientBrush(Context, rgBrush); //, extGState);
          string shName = Resources.AddShading(shading);

          WriteClip(path.Data);
          WriteLiteral("BX " + shName + " sh EX\n");
#endif
#endif

          // 2nd draw stroke
          if (path.Stroke != null)
            WriteStrokeGeometry(path);
        }
        else if ((iBrush = path.Fill as ImageBrush) != null)
        {
          PdfExtGState xgState = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
          xgState.SetDefault1();

          double opacity = Opacity * iBrush.Opacity;
          if (opacity <= 1)
          {
            xgState.StrokeAlpha = opacity;
            xgState.NonStrokeAlpha = opacity;
          }
          RealizeExtGState(xgState);

          // 1st draw fill
          PdfTilingPattern pattern = TilingPatternBuilder.BuildFromImageBrush(Context, iBrush, Transform);
          string name = Resources.AddPattern(pattern);

          WriteLiteral("/Pattern cs " + name + " scn\n");
          WriteGeometry(path.Data);
          WritePathFillStroke(path);

          // 2nd draw stroke
          if (path.Stroke != null)
            WriteStrokeGeometry(path);
        }
        else if ((vBrush = path.Fill as VisualBrush) != null)
        {
          PdfExtGState xgState = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
          xgState.SetDefault1();

          double opacity = Opacity * vBrush.Opacity;
          if (opacity < 1)
          {
            xgState.StrokeAlpha = opacity;
            xgState.NonStrokeAlpha = opacity;
          }
          RealizeExtGState(xgState);

          // 1st draw fill
          PdfTilingPattern pattern = TilingPatternBuilder.BuildFromVisualBrush(Context, vBrush, Transform);
          string name = Resources.AddPattern(pattern);

          WriteLiteral("/Pattern cs " + name + " scn\n");
          WriteGeometry(path.Data);
          WritePathFillStroke(path);

          // 2nd draw stroke
          if (path.Stroke != null)
            WriteStrokeGeometry(path);
        }
        else
        {
          Debug.Assert(false, "Unknown brush type encountered.");
        }
      }
      WriteRestoreState("end Path", path.Name);
    }

    /// <summary>
    /// Strokes the path geometry with the Stroke brush.
    /// </summary>
    private void WriteStrokeGeometry(Path path)
    {
      if (path.Stroke == null)
        return;

      SolidColorBrush sBrush;
      LinearGradientBrush lgBrush;
      RadialGradientBrush rgBrush;
      VisualBrush vBrush;
      ImageBrush iBrush;

      //if (path.Stroke != null && this.renderMode == RenderMode.Default) // HACK

      if ((sBrush = path.Stroke as SolidColorBrush) != null)
      {
        RealizeStroke(path);
        WriteGeometry(path.Data);
        WriteLiteral("S\n");
      }
      else if ((lgBrush = path.Stroke as LinearGradientBrush) != null)
      {
        PdfExtGState xgState = Context.PdfDocument.Internals.CreateIndirectObject<PdfExtGState>();
        xgState.SetDefault1();

        double opacity = Opacity * lgBrush.Opacity; ;
        if (opacity < 1)
        {
          xgState.StrokeAlpha = opacity;
          xgState.NonStrokeAlpha = opacity;
        }
        RealizeExtGState(xgState);

        // /CS1 CS /P0 SCN
        // 7.5 w 
        // q 1 0 0 1 15.5 462.9 cm
        // 0 0 m
        // 153 0 l
        // 153 -93 l
        // 0 -93 l
        // h
        // S
        // Q
        if (lgBrush.GradientStops.HasTransparency)
        {
          // TODO: Create Form
          PdfShadingPattern pattern = LinearShadingBuilder.BuildPatternFromLinearGradientBrush(Context, lgBrush, Transform);
          string paName = Resources.AddPattern(pattern);
          WriteLiteral("/Pattern CS " + paName + " SCN\n");
          WriteLiteral("q {0:0.###} w", path.StrokeThickness);
          WriteGeometry(path.Data);
          WriteLiteral("S Q\n");

          //// Create a FormXObject with a soft mask
          //PdfFormXObject form = LinearShadingBuilder.BuildFormFromLinearGradientBrush(Context, lgBrush, path.Data);
          //string foName = Resources.AddForm(form);
          //WriteLiteral(foName + " Do\n");
        }
        else
        {
          PdfShadingPattern pattern = LinearShadingBuilder.BuildPatternFromLinearGradientBrush(Context, lgBrush, Transform);
          string paName = Resources.AddPattern(pattern);
          WriteLiteral("/Pattern CS " + paName + " SCN\n");
          WriteLiteral("q {0:0.###} w", path.StrokeThickness);
          WriteGeometry(path.Data);
          WriteLiteral("S Q\n");
        }
      }
      else if ((rgBrush = path.Stroke as RadialGradientBrush) != null)
      {
        // HACK
        WriteLiteral("/DeviceRGB CS 0 1 0 RG\n");
        WriteLiteral("q {0:0.###} w", path.StrokeThickness);
        WriteGeometry(path.Data);
        WriteLiteral("S Q\n");
      }
      else if ((iBrush = path.Stroke as ImageBrush) != null)
      {
        // HACK
        WriteLiteral("/DeviceRGB CS 0 1 0 RG\n");
        WriteLiteral("q {0:0.###} w", path.StrokeThickness);
        WriteGeometry(path.Data);
        WriteLiteral("S Q\n");
      }
      else if ((vBrush = path.Stroke as VisualBrush) != null)
      {
        // HACK
        WriteLiteral("/DeviceRGB CS 0 1 0 RG\n");
        WriteLiteral("q {0:0.###} w", path.StrokeThickness);
        WriteGeometry(path.Data);
        WriteLiteral("S Q\n");
      }
      else
        Debug.Assert(false);
    }

    /// <summary>
    /// If the path is a single line with different start and end caps, convert the line into an area.
    /// </summary>
    private bool WriteSingleLineStrokeWithSpecialCaps(Path path)
    {
      if (path.StrokeStartLineCap == path.StrokeEndLineCap && path.StrokeStartLineCap != LineCap.Triangle)
        return false;
      if (path.Data.Figures.Count != 1)
        return false;
      PathFigure figure = path.Data.Figures[0];
      if (figure.Segments.Count != 1)
        return false;
      PolyLineSegment polyLineSegment = figure.Segments[0] as PolyLineSegment;
      if (polyLineSegment.Points.Count != 1)
        return false;

      // TODO: Create a new path that draws the line
      path.GetType();

      return false;
    }

    void AddCapToPath(Path path, PathFigure figure, double length, double lineWidthHalf, LineCap lineCap, XMatrix matrix)
    {
      // sketch:
      // 1. create Transform that make a horizontal line with start in 0,0
      // 2. create a Polygon with the shape of the line including its caps
      // 3. render the shape with the brush of the pen
      //PolyLineSegment seg;
      switch (lineCap)
      {
        case LineCap.Flat:
          matrix.Transform(new XPoint(length + lineWidthHalf, -lineWidthHalf));
          break;

        case LineCap.Square:
          matrix.Transform(new XPoint(length + lineWidthHalf, -lineWidthHalf));
          break;

        case LineCap.Round:
          break;

        case LineCap.Triangle:
          break;
      }
    }












    /// <summary>
    /// Gets the content created by this renderer.
    /// </summary>
    string GetContent()
    {
      EndContent();
      return this.content.ToString();
    }

    //    public XGraphicsPdfPageOptions PageOptions
    //    {
    //      get { return this.options; }
    //    }

    /// <summary>
    /// Closes the underlying content stream.
    /// </summary>
    void Close()
    {
      if (this.page != null)
      {
        PdfContent content = page.RenderContent;
        content.CreateStream(PdfEncoders.RawEncoding.GetBytes(GetContent()));

        //this.gfx = null;
        this.page.RenderContent.pdfRenderer = null;
        this.page.RenderContent = null;
        this.page = null;
      }
      else if (this.form != null)
      {
        this.form.pdfForm.CreateStream(PdfEncoders.RawEncoding.GetBytes(GetContent()));
        this.form.pdfRenderer = null;
        this.form = null;
      }
      else if (this.contentDictionary != null)
      {
        this.contentDictionary.CreateStream(PdfEncoders.RawEncoding.GetBytes(GetContent()));
        this.contentDictionary = null;
      }
      else
        Debug.Assert(false, "Undefined content target.");
    }

    //    #region Realizing graphical state

    /// <summary>
    /// Initializes the default view transformation, i.e. the transformation from the user page
    /// space to the PDF page space.
    /// </summary>
    public void BeginContent(bool hacks4softmask)
    {
      if (!this.contentInitialized)
      {
        //this.defaultViewMatrix = new XMatrix();  //XMatrix.Identity;
        //// Take TrimBox into account
        double pageHeight = Size.Height;
        //XPoint trimOffset = new XPoint();
        //if (this.page != null && this.page.TrimMargins.AreSet)
        //{
        //  pageHeight += this.page.TrimMargins.Top.Point + this.page.TrimMargins.Bottom.Point;
        //  trimOffset = new XPoint(this.page.TrimMargins.Left.Point, this.page.TrimMargins.Top.Point);
        //}

        //if (this.page != null && this.page.Elements.GetInteger("/Rotate") == 90)  // HACK for InDesign flyer
        //{
        //  defaultViewMatrix.RotatePrepend(90);
        //  defaultViewMatrix.ScalePrepend(1, -1);
        //}
        //else
        //{
        //  // Recall that the value of Height depends on Orientation.

        // Flip page horizontaly and mirror text.
        XMatrix defaultViewMatrix = new XMatrix();
        if (!hacks4softmask)
        {
          defaultViewMatrix.TranslatePrepend(0, pageHeight);
          defaultViewMatrix.ScalePrepend(0.75, -0.75);
        }
        //if (!trimOffset.IsEmpty)
        //{
        //  Debug.Assert(this.gfx.PageUnit == XGraphicsUnit.Point, "With TrimMargins set the page units must be Point. Ohter cases nyi.");
        //  defaultViewMatrix.TranslatePrepend(trimOffset.x, trimOffset.y);
        //}

        // Save initial graphic state
        WriteSaveState("BeginContent", null);
        // Set page transformation
        WriteRenderTransform(defaultViewMatrix);
        this.graphicsState.DefaultPageTransform = defaultViewMatrix;
        MultiplyTransform(defaultViewMatrix);
        if (!hacks4softmask)
          WriteLiteral("-100 Tz\n");
        this.contentInitialized = true;
      }
    }
    bool contentInitialized;

    /// <summary>
    /// Just save current state.
    /// </summary>
    public void BeginContentRaw()
    {
      if (!this.contentInitialized)
      {
        // Save initial graphic state
        WriteSaveState("BeginContent", null);
        this.contentInitialized = true;
      }
    }

    ///// <summary>
    ///// Initializes the default view transformation, i.e. the transformation from the user page
    ///// space to the PDF page space.
    ///// Ends the content stream, i.e. ends the text mode and balances the graphic state stack.
    ///// </summary>
    ///// </summary>
    //public void EndPage()
    //{
    //  if (this.streamMode == StreamMode.Text)
    //  {
    //    WriteLiteral("ET\n");
    //    this.streamMode = StreamMode.Graphic;
    //  }

    //  while (this.graphicsStateStack.Count != 0)
    //    WriteRestoreState("EndPage");
    //}

    public void EndContent()
    {
      if (!this.pageFinished)
      {
        this.pageFinished = true;
        if (this.streamMode == StreamMode.Text)
        {
          WriteLiteral("ET\n");
          this.streamMode = StreamMode.Graphic;
        }
        WriteRestoreState("EndContent", null);
        Debug.Assert(this.graphicsStateStack.Count == 0);
        //while (this.graphicsStateStack.Count != 0)
        //  WriteRestoreState("EndPage");
        Close();
      }
    }
    bool pageFinished;

    internal void WriteMoveStart(Point point)
    {
      WriteLiteral("{0:0.###} {1:0.###} m\n", point.X, point.Y);
      this.currentPoint = point;
    }
    Point currentPoint = new Point();

    /// <summary>
    /// Writes the specified PathGeometry to the content stream.
    /// </summary>
    internal void WriteGeometry(PathGeometry geo)
    {
      BeginGraphic();

      // PathGeometry itself may have its own transform
      if (geo.Transform != null) // also check render mode?
      {
          MultiplyTransform(geo.Transform);
          WriteRenderTransform(geo.Transform);
      }

      foreach (PathFigure figure in geo.Figures)
      {
        PolyLineSegment pseg;
        PolyBezierSegment bseg;
        ArcSegment aseg;
        PolyQuadraticBezierSegment qseg;

        // And now for the most superfluous and unnecessary optimization within the whole PDFsharp library
        if (figure.IsClosed && figure.Segments.Count == 1 && (pseg = figure.Segments[0] as PolyLineSegment) != null && pseg.Points.Count == 3)
        {
          // Identify rectangles
          Point pt0 = figure.StartPoint;
          Point pt1 = pseg.Points[0];
          Point pt2 = pseg.Points[1];
          Point pt3 = pseg.Points[2];
          // This
          //   M16,0 L24,0 24,144 16,144 Z
          // becomes
          //   16 0 m  24 0 l  24 144 l  16 144 l  h
          // but shorter is this
          //   16 0 8 144 re
          if (pt0.X == pt3.X && pt0.Y == pt1.Y && pt1.X == pt2.X && pt2.Y == pt3.Y)
          {
            WriteLiteral("{0:0.###} {1:0.###} {2:0.###} {3:0.###} re \n", pt0.X, pt0.Y, pt2.X - pt0.X, pt2.Y - pt1.Y);
            continue;
          }
        }
        WriteMoveStart(figure.StartPoint);
        foreach (PathSegment seg in figure.Segments)
        {
          if ((pseg = seg as PolyLineSegment) != null)
            WriteSegment(pseg);
          else if ((bseg = seg as PolyBezierSegment) != null)
            WriteSegment(bseg);
          else if ((aseg = seg as ArcSegment) != null)
            WriteSegment(aseg);
          else if ((qseg = seg as PolyQuadraticBezierSegment) != null)
            WriteSegment(qseg);
        }
        if (figure.IsClosed)
          WriteLiteral("h\n"); // Close current figure
      }
    }

    /// <summary>
    /// Writes the specified PolyLineSegment to the content stream.
    /// </summary>
    void WriteSegment(PolyLineSegment seg)
    {
#if DEBUG_
      // WPF uses zero lengh rounded line to draw circles. These circles PDF renders without antializing (Acrobat 8.1)
      // TODO: Bug in Acrobat or rendering issue?
      if (seg.Points.Count == 1)
      {
        if (seg.Points[0] == this.currentPoint)
          seg.Points[0] = new Point(this.currentPoint.X + 10, this.currentPoint.Y);
      }
#endif
      foreach (Point point in seg.Points)
      {
        WriteLiteral("{0:0.###} {1:0.###} l\n", point.X, point.Y);
        this.currentPoint = point;
      }
    }

    /// <summary>
    /// Writes the specified PolyBezierSegment to the content stream.
    /// </summary>
    internal void WriteSegment(PolyBezierSegment seg)
    {
      int count = seg.Points.Count;
      PointStopCollection points = seg.Points;
      for (int idx = 0; idx < count - 2; idx += 3)
      {
        WriteLiteral("{0:0.###} {1:0.###} {2:0.###} {3:0.###} {4:0.###} {5:0.###} c\n",
          points[idx].X, points[idx].Y, points[idx + 1].X, points[idx + 1].Y, points[idx + 2].X, points[idx + 2].Y);
        this.currentPoint = points[idx + 2];
      }
    }

    /// <summary>
    /// Writes the specified PolyQuadraticBezierSegment to the content stream.
    /// </summary>
    internal void WriteSegment(PolyQuadraticBezierSegment seg)
    {
#if true
      if (!DevHelper.FlattenPolyQuadraticBezierSegment)
      {
        int count = seg.Points.Count;
        PointStopCollection points = seg.Points;
        Point pt0 = this.currentPoint;
        for (int idx = 0; idx < count - 1; )
        {
          Point pt1 = points[idx++];
          Point pt2 = points[idx++];
          // Cannot believe it: I just guessed the formula and it works!
          WriteLiteral("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
            pt1.X - (pt1.X - pt0.X) / 3, pt1.Y - (pt1.Y - pt0.Y) / 3,
            pt1.X + (pt2.X - pt1.X) / 3, pt1.Y + (pt2.Y - pt1.Y) / 3,
            pt2.X, pt2.Y);
          this.currentPoint = pt0 = pt2;
        }
      }
      else
      {
        PolyLineSegment lseg = WpfUtils.FlattenSegment(this.currentPoint, seg);
        WriteSegment(lseg);
      }
#else
      // TODO: Convert quadratic Bezier curve in cubic Bézier curve
      PolyLineSegment lseg = WpfUtils.FlattenSegment(this.currentPoint, seg);
      WriteSegment(lseg);
#endif
    }

    /// <summary>
    /// Writes the specified ArcSegment to the content stream.
    /// </summary>
    internal void WriteSegment(ArcSegment seg)
    {
      if (!DevHelper.FlattenArcSegments)
      {
        int pieces;
        System.Windows.Media.PointCollection points =
          GeometryHelper.ArcToBezier(this.currentPoint.X, this.currentPoint.Y, seg.Size.Width, seg.Size.Height, seg.RotationAngle, seg.IsLargeArc,
            seg.SweepDirection == SweepDirection.Clockwise, seg.Point.X, seg.Point.Y, out pieces);
        if (pieces == 0)
        {
          // just draw single line
          WriteLiteral("{0:0.####} {1:0.####} l\n", seg.Point.X, seg.Point.Y);
          this.currentPoint = seg.Point;
          return;
        }
        else if (pieces < 0)
          return;

        int count = points.Count;
        Debug.Assert(count % 3 == 0);
        for (int idx = 0; idx < count - 2; idx += 3)
        {
          WriteLiteral("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} c\n",
            points[idx].X, points[idx].Y, points[idx + 1].X, points[idx + 1].Y, points[idx + 2].X, points[idx + 2].Y);
          this.currentPoint = new Point(points[idx + 2].X, points[idx + 2].Y);
        }
      }
      else
      {
        PolyLineSegment lseg = WpfUtils.FlattenSegment(this.currentPoint, seg);
        WriteSegment(lseg);
      }
    }

    /// <summary>
    /// Writes the path fill and/or stroke operators to the content stream.
    /// </summary>
    internal void WritePathFillStroke(Path path)
    {
      if (path.Data.FillRule == FillRule.NonZero) // NonZero means Winding
      {
        if (path.Fill != null && path.Stroke != null)
          WriteLiteral("B\n");
        else if (path.Stroke != null)
          WriteLiteral("S\n");
        else
          WriteLiteral("f\n");
      }
      else
      {
        if (path.Fill != null && path.Stroke != null)
          WriteLiteral("B*\n");
        else if (path.Stroke != null)
          WriteLiteral("S\n");
        else
          WriteLiteral("f*\n");
      }
    }

    /// <summary>
    /// Begins the graphic mode (i.e. ends the text mode).
    /// </summary>
    internal void BeginGraphic()
    {
      if (this.streamMode != StreamMode.Graphic)
      {
        if (this.streamMode == StreamMode.Text)
          WriteLiteral("ET\n");

        this.streamMode = StreamMode.Graphic;
      }
    }

    /// <summary>
    /// Begins the text mode.
    /// </summary>
    internal void BeginText()
    {
      if (this.streamMode != StreamMode.Text)
      {
        this.streamMode = StreamMode.Text;
        WriteLiteral("BT\n");
        // Text matrix is empty after BT
        this.graphicsState.realizedTextPosition = new XPoint();
      }
    }
    StreamMode streamMode;

    //public void RenderTransform(MatrixTransform transform)
    //{
    //  BeginGraphic();
    //  AppendFormat("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} cm\n",
    //    transform.Matrix.m11, transform.Matrix.m12, transform.Matrix.m21, transform.Matrix.m22, transform.Matrix.offsetX, transform.Matrix.offsetY);
    //}

    /// <summary>
    /// Writes the specified transformation matrix to the content stream.
    /// </summary>
    public void WriteRenderTransform(XMatrix matrix)
    {
      BeginGraphic();
      WriteLiteral("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} cm\n",
        matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.OffsetX, matrix.OffsetY);
      this.graphicsState.currentTransform.Prepend(matrix);
    }

    /// <summary>
    /// Writes the specified text transformation matrix to the content stream.
    /// </summary>
    public void WriteTextTransform(XMatrix matrix)
    {
      Debug.Assert(this.streamMode == StreamMode.Text, "Must be in text mode when setting text matrix.");
      WriteLiteral("{0:0.####} {1:0.####} {2:0.####} {3:0.####} {4:0.####} {5:0.####} Tm\n",
        matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.OffsetX, matrix.OffsetY);
    }

    /// <summary>
    /// Writes the specified rectangle as intersection with the current clip path to the content stream.
    /// </summary>
    public void WriteClip(XRect rect)
    {
      BeginGraphic();
      WriteLiteral("{0:0.####} {1:0.####} {2:0.####} {3:0.####} re W n",
        rect.x, rect.y, rect.x + rect.width, rect.y + rect.height);
    }

    /// <summary>
    /// Writes the specified PathGeometry as intersection with the current clip path to the content stream.
    /// </summary>
    public void WriteClip(PathGeometry geo)
    {
      BeginGraphic();

      WriteGeometry(geo);

      if (geo.FillRule == FillRule.NonZero)
        WriteLiteral("W n\n");
      else
        WriteLiteral("W* n\n");
    }

    public void WriteOpacityMask(Brush brush)
    {
      // TODO
    }

    /// <summary>
    /// Gets the default page transformation.
    /// </summary>
    public XMatrix DefaultPageTransform
    {
      get { return this.graphicsState.DefaultPageTransform; }
    }

    /// <summary>
    /// Gets the current transformation.
    /// </summary>
    public XMatrix Transform
    {
      get { return this.graphicsState.Transform; }
    }

    /// <summary>
    /// Muliplies the spcified transformation with the current transformation and returns the new value;
    /// </summary>
    public XMatrix MultiplyTransform(XMatrix matrix)
    {
      return this.graphicsState.MultiplyTransform(matrix);
    }

    /// <summary>
    /// Gets the current opacity value.
    /// </summary>
    public double Opacity
    {
      get { return this.graphicsState.Opacity; }
    }

    /// <summary>
    /// Muliplies the spcified opacity with the current opacity and returns the new value;
    /// </summary>
    public double MultiplyOpacity(double opacity)
    {
      return this.graphicsState.MuliplyOpacity(opacity);
    }

    ///// <summary>
    ///// Makes the specified pen and brush to the current graphics objects.
    ///// </summary>
    //void Realize(Pen pen, XBrush brush)
    //{
    //  BeginPage();
    //  BeginGraphic();
    //  RealizeTransform();

    //  if (pen != null)
    //    this.gfxState.RealizePen(pen, this.colorMode); // this.page.document.Options.ColorMode);

    //  if (brush != null)
    //    this.gfxState.RealizeBrush(brush, this.colorMode); // this.page.document.Options.ColorMode);
    //}

    /// <summary>
    /// Makes the specified brush to the current graphics object.
    /// </summary>
    public void RealizeFill(Brush brush, ref XForm xform, ref XImage ximage)
    {
      this.graphicsState.RealizeFill(brush, 1, ref xform, ref ximage);
    }

    /// <summary>
    /// Realizes the opacity for non-stroke operations.
    /// </summary>
    public void RealizeFillOpacity(double opacity)
    {
      this.graphicsState.RealizeFillOpacity(opacity);
    }

    /// <summary>
    /// Makes the specified pen to the current graphics object.
    /// </summary>
    public void RealizeStroke(Path path)
    {
      this.graphicsState.RealizeStroke(path);
    }

    /// <summary>
    /// Realizes the opacity for stroke operations.
    /// </summary>
    public void RealizeStrokeOpacity(double opacity)
    {
      this.graphicsState.RealizeStrokeOpacity(opacity);
    }

    /// <summary>
    /// Realizes the specified graphic state.
    /// </summary>
    public void RealizeExtGState(PdfExtGState xgState)
    {
      this.graphicsState.RealizeExtGState(xgState);
    }

    /// <summary>
    /// Makes the specified font and brush to the current graphics objects.
    /// </summary>
    void RealizeFont(Glyphs glyphs)
    {
      if (this.streamMode != StreamMode.Text)
      {
        this.streamMode = StreamMode.Text;
        WriteLiteral("BT\n");
        // Text matrix is empty after BT
        this.graphicsState.realizedTextPosition = new XPoint();
      }
      this.graphicsState.RealizeFont(glyphs);
    }

    void AdjustTextMatrix(ref XPoint pos)
    {
      XPoint posSave = pos;
      pos = pos - new XVector(this.graphicsState.realizedTextPosition.x, this.graphicsState.realizedTextPosition.y);
      this.graphicsState.realizedTextPosition = posSave;
    }

    //    /// <summary>
    //    /// Makes the specified image to the current graphics object.
    //    /// </summary>
    //    string Realize(XImage image)
    //    {
    //      BeginPage();
    //      BeginGraphic();
    //      RealizeTransform();

    //      string imageName;
    //      if (image is XForm)
    //        imageName = GetFormName(image as XForm);
    //      else
    //        imageName = GetImageName(image);
    //      return imageName;
    //    }

    //    /// <summary>
    //    /// Realizes the current transformation matrix, if necessary.
    //    /// </summary>
    //    void RealizeTransform()
    //    {
    //      BeginPage();

    //      if (this.gfxState.Level == GraphicsStackLevelPageSpace)
    //      {
    //        BeginGraphic();
    //        SaveState();
    //      }

    //      if (gfxState.MustRealizeCtm)
    //      {
    //        BeginGraphic();
    //        this.gfxState.RealizeCtm();
    //      }
    //    }

    //    #endregion


    /// <summary>
    /// Gets the owning PdfDocument of this page or form.
    /// </summary>
    internal PdfDocument Owner
    {
      get
      {
        if (this.page != null)
          return this.page.Owner;
        else if (this.form != null)
          return this.form.Owner;
        else if (this.contentDictionary != null)
          return this.contentDictionary.Owner;

        Debug.Assert(false, "Undefined conent target.");
        return null;
      }
    }

    /// <summary>
    /// Gets the PdfResources of this page or form.
    /// </summary>
    internal PdfResources Resources
    {
      get
      {
        if (this.page != null)
          return this.page.Resources;
        else if (this.form != null)
          return this.form.Resources;
        else if (this.contentStreamDictionary != null)
          return this.contentStreamDictionary.Resources;

        Debug.Assert(false, "Undefined conent target.");
        return null;
      }
    }

    /// <summary>
    /// Gets the size of this page or form.
    /// </summary>
    internal XSize Size
    {
      get
      {
        if (this.page != null)
          return new XSize(this.page.Width, this.page.Height);
        else if (this.form != null)
          return this.form.Size;
        else if (this.contentDictionary != null)
        {
          throw new NotImplementedException("Size");
        }


        Debug.Assert(false, "Undefined conent target.");
        return new XSize();
      }
    }

    /// <summary>
    /// Gets the resource name of the specified font within this page or form.
    /// </summary>
    internal string GetFontName(XFont font, out PdfFont pdfFont)
    {
      if (this.page != null)
        return this.page.GetFontName(font, out pdfFont);
      else if (this.form != null)
        return this.form.GetFontName(font, out pdfFont);
      else if (this.contentDictionary != null)
      {
        throw new NotImplementedException("GetFontName");
      }

      Debug.Assert(false, "Undefined conent target.");
      pdfFont = null;
      return null;
    }

    /// <summary>
    /// Tries to get the resource name of the specified font within this page or form.
    /// Returns null if no such font exists.
    /// </summary>
    internal string TryGetFontName(string idName, out PdfFont pdfFont)
    {
      if (this.page != null)
        return this.page.TryGetFontName(idName, out pdfFont);
      else if (this.form != null)
        return this.form.TryGetFontName(idName, out pdfFont);
      else if (this.contentDictionary != null)
      {
        throw new NotImplementedException("GetFontName");
      }

      Debug.Assert(false, "Undefined conent target.");
      pdfFont = null;
      return null;
    }

    /// <summary>
    /// Gets the resource name of the specified font within this page or form.
    /// </summary>
    internal string GetFontName(string idName, byte[] fontData, out PdfFont pdfFont)
    {
      if (this.page != null)
        return this.page.GetFontName(idName, fontData, out pdfFont);
      else if (this.form != null)
        return this.form.GetFontName(idName, fontData, out pdfFont);
      else if (this.contentDictionary != null)
      {
        throw new NotImplementedException("GetFontName");
      }

      Debug.Assert(false, "Undefined conent target.");
      pdfFont = null;
      return null;
    }

    /// <summary>
    /// Gets the resource name of the specified font within this page or form.
    /// </summary>
    internal string GetFontName(Font font)
    {
      PdfFont pdfFont;
      string name = null;
      if (this.page != null)
        name = this.page.GetFontName(font.Name, font.FontData, out pdfFont);
      else if (this.form != null)
        name = this.form.GetFontName(font.Name, font.FontData, out pdfFont);
      else if (this.contentDictionary != null)
      {
        Debug.Assert(this.contentStreamDictionary != null);
        name = this.contentStreamDictionary.GetFontName(font.Name, font.FontData, out pdfFont);
      }
      else
      {
        Debug.Assert(false, "Undefined conent target.");
        pdfFont = null;  // supress compiler warning
      }

      Debug.Assert(font.PdfFont == null || Object.ReferenceEquals(font.PdfFont, pdfFont));
      if (font.PdfFont == null)
        font.PdfFont = pdfFont;
      return name;
    }

    //    /// <summary>
    //    /// Gets the resource name of the specified image within this page or form.
    //    /// </summary>
    //    internal string GetImageName(XImage image)
    //    {
    //      if (this.page != null)
    //        return this.page.GetImageName(image);
    //      else
    //        return this.form.GetImageName(image);
    //    }

    //    /// <summary>
    //    /// Gets the resource name of the specified form within this page or form.
    //    /// </summary>
    //    internal string GetFormName(XForm form)
    //    {
    //      if (this.page != null)
    //        return this.page.GetFormName(form);
    //      else
    //        return this.form.GetFormName(form);
    //    }

    //    internal PdfPage page;
    //    internal XForm form;
    //    internal PdfColorMode colorMode;
    //    XGraphicsPdfPageOptions options;



    ///// <summary>
    ///// Writes a comment to the content stream.
    ///// </summary>
    //public void WriteComment(string message)
    //{
    //  if (!String.IsNullOrEmpty(message) && this.traceLevel != PdfTraceLevel.None)
    //    Append("q % " + traceMessage + "\n");
    //}


    /// <summary>
    /// Saves the current graphical state and writes a push state operator to the content stream.
    /// </summary>
    public void WriteSaveState(string traceMessage, string elementName)
    {
      //Debug.Assert(this.streamMode == StreamMode.Graphic, "Cannot restore state in text mode.");

      if (traceMessage == null || this.traceLevel == PdfTraceLevel.None)
        WriteLiteral("q\n");
      else
      {
        if (!String.IsNullOrEmpty(elementName))
          traceMessage = traceMessage + ": '" + elementName + "'";
        WriteLiteral("q % -- " + traceMessage + "\n");
      }
      this.graphicsStateStack.Push(this.graphicsState);
      this.graphicsState = this.graphicsState.Clone();
      this.graphicsState.Level = this.graphicsStateStack.Count;
    }

    /// <summary>
    /// Restores the previous graphical state and writes a pop state operator to the content stream.
    /// </summary>
    public void WriteRestoreState(string traceMessage, string elementName)
    {
      //Debug.Assert(this.streamMode == StreamMode.Graphic, "Cannot restore state in text mode.");
      BeginGraphic();
      this.graphicsState = (PdfGraphicsState)this.graphicsStateStack.Pop();

      if (traceMessage == null || this.traceLevel == PdfTraceLevel.None)
        WriteLiteral("Q\n");
      else
      {
        if (!String.IsNullOrEmpty(elementName))
          traceMessage = traceMessage + ": '" + elementName + "'";
        WriteLiteral("Q % -- " + traceMessage + "\n");
      }
    }

    /// <summary>
    /// The current graphics state of the PDF content.
    /// </summary>
    PdfGraphicsState graphicsState;

    /// <summary>
    /// The graphical state stack.
    /// </summary>
    Stack<PdfGraphicsState> graphicsStateStack = new Stack<PdfGraphicsState>();
  }
}
