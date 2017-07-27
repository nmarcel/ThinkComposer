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
using IOPath = System.IO.Path;

#pragma warning disable 414, 169 // incomplete code state

namespace PdfSharp.Xps.Rendering
{
    internal sealed partial class PdfGraphicsState : ICloneable
    {
        public PdfGraphicsState(PdfContentWriter writer)
        {
            this.writer = writer;
        }
        PdfContentWriter writer;

        internal XMatrix currentTransform = new XMatrix();

        public PdfGraphicsState Clone()
        {
            PdfGraphicsState state = (PdfGraphicsState)MemberwiseClone();
            return state;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        internal int Level;

        //internal InternalGraphicsState InternalState;

        //public void PushState()
        //{
        //  //BeginGraphic
        //  this.writer.Append("q/n");
        //}

        //public void PopState()
        //{
        //  //BeginGraphic
        //  this.writer.Append("Q/n");
        //}

        public XMatrix DefaultPageTransform
        {
            get { return this.defaultPageTransform; }
            set { this.defaultPageTransform = value; }
        }
        XMatrix defaultPageTransform = new XMatrix();

        public XMatrix Transform
        {
            get { return this.transform; }
        }
        XMatrix transform = new XMatrix();

        public XMatrix MultiplyTransform(XMatrix matrix)
        {
            this.transform.Prepend(matrix);
            return this.transform;
        }

        /// <summary>
        /// Gets the current opacity value.
        /// </summary>
        public double Opacity
        {
            get { return this.opacity; }
        }
        double opacity = 1;

        /// <summary>
        /// Muliplies the spcified opacity with the current opacity and returns the new value;
        /// </summary>
        public double MuliplyOpacity(double opacity)
        {
            if (opacity < 0 || opacity > 1)
                throw new ArgumentException("opacity");

            this.opacity *= opacity;
            return this.opacity;
        }

        #region Stroke

        double realizedLineWith = -1;
        int realizedLineCap = -1;
        int realizedLineJoin = -1;
        double realizedMiterLimit = -1;
        XDashStyle realizedDashStyle = (XDashStyle)(-1);
        string realizedDashPattern;
        Color realizedStrokeColor = new Color();

        public void RealizeStroke(Path path) // XPen pen, PdfColorMode colorMode)
        {
            //XColor color = pen.Color;
            //color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

            Brush brush = path.Stroke;
            SolidColorBrush sBrush;
            if ((sBrush = brush as SolidColorBrush) != null)
            {

                var thickness = path.StrokeThickness;

                // Adjust based on path geometry transform since XPS doesn't apply scale to stroke thickness
                if (path.Data.Transform != null)
                {
                    var actualThickness = thickness * ((path.Data.Transform.Matrix.m11 + path.Data.Transform.Matrix.m12) + (path.Data.Transform.Matrix.m21 + path.Data.Transform.Matrix.m22)) / 2;
                    /*
                     * actual     requir
                     * ------  =  ------
                     * requir     necess
                     */
                    thickness = thickness * thickness / actualThickness;
                }

                //if (this.realizedLineWith != width)
                {
                    this.writer.WriteLiteral("{0:0.###} w\n", thickness);
                    this.realizedLineWith = thickness;
                }

                RealizeStrokeStyle(path);

                Color color = sBrush.Color;

                double opacity = path.Opacity * color.ScA;
                if (opacity < 1)
                {
                    PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateStroke(color.ScA);
                    string gs = this.writer.Resources.AddExtGState(extGState);
                    this.writer.WriteLiteral("{0} gs\n", gs);

                    // Must create transparany group
                    //if (color.ScA < 1)
                    this.writer.CreateDefaultTransparencyGroup();
                }
                this.writer.WriteRgb(color, " RG\n");

                this.realizedStrokeColor = color;
            }
            else
            {
                Debug.WriteLine("Stroke with non SolidColorBrush");
                //throw new NotImplementedException("Stroke with non SolidColorBrush");
            }




            //if (this.realizedLineCap != (int)pen.lineCap)
            //{
            //  this.renderer.AppendFormat("{0} J\n", (int)pen.lineCap);
            //  this.realizedLineCap = (int)pen.lineCap;
            //}

            //if (this.realizedLineJoin != (int)pen.lineJoin)
            //{
            //  this.renderer.AppendFormat("{0} j\n", (int)pen.lineJoin);
            //  this.realizedLineJoin = (int)pen.lineJoin;
            //}

            //if (this.realizedLineCap == (int)XLineJoin.Miter)
            //{
            //  if (this.realizedMiterLimit != (int)pen.miterLimit && (int)pen.miterLimit != 0)
            //  {
            //    this.renderer.AppendFormat("{0} M\n", (int)pen.miterLimit);
            //    this.realizedMiterLimit = (int)pen.miterLimit;
            //  }
            //}

            //if (this.realizedDashStyle != pen.dashStyle || pen.dashStyle == XDashStyle.Custom)
            //{
            //  double dot = pen.Width;
            //  double dash = 3 * dot;

            //  // Line width 0 is not recommended but valid
            //  XDashStyle dashStyle = pen.DashStyle;
            //  if (dot == 0)
            //    dashStyle = XDashStyle.Solid;

            //  switch (dashStyle)
            //  {
            //    case XDashStyle.Solid:
            //      this.renderer.Append("[]0 d\n");
            //      break;

            //    case XDashStyle.Dash:
            //      this.renderer.AppendFormat("[{0:0.##} {1:0.##}]0 d\n", dash, dot);
            //      break;

            //    case XDashStyle.Dot:
            //      this.renderer.AppendFormat("[{0:0.##}]0 d\n", dot);
            //      break;

            //    case XDashStyle.DashDot:
            //      this.renderer.AppendFormat("[{0:0.##} {1:0.##} {1:0.##} {1:0.##}]0 d\n", dash, dot);
            //      break;

            //    case XDashStyle.DashDotDot:
            //      this.renderer.AppendFormat("[{0:0.##} {1:0.##} {1:0.##} {1:0.##} {1:0.##} {1:0.##}]0 d\n", dash, dot);
            //      break;

            //    case XDashStyle.Custom:
            //      {
            //        StringBuilder pdf = new StringBuilder("[", 256);
            //        int len = pen.dashPattern == null ? 0 : pen.dashPattern.Length;
            //        for (int idx = 0; idx < len; idx++)
            //        {
            //          if (idx > 0)
            //            pdf.Append(' ');
            //          pdf.Append(PdfEncoders.ToString(pen.dashPattern[idx] * pen.width));
            //        }
            //        // Make an even number of values look like in GDI+
            //        if (len > 0 && len % 2 == 1)
            //        {
            //          pdf.Append(' ');
            //          pdf.Append(PdfEncoders.ToString(0.2 * pen.width));
            //        }
            //        pdf.AppendFormat(CultureInfo.InvariantCulture, "]{0:0.###} d\n", pen.dashOffset * pen.width);
            //        string pattern = pdf.ToString();

            //        // BUG: drice2@ageone.de reported a realizing problem
            //        // HACK: I romove the if clause
            //        //if (this.realizedDashPattern != pattern)
            //        {
            //          this.realizedDashPattern = pattern;
            //          this.renderer.Append(pattern);
            //        }
            //      }
            //      break;
            //  }
            //  this.realizedDashStyle = dashStyle;
            //}

            //if (colorMode != PdfColorMode.Cmyk)
            //{
            //if (this.realizedStrokeColor.Rgb != color.Rgb)
            //{
            //  this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
            //  this.renderer.Append(" RG\n");
            //}
            //}
            //else
            //{
            //  if (!ColorSpaceHelper.IsEqualCmyk(this.realizedStrokeColor, color))
            //  {
            //    this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
            //    this.renderer.Append(" K\n");
            //  }
            //}

            //if (this.renderer.Owner.Version >= 14 && this.realizedStrokeColor.A != color.A)
            //{
            //  PdfExtGState extGState = this.renderer.Owner.ExtGStateTable.GetExtGStateStroke(color.A);
            //  string gs = this.renderer.Resources.AddExtGState(extGState);
            //  this.renderer.AppendFormat("{0} gs\n", gs);

            //  // Must create transparany group
            //  if (this.renderer.page != null && color.A < 1)
            //    this.renderer.page.transparencyUsed = true;
            //}
            //this.realizedStrokeColor = color;
        }

        void RealizeStrokeStyle(Path path)
        {
            if (this.realizedLineCap != (int)path.StrokeStartLineCap)
            {
                // HACK: Set Triangle to Square
                int[] pdfValue = { 0, 1, 2, 2 };  // Flat, Round, Square, Triangle,
                //int[] pdfValue = { 1, 1, 1, 1 };  // Flat, Round, Square, Triangle,

                int value = pdfValue[(int)path.StrokeStartLineCap];
                this.writer.WriteLiteral("{0} J\n", value);
                this.realizedLineCap = value;
            }

            if (this.realizedLineJoin != (int)path.StrokeLineJoin)
            {
                int[] pdfValue = { 0, 2, 1 };  // Miter, Bevel, Round

                int value = pdfValue[(int)path.StrokeLineJoin];
                this.writer.WriteLiteral("{0} j\n", value);
                this.realizedLineJoin = value;

                // TODO: Check implementation in PDFsharp PDF renderer!
                if (path.StrokeLineJoin == LineJoin.Miter)
                {
                    if (this.realizedMiterLimit != path.StrokeMiterLimit && path.StrokeMiterLimit != 0)
                    {
                        this.writer.WriteLiteral("{0:0.##} M\n", path.StrokeMiterLimit);
                        this.realizedMiterLimit = path.StrokeMiterLimit;
                    }
                }
            }

            if (!String.IsNullOrEmpty(path.StrokeDashArray))
            {
                string[] values = path.StrokeDashArray.Split(new char[] { ' ' });
                int count = values.Length;
                double[] dashArray = new double[count];
                for (int idx = 0; idx < count; idx++)
                    Double.TryParse(values[idx], NumberStyles.Float, CultureInfo.InvariantCulture, out dashArray[idx]);


                // Line width 0 is not recommended but valid
                //XDashStyle dashStyle = pen.DashStyle;
                //if (dot == 0)
                //  dashStyle = XDashStyle.Solid;

                double dot = path.StrokeThickness;
                double dash = 3 * path.StrokeThickness;
                if (dot > 0)
                {
#if old
          //switch (dashStyle)
          //{
          //  case XDashStyle.Solid:
          //    this.renderer.Append("[]0 d\n");
          //    break;

          //  case XDashStyle.Dash:
          //    this.renderer.AppendFormat("[{0:0.##} {1:0.##}]0 d\n", dash, dot);
          //    break;

          //  case XDashStyle.Dot:
          //    this.renderer.AppendFormat("[{0:0.##}]0 d\n", dot);
          //    break;

          //  case XDashStyle.DashDot:
          //    this.renderer.AppendFormat("[{0:0.##} {1:0.##} {1:0.##} {1:0.##}]0 d\n", dash, dot);
          //    break;

          //  case XDashStyle.DashDotDot:
          //    this.renderer.AppendFormat("[{0:0.##} {1:0.##} {1:0.##} {1:0.##} {1:0.##} {1:0.##}]0 d\n", dash, dot);
          //    break;

          //  case XDashStyle.Custom:
          //    {
          //      StringBuilder pdf = new StringBuilder("[", 256);
          //      int len = pen.dashPattern == null ? 0 : pen.dashPattern.Length;
          //      for (int idx = 0; idx < len; idx++)
          //      {
          //        if (idx > 0)
          //          pdf.Append(' ');
          //        pdf.Append(PdfEncoders.ToString(pen.dashPattern[idx] * pen.width));
          //      }
          //      // Make an even number of values look like in GDI+
          //      if (len > 0 && len % 2 == 1)
          //      {
          //        pdf.Append(' ');
          //        pdf.Append(PdfEncoders.ToString(0.2 * pen.width));
          //      }
          //      pdf.AppendFormat(CultureInfo.InvariantCulture, "]{0:0.###} d\n", pen.dashOffset * pen.width);
          //      string pattern = pdf.ToString();

          //      // BUG: drice2@ageone.de reported a realizing problem
          //      // HACK: I romove the if clause
          //      //if (this.realizedDashPattern != pattern)
          //      {
          //        this.realizedDashPattern = pattern;
          //        this.renderer.Append(pattern);
          //      }
          //    }
          //    break;
          //}
          //this.realizedDashStyle = dashStyle;
#endif
                    StringBuilder pdf = new StringBuilder("[", 256);
                    for (int idx = 0; idx < count; idx++)
                    {
                        if (idx > 0)
                            pdf.Append(' ');
                        pdf.Append(PdfEncoders.ToString(dashArray[idx] * path.StrokeThickness));
                    }

                    // TODO: ??
                    // Make an even number of values look like in GDI+  
                    if (count > 0 && count % 2 == 1)
                    {
                        pdf.Append(' ');
                        pdf.Append(PdfEncoders.ToString(0.2 * path.StrokeThickness));
                    }
                    pdf.AppendFormat(CultureInfo.InvariantCulture, "]{0:0.###} d\n", path.StrokeDashOffset * path.StrokeThickness);
                    string pattern = pdf.ToString();

                    //// BUG: drice2@ageone.de reported a realizing problem
                    //// HACK: I romove the if clause
                    ////if (this.realizedDashPattern != pattern)
                    //{
                    //  this.realizedDashPattern = pattern;
                    this.writer.WriteLiteral(pattern);
                    //}
                }
            }
        }

        /// <summary>
        /// Realizes the opacity for stroke operations.
        /// </summary>
        public void RealizeStrokeOpacity(double opacity)
        {
            PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateStroke(opacity);
            string gsName = this.writer.Resources.AddExtGState(extGState);
            this.writer.WriteLiteral(gsName + " gs\n");
        }

        /// <summary>
        /// Realizes the specified graphic state.
        /// </summary>
        public void RealizeExtGState(PdfExtGState xgState)
        {
            string gsName = this.writer.Resources.AddExtGState(xgState);
            this.writer.WriteLiteral(gsName + " gs\n");
        }

        #endregion

        #region Fill

        Color realizedFillColor = new Color();

        public void RealizeFill(Brush brush, double opacity, ref XForm xform, ref XImage ximage) // PdfColorMode colorMode)
        {
            SolidColorBrush sbrush;
            LinearGradientBrush lbrush;
            RadialGradientBrush rbrush;
            ImageBrush ibrush;
            VisualBrush vbrush;
            if ((sbrush = brush as SolidColorBrush) != null)
            {
                Color color = sbrush.Color;
                //color = ColorSpaceHelper.EnsureColorMode(colorMode, color);

                this.writer.WriteRgb(color, " rg\n");

                //if (this.renderer.Owner.Version >= 14 && this.realizedStrokeColor.A != color.A)
                //if (this.realizedFillColor.ScA != color.ScA)
                {
                    PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(color.ScA);
                    string gs = this.writer.Resources.AddExtGState(extGState);
                    this.writer.WriteLiteral("{0} gs\n", gs);

                    // Must create transparany group
                    if (color.ScA < 1)
                        this.writer.CreateDefaultTransparencyGroup();
                }
                this.realizedFillColor = color;

                //if (colorMode != PdfColorMode.Cmyk)
                //{
                //  if (this.realizedFillColor.Rgb != color.Rgb)
                //  {
                //    this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Rgb));
                //    this.renderer.Append(" rg\n");
                //  }
                //}
                //else
                //{
                //  if (!ColorSpaceHelper.IsEqualCmyk(this.realizedFillColor, color))
                //  {
                //    this.renderer.Append(PdfEncoders.ToString(color, PdfColorMode.Cmyk));
                //    this.renderer.Append(" k\n");
                //  }
                //}

                //if (this.renderer.Owner.Version >= 14 && this.realizedFillColor.A != color.A)
                //{
                //  PdfExtGState extGState = this.renderer.Owner.ExtGStateTable.GetExtGStateNonStroke(color.A);
                //  string gs = this.renderer.Resources.AddExtGState(extGState);
                //  this.renderer.AppendFormat("{0} gs\n", gs);

                //  // Must create transparany group
                //  if (this.renderer.page != null && color.A < 1)
                //    this.renderer.page.transparencyUsed = true;
                //}
                //this.realizedFillColor = color;
            }
            else if ((lbrush = brush as LinearGradientBrush) != null)
            {
                // NOT IN USE ANYMORE
                //RealizeLinearGradientBrush(lbrush, xform);
            }
            else if ((rbrush = brush as RadialGradientBrush) != null)
            {
                // NOT IN USE ANYMORE
                //RealizeRadialGradientBrush(rbrush, xform);
            }
            else if ((ibrush = brush as ImageBrush) != null)
            {
                // NOT IN USE ANYMORE
                //RealizeImageBrush(ibrush, ref xform, ref ximage);
            }
            else if ((vbrush = brush as VisualBrush) != null)
            {
                // NOT IN USE ANYMORE
                //RealizeVisualBrush(vbrush, ref xform);
            }
            else
            {
                //return new SolidColorBrush(Colors//
                //Debugger.Break();
            }
        }

        /// <summary>
        /// Realizes the opacity for non-stroke operations.
        /// </summary>
        public void RealizeFillOpacity(double opacity)
        {
            PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(opacity);
            string gsName = this.writer.Resources.AddExtGState(extGState);
            this.writer.WriteLiteral(gsName + " gs\n");
        }

        //void RealizeLinearGradientBrush(LinearGradientBrush brush)
        //{
        //  // HACK
        //  int count = brush.GradientStops.Count;
        //  int idx = count / 2;
        //  RealizeFillHack(brush.GradientStops[idx].Color);
        //  //Debugger.Break();
        //}

        //void RealizeRadialGradientBrush(RadialGradientBrush brush)
        //{
        //  int count = brush.GradientStops.Count;
        //  int idx = count / 2;
        //  RealizeFillHack(brush.GradientStops[idx].Color);
        //}
        #endregion

        /// <summary>
        /// Helper for not yet implemented brush types.
        /// </summary>
        void RealizeFillHack(Color color)
        {
            this.writer.WriteRgb(color, " rg\n");

            //if (this.realizedFillColor.ScA != color.ScA)
            {
                PdfExtGState extGState = this.writer.Owner.ExtGStateTable.GetExtGStateNonStroke(color.ScA);
                string gs = this.writer.Resources.AddExtGState(extGState);
                this.writer.WriteLiteral("{0} gs\n", gs);

                // Must create transparany group
                if (color.ScA < 1)
                    this.writer.CreateDefaultTransparencyGroup();
            }
            this.realizedFillColor = color;
        }
        //    #region Text

        internal PdfFont realizedFont;
        //    string realizedFontName = String.Empty;
        //    double realizedFontSize = 0;

        public void RealizeFont(Glyphs glyphs)
        {
            // So far rendering mode 0 only
            //RealizeBrush(brush, this.renderer.colorMode); // this.renderer.page.document.Options.ColorMode);

            try
            {
                // Get fixed payload which contains the font manager
                FixedPage fpage = glyphs.GetParent<FixedPage>();
                if (fpage == null)
                {
                    GetType();
                    this.realizedFont = new PdfFont(null);
                }
                FixedPayload payload = fpage.Document.Payload;

                // Get the font object.
                // Important: font.PdfFont is not yet defined here on the first call
                string uriString = glyphs.FontUri;
                Font font = payload.GetFont(uriString);

                // Get the page local resource name and define font.PdfFont if it is not yet defined
                string fontName = writer.GetFontName(font);
                this.realizedFont = font.PdfFont;

                //if (fontName != this.realizedFontName || this.realizedFontSize != font.Size)
                {
                    //this.writer.WriteLiteral("{0} {1:0.###} Tf\n", fontName, -glyphs.FontRenderingEmSize);
                    this.writer.WriteLiteral("{0} {1:0.###} Tf\n", fontName, -glyphs.FontRenderingEmSize);

                    //this.realizedFontName = fontName;
                    //this.realizedFontSize = font.Size;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public XPoint realizedTextPosition = new XPoint();

        //    #endregion

        //    #region Transformation

        //    /// <summary>
        //    /// The realized current transformation matrix.
        //    /// </summary>
        //    XMatrix realizedCtm = new XMatrix();  //XMatrix.Identity;

        //    /// <summary>
        //    /// The unrealized current transformation matrix.
        //    /// </summary>
        //    XMatrix unrealizedCtm = new XMatrix();  //XMatrix.Identity;

        //    /// <summary>
        //    /// A flag indicating whether the CTM must be realized.
        //    /// </summary>
        //    public bool MustRealizeCtm;

        //    public XMatrix Transform
        //    {
        //      get
        //      {
        //        if (this.MustRealizeCtm)
        //        {
        //          XMatrix matrix = this.realizedCtm;
        //          matrix.MultiplyPrepend(this.unrealizedCtm);
        //          return matrix;
        //        }
        //        return this.realizedCtm;
        //      }
        //      set
        //      {
        //        XMatrix matrix = this.realizedCtm;
        //        matrix.Invert();
        //        matrix.MultiplyPrepend(value);
        //        this.unrealizedCtm = matrix;
        //        this.MustRealizeCtm = !this.unrealizedCtm.IsIdentity;
        //      }
        //    }

        //    /// <summary>
        //    /// Modifies the current transformation matrix.
        //    /// </summary>
        //    public void MultiplyTransform(XMatrix matrix, XMatrixOrder order)
        //    {
        //      if (!matrix.IsIdentity)
        //      {
        //        this.MustRealizeCtm = true;
        //        this.unrealizedCtm.Multiply(matrix, order);
        //      }
        //    }

        //    /// <summary>
        //    /// Realizes the CTM.
        //    /// </summary>
        //    public void RealizeCtm()
        //    {
        //      if (this.MustRealizeCtm)
        //      {
        //        Debug.Assert(!this.unrealizedCtm.IsIdentity, "mrCtm is unnecessarily set.");

        //        double[] matrix = this.unrealizedCtm.Elements;
        //        // Up to six decimal digits to prevent round up problems
        //        this.renderer.AppendFormat("{0:0.######} {1:0.######} {2:0.######} {3:0.######} {4:0.######} {5:0.######} cm\n",
        //          matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5]);

        //        this.realizedCtm.MultiplyPrepend(this.unrealizedCtm);
        //        this.unrealizedCtm = new XMatrix();  //XMatrix.Identity;
        //        this.MustRealizeCtm = false;
        //      }
        //    }

        //    #endregion

        //    #region Clip Path

        //    public void SetAndRealizeClipRect(XRect clipRect)
        //    {
        //      XGraphicsPath clipPath = new XGraphicsPath();
        //      clipPath.AddRectangle(clipRect);
        //      RealizeClipPath(clipPath);
        //    }

        //    public void SetAndRealizeClipPath(XGraphicsPath clipPath)
        //    {
        //      RealizeClipPath(clipPath);
        //    }

        //    void RealizeClipPath(XGraphicsPath clipPath)
        //    {
        //      this.renderer.BeginGraphic();
        //      RealizeCtm();
        //#if GDI
        //      this.renderer.AppendPath(clipPath.gdipPath);
        //#endif
        //#if WPF
        //      this.renderer.AppendPath(clipPath.pathGeometry);
        //#endif
        //      if (clipPath.FillMode == XFillMode.Winding)
        //        this.renderer.Append("W n\n");
        //      else
        //        this.renderer.Append("W* n\n");
        //    }

        //    #endregion
    }
}