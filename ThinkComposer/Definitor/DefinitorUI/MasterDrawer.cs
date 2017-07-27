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
// File   : MasterDrawer.cs
// Object : Instrumind.ThinkComposer.Definitor.DefinitorUI.MasterDrawer (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.03 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.ApplicationProduct;

/// Provides user-interface common services, plus the components for the Domain related Definitions.
namespace Instrumind.ThinkComposer.Definitor.DefinitorUI
{
    /// <summary>
    /// Provides general services for drawing visual representations
    /// </summary>
    public static class MasterDrawer
    {
        /// Height of the base for drawing (canvas).
        public const double BASE_HEIGHT = 16.0;

        /// <summary>
        /// Width of the base for drawing (canvas).
        /// </summary>
        public const double BASE_WIDTH = 48.0;

        /// <summary>
        /// Height for sample Symbols.
        /// </summary>
        public const double SAMPLE_SYMB_HEIGHT = 13.0;

        /// <summary>
        /// Width for sample Symbols.
        /// </summary>
        public const double SAMPLE_SYMB_WIDTH = 40.0;

        /// <summary>
        /// Height for sample Connectors.
        /// </summary>
        public const double SAMPLE_CONN_HEIGHT = 9.0;

        /// <summary>
        /// Width for Central/Main-Symbols of sample Connectors.
        /// </summary>
        public const double SAMPLE_CONN_WIDTH_CSYM = 17.0;

        /// <summary>
        /// Offset point of sample for cursor.
        /// </summary>
        public static readonly Point SAMPLE_CURSOR_OFFSET = new Point(2, 8);

        /// <summary>
        /// Target point of sample Connectors.
        /// </summary>
        public static readonly Point SAMPLE_CONN_TARGET = new Point(2, 8);

        /// <summary>
        /// Size of the offset applied to draw shadows.
        /// </summary>
        public const double SHADOW_SIZE = 5.0;

        /// <summary>
        /// Opacity of the shadows.
        /// </summary>
        public const double SHADOW_OPACITY = 0.25;

        /// <summary>
        /// Margin for de datils respect the details poster
        /// </summary>
        public const double DETAIL_MARGIN = 4;

        /// <summary>
        /// Size of the triangular part connecting the poster board to the main symbol.
        /// </summary>
        public static readonly Size POSTER_HOOK_SIZE = new Size(10, 8);

        /// <summary>
        /// Heading content separation (space within heading content: pictogram and titles).
        /// </summary>
        public const double HEAD_SEPARATION_SIZE = 1.0;

        /// <summary>
        /// Maximum horizontal positioning factor of a shown pictogram respect the heading content area.
        /// </summary>
        public const double PIC_MAXFACTOR_POSHORIZ = 0.33;

        /// <summary>
        /// Maximum vertical positioning factor of a shown pictogram respect the heading content area.
        /// </summary>
        public const double PIC_MAXFACTOR_POSVERTI = 0.5;

        /// <summary>
        /// Image margin size.
        /// </summary>
        public const double IMG_MARGIN_SIZE = 1.0;

        /// <summary>
        /// Relative size for the heading Title respect the subtitle.
        /// </summary>
        public const double RELSIZE_HEADING_TITLE = 0.6;    // 0.67

        /// <summary>
        /// Relative size for the heading Subtitle respect the title.
        /// </summary>
        public const double RELSIZE_HEADING_SUBTITLE = 0.4; // 0.33

        /// <summary>
        /// Available Thicknesses.
        /// </summary>
        public static readonly List<SimplePresentationElement> AvailableThicknesses = new List<SimplePresentationElement>();

        /// <summary>
        /// Available Dash-Styles.
        /// </summary>
        public static readonly List<SimplePresentationElement> AvailableDashStyles = new List<SimplePresentationElement>();

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Static constructor.
        /// </summary>
        static MasterDrawer()
        {
            foreach (var Value in General.CreateList(0.1, 0.25).Concat(General.RangeOfDoubles(0.5, 10.0, 0.5)))
            {
                var Pencil = new Pen(Brushes.Black, Value);   // Do not use Effects, it looks corrupt (WPF or video-driver bug?!)

                // Do not put over white rectangle, sample drawing did not improve
                var Sample = PathDrawer.CreatePath(EPathStyle.SinglelineStraight, EPathCorner.Sharp, Pencil, Brushes.Black,
                                                   new Point(0, 0), new Point(32, 0));
                AvailableThicknesses.Add(new SimplePresentationElement(Value.ToString(), Value.ToString(CultureInfo.InvariantCulture.NumberFormat), "", Sample.ToDrawingImage()));
            }

            foreach (var DashKind in Display.DeclaredDashStyles)
            {
                var Pencil = new Pen(Brushes.Black, 1.5);   // Do not use 1.0 it looks weird, plus corrupt with Effects (WPF or video-driver bug?!)
                Pencil.DashStyle = DashKind.Item1;

                // Do not put over white rectangle, sample drawing did not improve
                var Sample = PathDrawer.CreatePath(EPathStyle.SinglelineStraight, EPathCorner.Sharp, Pencil, Brushes.Black,
                                                   new Point(0, 0), new Point(32, 0));
                AvailableDashStyles.Add(new SimplePresentationElement(DashKind.Item2, DashKind.Item2, "", Sample.ToDrawingImage()));
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a drawing sample for a supplied Idea Definition (e.g.: to be placed on a palette).
        /// Optionally, more parameters can be specified:
        /// - Whether the sample is for cursor (as attached adorner, with 50% opacity)
        /// - Show upper-left pointer.
        /// - Cursor feedback indicator.
        /// </summary>
        public static Drawing CreateDrawingSample(IdeaDefinition Definition, bool IsForCursor = false, bool ShowUpperLeftPointer = false,
                                                  double Width = 0.0, double Height = 0.0,
                                                  Brush NewMainBackground = null, Brush NewLineBrush = null,
                                                  double NewLineThickness = double.NaN, DashStyle NewLineDash = null,
                                                  double NewOpacity = double.NaN)
        {
            /* NOTE: The Drawing Sample will draw one of these on a 48 x 16 rectangle ...
             *       (The last row and column are lost for having center lines, hence all
             *        dimensions are odd and the actual rectangle used is 47 x 15 in size)
             * 
             * 1.- SYMBOL:
             *     Width x Height = 39 x 13
             *     Drawings       = 1 (Just the Shape required)
             *     Structure      = Individual
             *
             * 2.- CONNECTOR:
             *     Width x Height = 45 x 9
             *     Drawings       = 4 (Head + Linea + Tail + Central/Main-Symbol)
             *     Structure      = Composed of...
             *       Line Path (Width = 31, X = 8)
             *       Head Plug (Width = 7, X = 1)
             *       Tail Plug (Width = 7, X = 39)
             *       Central/Main-Symbol (Width = 17, X = 14)
             */
            DrawingGroup Result = new DrawingGroup();

            if (Definition is ConceptDefinition)
            {
                var Format = Definition.DefaultSymbolFormat;

                if (Width == 0 && Height == 0)
                {
                    Width = Format.InitialWidth;
                    Height = Format.InitialHeight;
                    var Ratio = Width / Height;

                    if (Width > Height)
                    {
                        Width = Width.EnforceMaximum(SAMPLE_SYMB_WIDTH);
                        Height = (Width / Ratio).EnforceMaximum(SAMPLE_SYMB_HEIGHT);
                    }
                    else
                    {
                        Height = Height.EnforceMaximum(SAMPLE_SYMB_HEIGHT);
                        Width = (Height * Ratio).EnforceMaximum(SAMPLE_SYMB_WIDTH);
                    }
                }

                var Position = new Point(Width.SubstituteFor(0.0, BASE_WIDTH) / 2.0,
                                         Height.SubstituteFor(0.0, BASE_HEIGHT) / 2.0);

                if (IsForCursor)
                {
                    Format = new VisualSymbolFormat(Format);
                    Format.Opacity = 0.5;
                    Position = new Point(Position.X + SAMPLE_CURSOR_OFFSET.X, Position.Y + SAMPLE_CURSOR_OFFSET.Y);
                }

                if (Definition.DefaultSymbolFormat != null)
                {
                    Result.Children.Add(CreateDrawingSymbol(Definition.RepresentativeShape.SubstituteFor(Shapes.None, Shapes.RoundedRectangle),
                                                            null, null,   // IMPORTANT: Do not pass Definition.Pictogram (can cause infinite-loop)
                                                            NewLineBrush.NullDefault(Format.LineBrush), NewLineThickness.NaNDefault(Format.LineThickness),
                                                            NewLineDash.NullDefault(Format.LineDash), Format.LineJoin,
                                                            Format.LineCap, Format.UsePictogramAsSymbol,
                                                            NewMainBackground.NullDefault(Format.MainBackground), Format.DetailsPosterIsHanging,
                                                            NewOpacity.NaNDefault(Format.Opacity), Position, Width, Height, 0, Format.AsMultiple,
                                                            Format.InitiallyFlippedHorizontally, Format.InitiallyFlippedVertically, Format.InitiallyTilted).Item1);
                }
            }
            else
            {
                var RelDef = Definition as RelationshipDefinition;
                var OriginRoleLink = RelDef.GetLinkForRole(ERoleType.Origin);
                var TargetRoleLink = RelDef.GetLinkForRole(ERoleType.Target);

                var LineBrush = RelDef.DefaultConnectorsFormat.LineBrush;
                var LineThickness = RelDef.DefaultConnectorsFormat.LineThickness;
                var LineDash = RelDef.DefaultConnectorsFormat.LineDash;
                /*- Previous
                var OriginVariant = RelDef.TargetLinkRoleDef.NullDefault(RelDef.OriginOrParticipantLinkRoleDef).AllowedVariants.FirstOrDefault();
                var HeadPlug = RelDef.DefaultConnectorsFormat.HeadPlugs.GetMatchingOrFirst((key, value) => key.TechName == OriginVariant.TechName);
                var TargetVariant = RelDef.OriginOrParticipantLinkRoleDef.AllowedVariants.FirstOrDefault();
                var TailPlug = RelDef.DefaultConnectorsFormat.TailPlugs.GetMatchingOrFirst((key, value) => key.TechName == TargetVariant.TechName); */

                var OriginVariant = RelDef.OriginOrParticipantLinkRoleDef.AllowedVariants.FirstOrDefault();
                var TailPlug = RelDef.DefaultConnectorsFormat.TailPlugs.GetMatchingOrFirst((key, value) => key.TechName == OriginVariant.TechName);
                var TargetVariant = RelDef.TargetLinkRoleDef.NullDefault(RelDef.OriginOrParticipantLinkRoleDef).AllowedVariants.FirstOrDefault();
                var HeadPlug = RelDef.DefaultConnectorsFormat.HeadPlugs.GetMatchingOrFirst((key, value) => key.TechName == TargetVariant.TechName);

                var AreaWidth = BASE_WIDTH;
                var AreaHeight = BASE_HEIGHT;

                var CentralSymbolFormat = RelDef.DefaultSymbolFormat;

                var TargetPosition = SAMPLE_CONN_TARGET;
                if (IsForCursor)
                {
                    CentralSymbolFormat = new VisualSymbolFormat(CentralSymbolFormat);
                    CentralSymbolFormat.Opacity = 0.5;
                    TargetPosition.X += SAMPLE_CURSOR_OFFSET.X;
                    TargetPosition.Y += SAMPLE_CURSOR_OFFSET.Y;
                    AreaWidth += SAMPLE_CURSOR_OFFSET.X;
                    AreaHeight += SAMPLE_CURSOR_OFFSET.Y;
                }

                Definition.EditEngine.Pause();  // To not change Existente-Status

                var Format = new VisualConnectorsFormat(OriginVariant, TailPlug, TargetVariant, HeadPlug, LineBrush, LineDash, LineThickness, (IsForCursor ? 0.5 : 1.0));

                Definition.EditEngine.Resume();

                var SourcePosition = new Point(TargetPosition.X + AreaWidth, TargetPosition.Y);
                var Connector = CreateDrawingConnector(TailPlug, (RelDef.IsDirectional ? HeadPlug : TailPlug),
                                                       NewLineBrush.NullDefault(Format.LineBrush), NewLineThickness.NaNDefault(Format.LineThickness),
                                                       NewLineDash.NullDefault(Format.LineDash), Format.LineJoin,
                                                       Format.LineCap, Format.PathStyle,
                                                       Format.PathCorner, NewMainBackground.NullDefault(Format.MainBackground),
                                                       NewOpacity.NaNDefault(Format.Opacity), TargetPosition, SourcePosition, null, 0.5);

                Point Center = SourcePosition.DetermineCenterRespect(TargetPosition);

                /* CANCELLED: Put visual indication of multiconnectability
                if (!RelDef.IsSimple)
                {
                    Connector.Children.Add(new GeometryDrawing(Brushes.WhiteSmoke, new Pen(Brushes.Gray, 1.0),
                                           new LineGeometry(Center, new Point(SourcePosition.X, SourcePosition.Y - 10))));
                    Connector.Children.Add(new GeometryDrawing(Brushes.WhiteSmoke, new Pen(Brushes.Gray, 1.0),
                                           new LineGeometry(Center, new Point(SourcePosition.X, SourcePosition.Y + 10))));
                    Connector.Children.Add(new GeometryDrawing(Brushes.WhiteSmoke, new Pen(Brushes.Gray, 1.0),
                                           new LineGeometry(Center, new Point(TargetPosition.X, TargetPosition.Y - 10))));
                    Connector.Children.Add(new GeometryDrawing(Brushes.WhiteSmoke, new Pen(Brushes.Gray, 1.0),
                                           new LineGeometry(Center, new Point(TargetPosition.X, TargetPosition.Y + 10))));
                } */

                if (!(RelDef.IsSimple && RelDef.HideCentralSymbolWhenSimple))
                {
                    if (Width == 0 && Height == 0)
                    {
                        Width = CentralSymbolFormat.InitialWidth;
                        Height = CentralSymbolFormat.InitialHeight;
                        var Ratio = Width / Height;

                        if (Width > Height)
                        {
                            Width = Width.EnforceMaximum(SAMPLE_SYMB_WIDTH);
                            Height = (Width / Ratio).EnforceMaximum(SAMPLE_SYMB_HEIGHT);
                        }
                        else
                        {
                            Height = Height.EnforceMaximum(SAMPLE_SYMB_HEIGHT);
                            Width = (Height * Ratio).EnforceMaximum(SAMPLE_SYMB_WIDTH);
                        }

                        Width = Width * 0.4;
                        Height = Height * 0.65;
                    }

                    PutSymbolDrawing(Connector.Children, Center, RelDef.RepresentativeShape, false, null, null,   // IMPORTANT: Do not pass Definition.Pictogram (can cause infinite-loop)
                                     NewLineBrush.NullDefault(Format.LineBrush), NewLineThickness.NaNDefault(Format.LineThickness),
                                     NewLineDash.NullDefault(Format.LineDash), CentralSymbolFormat.LineJoin,
                                     CentralSymbolFormat.LineCap, CentralSymbolFormat.UsePictogramAsSymbol,
                                     NewMainBackground.NullDefault(CentralSymbolFormat.MainBackground), true, Width, Height, CentralSymbolFormat.AsMultiple,
                                     CentralSymbolFormat.InitiallyFlippedHorizontally, CentralSymbolFormat.InitiallyFlippedVertically, CentralSymbolFormat.InitiallyTilted);
                }

                /* Alternate for Simple + Hidden-Central-Symbol
                PutSymbolDrawing(Connector.Children, Center, Shapes.Ellipse, false, null, null,   // IMPORTANT: Do not pass Definition.Pictogram (can cause infinite-loop)
                    Brushes.Black, 1.0, DashStyles.Solid, PenLineJoin.Round, PenLineCap.Round, false,
                    Brushes.Black, true, 2.0, 2.0); */

                Connector.ClipGeometry = new RectangleGeometry(new Rect(TargetPosition.X, 0, TargetPosition.X + AreaWidth, AreaHeight));

                Result.Children.Add(Connector);
            }

            if (ShowUpperLeftPointer)
            {
                var Target = Display.ZERO_POINT;
                var Source = new Point(20, 20);
                var Pencil = new Pen(Brushes.Black, 0.5);
                PutConnectorPlugDrawing(Result.Children, Target, Source, Plugs.PointerArrow, Pencil, Brushes.White);
            }

            return Result;
        }

        /// <summary>
        /// Creates and returns a drawing sample for a supplied Link-Role Definition (e.g.: to be shown as preview).
        /// </summary>
        public static Drawing CreateDrawingSample(LinkRoleDefinition Definition)
        {
            DrawingGroup Result = new DrawingGroup();

            var LineBrush = Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineBrush;
            var LineThickness = Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineThickness;
            /*- Previous
            var OriginVariant = Definition.OwnerRelationshipDef.TargetLinkRoleDef.NullDefault(Definition.OwnerRelationshipDef
                                                                                              .OriginOrParticipantLinkRoleDef).AllowedVariants.First();
            var TargetVariant = Definition.OwnerRelationshipDef.OriginOrParticipantLinkRoleDef.AllowedVariants.First(); */

            var OriginVariant = Definition.OwnerRelationshipDef.OriginOrParticipantLinkRoleDef.AllowedVariants.First();
            var TargetVariant = Definition.OwnerRelationshipDef.TargetLinkRoleDef.NullDefault(Definition.OwnerRelationshipDef
                                                                                              .OriginOrParticipantLinkRoleDef).AllowedVariants.First();

            var HeadPlug = Definition.OwnerRelationshipDef.DefaultConnectorsFormat.HeadPlugs.GetMatchingOrFirst((key, value) => key.TechName == TargetVariant.TechName);
            var TailPlug = Definition.OwnerRelationshipDef.DefaultConnectorsFormat.TailPlugs.GetMatchingOrFirst((key, value) => key.TechName == OriginVariant.TechName);
            var AreaWidth = BASE_WIDTH / 1.5;
            var AreaHeight = BASE_HEIGHT;

            var TargetPosition = SAMPLE_CONN_TARGET;

            var SourcePosition = new Point(TargetPosition.X + AreaWidth, TargetPosition.Y);
            var Connector = CreateDrawingConnector(TailPlug, (Definition.OwnerRelationshipDef.IsDirectional ? HeadPlug : TailPlug),
                                                   Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineBrush, Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineThickness,
                                                   Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineDash, Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineJoin,
                                                   Definition.OwnerRelationshipDef.DefaultConnectorsFormat.LineCap, Definition.OwnerRelationshipDef.DefaultConnectorsFormat.PathStyle,
                                                   Definition.OwnerRelationshipDef.DefaultConnectorsFormat.PathCorner, Definition.OwnerRelationshipDef.DefaultConnectorsFormat.MainBackground,
                                                   Definition.OwnerRelationshipDef.DefaultConnectorsFormat.Opacity,
                                                   TargetPosition, SourcePosition, null, 0.5);

            Connector.ClipGeometry = new RectangleGeometry(new Rect(TargetPosition.X, 0, TargetPosition.X + AreaWidth, AreaHeight));

            Result.Children.Add(Connector);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a drawing and its content and details areas for the supplied visual symbol shape, pictogram and format,
        /// center position, width and height (used to draw the actual visual representation on the document's view), plus an specified details poster height, if any.
        /// </summary>
        public static Tuple<DrawingGroup, Rect, Rect> CreateDrawingSymbol(string SymbolShape, ImageSource Pictogram, ImageSource AltPictogram,
                                                                          Brush FormatLineBrush, double FormatLineThickness,
                                                                          DashStyle FormatLineDash, PenLineJoin FormatLineJoin,
                                                                          PenLineCap FormatLineCap, bool FormatUsePictogramAsSymbol,
                                                                          Brush FormatMainBackground, bool FormatDetailsPosterIsHanging,
                                                                          double FormatOpacity, Point CenterPosition,
                                                                          double Width, double Height, double DetailsPosterHeight = 0, bool AsMultiple = false,
                                                                          bool FlipHorizontally = false, bool FlipVertically = false, bool Tilted = false)
        {
            DrawingGroup ResultDrawing = new DrawingGroup();

            Rect ResultDetailsArea = Rect.Empty;

            if (DetailsPosterHeight > 0)
            {
                Point PosterPosition = new Point(CenterPosition.X, CenterPosition.Y + (Height / 2.0) + (DetailsPosterHeight / 2.0));

                /* The old drawing method of the symbol was proportionally, so the content area looks losing available board space
                ResultDetailsArea = PutSymbolDrawing(ResultDrawing.Children, PosterPosition, Shapes.Poster, true,
                                                     Pictogram, null, Format, false, Width, DetailsPosterHeight);*/

                ResultDetailsArea = PutPosterDrawing(ResultDrawing.Children, PosterPosition,
                                                     FormatLineBrush, FormatLineThickness,
                                                     FormatLineDash, FormatLineJoin,
                                                     FormatLineCap, FormatUsePictogramAsSymbol,
                                                     FormatMainBackground, FormatDetailsPosterIsHanging,
                                                     Width, DetailsPosterHeight);
            }

            Rect ResultContentArea = PutSymbolDrawing(ResultDrawing.Children, CenterPosition, SymbolShape,
                                                      false, Pictogram, AltPictogram,
                                                      FormatLineBrush, FormatLineThickness,
                                                      FormatLineDash, FormatLineJoin,
                                                      FormatLineCap, FormatUsePictogramAsSymbol,
                                                      FormatMainBackground, false,
                                                      Width, Height, AsMultiple,
                                                      FlipHorizontally, FlipVertically, Tilted);
            ResultDrawing.Opacity = FormatOpacity;

            /* wait for the next WPF version (effects enhancements)
            var Look = new DropShadowEffect();
            Look.Color = Colors.Blue;
            Look.Direction = 330;
            Look.ShadowDepth = 8;
            Look.BlurRadius = 4;
            Look.Opacity = 0.5;
            ResultDrawing.BitmapEffect = Look;   // didn't accepted the new effects, just the old deprecated. CAN CAUSE MEMORY LEAKS!?
            */

            return Tuple.Create<DrawingGroup, Rect, Rect>(ResultDrawing, ResultContentArea, ResultDetailsArea);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a drawing for the supplied plugs, visual connector format, positions (target, source and optional intermediate), plus a magnitude factor.
        /// (used to draw the actual visual representation on the document's view).
        /// </summary>
        public static DrawingGroup CreateDrawingConnector(string OriginPlug, string TargetPlug,
                                                          Brush FormatLineBrush, double FormatLineThickness,
                                                          DashStyle FormatLineDash, PenLineJoin FormatLineJoin,
                                                          PenLineCap FormatLineCap, EPathStyle FormatPathStyle,
                                                          EPathCorner FormatPathCorner, Brush FormatMainBackground,
                                                          double FormatOpacity,
                                                          Point TargetPosition, Point SourcePosition, IEnumerable<Point> IntermediatePositions = null, double Magnitude = 1.0)
        {
            DrawingGroup Result = new DrawingGroup();

            var DrawingPathPen = new Pen(FormatLineBrush, FormatLineThickness);
            DrawingPathPen.DashStyle = FormatLineDash;
            DrawingPathPen.LineJoin = FormatLineJoin;
            DrawingPathPen.StartLineCap = FormatLineCap;
            DrawingPathPen.EndLineCap = FormatLineCap;

            PutConnectorPathDrawing(Result.Children, TargetPosition, SourcePosition, FormatPathStyle, FormatPathCorner, DrawingPathPen,
                                    FormatMainBackground, IntermediatePositions, Magnitude);

            // IMPORTANT:
            // This particular Pen (with no specific Dash-Style, Join or Caps for line) must be apart because...
            // - Plugs are fine-delineated
            // - Pens are referenced (shared) until the drawing itself
            var DrawingPlugPen = new Pen(FormatLineBrush, FormatLineThickness);

            PutConnectorPlugDrawing(Result.Children, TargetPosition, SourcePosition, TargetPlug, DrawingPlugPen, FormatMainBackground, Magnitude);
            PutConnectorPlugDrawing(Result.Children, SourcePosition, TargetPosition, OriginPlug, DrawingPlugPen, FormatMainBackground, Magnitude);

            // Show Circles to notice connecting points
            //T Result.Children.Add(new GeometryDrawing(null, new Pen(Brushes.Blue, 0.5), new EllipseGeometry(SourcePosition, 3, 3)));
            //T Result.Children.Add(new GeometryDrawing(null, new Pen(Brushes.Red, 0.5), new EllipseGeometry(TargetPosition, 3, 3)));

            Result.Opacity = FormatOpacity;

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Rect PutPosterDrawing(DrawingCollection Target, Point Center,
                                             Brush FormatLineBrush, double FormatLineThickness,
                                             DashStyle FormatLineDash, PenLineJoin FormatLineJoin,
                                             PenLineCap FormatLineCap, bool FormatUsePictogramAsSymbol,
                                             Brush FormatMainBackground, bool FormatDetailsPosterIsHanging,
                                             double Width, double Height)
        {
            var LocalPen = new Pen(FormatLineBrush, FormatLineThickness);
            LocalPen.DashStyle = FormatLineDash;
            LocalPen.LineJoin = FormatLineJoin;
            LocalPen.StartLineCap = FormatLineCap;
            LocalPen.EndLineCap = FormatLineCap;

            var PosterArea = new Rect(Center.X - (Width / 2.0), Center.Y - (Height / 2.0), Width, Height);
            double BoardTop = PosterArea.Top;
            Geometry Poster = null;

            if (FormatDetailsPosterIsHanging)
            {
                BoardTop = (PosterArea.Top + POSTER_HOOK_SIZE.Height).EnforceMaximum(PosterArea.Bottom);

                Poster = Display.PathFigureGeometry(new Point((Center.X - POSTER_HOOK_SIZE.Width / 2).EnforceMinimum(PosterArea.Left), BoardTop),
                                                    new Point(Center.X, PosterArea.Top),
                                                    new Point((Center.X + POSTER_HOOK_SIZE.Width / 2).EnforceMaximum(PosterArea.Right), BoardTop),
                                                    new Point(PosterArea.Right, BoardTop),
                                                    new Point(PosterArea.Right, PosterArea.Bottom),
                                                    new Point(PosterArea.Left, PosterArea.Bottom),
                                                    new Point(PosterArea.Left, BoardTop));
            }
            else
                Poster = new RectangleGeometry(PosterArea);

            var ContentSize = new Size((PosterArea.Width - DETAIL_MARGIN * 2).EnforceMinimum(0),
                                       ((PosterArea.Bottom - BoardTop) - DETAIL_MARGIN * 2).EnforceMinimum(0));
            var ContentArea = (ContentSize.Width <= 0 || ContentSize.Height <= 0
                               ? Rect.Empty
                               : new Rect(PosterArea.X + DETAIL_MARGIN, BoardTop + DETAIL_MARGIN - 1,
                                          ContentSize.Width.EnforceMinimum(0), ContentSize.Height.EnforceMinimum(0)));

            Target.Add(new GeometryDrawing(FormatMainBackground, LocalPen, Poster));

            return ContentArea;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        private static Rect PutSymbolDrawing(DrawingCollection Target, Point CenterPosition, string SymbolShape, bool IsPoster,
                                             ImageSource Pictogram, ImageSource AltPictogram,
                                             Brush FormatLineBrush, double FormatLineThickness,
                                             DashStyle FormatLineDash, PenLineJoin FormatLineJoin,
                                             PenLineCap FormatLineCap, bool FormatUsePictogramAsSymbol,
                                             Brush FormatMainBackground, bool IsConnectorCentralSymbol = false,
                                             double Width = double.NaN, double Height = double.NaN, bool AsMultiple = false,
                                             bool FlipHorizontally = false, bool FlipVertically = false, bool Tilted = false)
        {
            var LocalPen = new Pen(FormatLineBrush, FormatLineThickness);
            LocalPen.DashStyle = FormatLineDash;
            LocalPen.LineJoin = FormatLineJoin;
            LocalPen.StartLineCap = FormatLineCap;
            LocalPen.EndLineCap = FormatLineCap;

            Tuple<Drawing, Rect> Result = null;

            if (!IsPoster && FormatUsePictogramAsSymbol && (Pictogram != null || AltPictogram != null))
            {
                Pictogram = Pictogram.NullDefault(AltPictogram);
                Width = (Width.IsNan() ? Pictogram.GetWidth() : Width).EnforceMinimum(0);
                Height = (Height.IsNan() ? Pictogram.GetHeight() : Height).EnforceMinimum(0);
                var Zone = new Rect(CenterPosition.X - Width / 2.0, CenterPosition.Y - Height / 2.0, Width, Height);
                var Draw = new ImageDrawing(Pictogram, Zone);
                Result = Tuple.Create<Drawing, Rect>(Draw, Zone);
            }
            else
                Result = SymbolDrawer.CreateSymbol(SymbolShape, LocalPen, FormatMainBackground, CenterPosition,
                                                   Width, Height, AsMultiple, FlipHorizontally, FlipVertically, Tilted);

            if (Result == null)
                return Rect.Empty;

            /* POSTPONED: Shadow must be shown in a layer under all visual elements except the background.
             *            Otherwise the connectors points to the shadow instead of the visual element.
            if (SampleFormat.HasShadow)
            {
                

                var ShadowBrush = Brushes.Black.Clone();
                ShadowBrush.Opacity = SHADOW_OPACITY;

                LocalPen = LocalPen.Clone();
                LocalPen.Brush = null;
                CenterPosition = new Point(CenterPosition.X + SHADOW_SIZE, CenterPosition.Y + SHADOW_SIZE);

                var Shadow = SymbolDrawer.CreateSymbol(SampleFormat.Shape, LocalPen, ShadowBrush, CenterPosition, Width, Height);

                Target.Add(Shadow.Item1);
            } */

            Target.Add(Result.Item1);

            return Result.Item2;
        }

        public static Rect PutConnectorLabeling(DrawingContext Context, RelationshipDefinition RelDef,
                                                Point DecoratorCenter, TextFormat Format, Brush Background, Brush LineBrush,
                                                string LinkDescriptorLabel = null,
                                                string LinkDefinitorLabel = null,
                                                string LinkVariantLabel = null)
        {
            var Area = Rect.Empty;
            var MaxDecoratorWidth = RelDef.DefaultSymbolFormat
                                        .InitialWidth.SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Width);
            var MaxDecoratorHeight = RelDef.DefaultSymbolFormat
                                        .InitialHeight.SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Height);

            string Text = LinkDescriptorLabel ?? "";

            if (LinkDefinitorLabel != null)
                Text = Text + " [" + LinkDefinitorLabel + "] ";

            if (LinkVariantLabel != null)
                Text = Text + " [" + LinkVariantLabel + "] ";

            Text = Text.Trim();

            if (!Text.IsAbsent())
            {
                var Label = Format.GenerateFormattedText(Text, MaxDecoratorWidth, MaxDecoratorHeight, TextAlignment.Center, 1, 2, 2, 8.0);

                MaxDecoratorWidth = Label.Width + 4.0;
                MaxDecoratorHeight = Label.Height + 3.0;
                Area = new Rect(DecoratorCenter.X - (MaxDecoratorWidth / 2.0), DecoratorCenter.Y - (MaxDecoratorHeight / 2.0),
                                MaxDecoratorWidth, MaxDecoratorHeight);

                var RoundedRectGeom = new RectangleGeometry(Area);
                RoundedRectGeom.RadiusX = VisualSymbol.ROUNDEDRECT_RADIUS;
                RoundedRectGeom.RadiusY = VisualSymbol.ROUNDEDRECT_RADIUS;
                Context.DrawGeometry(Background, new Pen(LineBrush, 1.0), RoundedRectGeom);

                Context.DrawText(Label, new Point((Area.X + Area.Width / 2.0) - (Label.MaxTextWidth / 2.0),
                                                  Area.Y + 1));
            }

            return Area;
        }

        private static void PutConnectorPlugDrawing(DrawingCollection Target, Point TargetPosition, Point SourcePosition, string PlugType, Pen DrawingPen, Brush DrawingBrush,
                                                    double Magnitude = 1.0)
        {
            var Plug = PlugDrawer.CreatePlug(PlugType, DrawingPen, TargetPosition, SourcePosition, Magnitude);

            if (Plug == null)
                return;

            Target.Add(Plug);
        }

        private static void PutConnectorPathDrawing(DrawingCollection Target, Point TargetPosition, Point SourcePosition,
                                                    EPathStyle PathStyle, EPathCorner CornerStyle, Pen DrawingPen, Brush DrawingBrush,
                                                    IEnumerable<Point> IntermediatePositions = null, double Magnitude = 1.0)
        {
            var Path = PathDrawer.CreatePath(PathStyle, CornerStyle, DrawingPen, DrawingBrush, TargetPosition, SourcePosition, IntermediatePositions, Magnitude);

            if (Path == null)
                return;

            Target.Add(Path);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For a given drawing-context, generates and draws a Label for the specified Text, considering
        /// Format, Available-Area, Fill, Stroke, Padding parameters and indication to draw only over the text used area (true) or all the available (false).
        /// Returns the area of the drawn label.
        /// </summary>
        public static Rect DrawTextLabel(DrawingContext Context, string Text, TextFormat Format, Rect AvailableArea, Brush Fill, Pen Stroke,
                                         double VerticalPadding, double LeftPadding, double RightPadding, bool DrawOnlyOverUsedArea = true)
        {
            if (Text.IsAbsent())
                Text = " ";     // This is necessary to show at least the empty area.

            var GeneratedFormattedText = Format.GenerateFormattedText(Text, AvailableArea.Width, AvailableArea.Height, VerticalPadding, LeftPadding, RightPadding);
            if (GeneratedFormattedText != null)
                return DrawTextLabel(Context, GeneratedFormattedText, AvailableArea, Fill, Stroke, VerticalPadding, LeftPadding, RightPadding, DrawOnlyOverUsedArea);

            return Rect.Empty;
        }

        /// <summary>
        /// For a given drawing-context, generates and draws a Label for the specified Formatted Text, considering
        /// Available-Area, Fill, Stroke, Padding parameters and indication to draw only over the text used area (true) or all the available (false).
        /// Returns the area of the drawn label.
        /// </summary>
        public static Rect DrawTextLabel(DrawingContext Context, FormattedText Text, Rect AvailableArea, Brush Fill, Pen Stroke,
                                         double VerticalPadding, double LeftPadding, double RightPadding, bool DrawOnlyOverUsedArea = true, bool DrawRectangle = true)
        {
            Rect LabelArea = Rect.Empty;

            // Notice that Width can be zero when drawing blank/space.
            if (Text.Width > 0 || Text.Height > 0)
            {
                if (DrawOnlyOverUsedArea)
                    LabelArea = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, Text.Height + (VerticalPadding * 2.0) );
                else
                    LabelArea = AvailableArea;

                // Draw the background box
                if (DrawRectangle)
                    Context.DrawRectangle(Fill, Stroke, LabelArea);

                // Draw the text over the background
                var PosX = AvailableArea.X + LeftPadding;
                var PosY = AvailableArea.Y + VerticalPadding;

                Context.DrawText(Text, new Point(PosX, PosY));
            }

            return LabelArea;
        }

        public static readonly Pen RedPen = new Pen(Brushes.Red, 1.0);
        public static readonly Pen BluePen = new Pen(Brushes.Blue, 1.0);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a tuple with the calculated Pictogram and Entitling areas for the supplied Available Area, indication of Pictogram existence and its visual disposition.
        /// </summary>
        public static Tuple<Rect, Rect> DeterminePictogramAndEntitlingAreas(Rect AvailableArea, bool PictogramExists, EVisualDispositionBidimensional PictogramVisualDisposition)
        {
            Rect PictogramArea = Rect.Empty;
            Rect EntitlingArea = AvailableArea;

            if (PictogramExists && PictogramVisualDisposition != EVisualDispositionBidimensional.Hidden)
            {
                var MinPicLength = Math.Min(AvailableArea.Width, AvailableArea.Height);
                var MaxPicLength = Math.Max(AvailableArea.Width, AvailableArea.Height);

                if (MinPicLength > ((MaxPicLength / 2.0) - HEAD_SEPARATION_SIZE))
                    MinPicLength = ((MaxPicLength / 2.0) - HEAD_SEPARATION_SIZE);

                if (PictogramVisualDisposition == EVisualDispositionBidimensional.Left)
                {
                    PictogramArea = new Rect(AvailableArea.X, AvailableArea.Y, MinPicLength, AvailableArea.Height);
                    PictogramArea.Width = PictogramArea.Width.EnforceRange(0, AvailableArea.Width * PIC_MAXFACTOR_POSHORIZ);
                    EntitlingArea = new Rect(AvailableArea.X + (PictogramArea.Width + HEAD_SEPARATION_SIZE), AvailableArea.Y,
                                             (AvailableArea.Width - (PictogramArea.Width + HEAD_SEPARATION_SIZE)).EnforceMinimum(1.0), AvailableArea.Height);
                }
                else
                    if (PictogramVisualDisposition == EVisualDispositionBidimensional.Right)
                    {
                        PictogramArea = new Rect((AvailableArea.X + AvailableArea.Width - (MinPicLength + HEAD_SEPARATION_SIZE)).EnforceMinimum(1.0),
                                                 AvailableArea.Y, MinPicLength, AvailableArea.Height);
                        PictogramArea.Width = PictogramArea.Width.EnforceRange(0, AvailableArea.Width * PIC_MAXFACTOR_POSHORIZ);
                        PictogramArea.X = PictogramArea.X.EnforceMinimum(AvailableArea.X + (AvailableArea.Width - (AvailableArea.Width * PIC_MAXFACTOR_POSHORIZ)));
                        EntitlingArea = new Rect(AvailableArea.X, AvailableArea.Y,
                                                 (AvailableArea.Width - (PictogramArea.Width + HEAD_SEPARATION_SIZE)).EnforceMinimum(1.0), AvailableArea.Height);
                    }
                    else
                        if (PictogramVisualDisposition == EVisualDispositionBidimensional.Top)
                        {
                            PictogramArea = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width, MinPicLength);
                            PictogramArea.Height = PictogramArea.Height.EnforceRange(0, AvailableArea.Height * PIC_MAXFACTOR_POSVERTI);
                            EntitlingArea = new Rect(AvailableArea.X, AvailableArea.Y + (PictogramArea.Height + HEAD_SEPARATION_SIZE),
                                                     AvailableArea.Width, (AvailableArea.Height - (PictogramArea.Height + HEAD_SEPARATION_SIZE)).EnforceMinimum(1.0));
                        }
                        else
                        {
                            PictogramArea = new Rect(AvailableArea.X, (AvailableArea.Y + AvailableArea.Height - (MinPicLength + HEAD_SEPARATION_SIZE)).EnforceMinimum(1.0),
                                                     AvailableArea.Width, MinPicLength);
                            PictogramArea.Height = PictogramArea.Height.EnforceRange(0, AvailableArea.Height * PIC_MAXFACTOR_POSVERTI);
                            PictogramArea.Y = PictogramArea.Y.EnforceMinimum(AvailableArea.Y + (AvailableArea.Height - (AvailableArea.Height * PIC_MAXFACTOR_POSVERTI)));
                            EntitlingArea = new Rect(AvailableArea.X, AvailableArea.Y, AvailableArea.Width,
                                                     (AvailableArea.Height - (PictogramArea.Height + HEAD_SEPARATION_SIZE)).EnforceMinimum(1.0));
                        }
            }

            return Tuple.Create(PictogramArea, EntitlingArea);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For a given drawing-context, generates and draws an Image for the specified Image-Data, considering the Available-Area.
        /// Optionally, an idication can be specified for draw the image content frame even if no image data was passed,
        /// plus indication of center image horizontally and/or vertically when there is extra space and indication for show background.
        /// Returns the area of the drawn Image.
        /// </summary>
        public static Rect DrawImage(DrawingContext Context, byte[] ImageData, Rect AvailableArea,
                                     bool DrawFrameForEmptyImage = false, bool HorizontallyCentered = true, bool VerticallyCentered = true, bool ShowBackground = false)
        {
            return DrawImage(Context, ImageData.ToImageSource(), AvailableArea, DrawFrameForEmptyImage, HorizontallyCentered, VerticallyCentered, ShowBackground);
        }

        /// <summary>
        /// For a given drawing-context, generates and draws an Image for the specified Source, considering the Available-Area.
        /// Optionally, an idication can be specified for draw the image content frame even if no image data was passed,
        /// plus indication of center image horizontally and/or vertically when there is extra space and indication for show background.
        /// Returns the area of the drawn Image.
        /// </summary>
        public static Rect DrawImage(DrawingContext Context, ImageSource Source, Rect AvailableArea,
                                     bool DrawFrameForEmptyImage = false, bool HorizontallyCentered = true, bool VerticallyCentered = true, bool ShowBackground = false)
        {
            var ImageArea = AvailableArea;
            var UsedArea = AvailableArea;

            if (Source != null)
            {

                var Ratio = Source.GetHeight() / Source.GetWidth();
                var RenderWidth = AvailableArea.Width - IMG_MARGIN_SIZE;
                var RenderHeight = (RenderWidth * Ratio) - IMG_MARGIN_SIZE;

                if (RenderHeight > AvailableArea.Height)
                {
                    Ratio = Source.GetWidth() / Source.GetHeight();
                    RenderHeight = AvailableArea.Height - IMG_MARGIN_SIZE;
                    RenderWidth = (RenderHeight * Ratio) - IMG_MARGIN_SIZE;
                }

                var RenderX = AvailableArea.X + (IMG_MARGIN_SIZE / 2.0);
                var RenderY = AvailableArea.Y + (IMG_MARGIN_SIZE / 2.0);

                if (HorizontallyCentered && RenderWidth < (AvailableArea.Width - IMG_MARGIN_SIZE))
                    RenderX = RenderX + ((AvailableArea.Width / 2.0) - (RenderWidth / 2.0));

                if (VerticallyCentered && RenderHeight < (AvailableArea.Height - IMG_MARGIN_SIZE))
                    RenderY = RenderY + ((AvailableArea.Height / 2.0) - (RenderHeight / 2.0));

                ImageArea = new Rect(RenderX, RenderY,
                                     (RenderWidth - (IMG_MARGIN_SIZE / 2.0)).EnforceMinimum(1.0),
                                     (RenderHeight - (IMG_MARGIN_SIZE / 2.0)).EnforceMinimum(1.0));

                // Notice that Available width is not changed (Symbol's Details Poster shows content stacked towards bottom)
                UsedArea = new Rect(AvailableArea.X, AvailableArea.Y,
                                    AvailableArea.Width, Math.Min(ImageArea.Height + IMG_MARGIN_SIZE, AvailableArea.Height));
            }
            else
                if (!DrawFrameForEmptyImage)
                    return UsedArea;

            var Stroke = new Pen(Brushes.LightGray, 1.0);

            if (ShowBackground)
                Context.DrawRectangle(Brushes.White, Stroke, UsedArea);

            // Draw image or empty white box
            if (Source != null)
                Context.DrawImage(Source, ImageArea);
            else
                Context.DrawRectangle(Brushes.White, Stroke, ImageArea);

            return UsedArea;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates the heading content for a symbol.
        /// </summary>
        public static void GenerateHeadingContent(DrawingGroup Target, Rect BaseContentArea,
                                                  ImageSource WorkingPictogram, bool PictogramShownAsSymbol, EVisualDispositionBidimensional PictogramVisualDisposition,
                                                  TextFormat TitleFormat, TextFormat SubtitleFormat, EVisualDispositionMonodimensional SubtitleVisualDisposition,
                                                  bool UseNameAsMainTitle, string Name, string TechName)
        {
            // For Testing: Draw content-area rectangle
            //- Result.Children.Add(new GeometryDrawing(Brushes.Transparent, new Pen(Brushes.Red, 1), new RectangleGeometry(ContentArea)));

            // Determine Pictogram placement and entitling area
            var HeadingAreas = MasterDrawer.DeterminePictogramAndEntitlingAreas(BaseContentArea, (WorkingPictogram != null || PictogramShownAsSymbol),
                                                                                PictogramVisualDisposition);

            // Draw the Pictogram (if it is not shown as the Symbol), Title and Subtitle texts
            using (var Context = Target.Append())
            {
                Context.PushClip(new RectangleGeometry(BaseContentArea));

                if (HeadingAreas.Item1 != Rect.Empty && !PictogramShownAsSymbol)
                    MasterDrawer.DrawImage(Context, WorkingPictogram, HeadingAreas.Item1);

                GenerateTitles(HeadingAreas.Item2, TitleFormat, SubtitleFormat, SubtitleVisualDisposition, UseNameAsMainTitle, Name, TechName)
                    .ForEach(gentext => Context.DrawText(gentext.Item1, gentext.Item2));
            }
        }


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates Title and Subtitle texts for the supplied Available Area, considering the appropriate order.
        /// Returns the generated formatted-text objects plus their display positions.
        /// </summary>
        public static IEnumerable<Tuple<FormattedText, Point>> GenerateTitles(Rect AvailableArea, TextFormat TitleFormat,
                                                                              TextFormat SubtitleFormat, EVisualDispositionMonodimensional SubtitleVisualDisposition,
                                                                              bool UseNameAsMainTitle, string Name, string TechName)
        {
            var Result = new List<Tuple<FormattedText, Point>>();

            //T Console.WriteLine("Applying Format=[{0}], ForeColor=[{1}]", TitleFormat.GetHashCode(), (TitleFormat.ForegroundBrush is SolidColorBrush ? ((SolidColorBrush)TitleFormat.ForegroundBrush).Color : Colors.White));

            string TitleText = (UseNameAsMainTitle ? Name : TechName);
            double TextHeight = AvailableArea.Height;
            double TextPosY = AvailableArea.Y;

            if (SubtitleVisualDisposition >= 0)
            {
                string SubtitleText = (UseNameAsMainTitle ? TechName : Name);

                TextHeight = TextHeight * RELSIZE_HEADING_SUBTITLE;
                if (SubtitleVisualDisposition > 0)
                    TextPosY = TextPosY + (AvailableArea.Height * RELSIZE_HEADING_TITLE);

                var SubtitleTextDrawing = SubtitleFormat.GenerateFormattedText(SubtitleText, AvailableArea.Width, TextHeight);
                if (SubtitleTextDrawing != null)
                {
                    var SubtitleTextPosition = new Point(AvailableArea.X, TextPosY + ((TextHeight - SubtitleTextDrawing.Height) / 2.0));
                    Result.Add(Tuple.Create<FormattedText, Point>(SubtitleTextDrawing, SubtitleTextPosition));

                    // Reestablish to be used by the Title
                    TextHeight = AvailableArea.Height * RELSIZE_HEADING_TITLE;
                    if (SubtitleVisualDisposition == 0)
                        TextPosY = AvailableArea.Y + (AvailableArea.Height * RELSIZE_HEADING_SUBTITLE);
                    else
                        TextPosY = AvailableArea.Y;
                }
                else
                {
                    TextHeight = AvailableArea.Height;
                    TextPosY = AvailableArea.Y;
                }
            }

            var TitleTextDrawing = TitleFormat.GenerateFormattedText(TitleText, AvailableArea.Width, TextHeight);
            if (TitleTextDrawing != null)
            {
                var TitleTextPosition = new Point(AvailableArea.X, TextPosY + ((TextHeight - TitleTextDrawing.Height) / 2.0));
                Result.Add(Tuple.Create<FormattedText, Point>(TitleTextDrawing, TitleTextPosition));
            }

            return Result.OrderBy(tup => tup.Item2.Y);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}