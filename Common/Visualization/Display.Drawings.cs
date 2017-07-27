// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : Display.Drawings.cs
// Object : Instrumind.Common.Visualization.Display (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.01 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.IO.Packaging;
using System.Printing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

using Instrumind.Common;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization.Widgets;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Provides common services for working with WPF. Drawings part.
    /// </summary>
    public static partial class Display
    {
        /// <summary>
        /// Default approximate character pixel width (assuming an standard little font).
        /// </summary>
        public const double CHAR_PXWIDTH_DEFAULT = 7;

        /// <summary>
        /// Default approximate character pixel height (assuming an standard little font).
        /// </summary>
        public const double CHAR_PXHEIGHT_DEFAULT = 8;

        /// <summary>
        /// Standard size for little (16x16) icons.
        /// </summary>
        public const double ICOSIZE_LIT = 16.0;

        /// <summary>
        /// Standard size for small (24x24) icons.
        /// </summary>
        public const double ICOSIZE_SMA = 24.0;

        /// <summary>
        /// Standard size for medium (32x32) icons.
        /// </summary>
        public const double ICOSIZE_MED = 32.0;

        /// <summary>
        /// Standard size for big (48x48) icons.
        /// </summary>
        public const double ICOSIZE_BIG = 48.0;

        /// <summary>
        /// Defines a coordinate tolerance for considering a point near another.
        /// </summary>
        public const double POINT_NEAR_TOLERANCE = 1.0;

        /// <summary>
        /// Defines a hue tolerance (0 to 360) for considering a color different from another.
        /// </summary>
        public const double COLOR_HUE_DIFF_TOLERANCE = 20.0;

        /// <summary>
        /// Defines a stauration tolerance (0 to 1) for considering a color different from another.
        /// </summary>
        public const double COLOR_SAT_DIFF_TOLERANCE = 0.5;

        /// <summary>
        /// Defines a brightness tolerance (0 to 1) for considering a color different from another.
        /// </summary>
        public const double COLOR_BRI_DIFF_TOLERANCE = 0.1;

        /// <summary>
        /// Represents a conceptual not initialized point.
        /// (Its both coordinates are double.NegativeInfinity)
        /// </summary>
        public static readonly Point NULL_POINT = new Point(double.NegativeInfinity, double.NegativeInfinity);

        /// <summary>
        /// Initial point at the zero coordinate for X and Y.
        /// </summary>
        public static readonly Point ZERO_POINT = new Point(0.0, 0.0);

        /// <summary>
        /// Orientation point used as end-point for a vertical vector (such as a linear-gradient-brush).
        /// </summary>
        public static readonly Point DIR_ENDPOINT_VERTICAL = new Point(0.0, 1.0);

        /// <summary>
        /// Orientation point used as end-point for an horizontal vector (such as a linear-gradient-brush).
        /// </summary>
        public static readonly Point DIR_ENDPOINT_HORIZONTAL = new Point(1.0, 0.0);

        /// <summary>
        /// Standard initial coordinate offset for a linear gradient brush.
        /// </summary>
        public const double GRADIENT_OFFSET_INITIAL = 0.1;

        /// <summary>
        /// Standard central coordinate offset for a linear gradient brush.
        /// </summary>
        public const double GRADIENT_OFFSET_CENTRAL = 0.5;

        /// <summary>
        /// Standard final coordinate offset for a linear gradient brush.
        /// </summary>
        public const double GRADIENT_OFFSET_FINAL = 0.9;

        /* POSTPONED (this would require to have 4 stops for the central offset, and manage its serialization)
        /// <summary>
        /// Size of the virtual gradient space for preserving the original stop colors.
        /// </summary>
        public const double GRADIENT_COLOR_PRESERVING_SIZE = 0.1; */

        /// <summary>
        /// Predefined segmented dash style for lines.
        /// </summary>
        public static readonly DashStyle SegmentedLineStyle = new DashStyle(General.Concatenate(5.0, 5.0), 2.0);
        
        /// <summary>
        /// Predefined workable (serializable) dash styles for lines.
        /// </summary>
        public static readonly List<Tuple<DashStyle, string>> DeclaredDashStyles =
            new List<Tuple<DashStyle, string>>() { Tuple.Create(DashStyles.Dash, "Dash"),
                                                   Tuple.Create(DashStyles.DashDot, "Dash-Dot"),
                                                   Tuple.Create(DashStyles.DashDotDot, "Dash-Dot-Dot"),
                                                   Tuple.Create(DashStyles.Dot, "Dot"),
                                                   Tuple.Create(DashStyles.Solid, "Solid"),
                                                   Tuple.Create(SegmentedLineStyle, "Segmented") };

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the visual measure and arrangement of this supplied Target to the specified available width and height
        /// (pass double.PositiveInfinity to indicate unconstrained size).
        /// This calculates the ActualWidth and ActualHeigth of the Target without the need of rendering.
        /// </summary>
        public static void UpdateVisualization(this FrameworkElement Target, double AvailableWidth, double AvailableHeight,
                                               bool EnforceLimit = false)
        {
            if (AvailableWidth < 0 || AvailableHeight < 0)
                return;

            Target.Measure(new Size(AvailableWidth, AvailableHeight));

            if (EnforceLimit)
            {
                Target.MaxHeight = AvailableHeight;
                Target.MaxWidth = AvailableWidth;
            }

            // PENDING: From the original thread, Clone the original Brushes, Text-Format, etc... into the new thread.
            Target.Arrange(new Rect(0, 0, Target.DesiredSize.Width.EnforceMaximum(AvailableWidth),
                                          Target.DesiredSize.Height.EnforceMaximum(AvailableHeight)));
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the standard solid color brush having the same color as the supplied Source one,
        /// or, if requested, a new one created else null.
        /// </summary>
        public static Brush GetStandardBrush(this Color SourceColor, bool RequestCreationIfNoFound = true)
        {
            return SolidColorBrushes.FirstOrDefault(brush => brush.Color == SourceColor)
                    .NullDefault(RequestCreationIfNoFound ? new SolidColorBrush(SourceColor) : null);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the interpolated color calculated from this supplied one, the other specified and the percentage provided.
        /// </summary>
        public static Color GetInterpolatedColor(this Color Initial, Color Final, double Factor = 0.5)
        {
            double a1 = Initial.A / 255.0, r1 = Initial.R / 255.0, g1 = Initial.G / 255.0, b1 = Initial.B / 255.0;
            double a2 = Final.A / 255.0, r2 = Final.R / 255.0, g2 = Final.G / 255.0, b2 = Final.B / 255.0;

            byte a3 = Convert.ToByte(((a1 + (a2 - a1) * Factor) * 255).EnforceRange(0, 255));
            byte r3 = Convert.ToByte(((r1 + (r2 - r1) * Factor) * 255).EnforceRange(0, 255));
            byte g3 = Convert.ToByte(((g1 + (g2 - g1) * Factor) * 255).EnforceRange(0, 255));
            byte b3 = Convert.ToByte(((b1 + (b2 - b1) * Factor) * 255).EnforceRange(0, 255));

            var Result = Color.FromArgb(a3, r3, g3, b3);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the color of the pixel under the supplied position.
        /// </summary>
        public static Color GetPixelColor(Point Position)
        {
            int WindowDC = GetWindowDC( 0 );
            int ColorInteger = GetPixel( WindowDC, (int)Position.X, (int)Position.Y );
            //
            // Release the device-context after getting the Color.
            ReleaseDC(0, WindowDC );

            //byte a = (byte)( ( intColor >> 0x18 ) & 0xffL );
            byte b = (byte)( ( ColorInteger >> 0x10 ) & 0xffL );
            byte g = (byte)( ( ColorInteger >> 8 ) & 0xffL );
            byte r = (byte)( ColorInteger & 0xffL );
            var Result = Color.FromRgb( r, g, b );

            return Result;
        }

        [DllImport("gdi32")]
        private static extern int GetPixel(int hdc, int nXPos, int nYPos);

        [DllImport("user32")]
        private static extern int GetWindowDC(int hwnd);

        [DllImport("user32")]
        private static extern int ReleaseDC(int hWnd, int hDC);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this supplied WPF points converted to pixels, with the specified dot-per-inch.
        /// </summary>
        public static double PointsToPixels(this double Points, double Dpi = 96.0)
        {
            var Result = Points * (Dpi / 72.0);
            return Result;
        }

        /// <summary>
        /// Returns this supplied pixels converted to WPF points, with the specified dot-per-inch.
        /// </summary>
        public static double PixelsToPoints(this double Pixels, double Dpi = 96.0)
        {
            var Result = Pixels * (72.0 / Dpi);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the pixel based Width of this supplied image Source.
        /// </summary>
        public static double GetWidth(this ImageSource Source)
        {
            var Bitmap = Source as BitmapSource;
            if (Bitmap != null)
                return (double)Bitmap.PixelWidth;

            return Source.Width;
        }

        /// <summary>
        /// Gets the pixel based Height of this supplied image Source.
        /// </summary>
        public static double GetHeight(this ImageSource Source)
        {
            var Bitmap = Source as BitmapSource;
            if (Bitmap != null)
                return (double)Bitmap.PixelHeight;

            return Source.Height;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the Text-Alignment equivalent of this supplied Horizontal-Alignment Value.
        /// </summary>
        public static TextAlignment ToTextAlignment(this HorizontalAlignment Value)
        {
            if (Value == HorizontalAlignment.Left)
                return TextAlignment.Left;

            if (Value == HorizontalAlignment.Center)
                return TextAlignment.Center;

            if (Value == HorizontalAlignment.Right)
                return TextAlignment.Right;

            return TextAlignment.Justify;
        }

        /// <summary>
        /// Returns the Horizontal-Alignment equivalent of this supplied Text-Alignment Value.
        /// </summary>
        public static HorizontalAlignment ToHorizontalAlignment(this TextAlignment Value)
        {
            if (Value == TextAlignment.Left)
                return HorizontalAlignment.Left;

            if (Value == TextAlignment.Center)
                return HorizontalAlignment.Center;

            if (Value == TextAlignment.Right)
                return HorizontalAlignment.Right;

            return HorizontalAlignment.Stretch;
        }

        /// <summary>
        /// Returns the HTML text-align equivalent of this supplied Text-Alignment Value.
        /// </summary>
        public static string ToHtmlTextAlign(this TextAlignment Value)
        {
            if (Value == TextAlignment.Left)
                return "left";

            if (Value == TextAlignment.Center)
                return "center";

            if (Value == TextAlignment.Right)
                return "right";

            return "justify";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the number of gradient stops of the supplied Source brush, 1 if the brush is not a Linear-Gradient brush or 0 when null.
        /// </summary>
        public static int GetGradientMultiplicity(this Brush Source)
        {
            if (Source == null)
                return 0;

            var LinearBrush = Source as LinearGradientBrush;
            if (LinearBrush == null)
                return 1;

            return LinearBrush.GradientStops.Count;
        }

        /// <summary>
        /// Returns the orientation of gradient stops of the supplied Source brush, or always "Vertical" if the brush is not a Linear-Gradient brush.
        /// </summary>
        public static Orientation GetGradientOrientation(this Brush Source)
        {
            var LinearBrush = Source as LinearGradientBrush;
            if (LinearBrush == null)
                return Orientation.Vertical;

            return (LinearBrush.StartPoint.X == LinearBrush.EndPoint.X ? Orientation.Vertical : Orientation.Horizontal);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a double colored linear-gradient-brush with the supplied Initial and Final colors, plus optional orientation indication and offsets.
        /// </summary>
        public static LinearGradientBrush GetGradientBrush(Color Initial, Color Final, bool IsVertical = true,
                                                           double InitialOffset = Display.GRADIENT_OFFSET_INITIAL,
                                                           double FinalOffset = Display.GRADIENT_OFFSET_FINAL)
        {
            var Stops = new GradientStopCollection(2);
            Stops.Add(new GradientStop(Initial, InitialOffset));
            Stops.Add(new GradientStop(Final, FinalOffset));

            var Result = new LinearGradientBrush(Stops, ZERO_POINT, (IsVertical ? DIR_ENDPOINT_VERTICAL : DIR_ENDPOINT_HORIZONTAL));
            return Result;
        }

        /// <summary>
        /// Creates and returns a triple colored linear-gradient-brush with the supplied Initial, Central and Final colors, plus optional orientation indication and offsets.
        /// </summary>
        public static LinearGradientBrush GetGradientBrush(Color Initial, Color Central, Color Final, bool IsVertical = true,
                                                           double InitialOffset = Display.GRADIENT_OFFSET_INITIAL,
                                                           double CentralOffset = Display.GRADIENT_OFFSET_CENTRAL,
                                                           double FinalOffset = Display.GRADIENT_OFFSET_FINAL)
        {
            var Stops = new GradientStopCollection(3);
            Stops.Add(new GradientStop(Initial, InitialOffset));
            Stops.Add(new GradientStop(Central, CentralOffset));
            Stops.Add(new GradientStop(Final, FinalOffset));

            var Result = new LinearGradientBrush(Stops, ZERO_POINT, (IsVertical ? DIR_ENDPOINT_VERTICAL : DIR_ENDPOINT_HORIZONTAL));
            return Result;
        }

        /// <summary>
        /// Creates and returns a triple colored linear-gradient-brush in a contrasted style, with the supplied Peripheral and Central colors, plus orientation indication.
        /// </summary>
        public static LinearGradientBrush GetContrastedBrush(Color Peripheral, Color Central, bool IsVertical = true)
        {
            return GetGradientBrush(Peripheral, Central, Peripheral, IsVertical);
        }

        /// <summary>
        /// Gets a solid-color brush from a brush which maybe is a linear gradient one.
        /// Optionally, indication of getting the first sub-brush can be specified, else is the second.
        /// </summary>
        public static SolidColorBrush GetSolidBrush(this Brush Source, bool SelectFirst = true)
        {
            if (Source == null)
                return null;

            if (Source is SolidColorBrush)
                return Source as SolidColorBrush;

            var Gradient = Source as LinearGradientBrush;

            if (Gradient == null)
                throw new UsageAnomaly("Brush is not solid or linear-gradient.");

            if (SelectFirst || Gradient.GradientStops.Count < 2)
                return (new SolidColorBrush(Gradient.GradientStops[0].Color));

            return (new SolidColorBrush(Gradient.GradientStops[1].Color));
        }

        /// <summary>
        /// Returns this supplied Source bush, as HTML color string.
        /// Optionally and for linear-gradient-brushs, indication of getting the first sub-brush can be specified, else is the second.
        /// </summary>
        public static string ToHtmlColor(this Brush Source, bool SelectFirst = true)
        {
            var SourceColor = Colors.White;

            if (Source != null)
                if (Source is SolidColorBrush)
                    SourceColor = ((SolidColorBrush)Source).Color;
                else
                {
                    var Gradient = Source as LinearGradientBrush;            
                    if (Gradient == null)
                        throw new UsageAnomaly("Brush is not solid or linear-gradient.");

                    if (SelectFirst || Gradient.GradientStops.Count < 2)
                        SourceColor = Gradient.GradientStops[0].Color;
                    else
                        SourceColor = Gradient.GradientStops[1].Color;
                }

            var Result = SourceColor.ToHexString(false);
            return Result;
        }

        /// <summary>
        /// Returns this supplied color as hex-string, optionally including transparency information.
        /// </summary>
        public static string ToHexString(this Color Source, bool IncludeTransparencyInfo = true)
        {
            var Result = String.Format("#{0}{1}{2}{3}",
                                       (IncludeTransparencyInfo
                                        ? Source.A.ToString("X2")
                                        : String.Empty),
                                       Source.R.ToString("X2"),
                                       Source.G.ToString("X2"),
                                       Source.B.ToString("X2"));
            return Result;
        }

        /// <summary>
        /// Determines whether two brushes are visually almost equivalent (currently working only for solid and linear-gradient brushes).
        /// </summary>
        public static bool IsLike(this Brush Original, Brush Alternative, bool TestForExactMatch = false)
        {
            if (Original is SolidColorBrush && Alternative is SolidColorBrush)
                return ((SolidColorBrush)Original).Color.IsLike(((SolidColorBrush)Alternative).Color, TestForExactMatch);

            if (Original is LinearGradientBrush && Alternative is LinearGradientBrush)
            {
                var OriGrad = (LinearGradientBrush)Original;
                var AltGrad = (LinearGradientBrush)Alternative;

                if (OriGrad.GradientStops.Count != AltGrad.GradientStops.Count)
                    return false;

                for (int Index = 0; Index < OriGrad.GradientStops.Count; Index++)
                    if (!OriGrad.GradientStops[Index].Color.IsLike(AltGrad.GradientStops[Index].Color, TestForExactMatch))
                        return false;

                return true;
            }

            return Original.IsEqual(Alternative);
        }

        /// <summary>
        /// Determines whether two colors are visually almost equivalent.
        /// </summary>
        public static bool IsLike(this Color Original, Color Alternative, bool TestForExactMatch = false)
        {
            /*
            var ColorOri = System.Drawing.Color.FromArgb(Original.A, Original.R, Original.G, Original.B);
            var ColorAlt = System.Drawing.Color.FromArgb(Alternative.A, Alternative.R, Alternative.G, Alternative.B);

            var HueDiff = Math.Abs(ColorOri.GetHue() - ColorAlt.GetHue());
            var SatDiff = Math.Abs(ColorOri.GetSaturation() - ColorAlt.GetSaturation());
            var BriDiff = Math.Abs(ColorOri.GetBrightness() - ColorAlt.GetBrightness());

            var Result = (HueDiff < COLOR_HUE_DIFF_TOLERANCE
                          // || SatDiff < COLOR_SAT_DIFF_TOLERANCE
                          // || BriDiff < COLOR_BRI_DIFF_TOLERANCE
                          );
             
            if (TestForExactMatch)
            */

            var Result = (Original == Alternative);

            return Result;
        }

        /// <summary>
        /// Indicates whether this supplied Original brush is null or fully transparent.
        /// </summary>
        public static bool IsTransparent(this Brush Original)
        {
            if (Original == null || Original == Brushes.Transparent
                || Original.Opacity == 0)
                return true;

            if (!(Original is GradientBrush))
                return false;

            var OriGrad = (LinearGradientBrush)Original;

            for (int Index = 0; Index < OriGrad.GradientStops.Count; Index++)
                if (OriGrad.GradientStops[Index].Color != Colors.Transparent
                    && OriGrad.GradientStops[Index].Color.A != 0)
                    return false;

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied points as a Path Geometry with one Figure,
        /// or null if less than 2 points are specified.
        /// </summary>
        public static PathGeometry PathFigureGeometry(params Point[] PathPoints)
        {
            if (PathPoints == null || PathPoints.Length <= 1)
                return null;

            var Figure = new PathFigure(PathPoints[0], PathPoints.Skip(1).Select(pnt => new LineSegment(pnt, true)), true);
            var Result = new PathGeometry(Figure.IntoEnumerable());

            return Result;
        }

        /// <summary>
        /// Returns a drawing visual based on the supplied shape geometry and other parameters.
        /// </summary>
        public static DrawingVisual DrawGeometry(Geometry Shape, Brush BackgroundBrush = null, Brush LineBrush = null, double LineThickness = 1.0)
        {
            var Result = new DrawingVisual();
            using (var Context = Result.RenderOpen())
            {
                Context.DrawGeometry(BackgroundBrush ?? Brushes.Blue, new Pen(LineBrush ?? Brushes.Red, LineThickness), Shape);
            }

            return Result;
        }

        /// <summary>
        /// Returns this supplied Image-Source as Visual.
        /// </summary>
        public static Visual ToVisual(this ImageSource Source, double Width = double.NaN, double Height = double.NaN)
        {
            if (Source == null)
                return null;

            var Picture = new ImageDrawing(Source, new Rect(0, 0,
                                                            Width.NaNDefault(Source.GetWidth()),
                                                            Height.NaNDefault(Source.GetHeight())));
            var Result = Picture.RenderToDrawingVisual();
            return Result;
        }

        /// <summary>
        /// Returns this supplied Drawing source as DrawingImage.
        /// </summary>
        public static DrawingImage ToDrawingImage(this Drawing Source)
        {
            var Result = new DrawingImage(Source);
            return Result;
        }

        /// <summary>
        /// Generates and returns a rendered DrawingVisual from the supplied DrawingGroup.
        /// </summary>
        public static DrawingVisual RenderToDrawingVisual(this Drawing Source)
        {
            var Drawer = new DrawingVisual();
            using (var Context = Drawer.RenderOpen())
                Context.DrawDrawing(Source);

            return Drawer;
        }

        /// <summary>
        /// Inside a visual presenter, and for an imaginary line between two supplied points
        /// (the first one inside a supplied target and the second maybe outside that target),
        /// returns the nearest point of intersection with the possible target border/outline, considering the Standard Flattening Tolerance.
        /// If both points were on the same target (no intersection), then the final point is returned.
        /// </summary>
        /// <param name="InitialPointInSource">Origin point of the intersecting line which is inside the evaluated target.</param>
        /// <param name="FinalPoint">Destination point of the intersecting line which maybe is outside the evaluated target.</param>
        /// <param name="Presenter">Visual presentation context.</param>
        /// <param name="Source">Object which contains the origin point of the intersecting line.</param>
        /// <param name="Filter">Function to determine what objects can be considered as valid visual hits.</param>
        /// <returns></returns>
        public static Point DetermineNearestIntersectingPoint(this Point InitialPointInSource, Point FinalPoint, UIElement Presenter, Visual Source,
                                                              Func<DependencyObject, HitTestFilterBehavior> Filter = null)
        {
            if (Source == null)
                return FinalPoint;  // OLD: General.ContractRequiresNotNull(Source);

            DependencyObject InitialPointed = null;
            DependencyObject FinalPointed = null;

            var FilterEval = (Filter == null ? null : new HitTestFilterCallback(Filter));

            var InitialSelectionEval = new HitTestResultCallback(
                    res =>
                        {
                            if (res == null)
                                InitialPointed = (new PointHitTestResult(Source, InitialPointInSource)).VisualHit;
                            else
                                InitialPointed = res.VisualHit;

                            return HitTestResultBehavior.Stop;
                        });
            VisualTreeHelper.HitTest(Source, FilterEval, InitialSelectionEval, new PointHitTestParameters(InitialPointInSource));

            var FinalSelectionEval = new HitTestResultCallback(
                    res =>
                    {
                        if (res == null)
                            FinalPointed = (new PointHitTestResult(Source, FinalPoint)).VisualHit;
                        else
                            FinalPointed = res.VisualHit;

                        return HitTestResultBehavior.Stop;
                    });
            VisualTreeHelper.HitTest(Source, FilterEval, FinalSelectionEval, new PointHitTestParameters(FinalPoint));

            /* OLD
            InitialPointed = VisualTreeHelper.HitTest(Presenter, InitialPointInSource).NullDefault(new PointHitTestResult(Source, InitialPointInSource)).VisualHit;
            FinalPointed =  VisualTreeHelper.HitTest(Presenter, FinalPoint).NullDefault(new PointHitTestResult(Source, FinalPoint)).VisualHit; */

            // IMPORTANT: These two variables can be NULL.
            if (InitialPointed == FinalPointed)
            {
                /*T var PntHC = FinalPointed.GetHashCode();
                var SrcHC = Source.GetHashCode();
                Console.WriteLine("Pointing to UIElement={0}. Source={1}", PntHC, SrcHC); */

                return FinalPoint;
            }

            /* if (!Presenter.ContainsObjectWithPoint(Source, InitialPointInSource))
                throw new UsageAnomaly("Evaluated target must contain the supplied Initial point", new DataWagon("Target", Target).Add("InitialPoint", InitialPoint));

            if (Presenter.ContainsObjectWithPoint(Source, FinalPoint))
                throw new UsageAnomaly("Evaluated target must not contain the supplied Final point.", new DataWagon("Target", Target).Add("FinalPoint", FinalPoint)); */

            var Result = FindBoundary(InitialPointInSource, FinalPoint, Presenter, Source, true);
            return Result;
        }

        /// <summary>
        /// Determines and returns the boundary point between Initial-Point-In-Source and Final-Point, on a graphic Presenter, for the specified visual Source.
        /// Plus, and indication to return of a point inside the source visual can be specified.
        /// </summary>
        public static Point FindBoundary(this Point InitialPointInSource, Point FinalPoint, UIElement Presenter, Visual Source, bool MustReturnPointInSource = false)
        {
            double XCenter = 0, YCenter = 0;
            bool XLimitReached = false, YLimitReached = false;
            bool SourceIsPointed = false;

            double XLastStart = InitialPointInSource.X;
            double XLastEnd = FinalPoint.X;
            double XOffset = (FinalPoint.X - InitialPointInSource.X);
            bool ToRight = (XOffset > 0);

            double YLastStart = InitialPointInSource.Y;
            double YLastEnd = FinalPoint.Y;
            double YOffset = (FinalPoint.Y - InitialPointInSource.Y);
            bool ToBottom = (YOffset > 0);

            double LastSourceX = InitialPointInSource.X, LastSourceY = InitialPointInSource.Y;

            while (true)
            {
                if (!XLimitReached)
                {
                    XCenter = (XLastStart + XLastEnd) / 2.0;
                    XLimitReached = Math.Abs(XLastStart - XCenter) <= Geometry.StandardFlatteningTolerance;
                }

                if (!YLimitReached)
                {
                    YCenter = (YLastStart + YLastEnd) / 2.0;
                    YLimitReached = Math.Abs(YLastStart - YCenter) <= Geometry.StandardFlatteningTolerance;
                }

                /*T Console.WriteLine("Pointed={0}; Center={1}; LastStart={2}; LastEnd={3}; XLimR={4}; YLimR={5}; ToRgh={6}; ToBtm={7}",
                                   SourceIsPointed, new Point(XCenter, YCenter), new Point(XLastStart, YLastStart), new Point(XLastEnd, YLastEnd),
                                   XLimitReached, YLimitReached, ToRight, ToBottom); */

                if (XLimitReached && YLimitReached)
                    break;

                SourceIsPointed = Presenter.ContainsObjectWithPoint(Source, new Point(XCenter, YCenter));
                if (MustReturnPointInSource && SourceIsPointed)
                {
                    LastSourceX = XCenter;
                    LastSourceY = YCenter;
                }

                if (!XLimitReached)
                    if (SourceIsPointed)
                        XLastStart = XCenter;
                    else
                        XLastEnd = XCenter;

                if (!YLimitReached)
                    if (SourceIsPointed)
                        YLastStart = YCenter;
                    else
                        YLastEnd = YCenter;
            }

            var Result = (MustReturnPointInSource
                          ? new Point(LastSourceX, LastSourceY)
                          : new Point(XCenter, YCenter));
            return Result;
        }

        /// <summary>
        /// Returns the calculated angle from an origin point to a target one.
        /// </summary>
        public static double DeterminePointingAngleTo(this Point Origin, Point Target)
        {
            double Radians = Math.Atan((Origin.Y - Target.Y) / (Origin.X - Target.X));
            double Angle = Radians * 180.0 / Math.PI;

            if (Origin.X - Target.X < 0)
                Angle += 180.0;

            return Angle;
        }

        /// <summary>
        /// Gets the distance between this supplied Origin point and the specified Target point.
        /// </summary>
        public static double DetermineDistanceTo(this Point Origin, Point Target)
        {
            var HorizDelta = Origin.X - Target.X;
            var VertiDelta = Origin.Y - Target.Y;
            var Result = Math.Sqrt(HorizDelta * HorizDelta + VertiDelta * VertiDelta);
            return Result;
        }

        // ===============================================================================================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct BITMAPFILEHEADER
        {
            public static readonly short BM = 0x4d42; // BM

            public short bfType;
            public int bfSize;
            public short bfReserved1;
            public short bfReserved2;
            public int bfOffBits;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            public int biSize;
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static BitmapSource ImageFromClipboardDib()
        {
            MemoryStream ms = General.Execute<MemoryStream>(() => Clipboard.GetData("DeviceIndependentBitmap") as MemoryStream,
                                                            "Cannot access Windows Clipboard!").Result;
            if (ms == null)
                return null;

            byte[] dibBuffer = new byte[ms.Length];
            ms.Read(dibBuffer, 0, dibBuffer.Length);
 
            BITMAPINFOHEADER infoHeader =
                BytesHandling.StructFromByteArray<BITMAPINFOHEADER>(dibBuffer);
 
            int fileHeaderSize = Marshal.SizeOf(typeof(BITMAPFILEHEADER));
            int infoHeaderSize = infoHeader.biSize;
            int fileSize = fileHeaderSize + infoHeader.biSize + infoHeader.biSizeImage;
 
            BITMAPFILEHEADER fileHeader = new BITMAPFILEHEADER();
            fileHeader.bfType = BITMAPFILEHEADER.BM;
            fileHeader.bfSize = fileSize;
            fileHeader.bfReserved1 = 0;
            fileHeader.bfReserved2 = 0;
            fileHeader.bfOffBits = fileHeaderSize + infoHeaderSize + infoHeader.biClrUsed * 4;
 
            byte[] fileHeaderBytes =
                BytesHandling.StructToByteArray<BITMAPFILEHEADER>(fileHeader);
 
            MemoryStream msBitmap = new MemoryStream();
            msBitmap.Write(fileHeaderBytes, 0, fileHeaderSize);
            msBitmap.Write(dibBuffer, 0, dibBuffer.Length);
            msBitmap.Seek(0, SeekOrigin.Begin);
 
            var Result = BitmapFrame.Create(msBitmap);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///   Returns a PrintTicket based on the current default printer.</summary>
        /// <returns>A PrintTicket for the current local default printer.</returns>
        private static PrintTicket GetPrintTicketFromPrinter()
        {
            PrintQueue printQueue = null;

            LocalPrintServer localPrintServer = new LocalPrintServer();

            // Retrieving collection of local printer on user machine
            PrintQueueCollection localPrinterCollection =
                localPrintServer.GetPrintQueues();

            System.Collections.IEnumerator localPrinterEnumerator =
                localPrinterCollection.GetEnumerator();

            if (localPrinterEnumerator.MoveNext())
            {
                // Get PrintQueue from first available printer
                printQueue = (PrintQueue)localPrinterEnumerator.Current;
            }
            else
            {
                // No printer exist, return null PrintTicket
                return null;
            }

            // Get default PrintTicket from printer
            PrintTicket printTicket = printQueue.DefaultPrintTicket;

            PrintCapabilities printCapabilites = printQueue.GetPrintCapabilities();

            // Modify PrintTicket
            if (printCapabilites.CollationCapability.Contains(Collation.Collated))
            {
                printTicket.Collation = Collation.Collated;
            }

            if (printCapabilites.DuplexingCapability.Contains(
                    Duplexing.TwoSidedLongEdge))
            {
                printTicket.Duplexing = Duplexing.TwoSidedLongEdge;
            }

            if (printCapabilites.StaplingCapability.Contains(Stapling.StapleDualLeft))
            {
                printTicket.Stapling = Stapling.StapleDualLeft;
            }

            return printTicket;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the content of the supplied XamlText as flow-document, or null if conversion was not possible.
        /// </summary>
        public static FlowDocument XamlRichTextToFlowDocument(string RichText)
        {
            var Result = new FlowDocument();

            if (RichText != RichTextEditor.ABSENT_TEXT)
                try
                {
                    var Range = new TextRange(Result.ContentStart, Result.ContentEnd);
                    using (var Torrent = RichText.StringToStream())
                        Range.Load(Torrent, DataFormats.Xaml);
                }
                catch (Exception Problem)
                {
                    Console.WriteLine("Cannot convert xaml rich-text to flow-document. Problem: " + Problem.Message);
                    Result = null;
                }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the content of the supplied XamlText as plain text, or null if conversion was not possible.
        /// </summary>
        public static string XamlRichTextToPlainText(string RichText)
        {
            var Document = XamlRichTextToFlowDocument(RichText);
            if (Document == null)
                return null;

            var Result = Document.GetText();
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied xaml-text converted to the specified data-format, or null if conversion was not possible.
        /// </summary>
        public static string XamlRichTextTo(string RichText, string RequestedFormat)
        {
            string Result = null;
            var ResultFlow = new MemoryStream();

            try
            {
                var Intermediate = XamlRichTextToFlowDocument(RichText);

                var Format = (RequestedFormat == DataFormats.Html
                                  ? DataFormats.Xaml
                                  : RequestedFormat);

                var Range = new TextRange(Intermediate.ContentStart, Intermediate.ContentEnd);
                using (var Torrent = RichText.StringToStream())
                    Range.Save(ResultFlow, Format);

                Result = ResultFlow.StreamToString();

                if (RequestedFormat == DataFormats.Html)
                    Result = XamlFlowDocumentToHtmlConverter.Convert("<FlowDocument>" + Result + "</FlowDocument>");
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot convert xaml rich-text to requested format. Problem: " + Problem.Message);
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// From this Position, finds the specified Text, optionally using the passed Comparer.
        /// </summary>
        public static TextRange FindText(this TextPointer Position, string Text, StringComparison Comparer = StringComparison.Ordinal)
        {
            while (Position != null)
            {
                if (Position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    var Run = Position.GetTextInRun(LogicalDirection.Forward);

                    // Find the starting index of any substring that matches "text".
                    var Index = Run.IndexOf(Text, Comparer);
                    if (Index >= 0)
                    {
                        var start = Position.GetPositionAtOffset(Index);
                        var end = start.GetPositionAtOffset(Text.Length);
                        return new TextRange(start, end);
                    }
                }

                Position = Position.GetNextContextPosition(LogicalDirection.Forward);
            }

            // position will be null if "text" is not found.
            return null;
        }

        public static char PreviousChar(this TextRange Range)
        {
            var Text = Range.Start.GetTextInRun(LogicalDirection.Backward);
            if (Text.IsAbsent())
                return (char)0;

            return Text.Last();
        }

        public static char NextChar(this TextRange Range)
        {
            var Text = Range.End.GetTextInRun(LogicalDirection.Forward);
            if (Text.IsAbsent())
                return (char)0;

            return Text.First();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the full plain text of this document.
        /// </summary>
        public static string GetText(this FlowDocument Document)
        {
            var Range = new TextRange(Document.ContentStart, Document.ContentEnd);
            return Range.Text;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns all the text-runs in a flowdocument.
        /// </summary>
        public static IList<Run> GetAllRuns(this FlowDocument Document)
        {
            var Result = Document.Blocks.SelectMany(block => block.GetAllRuns()).ToList();
            return Result;
        }

        /// <summary>
        /// Returns all the text-runs in the element.
        /// </summary>
        public static IEnumerable<Run> GetAllRuns(this TextElement Element)
        {
            var RunElement = Element as Run;
            if (RunElement != null)
                return RunElement.IntoEnumerable<Run>();

            var SpanElement = Element as Span;
            if (SpanElement != null)
                return SpanElement.Inlines.SelectMany(item => item.GetAllRuns());

            var AnchoredBlockElement = Element as AnchoredBlock;
            if (AnchoredBlockElement != null)
                return AnchoredBlockElement.Blocks.SelectMany(item => item.GetAllRuns());

            var ListElement = Element as List;
            if (ListElement != null)
                return ListElement.ListItems.SelectMany(item => item.GetAllRuns());

            var ListItemElement = Element as ListItem;
            if (ListItemElement != null)
                return ListItemElement.Blocks.SelectMany(item => item.GetAllRuns());

            var ParagraphElement = Element as Paragraph;
            if (ParagraphElement != null)
                return ParagraphElement.Inlines.SelectMany(item => item.GetAllRuns());

            var SectionElement = Element as Section;
            if (SectionElement != null)
                return SectionElement.Blocks.SelectMany(item => item.GetAllRuns());

            // PENDING: Tables -> TableRowGroups -> ...
            var TableElement = Element as Table;
            if (TableElement != null)
                return TableElement.RowGroups
                        .SelectMany(group => group.Rows
                            .SelectMany(row => row.Cells
                                .SelectMany(cell => cell.Blocks
                                    .SelectMany(block => block.GetAllRuns()))));

            return Enumerable.Empty<Run>();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Saves the supplied Source as XPS in the specified Target-Location.
        /// Returns error message or null when succeeded.
        /// </summary>
        public static string SaveDocumentAsXPS(Drawing Source, string TargetLocation)
        {
            string ErrorMessage = null;

            try
            {
                using (var Container = Package.Open(TargetLocation, FileMode.Create))
                {
                    using (var TargetDocument = new XpsDocument(Container, CompressionOption.Maximum))
                    {
                        var Writer = XpsDocument.CreateXpsDocumentWriter(TargetDocument);
                        var Ticket = GetPrintTicketFromPrinter();
                        if (Ticket == null)
                            return "No printer is defined.";

                        Ticket.PageMediaSize = new PageMediaSize(Source.Bounds.Width, Source.Bounds.Height);
                        var SourceVisual = Source.RenderToDrawingVisual();
                        Writer.Write(SourceVisual, Ticket);
                    }
                }
            }
            catch (Exception Problem)
            {
                ErrorMessage = "Cannot export document to XPS.\nProblem: " + Problem.Message;
            }

            return ErrorMessage;
        }

        /// <summary>
        /// Saves the supplied Source as XPS in the specified Target-Location.
        /// Returns error message or null when succeeded.
        /// </summary>
        // NOTE: Images appear very blurry
        public static string SaveDocumentAsXPS(FixedDocument Source, string TargetLocation)
        {
            string ErrorMessage = null;

            try
            {
                using (var Container = Package.Open(TargetLocation, FileMode.Create))
                {
                    using (var TargetDocument = new XpsDocument(Container, CompressionOption.Maximum))
                    {
                        var Writer = XpsDocument.CreateXpsDocumentWriter(TargetDocument);
                        Writer.Write(Source);

                        TargetDocument.Close();
                    }

                    Container.Close();
                }
            }
            catch (Exception Problem)
            {
                ErrorMessage = "Cannot export document to XPS.\nProblem: " + Problem.Message;
            }

            return ErrorMessage;
        }

        /*T Use this when exporting multi-page documents
        /// <summary>
        /// Saves the supplied Document as XPS in the specified Target-Location.
        /// Returns error message or null when succeeded.
        /// </summary>
        public static string SaveDocumentAsXPS(FixedDocument Document, string TargetLocation)
        {
            string ErrorMessage = null;

            try
            {
                using (var Container = Package.Open(TargetLocation, FileMode.Create))
                {
                    using (var TargetDocument = new XpsDocument(Container, CompressionOption.Maximum))
                    {
                        var Serializer = new XpsSerializationManager(new XpsPackagingPolicy(TargetDocument), false);

                        var Wrapper = new DocumentPaginatorWrapper(Document.DocumentPaginator,
                                                                   Document.DocumentPaginator.PageSize,
                                                                   new Size(0,0));

                        Serializer.SaveAsXaml(Wrapper);
                    }
                }
            }
            catch (Exception Problem)
            {
                ErrorMessage = "Cannot export document to XPS.\nProblem: " + Problem.Message;
            }

            return ErrorMessage;
        } */

        public static FixedDocument LoadDocumentFromXPS(string FilePath)
        {
            var Document = new XpsDocument(FilePath, FileAccess.Read);
            var Result = Document.GetFixedDocumentSequence().References.First().GetDocument(false);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the external application for converting XPS to PDF is available.
        /// </summary>
        public static bool CanConvertXpsToPdf
        {
            get
            {
                var Result = true; // File.Exists(XpsToPdfConverterLocation);
                return Result;
            }
        }

        /* Previously used when commercial...
        public static string XpsToPdfConverterLocation
        {
            get
            {
                var Result = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                          "xpstopdf.exe");
                return Result;
            }
        } */

        /// <summary>
        /// Converts an XPS file into a PDF one.
        /// It is recommended that the source XPS is a temporal one with a short name ended in ".xps".
        /// To avoid name clash, the source directory cannot have a .PDF filed with the same source name.
        /// </summary>
        public static string ConvertXPStoPDF(string SourceXpsLocation, string TargetPdfLocation)
        {
            string ErrorMessage = null;

            if (!SourceXpsLocation.ToLower().EndsWith(".xps"))
                SourceXpsLocation = SourceXpsLocation + ".xps";

            try
            {
                PdfSharp.Xps.XpsConverter.Convert(SourceXpsLocation, TargetPdfLocation, 0);
            }
            catch (Exception Problem)
            {
                ErrorMessage = "Cannot convert XPS to PDF.\nProblem: " + Problem.Message;
            }

            return ErrorMessage;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
