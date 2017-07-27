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
// File   : Display.cs
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Instrumind.Common;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization.Widgets;
using System.Reflection;
using System.Collections;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Provides common services for working with WPF. Main part.
    /// </summary>
    public static partial class Display
    {
        /// <summary>
        /// File extensions for reading Images.
        /// </summary>
        public const string IMAGE_FILE_READ_EXTENSIONS = "jpg;jpe;jpeg;png;gif;bmp;dib;ico;tif;tiff";

        /// <summary>
        /// File extensions for writing Images.
        /// </summary>
        public const string IMAGE_FILE_WRITE_EXTENSIONS = "jpg;png;gif;bmp;tif";

        /// <summary>
        /// Default encoding quality level for JPEG format.
        /// </summary>
        public const int DEF_JPEG_QUALITY = 90;

        /// <summary>
        /// Opacity level for visually unavailable objects.
        /// </summary>
        public const double VISUAL_UNAVAILABILITY_OPACITY = 0.25;

        /// <summary>
        /// Associations between standard data-type and a representative icon.
        /// </summary>
        public static readonly Dictionary<Type, string> DataTypeRelatedIcons =
            new Dictionary<Type, string>
            {
                { typeof(object), "type_any.png" },
                { typeof(byte), "type_numsigint.png" },
                { typeof(short), "type_numsigint.png" },
                { typeof(int), "type_numsigint.png" },
                { typeof(long), "type_numsigint.png" },
                { typeof(ushort), "type_numunsint.png" },
                { typeof(uint), "type_numunsint.png" },
                { typeof(ulong), "type_numunsint.png" },
                { typeof(float), "type_numsigdec.png" },
                { typeof(double), "type_numsigdec.png" },
                { typeof(decimal), "type_numsigdec.png" },
                { typeof(string), "type_text.png" },
                { typeof(DateTime), "type_datetime.png" }
            };

        private static readonly List<SolidColorBrush> SolidColorBrushes = new List<SolidColorBrush>();

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Display()
        {
            // Brushes.
            SolidColorBrushes.Add(Brushes.AliceBlue);
            SolidColorBrushes.Add(Brushes.AntiqueWhite);
            SolidColorBrushes.Add(Brushes.Aqua);
            SolidColorBrushes.Add(Brushes.Aquamarine);
            SolidColorBrushes.Add(Brushes.Azure);
            SolidColorBrushes.Add(Brushes.Beige);
            SolidColorBrushes.Add(Brushes.Bisque);
            SolidColorBrushes.Add(Brushes.Black);
            SolidColorBrushes.Add(Brushes.BlanchedAlmond);
            SolidColorBrushes.Add(Brushes.Blue);
            SolidColorBrushes.Add(Brushes.BlueViolet);
            SolidColorBrushes.Add(Brushes.Brown);
            SolidColorBrushes.Add(Brushes.BurlyWood);
            SolidColorBrushes.Add(Brushes.CadetBlue);
            SolidColorBrushes.Add(Brushes.Chartreuse);
            SolidColorBrushes.Add(Brushes.Chocolate);
            SolidColorBrushes.Add(Brushes.Coral);
            SolidColorBrushes.Add(Brushes.CornflowerBlue);
            SolidColorBrushes.Add(Brushes.Cornsilk);
            SolidColorBrushes.Add(Brushes.Crimson);
            SolidColorBrushes.Add(Brushes.Cyan);
            SolidColorBrushes.Add(Brushes.DarkBlue);
            SolidColorBrushes.Add(Brushes.DarkCyan);
            SolidColorBrushes.Add(Brushes.DarkGoldenrod);
            SolidColorBrushes.Add(Brushes.DarkGray);
            SolidColorBrushes.Add(Brushes.DarkGreen);
            SolidColorBrushes.Add(Brushes.DarkKhaki);
            SolidColorBrushes.Add(Brushes.DarkMagenta);
            SolidColorBrushes.Add(Brushes.DarkOliveGreen);
            SolidColorBrushes.Add(Brushes.DarkOrange);
            SolidColorBrushes.Add(Brushes.DarkOrchid);
            SolidColorBrushes.Add(Brushes.DarkRed);
            SolidColorBrushes.Add(Brushes.DarkSalmon);
            SolidColorBrushes.Add(Brushes.DarkSeaGreen);
            SolidColorBrushes.Add(Brushes.DarkSlateBlue);
            SolidColorBrushes.Add(Brushes.DarkSlateGray);
            SolidColorBrushes.Add(Brushes.DarkTurquoise);
            SolidColorBrushes.Add(Brushes.DarkViolet);
            SolidColorBrushes.Add(Brushes.DeepPink);
            SolidColorBrushes.Add(Brushes.DeepSkyBlue);
            SolidColorBrushes.Add(Brushes.DimGray);
            SolidColorBrushes.Add(Brushes.DodgerBlue);
            SolidColorBrushes.Add(Brushes.Firebrick);
            SolidColorBrushes.Add(Brushes.FloralWhite);
            SolidColorBrushes.Add(Brushes.ForestGreen);
            SolidColorBrushes.Add(Brushes.Fuchsia);
            SolidColorBrushes.Add(Brushes.Gainsboro);
            SolidColorBrushes.Add(Brushes.GhostWhite);
            SolidColorBrushes.Add(Brushes.Gold);
            SolidColorBrushes.Add(Brushes.Goldenrod);
            SolidColorBrushes.Add(Brushes.Gray);
            SolidColorBrushes.Add(Brushes.Green);
            SolidColorBrushes.Add(Brushes.GreenYellow);
            SolidColorBrushes.Add(Brushes.Honeydew);
            SolidColorBrushes.Add(Brushes.HotPink);
            SolidColorBrushes.Add(Brushes.IndianRed);
            SolidColorBrushes.Add(Brushes.Indigo);
            SolidColorBrushes.Add(Brushes.Ivory);
            SolidColorBrushes.Add(Brushes.Khaki);
            SolidColorBrushes.Add(Brushes.Lavender);
            SolidColorBrushes.Add(Brushes.LavenderBlush);
            SolidColorBrushes.Add(Brushes.LawnGreen);
            SolidColorBrushes.Add(Brushes.LemonChiffon);
            SolidColorBrushes.Add(Brushes.LightBlue);
            SolidColorBrushes.Add(Brushes.LightCoral);
            SolidColorBrushes.Add(Brushes.LightCyan);
            SolidColorBrushes.Add(Brushes.LightGoldenrodYellow);
            SolidColorBrushes.Add(Brushes.LightGray);
            SolidColorBrushes.Add(Brushes.LightGreen);
            SolidColorBrushes.Add(Brushes.LightPink);
            SolidColorBrushes.Add(Brushes.LightSalmon);
            SolidColorBrushes.Add(Brushes.LightSeaGreen);
            SolidColorBrushes.Add(Brushes.LightSkyBlue);
            SolidColorBrushes.Add(Brushes.LightSlateGray);
            SolidColorBrushes.Add(Brushes.LightSteelBlue);
            SolidColorBrushes.Add(Brushes.LightYellow);
            SolidColorBrushes.Add(Brushes.Lime);
            SolidColorBrushes.Add(Brushes.LimeGreen);
            SolidColorBrushes.Add(Brushes.Linen);
            SolidColorBrushes.Add(Brushes.Magenta);
            SolidColorBrushes.Add(Brushes.Maroon);
            SolidColorBrushes.Add(Brushes.MediumAquamarine);
            SolidColorBrushes.Add(Brushes.MediumBlue);
            SolidColorBrushes.Add(Brushes.MediumOrchid);
            SolidColorBrushes.Add(Brushes.MediumPurple);
            SolidColorBrushes.Add(Brushes.MediumSeaGreen);
            SolidColorBrushes.Add(Brushes.MediumSlateBlue);
            SolidColorBrushes.Add(Brushes.MediumSpringGreen);
            SolidColorBrushes.Add(Brushes.MediumTurquoise);
            SolidColorBrushes.Add(Brushes.MediumVioletRed);
            SolidColorBrushes.Add(Brushes.MidnightBlue);
            SolidColorBrushes.Add(Brushes.MintCream);
            SolidColorBrushes.Add(Brushes.MistyRose);
            SolidColorBrushes.Add(Brushes.Moccasin);
            SolidColorBrushes.Add(Brushes.NavajoWhite);
            SolidColorBrushes.Add(Brushes.Navy);
            SolidColorBrushes.Add(Brushes.OldLace);
            SolidColorBrushes.Add(Brushes.Olive);
            SolidColorBrushes.Add(Brushes.OliveDrab);
            SolidColorBrushes.Add(Brushes.Orange);
            SolidColorBrushes.Add(Brushes.OrangeRed);
            SolidColorBrushes.Add(Brushes.Orchid);
            SolidColorBrushes.Add(Brushes.PaleGoldenrod);
            SolidColorBrushes.Add(Brushes.PaleGreen);
            SolidColorBrushes.Add(Brushes.PaleTurquoise);
            SolidColorBrushes.Add(Brushes.PaleVioletRed);
            SolidColorBrushes.Add(Brushes.PapayaWhip);
            SolidColorBrushes.Add(Brushes.PeachPuff);
            SolidColorBrushes.Add(Brushes.Peru);
            SolidColorBrushes.Add(Brushes.Pink);
            SolidColorBrushes.Add(Brushes.Plum);
            SolidColorBrushes.Add(Brushes.PowderBlue);
            SolidColorBrushes.Add(Brushes.Purple);
            SolidColorBrushes.Add(Brushes.Red);
            SolidColorBrushes.Add(Brushes.RosyBrown);
            SolidColorBrushes.Add(Brushes.RoyalBlue);
            SolidColorBrushes.Add(Brushes.SaddleBrown);
            SolidColorBrushes.Add(Brushes.Salmon);
            SolidColorBrushes.Add(Brushes.SandyBrown);
            SolidColorBrushes.Add(Brushes.SeaGreen);
            SolidColorBrushes.Add(Brushes.SeaShell);
            SolidColorBrushes.Add(Brushes.Sienna);
            SolidColorBrushes.Add(Brushes.Silver);
            SolidColorBrushes.Add(Brushes.SkyBlue);
            SolidColorBrushes.Add(Brushes.SlateBlue);
            SolidColorBrushes.Add(Brushes.SlateGray);
            SolidColorBrushes.Add(Brushes.Snow);
            SolidColorBrushes.Add(Brushes.SpringGreen);
            SolidColorBrushes.Add(Brushes.SteelBlue);
            SolidColorBrushes.Add(Brushes.Tan);
            SolidColorBrushes.Add(Brushes.Teal);
            SolidColorBrushes.Add(Brushes.Thistle);
            SolidColorBrushes.Add(Brushes.Tomato);
            SolidColorBrushes.Add(Brushes.Transparent);
            SolidColorBrushes.Add(Brushes.Turquoise);
            SolidColorBrushes.Add(Brushes.Violet);
            SolidColorBrushes.Add(Brushes.Wheat);
            SolidColorBrushes.Add(Brushes.White);
            SolidColorBrushes.Add(Brushes.WhiteSmoke);
            SolidColorBrushes.Add(Brushes.Yellow);
            SolidColorBrushes.Add(Brushes.YellowGreen);

            // Registers non-serializable types for Store-Box storage.
            StoreBox.RegisterStorableType<ImageSource>(block => ConvertByteArrayToImageSource(block, true), image => ConvertImageSourceToByteArray(image, true));
            StoreBox.RegisterStorableType<Brush>(ConvertByteArrayToBrush, ConvertBrushToByteArray);
            StoreBox.RegisterStorableType<DashStyle>(ConvertByteArrayToDashStyle, ConvertDashStyleToByteArray);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Assigns this supplied image Source to an Image-Assignment and returns it.
        /// </summary>
        public static ImageAssignment AssignImage(this ImageSource Source)
        {
            var Assignment = new ImageAssignment();
            Assignment.Image = Source;
            return Assignment;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available font families.
        /// </summary>
        public static IEnumerable<FontFamily> AvailableFontFamilies
        {
            // This should be cached?
            get
            {
                var Result = Fonts.SystemFontFamilies.OrderBy(ffam => ffam.FamilyNames.First().Value);
                // Non compatible: var Result = (new System.Drawing.Text.InstalledFontCollection).Families.OrderBy(ffam => ffam.Name);
                return Result;
            }
        }

        /// <summary>
        /// Gets the available font sizes.
        /// </summary>
        public static IEnumerable<int> AvailableFontSizes
        {
            get { return AvailableFontSizes_; }
        }
        private static int[] AvailableFontSizes_ = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 32, 36, 40, 48,
                                                     56, 64, 72, 80, 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 256 };

        //------------------------------------------------------------------------------------------
        public static ImageSource ConvertByteArrayToImageSource(byte[] Value, bool HasImageTypePrefix = false)
        {
            if (Value == null || Value.Length == 0)
                return null;

            return Value.ToImageSource(HasImageTypePrefix);
        }

        public static byte[] ConvertImageSourceToByteArray(ImageSource Value, bool PutImageTypePrefix = false)
        {
            if (Value == null)
                return null;

            return Value.ToBytes(PutImageTypePrefix);
        }

        //------------------------------------------------------------------------------------------
        public static Brush ConvertByteArrayToBrush(byte[] Value)
        {
            if (Value == null || Value.Length == 0)
                return null;

            if (!Value.Length.IsOneOf(5, 10, 14))
                throw new UsageAnomaly("Cannot convert the supplied byte-array to Brush", Value);

            Brush Result = null;
            var Opacity = ((double)Value[0] / 100.0);

            if (Value.Length == 5)  // Single colored
                Result = new SolidColorBrush(Color.FromArgb(Value[1], Value[2], Value[3], Value[4]));
            else
            {
                var EndPoint = (Value[1] == 0 ? DIR_ENDPOINT_VERTICAL : DIR_ENDPOINT_HORIZONTAL);
                var Stops = new GradientStopCollection(Value.Length == 10 ? 2 : 3);
                Stops.Add(new GradientStop(Color.FromArgb(Value[2], Value[3], Value[4], Value[5]), Display.GRADIENT_OFFSET_INITIAL));
                LinearGradientBrush Gradient = null;

                if (Value.Length == 10) // Double colored
                {
                    Stops.Add(new GradientStop(Color.FromArgb(Value[6], Value[7], Value[8], Value[9]), Display.GRADIENT_OFFSET_FINAL));
                    Gradient = new LinearGradientBrush(Stops, Display.ZERO_POINT, EndPoint);
                }
                else // Triple colored
                {
                    Stops.Add(new GradientStop(Color.FromArgb(Value[6], Value[7], Value[8], Value[9]), Display.GRADIENT_OFFSET_CENTRAL));
                    Stops.Add(new GradientStop(Color.FromArgb(Value[10], Value[11], Value[12], Value[13]), Display.GRADIENT_OFFSET_FINAL));
                    Gradient = new LinearGradientBrush(Stops, Display.ZERO_POINT, EndPoint);
                }

                Result = Gradient;
            }

            Result.Opacity = Opacity;
            Result.Freeze();    // Important to allow use in other threads, such as for file/code-generation.
            return Result;
        }

        public static byte[] ConvertBrushToByteArray(Brush Value)
        {
            if (Value == null)
                return null;

            byte[] Result = null;
            var Opacity = (byte)((Value.Opacity * 100.0).EnforceRange(0.0, 100.0));
            var Solid = Value as SolidColorBrush;

            if (Solid != null)
                Result = new Byte[5] { Opacity, Solid.Color.A, Solid.Color.R, Solid.Color.G, Solid.Color.B };
            else
            {
                var Gradient = Value as LinearGradientBrush;
                if (Gradient == null || Gradient.GradientStops.Count < 2)
                    throw new UsageAnomaly("Cannot convert a non SolidColorBrush to byte-array", Value);

                byte EndPoint = (byte)(Gradient.EndPoint == DIR_ENDPOINT_VERTICAL ? 0 : 255);

                if (Gradient.GradientStops.Count == 2)
                    Result = new Byte[10] { Opacity, EndPoint,
                                            Gradient.GradientStops[0].Color.A, Gradient.GradientStops[0].Color.R, Gradient.GradientStops[0].Color.G, Gradient.GradientStops[0].Color.B,
                                            Gradient.GradientStops[1].Color.A, Gradient.GradientStops[1].Color.R, Gradient.GradientStops[1].Color.G, Gradient.GradientStops[1].Color.B };
                else
                    Result = new Byte[14] { Opacity, EndPoint,
                                            Gradient.GradientStops[0].Color.A, Gradient.GradientStops[0].Color.R, Gradient.GradientStops[0].Color.G, Gradient.GradientStops[0].Color.B,
                                            Gradient.GradientStops[1].Color.A, Gradient.GradientStops[1].Color.R, Gradient.GradientStops[1].Color.G, Gradient.GradientStops[1].Color.B,
                                            Gradient.GradientStops[2].Color.A, Gradient.GradientStops[2].Color.R, Gradient.GradientStops[2].Color.G, Gradient.GradientStops[2].Color.B };
            }

            return Result;
        }

        //------------------------------------------------------------------------------------------
        public static DashStyle ConvertByteArrayToDashStyle(byte[] Value)
        {
            if (Value == null || Value.Length == 0)
                return null;

            if (Value.Length != 1 || Value[0] > DeclaredDashStyles.Count)
                throw new UsageAnomaly("Cannot convert the supplied byte-array to DashStyle", Value);

            return DeclaredDashStyles[Value[0]].Item1;
        }

        public static byte[] ConvertDashStyleToByteArray(DashStyle Value)
        {
            if (Value == null)
                return null;

            return ((byte)DeclaredDashStyles.IndexOfMatch(val => val.Item1.IsEqual(Value))).IntoArray();
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the TreeViewItem containing the searched Item, within the complete sub-hierarchy.
        /// </summary>
        public static TreeViewItem FindContainerByItemInAllChildren(this ItemsControl Source, object SearchedItem)
        {
            foreach (var ChildItem in Source.Items)
            {
                var TvItem = Source.ItemContainerGenerator.ContainerFromItem(ChildItem) as TreeViewItem;
                if (ChildItem.IsEqual(SearchedItem))
                    return TvItem;
                else
                    if (TvItem != null && TvItem.HasItems)
                    {
                        var Result = FindContainerByItemInAllChildren(TvItem, SearchedItem);
                        if (Result != null)
                            return Result;
                    }
            }

            return null;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Calls, for this Source object thread-dispatcher, the supplied operation in asynchronous way (plus passing the source to the operation).
        /// Optionally an indication of using the lowest priority (Background) can be specified (else uses Loaded).
        /// </summary>
        public static void PostCall<TSource>(this TSource Source, Action<TSource> Operation,
                                             bool UseLowestPriority = false) where TSource : DispatcherObject
        {
            /*T DispatchId++;
            Console.Write("Dispatch [{0}] (ID={1}) -> {2}", DateTime.Now.ToString("hh:mm:ss.fff"), DispatchId, Source.ToStringAlways());
            var LocalDispatchId = Tuple.Create(DispatchId); */

            // IMPORTANT: Original priority was 'Background', NEVER USE 'Normal' or less.
            // (some events can be fired out of sync, example: Initial View centering)
            Source.Dispatcher.BeginInvoke((UseLowestPriority ? DispatcherPriority.Background : DispatcherPriority.Loaded),
                new DispatcherOperationCallback(delegate(Object Parameter)
                                                {
                                                    //T Console.WriteLine(">> Dispatched Id={0}...", LocalDispatchId.Item1);
                                                    Operation(Source);
                                                    //T Console.WriteLine("<< ...Processed Id={0}", LocalDispatchId.Item1);
                                                    return null;
                                                }),
                null);

            //T Console.WriteLine(". Invoking! (ID={0})", DispatchId);
        }
        //T private static int DispatchId = 0;

        //------------------------------------------------------------------------------------------
        public const byte IMAGETYPE_BITMAP = ((byte)'B');
        public const byte IMAGETYPE_DRAWING = ((byte)'D');

        /// <summary>
        /// Return this Image Data bytes-array (optionally preceded by an image-type byte prefix) as Image Source.
        /// </summary>
        public static ImageSource ToImageSource(this byte[] ImageData, bool HasImageTypePrefix = false)
        {
            if (ImageData == null || ImageData.Length <= 1)
                return null;

            byte ImageType = (HasImageTypePrefix ? ImageData[0] : IMAGETYPE_BITMAP);
            byte[] Source = (HasImageTypePrefix ? ImageData.ExtractSegment(1) : ImageData);

            try
            {
                if (ImageType == IMAGETYPE_DRAWING)
                {
                    var SerializedResult = BytesHandling.Decompress(Source);
                    var XamlResult = BytesHandling.DeserializeFromBytes<string>(SerializedResult);
                    var ObjectResult = XamlReader.Parse(XamlResult);
                    var Result = ObjectResult as DrawingImage;

                    return Result;
                }
                else
                {
                    var Result = new BitmapImage();

                    Result.BeginInit();
                    Result.StreamSource = new MemoryStream(Source);
                    Result.EndInit();

                    return Result;
                }
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Image cannot be read. Problem: {0}", Problem.Message);
                return null;
            }
        }

        /// <summary>
        /// Return this Image Source as bytes-array (optionally preceded by an image-type byte prefix).
        /// </summary>
        public static byte[] ToBytes(this ImageSource Source, bool PutImageTypePrefix = false)
        {
            if (Source == null)
                return null;

            byte[] Result = null;
            byte Prefix = 0;

            BitmapSource BitmappedSource = Source as BitmapSource; // BitmapImage;
            if (BitmappedSource == null)
                BitmappedSource = Source as RenderTargetBitmap;

            if (BitmappedSource != null)
            {
                var Torrent = new MemoryStream();
                var Converter = new PngBitmapEncoder(); // Supports transparency

                Converter.Frames.Add(BitmapFrame.Create(BitmappedSource));
                Converter.Save(Torrent);

                Result = Torrent.GetBuffer();
                Prefix = IMAGETYPE_BITMAP;
            }
            else
            {
                var DrawingSource = Source as DrawingImage;
                if (DrawingSource != null)
                {
                    var XamlResult = XamlWriter.Save(DrawingSource);
                    var SerializedResult = BytesHandling.SerializeToBytes(XamlResult);
                    Result = BytesHandling.Compress(SerializedResult);
                    Prefix = IMAGETYPE_DRAWING;
                }
                else
                {
                    try
                    {
                        var TempImage = new Image();
                        TempImage.Source = Source;
                        var Renderer = TempImage.RenderToBitmap((int)Source.GetWidth(), (int)Source.GetHeight());
                        Result = Renderer.ToBytes(PutImageTypePrefix);
                    }
                    catch (Exception Problem)
                    {
                        Console.WriteLine("Cannot convert image to Bitmap. Problem: " + Problem.Message);
                        return null;
                    }
                }
            }

            if (PutImageTypePrefix)
                Result = BytesHandling.FusionateByteArrays(Prefix.IntoArray(), Result);

            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the center point between this supplied source and target points.
        /// </summary>
        public static Point DetermineCenterRespect(this Point SourcePosition, Point TargetPosition)
        {
            return (new Point((SourcePosition.X + TargetPosition.X) / 2.0,
                              (SourcePosition.Y + TargetPosition.Y) / 2.0));
        }

        /// <summary>
        /// Indicates whether this Source point is "near" a supplied Target one.
        /// Optionally a Range for evaluating the coordinates proximity can be specified.
        /// </summary>
        public static bool IsNear(this Point Source, Point Target, double Range = POINT_NEAR_TOLERANCE)
        {
            return ((Source.X >= Target.X - Range && Source.X <= Target.X + Range) &&
                    (Source.Y >= Target.Y - Range && Source.Y <= Target.Y + Range));
        }

        /// <summary>
        /// Converts this supplied Extent to its scaled Size based on the specified Reference Size.
        /// If the supplied extent is already positive, then is assumed a scaling factor and returns the result of apply it to the reference size.
        /// else -if the supplied extent is negative- then it is considered as size and the same unchanged number is returned.
        /// </summary>
        public static double ExtentToSize(this double Extent, double ReferenceSize)
        {
            if (Extent >= 0)
                return (Extent * ReferenceSize);

            return (Extent * -1);
        }

        /// <summary>
        /// Converts this supplied Extent factor to Scale factor based on the specified Reference Size.
        /// If the supplied extent is already positive, then is assumed as scale and the same unchanged number is returned,
        /// else -if the supplied extent is negative- then it is considered as size, and returns its scale respect the reference size.
        /// </summary>
        public static double ExtentToScale(this double Extent, double ReferenceSize)
        {
            if (Extent < 0)
                return ((Extent * -1.0) / ReferenceSize);

            return Extent;
        }

        /// <summary>
        /// Returns this supplied Size to a Scale factor based on the specified Reference Size.
        /// </summary>
        public static double SizeToScale(this double Size, double ReferenceSize)
        {
            return (Size / ReferenceSize);
        }

        /// <summary>
        /// Returns true when no member property of the Source rect is Infinity or Nan.
        /// </summary>
        public static bool IsValid(this Rect Source)
        {
            var Result = (!double.IsInfinity(Source.Width) && !Source.Width.IsNan()
                          && !double.IsInfinity(Source.Height) && !Source.Height.IsNan()
                          && !double.IsInfinity(Source.X) && !Source.X.IsNan()
                          && !double.IsInfinity(Source.Y) && !Source.Y.IsNan());

            return Result;
        }

        /// <summary>
        /// For this specified Target, sets its Visibility, considering if Invisibility means Collapsed (the default, else Hidden).
        /// Returns the applied Visibility.
        /// </summary>
        public static Visibility SetVisible(this UIElement Target, bool IsVisible, bool InvisibleIsCollapsed = true)
        {
            Target.Visibility = (IsVisible ? Visibility.Visible : (InvisibleIsCollapsed ? Visibility.Collapsed : Visibility.Hidden));

            return Target.Visibility;
        }

        /// <summary>
        /// For this specified Target, sets its availability appearance as an opacity level applied.
        /// </summary>
        public static void SetAvailable(this UIElement Target, bool IsAvailable, double OpacityLevel = VISUAL_UNAVAILABILITY_OPACITY)
        {
            Target.Opacity = (IsAvailable ? 1.0 : OpacityLevel);
        }

        /// <summary>
        /// Pending: TEST!.
        /// Determines whether this Scroller is showing -completely- the Target UI-Element object or an optional specified Area within that Target.
        /// </summary>
        public static bool IsShowing(this ScrollViewer Scroller, UIElement Target, Rect TargetArea = default(Rect))
        {
            // position of your visual inside the scrollviewer
            GeneralTransform childTransform = Target.TransformToAncestor(Scroller);

            if (TargetArea == default(Rect))
                TargetArea = new Rect(ZERO_POINT, Target.RenderSize);

            Rect TransformedArea = childTransform.TransformBounds(TargetArea);

            //Check if the elements Rect intersects with that of the scrollviewer's
            Rect result = Rect.Intersect(new Rect(ZERO_POINT, Scroller.RenderSize), TransformedArea);

            //if result is Empty then the element is not in view
            //else obj is partially Or completely visible
            return (result!=Rect.Empty);
        }

        /// <summary>
        /// Creates a transform based on the position, scaling and rotation (angle and center) parameters.
        /// Notes:
        /// - For Scaling: Scaling is made using the supplied position as center.
        /// - For Rotation: If no center points are specified, then the supplied position is used.
        /// Returns null if no transform is needed.
        /// </summary>
        public static Transform CreateTransform(double OffsetX, double OffsetY,
                                                double ScaleX = 1.0, double ScaleY = 1.0,
                                                double RotationAngle = 0, double RotationCenterX = double.NaN, double RotationCenterY = double.NaN)
        {
            var Transformations = new TransformGroup();

            var Translation = (OffsetX != 0 || OffsetY != 0 ? new TranslateTransform(OffsetX, OffsetY) : null);
            if (Translation != null)
                Transformations.Children.Add(Translation);

            var Scaling = (ScaleX != 1.0 || ScaleY != 1.0 ? new ScaleTransform(ScaleX, ScaleY, OffsetX, OffsetY) : null);
            if (Scaling != null)
                Transformations.Children.Add(Scaling);

            //- RotationCenterX = ( RotationCenterX.IsNan() ? OffsetX : RotationCenterX);
            //- RotationCenterY = ( RotationCenterY.IsNan() ? OffsetY : RotationCenterY);
            var Rotating = (RotationAngle != 0 ? new RotateTransform(RotationAngle, RotationCenterX, RotationCenterY) : null);
            if (Rotating != null)
                Transformations.Children.Add(Rotating);

            if (Transformations.Children.Count < 1)
                return null;

            if (Transformations.Children.Count == 1)
                return (Transformations.Children[0]);

            return Transformations;
        }

        /// <summary>
        /// Gets the nearest dominant (parent) object of the supplied type,
        /// inside the containment hierarchy, for this FrameworkElement object.
        /// Optionally, it can be indicated to consider evaluate this same instance.
        /// </summary>
        public static TFind GetNearestDominantOfType<TFind>(this FrameworkElement TargetInstance,
                                                            bool ConsiderThisInstance = false)
            where TFind : class
        {
            if (TargetInstance == null)
                return null;

            FrameworkElement Target = (ConsiderThisInstance ? TargetInstance : TargetInstance.Parent as FrameworkElement);

            while (Target != null)
                if (Target is TFind)
                    return Target as TFind;
                else
                    Target = Target.Parent as FrameworkElement;

            return null;
        }

        /// <summary>
        /// Gets the nearest visual dominant (parent) object of the supplied type,
        /// inside the Visual-Tree hierarchy, for this FrameworkElement object.
        /// Optionally, it can be indicated to consider evaluate this same instance.
        /// </summary>
        public static TFind GetNearestVisualDominantOfType<TFind>(this DependencyObject TargetInstance,
                                                                  bool ConsiderThisInstance = false)
            where TFind : class
        {
            // Exit if TargetInstance is null or not a Visual (e.g. a text Run, a HyperLink, etc.)
            if (TargetInstance == null || !(TargetInstance is Visual))
                return null;

            var Target = (ConsiderThisInstance ? TargetInstance : VisualTreeHelper.GetParent(TargetInstance));

            while (Target != null)
                if (Target is TFind)
                    return Target as TFind;
                else
                    Target = VisualTreeHelper.GetParent(Target);

            return null;
        }

        /// <summary>
        /// Returns all the Bitmap-Images found in the application filtered by the specified resource name filter (i.e. ".png")
        /// </summary>
        public static IEnumerable<BitmapImage> GetAllImages(Func<string,bool> ResourceNameFilter)
        {
            var Resources = Display.GetAllResourceLocations(ResourceNameFilter);

            foreach (var Location in Resources)
            {
                BitmapImage Image = null;

                try
                {
                    Image = new BitmapImage(new Uri(Location));
                }
                catch { }

                if (Image != null)
                    yield return Image;
            }
        }

        /// <summary>
        /// Returns the locations of the resources of the application filtered by the specified resource name filter (i.e. ".png")
        /// </summary>
        public static IEnumerable<string> GetAllResourceLocations(Func<string, bool> ResourceNameFilter = null)
        {
            var AppAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(asm => asm.FullName.Contains(AppExec.BASE_NAMESPACE));

            foreach (var Asm in AppAssemblies)
            {
                var Resources = (ResourceNameFilter == null ? Display.GetAllResourceNames(Asm)
                                                            : Display.GetAllResourceNames(Asm).Where(ResourceNameFilter));

                // Sample of Uri to conform: "pack://application:,,,/Instrumind.ThinkComposer;component/ApplicationProduct/Images/sample.png"
                var Prefix = "pack://application:,,,/" + Asm.GetName().Name + ";component/";

                foreach (var Resource in Resources)
                {
                    var Location = Prefix + Resource;
                    yield return Location;
                }
            }
        }

        /// <summary>
        /// Returns the names of the resources of the suppplied Assembly
        /// </summary>
        public static IEnumerable<string> GetAllResourceNames(Assembly Source)
        {
            string resName = Source.GetName().Name + ".g.resources";

            using (var stream = Source.GetManifestResourceStream(resName))
                using (var reader = new System.Resources.ResourceReader(stream))
                {
                    if (reader != null)
                        foreach(var Entry in reader)
                        {
                            var Value = (DictionaryEntry)Entry;
                            var Result = Value.Key.ToString();
                            yield return Result;
                        }
                }
        }

        /// <summary>
        /// Gets the resource of the Definitor type, identified by the supplied Resource-Id, casted to the specified Resource type, from the current application.
        /// Optionally, if the resource is Freezable and is frozen, a clone will be returned.
        /// </summary>
        public static TResource GetResource<TResource, TDefinitor>(object ResourceId, bool ReturnCloneIfFrozen = false)
                where TResource : class
        {
            var ResourceKey = new ComponentResourceKey(typeof(TDefinitor), ResourceId);
            return GetResource<TResource>(ResourceKey, ReturnCloneIfFrozen);
        }

        /// <summary>
        /// Gets the resource identified by the supplied Key, casted to the specified type, from the current application.
        /// Optionally, if the resource is Freezable and is frozen, a clone will be returned.
        /// </summary>
        public static TResource GetResource<TResource>(object Key, bool ReturnCloneIfFrozen = false)
                where TResource : class
        {
            TResource Result = null;
            object Resource = null;

            Resource = Application.Current.TryFindResource(Key);

            if (ReturnCloneIfFrozen)
            {
                var FreezableRes = Resource as Freezable;

                if (FreezableRes != null && FreezableRes.IsFrozen)
                    Result = FreezableRes.Clone() as TResource;
            }

            if (Result == null && Resource != null)
                if (Resource is TResource)
                    Result = Resource as TResource;
                else
                    throw new UsageAnomaly("Cannot find resource having the specified type.", typeof(TResource));

            return Result;
        }

        /// <summary>
        /// Returns the embedded resource string, specified by Resource-Key, from the current or specified assembly.
        /// If no resource is found, then returns null.
        /// </summary>
        // Example key: Instrumind.ThinkComposer.ApplicationProduct.LanguageSyntaxes.TcTemplate.xshd
        public static string GetEmbeddedResourceString(string ResourceKey, Assembly SourceAssembly = null)
        {
            var Torrent = SourceAssembly.NullDefault(System.Reflection.Assembly.GetCallingAssembly())
                            .GetManifestResourceStream(ResourceKey);
            var Result = Torrent.StreamToString();
            return Result;
        }

        /// <summary>
        /// Returns an ImageSource from the Instrumind Common Library's resource name supplied or its previously cached instance.
        /// Optionally an exclusive (non cached) instance can be explicitly required.
        /// </summary>
        public static ImageSource GetLibImage(string ResourceName, bool RequiresExclusiveInstance = false, bool ResourceNameIsRoute = false)
        {
            string LoadRoute = ResourceName;

            if (!ResourceNameIsRoute)
            {
                if (!ResourceName.StartsWith("/") && !ResourceName.StartsWith("pack:"))
                {
                    LoadRoute = LibImagesRoute + ResourceName;
                    ResourceName = "\\" + ResourceName;
                }

                LoadRoute = "pack://application:,,," + LoadRoute;
            }

            var Location = new Uri(LoadRoute);

            if (RequiresExclusiveInstance)
                return CreateImageFromResourceUri(Location);

            var Result = CachedImages.GetOrAdd(ResourceName, CreateImageFromResourceUri(Location));
            return Result;
        }

        /// <summary>
        /// Returns an ImageSource from the Application's resource name supplied or its previously cached instance.
        /// Optionally an exclusive (non cached) instance can be explicitly required.
        /// </summary>
        public static ImageSource GetAppImage(string ResourceName, bool RequiresExclusiveInstance = false, bool ResourceNameIsRoute = false)
        {
            string LoadRoute = ResourceName;

            if (!ResourceNameIsRoute)
            {
                if (!ResourceName.StartsWith("/") && !ResourceName.StartsWith("pack:"))
                    LoadRoute = AppImagesRoute.AbsentDefault(LibImagesRoute) + ResourceName;

                LoadRoute = "pack://application:,,," + LoadRoute;
            }

            var Location = new Uri(LoadRoute);

            if (RequiresExclusiveInstance)
                return CreateImageFromResourceUri(Location);

            var Result = CachedImages.GetOrAdd(ResourceName, CreateImageFromResourceUri(Location));
            return Result;
        }

        /// <summary>
        /// Base route for the Instrumind Common Library's images.
        /// </summary>
        public static readonly string LibImagesRoute = "/Instrumind.Common;component/Visualization/Images/";

        /// <summary>
        /// Standard route for the Application's (product) images.
        /// </summary>
        public static string AppImagesRoute;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns an ImageSource from the External (outside the Application) route supplied or its previously cached instance.
        /// Optionally a cached (non exlusive) instance can be explicitly required.
        /// </summary>
        // IMPORTANT: Previously, de default behaviour was using a cached image,
        //            but was changed because users can edit image and reload images.
        public static ImageSource ImportImageFrom(string ImageRoute, bool RequiresCachedInstance = false)
        {
            var Location = new Uri(ImageRoute);

            try
            {
                if (!RequiresCachedInstance)
                    return ConvertByteArrayToImageSource(General.FileToBytes(ImageRoute));

                return CachedImages.GetOrAdd(ImageRoute, ConvertByteArrayToImageSource(General.FileToBytes(ImageRoute)));
            }
            catch(Exception Problem)
            {
                Console.WriteLine("Cannot import image file.\nProblem: " + Problem.Message);
            }

            return null;
        }

        /// <summary>
        /// Creates and returns an image from the supplied Resource Uri, or null when failed.
        /// </summary>
        private static ImageSource CreateImageFromResourceUri(Uri Location)
        {
            ImageSource Result = null;

            try
            {
                Result = new BitmapImage(Location);
            }
            catch (Exception Problem)
            {
                AppExec.LogException(Problem);
            }

            return Result;
        }

        /// <summary>
        /// Local cache for instantiated images.
        /// </summary>
        internal static ConcurrentDictionary<string, ImageSource> CachedImages = new ConcurrentDictionary<string, ImageSource>();

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the nearest child having the specified TRet type for the supplied Target.
        /// </summary>
        public static TRet GetVisualChild<TRet>(DependencyObject Target) where TRet : DependencyObject
        {
            if (Target == null)
                return null;

            for (int ChildIndex = 0; ChildIndex < VisualTreeHelper.GetChildrenCount(Target); ChildIndex++)
            {
                var Child = VisualTreeHelper.GetChild(Target, ChildIndex);

                if (Child != null && Child is TRet)
                    return (TRet)Child;
                else
                {
                    TRet childOfChild = GetVisualChild<TRet>(Child);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns a new double, based on this supplied one, with its value modified by the specified scaling percentaje.
        /// Optionally, it can be specified to inverse the scale.
        /// </summary>
        public static double ApplyScalePercent(this double Value, double Percent, bool Inverse = false)
        {
            return (Inverse ? Value * (100.0 / Percent) : Value * Percent / 100.0);
        }

        /// <summary>
        /// Returns a new double, based on this supplied one, with its value multiplied by the specified scaling factor.
        /// Optionally, it can be specified to inverse the scale.
        /// </summary>
        public static double ApplyScaleFactor(this double Value, double Factor, bool Inverse = false)
        {
            return (Inverse ? Value * (1.0 / Factor) : Value * Factor);
        }

        /// <summary>
        /// Returns a new point, based on this supplied one, with its coordinates modified by the specified scaling percentaje.
        /// Optionally, it can be specified to inverse the scale.
        /// </summary>
        public static Point ApplyScalePercent(this Point Target, double Percent, bool Inverse = false)
        {
            return (new Point(Target.X.ApplyScalePercent(Percent, Inverse),
                              Target.Y.ApplyScalePercent(Percent, Inverse)));
        }

        /// <summary>
        /// Returns a new point, based on this supplied one, with its coordinates multiplied by the specified scaling factor.
        /// Optionally, it can be specified to inverse the scale.
        /// </summary>
        public static Point ApplyScaleFactor(this Point Target, double Factor, bool Inverse = false)
        {
            return (new Point(Target.X.ApplyScaleFactor(Factor, Inverse),
                              Target.Y.ApplyScaleFactor(Factor, Inverse)));
        }

        /// <summary>
        /// Gets a Hit-Test-Result from the Hit-Testing on the Context-Reference exactly over the Target-Point and the nearest until find a dependency object.
        /// Plus, it can omit the supplied Ignorable object which is returned if no other is found.
        /// The nearest evaluated points are within a detection-radius clockwise (from clock position "4:30").
        /// </summary>
        // Note: The "clock" starts at 4:30 because a mouse arrow pointer tipycally covers (is over) that point
        public static HitTestResult HitTestNear(Visual ContextReference, Point TargetPoint,
                                                Visual Ignorable = null, double DetectionRadius = POINT_NEAR_TOLERANCE)
        {
            var Result = VisualTreeHelper.HitTest(ContextReference, TargetPoint);
            if (Result != null && Result.VisualHit != Ignorable)
                return Result;

            Result = null;
            var RadialIndex = 0;
            var DiagonalFactor = 0.5; // more "circular": 0.66

            while ((Result == null || Result.VisualHit == Ignorable) && RadialIndex < 4)
            {
                if (RadialIndex == 0) TargetPoint = new Point(TargetPoint.X + DetectionRadius * DiagonalFactor, TargetPoint.Y + DetectionRadius * DiagonalFactor);
                // if (RadialIndex == 1) TargetPoint = new Point(TargetPoint.X, TargetPoint.Y + DetectionRadius);
                if (RadialIndex == 1) TargetPoint = new Point(TargetPoint.X - DetectionRadius * DiagonalFactor, TargetPoint.Y + DetectionRadius * DiagonalFactor);
                // if (RadialIndex == 2) TargetPoint = new Point(TargetPoint.X - DetectionRadius, TargetPoint.Y);
                if (RadialIndex == 2) TargetPoint = new Point(TargetPoint.X - DetectionRadius * DiagonalFactor, TargetPoint.Y - DetectionRadius * DiagonalFactor);
                // if (RadialIndex == 5) TargetPoint = new Point(TargetPoint.X, TargetPoint.Y - DetectionRadius);
                if (RadialIndex == 3) TargetPoint = new Point(TargetPoint.X + DetectionRadius * DiagonalFactor, TargetPoint.Y - DetectionRadius * DiagonalFactor);
                // if (RadialIndex == 7) TargetPoint = new Point(TargetPoint.X + DetectionRadius, TargetPoint.Y);

                Result = VisualTreeHelper.HitTest(ContextReference, TargetPoint);

                RadialIndex++;
            }

            /*T if (RadialIndex < 4)
                Console.WriteLine("{0} Match near {1}: {2}", DateTime.Now, RadialIndex, (Result == null ? null : Result.VisualHit)); */

            return Result;
        }

        /// <summary>
        /// Indicates whether this Visual has the specified Target dependency Object with the supplied Location point within.
        /// (The result is determined applying hit-testing just for top-most matches).
        /// </summary>
        public static bool ContainsObjectWithPoint(this Visual Presenter, DependencyObject Target, Point Location)
        {
            if (Presenter == null || Target == null)
                return false;

            var Found = false;
            var Selector = new HitTestResultCallback(
                                    evres =>
                                    {
                                        if (evres.VisualHit != Target)
                                            return HitTestResultBehavior.Continue;

                                        Found = true;
                                        return HitTestResultBehavior.Stop;
                                    });

            VisualTreeHelper.HitTest(Presenter, null, Selector, new PointHitTestParameters(Location));

            //T Console.WriteLine("Evaluated={0}, Pointed={1}, Location={2}", Target.GetHashCode(), Hit.VisualHit.GetHashCode(), Location);
            return Found;
        }

        /// <summary>
        /// For a supplied Target, within a Context, indicates whether the mouse pointer position is within it.
        /// </summary>
        public static bool IsUnderPosition(this Visual Target, UIElement Context)
        {
            return IsUnderPosition(Target, Context, Mouse.GetPosition(Context));
        }

        /// <summary>
        /// For a supplied Target, within a Context, indicates whether the supplied Position is within it.
        /// </summary>
        public static bool IsUnderPosition(this Visual Target, UIElement Context, Point Position)
        {
            bool WasFound = false;

            HitTestFilterCallback HitTestOperation_EvaluateAll = delegate(DependencyObject Subject)
            {
                if (Subject == Target)
                {
                    WasFound = true;
                    return HitTestFilterBehavior.Stop;
                }

                return HitTestFilterBehavior.Continue;
            };

            HitTestResultCallback HitTestOperation_InmediateReturn = delegate(HitTestResult Result)
            {
                return HitTestResultBehavior.Stop;
            };

            var HitTestParams = new PointHitTestParameters(Position);
            VisualTreeHelper.HitTest(Target, HitTestOperation_EvaluateAll, HitTestOperation_InmediateReturn, HitTestParams);

            return WasFound;
        }

        #region IMAGE TECHNIQUES

        // SEE WritableBitmap MSDN example FOR IMAGE PROPAGATING CHANGES?

        /*
        private void btn_Click(object sender, RoutedEventArgs e)
        {
            string filepath = "c:\\1.jpg";
            FileStream stream = File.Open(filepath, FileMode.Open);
            imgTitle.Source = GetImage(stream);
            MessageBox.Show(GetImageType(stream));
        }
        public BitmapImage GetImage( Stream iconStream )
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(iconStream);
            var imgbrush = new BitmapImage();
            imgbrush.BeginInit();
            imgbrush.StreamSource = ConvertImageToMemoryStream( img );
            imgbrush.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            imgbrush.EndInit();
            var ib = new ImageBrush(imgbrush);
            return imgbrush;
        }
        public string GetImageType( Stream iconStream )
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(iconStream);
            System.Drawing.Imaging.ImageFormat bmpFormat = img.RawFormat;
            string strFormat = "unidentified format";
            if ( bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp)) strFormat = "BMP";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Emf)) strFormat = "EMF";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Exif)) strFormat = "EXIF";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif)) strFormat = "GIF";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Icon)) strFormat = "Icon";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)) strFormat = "JPEG";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp)) strFormat = "MemoryBMP";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Png)) strFormat = "PNG";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff)) strFormat = "TIFF";
            else if (bmpFormat.Equals(System.Drawing.Imaging.ImageFormat.Wmf)) strFormat = "WMF";
            return strFormat;
        }
        public  MemoryStream ConvertImageToMemoryStream(System.Drawing.Image img)
        {
            var ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms;
        }

         */

        #endregion

        /// <summary>
        /// Converts this GDI+ Bitmap to WPF BitmapSource.
        /// </summary>
        public static BitmapSource ConvertToBitmapSource(this System.Drawing.Bitmap Picture)
        {
            var Result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(Picture.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty,
                                                                                      BitmapSizeOptions.FromWidthAndHeight(Picture.Width, Picture.Height));
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Takes and screenshot from the specified source web-url, considering widht and height, applying the resulting bitmap-source image with the specified Setter.
        /// </summary>
        public static void GenerateWebScreenshot(string SourceWebURL, int Width, int Height, Action<BitmapSource> Setter)
        {
            if (SourceWebURL.IsAbsent() || Width < 1 || Height < 1 || Setter == null)
                return;

            var Browser = WorkingWebBrowser.NullDefault(new System.Windows.Forms.WebBrowser());

            Browser.DocumentCompleted +=
                (sender, args) =>
                {
                    try
                    {
                        System.Drawing.Bitmap Picture = new System.Drawing.Bitmap(Width, Height);
                        Browser.DrawToBitmap(Picture, new System.Drawing.Rectangle(0, 0, Width, Height));
                        Setter(Picture.ConvertToBitmapSource());
                    }
                    catch (Exception Problem)
                    {
                        Console.WriteLine("Cannot capture screenshot from Url: " + SourceWebURL +
                                          ".\nProblem: " + Problem.Message);
                    }
                };

            Browser.Navigate(SourceWebURL);
        }
        private static System.Windows.Forms.WebBrowser WorkingWebBrowser = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Export, to the specified Route, the supplied Source-Visual with the specified Width and Height.
        /// Returns message if error occurred.
        /// </summary>
        public static string ExportImageTo(string ImageRoute, Visual SourceVisual, int Width, int Height)
        {
            var Extension = Path.GetExtension(ImageRoute).ToLower();
            
            if (!Extension.IsAbsent())
                Extension = Extension.Substring(1)
                                .Replace("jpeg", "jpg").Replace("jpe", "jpg")
                                .Replace("ico", "bmp").Replace("dib", "bmp")
                                .Replace("tiff", "tif");

            BitmapEncoder Encoder = null;

            switch (Extension)
            {
                case "jpg":
                    var JpgEncoder = new JpegBitmapEncoder();
                    JpgEncoder.QualityLevel = DEF_JPEG_QUALITY;
                    Encoder = JpgEncoder;
                    break;

                case "png":
                    Encoder = new PngBitmapEncoder();
                    break;

                case "gif":
                    Encoder = new GifBitmapEncoder();
                    break;

                case "tif":
                    Encoder = new TiffBitmapEncoder();
                    break;

                case "bmp":
                    Encoder = new BmpBitmapEncoder();
                    break;

                default:
                    Encoder = new PngBitmapEncoder();
                    Console.WriteLine("Unrecognized image extension '" + Extension + "'. Image exported in PNG format.");
                    break;
            }

            string Result = null;
            using (var TargetFile = new FileStream(ImageRoute, FileMode.Create))
                Result = ExportImageTo(Encoder, TargetFile, SourceVisual, Width, Height);

            return Result;
        }

        /// <summary>
        /// Export, using the specified Encoder to the specified Stream, the supplied Source-Visual with the specified Width and Height.
        /// The Stream must be open at the begining and will be closed and flushed on exit.
        /// Returns message if error occurred.
        /// </summary>
        public static string ExportImageTo(BitmapEncoder Encoder, Stream ExportStream, Visual SourceVisual, int Width, int Height)
        {
            // NOTE: RENDER THE COMPOSITION-VIEW DIRECTLY TO HERE

            var WorkingBitmap = SourceVisual.RenderToBitmap(Width, Height);

            Encoder.Frames.Clear();
            Encoder.Frames.Add(BitmapFrame.Create(WorkingBitmap));
            Encoder.Save(ExportStream);

            ExportStream.Flush();
            ExportStream.Close();

            return null;
        }

        /// <summary>
        /// Returns a RenderTargetBitmap of the visual Source with the specified Width and Height.
        /// </summary>
        public static RenderTargetBitmap RenderToBitmap(this Visual Source, int Width, int Height)
        {
            var Result = new RenderTargetBitmap(Width, Height, WPF_DPI, WPF_DPI, PixelFormats.Default);

            Result.Render(Source);
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void RegisterSyntaxLanguage(string LanguageName, string Extension, string SyntaxDefinitionText)
        {
            try
            {
                using (var PrimaryReader = new StringReader(SyntaxDefinitionText))
                using (var SecondaryReader = new System.Xml.XmlTextReader(PrimaryReader))
                {
                    var IntermediateDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.LoadXshd(SecondaryReader);

                    var SyntaxDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader
                                            .Load(IntermediateDefinition, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);

                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance
                        .RegisterHighlighting(LanguageName, Extension.IntoArray(), SyntaxDefinition);
                }
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot register syntax for the '" + LanguageName + "' language.\nProblem: " + Problem.Message);
            }
        }

        // -------------------------------------------------------------------------------------------
        /// <summary>
        /// Shows the context menu with the supplied Options, if none is specified then shows the last used.
        /// Returns the generated context menu.
        /// </summary>
        public static ContextMenu DisplayContextMenu<TTarget>(this FrameworkElement VisualExpositor, TTarget Target,
                                                              IEnumerable<Tuple<SimplePresentationElement, Func<TTarget, FrameworkElement, bool?>, Action<TTarget>>> Options = null,
                                                              Action CloseAction = null)
            where TTarget : UniqueElement
        {
            return DisplayContextMenu(VisualExpositor, Target, Options.Select(opt => (opt == null ? null :
                    new Tuple<SimplePresentationElement, Func<TTarget, FrameworkElement, bool?>, Action<TTarget, SimplePresentationElement>, List<SimplePresentationElement>>(
                                opt.Item1, opt.Item2, (target, selection) => opt.Item3(target), null))), CloseAction);
        }

        /// <summary>
        /// Shows the context menu with the supplied Options and Sub-Options, if none is specified then shows the last used.
        /// Returns the generated context menu.
        /// </summary>
        public static ContextMenu DisplayContextMenu<TTarget>(this FrameworkElement VisualExpositor, TTarget Target,
                                                              IEnumerable<Tuple<SimplePresentationElement, Func<TTarget, FrameworkElement, bool?>,
                                                                                Action<TTarget, SimplePresentationElement>, List<SimplePresentationElement>>> Options = null,
                                                              Action CloseAction = null)
            where TTarget : UniqueElement
        {
            if (Options == null || !Options.Any())
            {
                VisualExpositor.ContextMenu = null;
                return null;
            }

            if (VisualExpositor.ContextMenu == null)
            {
                var Menu = new ContextMenu();
                Menu.HasDropShadow = true;
                Menu.Closed += ((sdr, args) =>
                                {
                                    var CloseOp = Menu.Tag as Action;
                                    if (CloseOp != null)
                                        CloseOp();
                                });

                VisualExpositor.ContextMenu = Menu;
            }

            VisualExpositor.ContextMenu.Items.Clear();
            VisualExpositor.ContextMenu.Tag = CloseAction;

            bool ForbidSeparator = true;
            foreach (var Option in Options)
            {
                object NewItem = null;

                if (Option == null)
                {
                    if (ForbidSeparator)
                        continue;

                    NewItem = new Separator();
                    ForbidSeparator = true;
                }
                else
                {
                    var Exposability = Option.Item2(Target, VisualExpositor);
                    if (Exposability == null)
                        continue;

                    var NewMenuItem = new MenuItem();
                    NewMenuItem.Icon = new ImprovedImage { Source = Option.Item1.Pictogram };
                    NewMenuItem.Header = Option.Item1.Name;
                    NewMenuItem.ToolTip = Option.Item1.Summary;
                    NewMenuItem.IsEnabled = (Option.Item2 == null || Exposability.IsTrue());
                    NewMenuItem.Tag = Option.Item3;
                    var IsolatedOption = Option; // For the RoutedEventHandler not be entangled with the first Lambda assigned

                    if (IsolatedOption.Item4 == null)
                        NewMenuItem.Click += new RoutedEventHandler((obj, args) => IsolatedOption.Item3(Target, null));
                    else
                    {
                        foreach (var SubOption in IsolatedOption.Item4)
                        {
                            var NewSubmenuItem = new MenuItem();
                            NewSubmenuItem.Header = SubOption.Name;
                            NewMenuItem.Items.Add(NewSubmenuItem);
                            NewMenuItem.Click += new RoutedEventHandler((obj, args) => IsolatedOption.Item3(Target, SubOption));
                        }
                    }

                    NewItem = NewMenuItem;
                    ForbidSeparator = false;
                }

                VisualExpositor.ContextMenu.Items.Add(NewItem);
            }

            VisualExpositor.ContextMenu.IsOpen = true;

            // Discard the menu in order to not be shown after Pan() or interfere with other commands
            VisualExpositor.PostCall(vpres => vpres.ContextMenu = null);

            return VisualExpositor.ContextMenu;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
