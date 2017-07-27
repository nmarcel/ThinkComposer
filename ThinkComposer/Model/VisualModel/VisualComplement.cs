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
// File   : VisualComplement.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualComplement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.08.14 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Definitor;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Individual visual object exposing attached information, such as note, callout, legend and info-card.
    /// </summary>
    [Serializable]
    public partial class VisualComplement : VisualObject, IModelEntity, IModelClass<VisualComplement>
    {
        public const double NOTE_BACKGRAD_INITIAL = 0.2;
        public const double NOTE_BACKGRAD_CENTRAL = 0.2;
        public const double NOTE_BACKGRAD_FINAL = 1.0;

        public const double CALLOUT_INI_WIDTH = 64;
        public const double CALLOUT_INI_HEIGHT = 32;

        public const double QUOTE_INI_WIDTH = 80;
        public const double QUOTE_INI_HEIGHT = 40;

        public const double TEXT_INI_WIDTH = 250;
        public const double TEXT_INI_HEIGHT = 52;

        public const double STAMP_INI_WIDTH = 128;
        public const double STAMP_INI_HEIGHT = 32;

        public const double NOTE_INI_WIDTH = 80;
        public const double NOTE_INI_HEIGHT = 50;

        public const double GROUPREGION_INI_WIDTH_FACTOR = 2;
        public const double GROUPREGION_INI_TOTALHEIGHT_FACTOR = 2;

        public const double GROUPLINE_INI_THICKNESS = 2;
        public const double GROUPLINE_INI_LENGTH_FACTOR = 10;

        public const string PROP_FIELD_IMAGE = "IMG";
        public const string PROP_FIELD_TEXT = "TXT";
        public const string PROP_FIELD_TEXTFORMAT = "TFM";
        public const string PROP_FIELD_FOREGROUND = "FRG";
        public const string PROP_FIELD_BACKGROUND = "BKG";
        public const string PROP_FIELD_ORIENTATION = "ORN";
        public const string PROP_FIELD_QUADRANT = "QDR";
        public const string PROP_FIELD_OFFSETX = "OFX";
        public const string PROP_FIELD_OFFSETY = "OFY";
        public const string PROP_FIELD_LINETHICK = "LTH";
        public const string PROP_FIELD_LINEDASH = "LDS";

        public static readonly Dictionary<string, Type> PropertyFieldsTypes = new Dictionary<string, Type>()
          { { PROP_FIELD_IMAGE, typeof(ImageSource) },
            { PROP_FIELD_TEXT, typeof(string) },
            { PROP_FIELD_TEXTFORMAT, typeof(TextFormat) },
            { PROP_FIELD_FOREGROUND, typeof(Brush) },
            { PROP_FIELD_BACKGROUND, typeof(Brush) },
            { PROP_FIELD_ORIENTATION, typeof(Orientation) },
            { PROP_FIELD_QUADRANT, typeof(EVecinityQuadrant) },
            { PROP_FIELD_OFFSETX, typeof(double) },
            { PROP_FIELD_OFFSETY, typeof(double) },
            { PROP_FIELD_LINETHICK, typeof(double) },
            { PROP_FIELD_LINEDASH, typeof(DashStyle) } };

        public static readonly Dictionary<string, string[]> ApplicablePropertiesByKind = new Dictionary<string, string[]>()
          { { Domain.ComplementDefCallout.TechName, new string[] { PROP_FIELD_TEXT, PROP_FIELD_TEXTFORMAT, PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND, PROP_FIELD_QUADRANT, PROP_FIELD_OFFSETX, PROP_FIELD_OFFSETY } },
            { Domain.ComplementDefGroupLine.TechName, new string[] { PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND, PROP_FIELD_LINEDASH, PROP_FIELD_LINETHICK, PROP_FIELD_ORIENTATION } },
            { Domain.ComplementDefGroupRegion.TechName, new string[] { PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND, PROP_FIELD_LINEDASH, PROP_FIELD_LINETHICK } },
            { Domain.ComplementDefImage.TechName, new string[] { PROP_FIELD_IMAGE } },
            { Domain.ComplementDefInfoCard.TechName, new string[] { PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND } },
            { Domain.ComplementDefLegend.TechName, new string[] { PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND } },
            { Domain.ComplementDefNote.TechName, new string[] { PROP_FIELD_TEXT, PROP_FIELD_TEXTFORMAT, PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND } },
            { Domain.ComplementDefQuote.TechName, new string[] { PROP_FIELD_TEXT, PROP_FIELD_TEXTFORMAT, PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND, PROP_FIELD_QUADRANT, PROP_FIELD_OFFSETX, PROP_FIELD_OFFSETY } },
            { Domain.ComplementDefStamp.TechName, new string[] { PROP_FIELD_TEXT, PROP_FIELD_TEXTFORMAT, PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND } },
            { Domain.ComplementDefText.TechName, new string[] { PROP_FIELD_TEXT, PROP_FIELD_TEXTFORMAT, PROP_FIELD_FOREGROUND, PROP_FIELD_BACKGROUND } } };

        /// <summary>
        /// Default minimum base figure size.
        /// </summary>
        public static Size DefaultMinBaseFigureSize { get; set; }

        /// <summary>
        /// Default minimum group-line thickness.
        /// </summary>
        public static Tuple<double,double> DefaultGroupLineThicknessRange { get; set; }

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualComplement()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualComplement>("VisualComplement", VisualObject.__ClassDefinitor, "Visual Complement",
                                                                         "Individual visual object exposing attached information, such as note, callout, legend and info-card.");
            __ClassDefinitor.DeclareProperty(__Kind);
            __ClassDefinitor.DeclareProperty(__Content);
            __ClassDefinitor.DeclareProperty(__Target);
            __ClassDefinitor.DeclareProperty(__BaseCenter);
            __ClassDefinitor.DeclareProperty(__BaseWidth);
            __ClassDefinitor.DeclareProperty(__BaseHeight);

            __ClassDefinitor.DeclareCollection(__PropertyFields);

            // Make these two coherent
            DefaultMinBaseFigureSize = new Size(8.0, 8.0);
            DefaultGroupLineThicknessRange = Tuple.Create(1.0, 8.0);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualComplement(SimplePresentationElement Kind, Ownership<View, VisualSymbol> Target, Point InitialPosition, double InitialWidth = 0.0)
        {
            this.Kind = Kind;
            this.Target = Target;
            this.BaseCenter = InitialPosition;

            this.PropertyFields = new EditableDictionary<string, object>(__PropertyFields.TechName, (IModelEntity)Target.Owner);

            if (this.IsComplementImage)
            {
                this.SetPropertyField(PROP_FIELD_IMAGE, Display.GetAppImage("picture_tree.png"));
                this.BaseWidth = 32;
                this.BaseHeight = 32;
            }
            else
            if (this.IsComplementCallout)
            {
                this.SetPropertyField(PROP_FIELD_TEXT, "");
                this.SetPropertyField(PROP_FIELD_TEXTFORMAT, new TextFormat("Arial", 9, Brushes.Blue));
                this.SetPropertyField(PROP_FIELD_FOREGROUND, Brushes.Gold);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Brushes.LightYellow);
                this.StoreFloaterInfo(Target.OwnerLocal.TotalArea, InitialPosition);
                this.BaseWidth = CALLOUT_INI_WIDTH;
                this.BaseHeight = CALLOUT_INI_HEIGHT;
            }
            else
            if (this.IsComplementQuote)
            {
                this.SetPropertyField(PROP_FIELD_TEXT, "");
                this.SetPropertyField(PROP_FIELD_TEXTFORMAT, new TextFormat("Comic Sans MS", 9, Brushes.MediumBlue));
                this.SetPropertyField(PROP_FIELD_FOREGROUND, Brushes.DeepSkyBlue);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Brushes.Azure);
                this.StoreFloaterInfo(Target.OwnerLocal.TotalArea, InitialPosition);
                this.BaseWidth = QUOTE_INI_WIDTH;
                this.BaseHeight = QUOTE_INI_HEIGHT;
            }
            else
            if (this.IsComplementText)
            {
                this.SetPropertyField(PROP_FIELD_TEXT, "");
                this.SetPropertyField(PROP_FIELD_TEXTFORMAT, new TextFormat("Arial", 18, Brushes.Black, true, false, false, TextAlignment.Center));
                this.SetPropertyField(PROP_FIELD_FOREGROUND, Brushes.Blue);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Brushes.WhiteSmoke);
                this.BaseWidth = TEXT_INI_WIDTH;
                this.BaseHeight = TEXT_INI_HEIGHT;
            }
            else
            if (this.IsComplementStamp)
            {
                this.SetPropertyField(PROP_FIELD_TEXT, "");
                this.SetPropertyField(PROP_FIELD_TEXTFORMAT, new TextFormat("Impact", 18, Brushes.Crimson, true, false, false, TextAlignment.Center));
                this.SetPropertyField(PROP_FIELD_FOREGROUND, Brushes.Crimson);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Brushes.Transparent);
                this.BaseWidth = STAMP_INI_WIDTH;
                this.BaseHeight = STAMP_INI_HEIGHT;
            }
            else
            if (this.IsComplementNote)
            {
                var Background = Display.GetGradientBrush(Colors.LightYellow, Colors.LightGoldenrodYellow, Colors.Gold, true,
                                                          NOTE_BACKGRAD_INITIAL, NOTE_BACKGRAD_CENTRAL, NOTE_BACKGRAD_FINAL);
                this.SetPropertyField(PROP_FIELD_TEXT, "");
                this.SetPropertyField(PROP_FIELD_TEXTFORMAT, new TextFormat("Comic Sans MS", 11, Brushes.Black, true));
                this.SetPropertyField(PROP_FIELD_FOREGROUND, Brushes.Goldenrod);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Background);
                this.BaseWidth = NOTE_INI_WIDTH;
                this.BaseHeight = NOTE_INI_HEIGHT;
            }
            else
            if (this.CanGroup)
            {
                var Foreground = VisualSymbolFormat.GetRegionForeground(this.Target.OwnerLocal)
                                        .NullDefault(VisualSymbolFormat.GetLineBrush(this.Target.OwnerLocal));

                var Background = VisualSymbolFormat.GetRegionBackground(this.Target.OwnerLocal)
                                        .NullDefault(VisualSymbolFormat.GetMainBackground(this.Target.OwnerLocal));

                var Dash = VisualSymbolFormat.GetRegionDash(this.Target.OwnerLocal)
                                .NullDefault(VisualSymbolFormat.GetLineDash(this.Target.OwnerLocal).NullDefault(DashStyles.Solid));

                var Thick = VisualSymbolFormat.GetRegionThickness(this.Target.OwnerLocal)
                                .SubstituteFor(0.0, VisualSymbolFormat.GetLineThickness(this.Target.OwnerLocal).SubstituteFor(0.0, 1.0));

                this.SetPropertyField(PROP_FIELD_FOREGROUND, Foreground);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Background);
                this.SetPropertyField(PROP_FIELD_LINEDASH, Dash);
                this.SetPropertyField(PROP_FIELD_LINETHICK, Thick);

                if (this.IsComplementGroupRegion)
                {
                    this.BaseWidth = (InitialWidth == 0.0 ? this.Target.OwnerLocal.BaseWidth * GROUPREGION_INI_WIDTH_FACTOR : InitialWidth);
                    this.BaseHeight = this.Target.OwnerLocal.TotalHeight * GROUPREGION_INI_TOTALHEIGHT_FACTOR;
                }

                if (this.IsComplementGroupLine)
                {
                    var PreviousGroupLine = Target.OwnerLocal.AttachedComplements.FirstOrDefault(comp => comp.IsComplementGroupLine);

                    var InitialOrientation = (PreviousGroupLine == null
                                              || PreviousGroupLine.GetPropertyField<Orientation>(PROP_FIELD_ORIENTATION) == Orientation.Horizontal
                                              ? Orientation.Vertical : Orientation.Horizontal);

                    this.SetPropertyField(PROP_FIELD_ORIENTATION, InitialOrientation);

                    if (InitialOrientation == Orientation.Vertical)
                    {
                        this.BaseWidth = GROUPLINE_INI_THICKNESS;
                        this.BaseHeight = Math.Min(this.Target.OwnerLocal.BaseWidth, this.Target.OwnerLocal.TotalHeight)
                                          * GROUPLINE_INI_LENGTH_FACTOR;
                    }
                    else
                    {
                        this.BaseWidth = Math.Min(this.Target.OwnerLocal.BaseWidth, this.Target.OwnerLocal.TotalHeight)
                                         * GROUPLINE_INI_LENGTH_FACTOR;
                        this.BaseHeight = GROUPLINE_INI_THICKNESS;
                    }
                }
            }
            else
            {
                this.SetPropertyField(PROP_FIELD_FOREGROUND, Brushes.Blue);
                this.SetPropertyField(PROP_FIELD_BACKGROUND, Brushes.White);

                if (this.IsComplementInfoCard)
                {
                    this.BaseWidth = 300;
                    this.BaseHeight = (Target.OwnerGlobal.OwnerCompositeContainer is Composition
                                        ? 120 : 156);
                }
                else    // Legend...
                {
                    this.BaseWidth = 300;
                    this.BaseHeight = 300;
                }
            }

            var ContextView = this.GetDisplayingView();
            if (ContextView.SnapToGrid)
            {
                var SnappedArea = ContextView.GetGridSnappedArea(this.BaseCenter, this.BaseWidth, this.BaseHeight);
                this.BaseWidth = SnappedArea.Width.EnforceMinimum(1.0);
                this.BaseHeight = SnappedArea.Height.EnforceMinimum(1.0);
                this.BaseCenter = new Point(SnappedArea.X + SnappedArea.Width / 2.0, SnappedArea.Y + SnappedArea.Height / 2.0);
            }
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized]
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (this.PropertyFields == null)
                this.PropertyFields = new EditableDictionary<string, object>(__PropertyFields.TechName, (IModelEntity)Target.Owner);

            if (!this.IsComplementNote)
                return;

            // Adjusts Note Background gradient offsets
            var NoteBackground = GetPropertyField<LinearGradientBrush>(PROP_FIELD_BACKGROUND);
            if (NoteBackground == null || NoteBackground.GradientStops.Count != 3
                || (NoteBackground.GradientStops[0].Offset == NOTE_BACKGRAD_INITIAL &&
                    NoteBackground.GradientStops[1].Offset == NOTE_BACKGRAD_CENTRAL &&
                    NoteBackground.GradientStops[2].Offset == NOTE_BACKGRAD_FINAL))
                return;

            SetPropertyField(PROP_FIELD_BACKGROUND, Display.GetGradientBrush(NoteBackground.GradientStops[0].Color,
                                                                             NoteBackground.GradientStops[1].Color,
                                                                             NoteBackground.GradientStops[2].Color,
                                                                             true, NOTE_BACKGRAD_INITIAL, NOTE_BACKGRAD_CENTRAL, NOTE_BACKGRAD_FINAL));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the View visually containing this visual object.
        /// </summary>
        public override View GetDisplayingView()
        {
            return (this.Target.IsGlobal ? this.Target.OwnerGlobal : this.Target.OwnerLocal.OwnerRepresentation.DisplayingView);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool IsComplementCallout { get { return this.Kind.TechName == Domain.ComplementDefCallout.TechName; } }
        public bool IsComplementQuote { get { return this.Kind.TechName == Domain.ComplementDefQuote.TechName; } }
        public bool IsComplementImage { get { return this.Kind.TechName == Domain.ComplementDefImage.TechName; } }
        public bool IsComplementInfoCard { get { return this.Kind.TechName == Domain.ComplementDefInfoCard.TechName; } }
        public bool IsComplementLegend { get { return this.Kind.TechName == Domain.ComplementDefLegend.TechName; } }
        public bool IsComplementNote { get { return this.Kind.TechName == Domain.ComplementDefNote.TechName; } }
        public bool IsComplementGroupRegion { get { return this.Kind.TechName == Domain.ComplementDefGroupRegion.TechName; } }
        public bool IsComplementGroupLine { get { return this.Kind.TechName == Domain.ComplementDefGroupLine.TechName; } }
        public bool IsComplementStamp { get { return this.Kind.TechName == Domain.ComplementDefStamp.TechName; } }
        public bool IsComplementText { get { return this.Kind.TechName == Domain.ComplementDefText.TechName; } }
        public bool IsAttachedFloater{ get { return (this.IsComplementCallout || this.IsComplementQuote); } }

        public bool CanGroup { get { return (this.IsComplementGroupRegion || this.IsComplementGroupLine); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Type of Complement implemented.
        /// </summary>
        public SimplePresentationElement Kind { get { return __Kind.Get(this); } set { __Kind.Set(this, value); } }
        protected SimplePresentationElement Kind_;
        public static readonly ModelPropertyDefinitor<VisualComplement, SimplePresentationElement> __Kind =
                   new ModelPropertyDefinitor<VisualComplement, SimplePresentationElement>("Kind", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.Kind_, (ins, val) => ins.Kind_ = val, false, false,
                                                                                           "Kind", "Type of Complement implemented.");

        /// <summary>
        /// DEPRECATED: Contained image or text.
        /// </summary>
        // Contained information in tuples:
        // For Text, Note and Stamp Complements is: Tuple<string, TextFormat, StoreBox<Brush>, StoreBox<Brush>>
        //                                  Having: Text, format, foreground-brush and background-brush.
        // For Callout and Quote Complement is: Tuple<string, TextFormat, StoreBox<Brush>, StoreBox<Brush>, EVecinityQuadrant, double, double>
        //                    Having: Text, format, foreground-brush, background-brush, relative positioning quadrant, horizotal-offset, vertical-offset.
        // For Region, InfoCard and Legend Complements is: Tuple<StoreBox<Brush>, StoreBox<Brush>>
        //                                         Having: Foreground-brush and background-brush.
        public object Content { get { return __Content.Get(this); } set { __Content.Set(this, value); } }
        protected object Content_ = null;
        public static readonly ModelPropertyDefinitor<VisualComplement, object> __Content =
                   new ModelPropertyDefinitor<VisualComplement, object>("Content", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Content_, (ins, val) => ins.Content_ = val, false, false,
                                                                        "Content", "Contained text or image.");

        /// <summary>
        /// Property-Fields of the complement.
        /// </summary>
        protected EditableDictionary<string, object> PropertyFields { get; set; }
        public static ModelDictionaryDefinitor<VisualComplement, string, object> __PropertyFields =
                   new ModelDictionaryDefinitor<VisualComplement, string, object>("PropertyFields", EEntityMembership.InternalCoreExclusive, ins => ins.PropertyFields, (ins, coll) => ins.PropertyFields = coll,
                                                                                  "Property-Fields", "Property-Fields of the complement.");

        /// <summary>
        /// Visual object targeted by this Complement.
        /// </summary>
        public Ownership<View, VisualSymbol> Target { get { return __Target.Get(this); } set { __Target.Set(this, value); } }
        protected Ownership<View, VisualSymbol> Target_;
        public static readonly ModelPropertyDefinitor<VisualComplement, Ownership<View, VisualSymbol>> __Target =
                   new ModelPropertyDefinitor<VisualComplement, Ownership<View, VisualSymbol>>("Target", EEntityMembership.InternalCoreExclusive, true, EPropertyKind.Common, ins => ins.Target_, (ins, val) => ins.Target_ = val, true, false,
                                                                                               "Target", "Visual object targeted by this Complement.");

        /// <summary>
        /// Central position where the figure is displayed around.
        /// </summary>
        public override Point BaseCenter { get { return __BaseCenter.Get(this); } set { __BaseCenter.Set(this, value); } }
        protected Point BaseCenter_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualComplement, Point> __BaseCenter =
                   new ModelPropertyDefinitor<VisualComplement, Point>("BaseCenter", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseCenter_, (ins, val) => ins.BaseCenter_ = val, false, false,
                                                                       "Base Center", "Central position where the figure is displayed around.");

        /// <summary>
        /// Horizontal left position of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// </summary>
        [Description("Horizontal left position of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.")]
        public override double BaseLeft
        {
            get { return (this.BaseCenter.X - this.BaseWidth / 2.0); }
            set
            {
                if (value == this.BaseLeft)
                    return;

                this.BaseCenter = new Point(value + this.BaseWidth / 2.0, this.BaseCenter.Y);
            }
        }

        /// <summary>
        /// Vertical top position of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// </summary>
        [Description("Vertical top position of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.")]
        public override double BaseTop
        {
            get { return (this.BaseCenter.Y - this.BaseHeight / 2.0); }
            set
            {
                if (value == this.BaseTop)
                    return;

                this.BaseCenter = new Point(this.BaseCenter.X, value + this.BaseHeight / 2.0);
            }
        }

        /// <summary>
        /// Width of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// </summary>
        public override double BaseWidth
        {
            get { return __BaseWidth.Get(this); }
            set
            {
                if (value < 0)
                    return;

                if (this.BaseWidth_ > 0)
                    this.BaseCenter = new Point(this.BaseCenter_.X + (value - this.BaseWidth_) / 2.0, this.BaseCenter_.Y);

                __BaseWidth.Set(this, value);
            }
        }
        protected double BaseWidth_ = 0;
        public static readonly ModelPropertyDefinitor<VisualComplement, double> __BaseWidth =
                   new ModelPropertyDefinitor<VisualComplement, double>("BaseWidth", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseWidth_, (ins, val) => ins.BaseWidth_ = val, false, false,
                                                                        "Base Width", "Width of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.");

        /// <summary>
        /// Height of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.
        /// </summary>
        public override double BaseHeight
        {
            get { return __BaseHeight.Get(this); }
            set
            {
                if (value < 0)
                    return;

                if (this.BaseHeight_ > 0)
                    this.BaseCenter = new Point(this.BaseCenter_.X, this.BaseCenter_.Y + (value - this.BaseHeight_) / 2.0);

                __BaseHeight.Set(this, value);
            }
        }
        protected double BaseHeight_ = 0;
        public static readonly ModelPropertyDefinitor<VisualComplement, double> __BaseHeight =
                   new ModelPropertyDefinitor<VisualComplement, double>("BaseHeight", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseHeight_, (ins, val) => ins.BaseHeight_ = val, false, false,
                                                                        "Base Height", "Height of the figure bounds rectangle area, containing the actual geometry which maybe is not rectangular.");

        /// <summary>
        /// Area of the figure.
        /// </summary>
        [Description("Area of the figure.")]
        public override Rect BaseArea { get { return new Rect(this.BaseLeft, this.BaseTop, this.BaseWidth, this.BaseHeight); } }

        /// <summary>
        /// Gets the current area of the object.
        /// </summary>
        public override Rect TotalArea { get { return this.BaseArea; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this object can be moved.
        /// </summary>
        public override bool CanMove { get { return true; } }

        /// <summary>
        /// Indicates whether this object can be resized.
        /// </summary>
        public override bool CanResize { get { return true; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the movable pieces which this visual-object considers as visually united, plus indication of being contained within a region.
        /// Note: Objects reported can appear repeated, so apply Distinct before use.
        /// </summary>
        /// <param name="IncludeRelatedOrigins">If applicable, indicates whether the Origins subtree must be considered as to be moved.</param>
        /// <param name="IncludeRelatedTargets">If applicable, indicates whether the Targets subtree must be considered as to be moved.</param>
        /// <param name="IsForVisualization">Indicates that this request is for visualizing and not movement (thus, the indirectly/implicitly selected objects must not be included).</param>
        public override IEnumerable<Tuple<VisualObject,bool>> GetMovableMembers(bool IncludeRelatedOrigins, bool IncludeRelatedTargets, bool IsForVisualization)
        {
            var Result = base.GetMovableMembers(IncludeRelatedOrigins, IncludeRelatedTargets, IsForVisualization);

            if (!this.Target.IsGlobal && this.CanGroup)
            {
                var FullyInside = this.IsComplementGroupRegion &&
                                  !this.Target.OwnerLocal.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.CanGroupIntersectingObjects;

                Result = Result.Concat(this.GetDisplayingView().GetVisualObjectsInside(this.TotalArea, true, FullyInside)
                                                                    .Select(obj => Tuple.Create(obj, true)));

                Result = Result.Concat(Result.CastAs<Tuple<VisualComplement,bool>, Tuple<VisualObject,bool>>(vcomp => vcomp.Item1.CanGroup)
                                                .Select(vcomp => Tuple.Create((VisualObject)vcomp.Item1.Target.OwnerLocal, true)));
            }

            if (!this.Target.IsGlobal && this.CanGroup)
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                    Result = Result.Where(obj => obj.Item1 != this.Target.OwnerLocal);    // Maybe included if over the group line
                else
                    if (!Result.Any(obj => obj.Item1 == this.Target.OwnerLocal))
                        Result = Result.Concatenate(Tuple.Create((VisualObject)this.Target.OwnerLocal, true));

            // IMPORTANT: Do not call again owner's movable-members to avoid an infinite-loop
            //            .Concat(Target.OwnerLocal.GetMovableMembers(IncludeRelatedOrigins, IncludeRelatedTargets, IsForVisualization))
            // IMPORTANT: Do not apply .Distinct() to give the caller a chance to detect duplicates

            return Result;
        }

        /// <summary>
        /// Moves the base figure to the specified coordinates.
        /// </summary>
        public override void MoveTo(double PosX, double PosY, bool LockNewPosition = false, bool IsResizing = false)
        {
            this.BaseCenter = new Point(PosX, PosY);

            if (this.IsComplementCallout || this.IsComplementQuote)
                this.StoreFloaterInfo(this.Target.OwnerLocal.TotalArea, new Point(PosX, PosY));

            this.Render();
        }

        /// <summary>
        /// Resizes the figure to the specified dimensions.
        /// Returns indication of valid resizing respect the minimum allowed.
        /// </summary>
        public override bool ResizeTo(double Width, double Height)
        {
            if (this.IsComplementGroupLine)
            {
                var Direction = this.GetPropertyField<Orientation>(PROP_FIELD_ORIENTATION);

                if (Direction == Orientation.Vertical)
                {
                    if (!Width.IsInRange(VisualComplement.DefaultGroupLineThicknessRange.Item1,
                                         VisualComplement.DefaultGroupLineThicknessRange.Item2) ||
                        Height < VisualComplement.DefaultMinBaseFigureSize.Height)
                        return false;
                }
                else
                {
                    if (!Height.IsInRange(VisualComplement.DefaultGroupLineThicknessRange.Item1,
                                          VisualComplement.DefaultGroupLineThicknessRange.Item2) ||
                        Width < VisualComplement.DefaultMinBaseFigureSize.Width)
                        return false;
                }
            }
            else
                if (Width < VisualComplement.DefaultMinBaseFigureSize.Width ||
                    Height < VisualComplement.DefaultMinBaseFigureSize.Height)
                    return false;

            this.BaseWidth = Width;
            this.BaseHeight = Height;

            this.Render();
 
            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the initial width of a text-based Complement.
        /// </summary>
        public double GetInitialWidth()
        {
            return this.Kind.TechName.SelectCorresponding(
                         new Dictionary<string, double>() { { Domain.ComplementDefCallout.TechName, CALLOUT_INI_WIDTH },
                                                            { Domain.ComplementDefQuote.TechName, QUOTE_INI_WIDTH },
                                                            { Domain.ComplementDefText.TechName, TEXT_INI_WIDTH },
                                                            { Domain.ComplementDefStamp.TechName, STAMP_INI_WIDTH },
                                                            { Domain.ComplementDefNote.TechName, NOTE_INI_WIDTH } }, 10.0);
        }

        /// <summary>
        /// Gets the initial height of a text-based Complement.
        /// </summary>
        public double GetInitialHeight()
        {
            return this.Kind.TechName.SelectCorresponding(
                         new Dictionary<string, double>() { { Domain.ComplementDefCallout.TechName, CALLOUT_INI_HEIGHT },
                                                            { Domain.ComplementDefQuote.TechName, QUOTE_INI_HEIGHT },
                                                            { Domain.ComplementDefText.TechName, TEXT_INI_HEIGHT },
                                                            { Domain.ComplementDefStamp.TechName, STAMP_INI_HEIGHT },
                                                            { Domain.ComplementDefNote.TechName, NOTE_INI_HEIGHT } }, 10.0);
        }

        public bool CanSetText
        {
            get
            {
                return this.Kind.TechName.IsOneOf(Domain.ComplementDefText.TechName, Domain.ComplementDefNote.TechName,
                                                                         Domain.ComplementDefStamp.TechName, Domain.ComplementDefCallout.TechName, Domain.ComplementDefQuote.TechName); } }

        public bool CanSetColors { get { return !this.IsComplementImage; } }

        public TField GetPropertyField<TField>(string FieldName, bool ReturnDefault = true)
        {
            if (!this.PropertyFields.ContainsKey(FieldName))
                if (ReturnDefault)
                {
                    var Default = default(TField);

                    if (FieldName == PROP_FIELD_LINETHICK && typeof(TField) == typeof(double))
                    {
                        var DefThick = (this.IsComplementGroupRegion
                                        ? this.Target.OwnerLocal.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.LineThickness
                                        : (this.IsComplementStamp
                                           ? 2.0 : (this.IsComplementNote
                                                    ? 0.1 : (this.IsComplementCallout || this.IsComplementQuote
                                                             ? 0.5 : (this.IsComplementInfoCard || this.IsComplementLegend
                                                                      ? 0.25 : 1.0)))));
                        Default = (TField)((object)DefThick);
                    }
                    else
                        if (FieldName == PROP_FIELD_LINEDASH && typeof(TField) == typeof(DashStyle))
                            Default = (TField)((object)DashStyles.Solid);

                    return Default;
                }
                else
                    throw new UsageAnomaly("Property field not assigned: " + FieldName);

            var Value = this.PropertyFields[FieldName];

            if (Value != null && Value is ImageAssignment)
                Value = ((ImageAssignment)Value).Image;

            if (Value != null && Value is StoreBoxBase)
                Value = ((StoreBoxBase)Value).StoredObject;

            var Result = (Value is TField ? (TField)Value : default(TField));
            return Result;
        }

        public void SetPropertyField(string FieldName, object Value)
        {
            if (!ApplicablePropertiesByKind[this.Kind.TechName].Contains(FieldName))
                return;

            if (!PropertyFieldsTypes.ContainsKey(FieldName))
                throw new UsageAnomaly("Unknown property field: " + FieldName);

            if (Value != null)
            {
                if (!PropertyFieldsTypes[FieldName].IsAssignableFrom(Value.GetType()))
                    throw new UsageAnomaly("Value assigned to '" + FieldName+ "' property field has wrong type.");

                if (Value is ImageSource)
                    Value = ((ImageSource)Value).AssignImage();

                if (Value is Brush)
                    Value = StoreBox.Store<Brush>((Brush)Value);

                if (Value is DashStyle)
                    Value = StoreBox.Store<DashStyle>((DashStyle)Value);
            }

            this.PropertyFields[FieldName] = Value;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Recreates and returns the Graphic of this visual object descendant and its children, for a presentation context
        /// and indicating whether to show manipulation adorners.
        /// </summary>
        public override ContainerVisual GenerateGraphic(UIElement PresentationContext, bool ShowManipulationAdorners)
        {
            this.Graphic = this.CreateDraw(ShowManipulationAdorners).RenderToDrawingVisual();
            return this.Graphic;
        }

        /// <summary>
        /// Generates and returns this Complement visual representation as Drawing.
        /// </summary>
        public DrawingGroup CreateDraw(bool ShowManipulationAdorners)
        {
            DrawingGroup Result = null;

            if (this.IsComplementImage)
                Result = GenerateImage(this);
            else
                if (this.IsComplementInfoCard)
                    Result = GenerateInfoCard(this, this.GetDisplayingView());
                else
                    if (this.IsComplementLegend)
                        Result = GenerateLegend(this, this.GetDisplayingView());
                    else
                        if (this.IsComplementGroupRegion)
                            Result = GenerateVisualSymbolGroupRegion(this.GetDisplayingView(), this, this.Target.OwnerLocal);
                        else
                            if (this.IsComplementGroupLine)
                                Result = GenerateVisualSymbolGroupLine(this.GetDisplayingView(), this, this.Target.OwnerLocal);
                            else
                                if (this.IsComplementCallout || this.IsComplementQuote)
                                    Result = GenerateVisualSymbolFloater(this.GetDisplayingView(), this, this.Target.OwnerLocal);
                                else
                                    if (this.IsComplementNote)
                                        Result = GenerateNoteGraphic(this);
                                    else
                                        if (this.IsComplementText)
                                           Result = GenerateTextGraphic(this);
                                        else
                                            if (this.IsComplementStamp)
                                                Result = GenerateStampGraphic(this);

            // Register Selection Indicators for drawing
            if (ShowManipulationAdorners)
                if (this.IsSelected)
                    this.GetDisplayingView().AttachAdorner(this, GenerateSelectionIndicators(INDICATOR_SIZE, SelectionIndicatorBackground,
                                                                                             SelectionIndicatorForeground, SelectionIndicatorGeometryCreator,
                                                                                             (this.IsComplementGroupLine
                                                                                              ? new Nullable<Orientation>(this.GetPropertyField<Orientation>(VisualComplement.PROP_FIELD_ORIENTATION))
                                                                                              : null))
                                                                    .Select(tup => tup.Item1));
                else
                    this.GetDisplayingView().DetachAdorner(this);

            // Vanish if required (e.g. when cutting)
            if (this.IsVanished)
                Result.Opacity = VisualRepresentation.SELECTION_VANISHING_OPACITY;

            return Result;
        }

        /// <summary>
        /// Indicates wether this visual object is selected for later applying a command.
        /// </summary>
        public override bool IsSelected { get { return this.IsSelected_; } set { this.IsSelected_ = value; } }
        [NonSerialized]
        private bool IsSelected_ = false;

        /// <summary>
        /// Indicates wether this visual object is vanished as a result of been marked for deletion.
        /// </summary>
        public override bool IsVanished { get { return this.IsVanished_; } set { this.IsVanished_ = value; } }
        [NonSerialized]
        private bool IsVanished_ = false;

        /// <summary>
        /// Renders the represented semantic object.
        /// </summary>
        public override void RenderRepresentatedObject()
        {
            this.Render();
        }

        /// <summary>
        /// Updates the visual presentation of this object on the displaying View.
        /// </summary>
        public void Render()
        {
            this.GetDisplayingView().Show(this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualComplement> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualComplement> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualComplement> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualComplement CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualComplement)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualComplement PopulateFrom(VisualComplement SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        /// <summary>
        /// Draws and returns a set of indicator adorners plus manipulation-direction, based on supplied
        /// Indicator Size, Stroke, Pencil and optional Geometry-Creator, for mark the selection of this visual element.
        /// </summary>
        public List<Tuple<Drawing,EManipulationDirection>> GenerateSelectionIndicators(double IndicatorSize, Brush IndStroke, Pen IndPencil,
                                                                                       Func<Rect, Geometry> GeometryCreator = null, Orientation? UniqueDirection = null)
        {
            GeometryCreator = GeometryCreator.NullDefault((rect) => new RectangleGeometry(rect));

            var StandardIndicators = new List<Tuple<Drawing,EManipulationDirection>>();
            double PosX = 0, PosY = 0;
            Drawing IndHeadingTop = null, IndHeadingBottom = null, IndHeadingLeft = null, IndHeadingRight = null,
                    IndHeadingTopLeft = null, IndHeadingTopRight = null, IndHeadingBottomLeft = null, IndHeadingBottomRight = null;

            // Centrals...
            if ((UniqueDirection != null && UniqueDirection.Value == Orientation.Vertical)
                || (UniqueDirection == null && this.BaseArea.Width >= IndicatorSize * 3.0))
            {
                PosX = (this.BaseArea.X + this.BaseArea.Width / 2.0) - IndicatorSize / 2.0;
                PosY = this.BaseArea.Y - IndicatorSize / 2.0;
                IndHeadingTop = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingTop, EManipulationDirection.Top));

                PosY = this.BaseArea.Y + this.BaseArea.Height - IndicatorSize / 2.0;
                IndHeadingBottom = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingBottom, EManipulationDirection.Bottom));

            }

            if ((UniqueDirection != null && UniqueDirection.Value == Orientation.Horizontal)
                || (UniqueDirection == null && this.BaseArea.Height >= IndicatorSize * 3.0))
            {
                PosX = this.BaseArea.X - IndicatorSize / 2.0;
                PosY = (this.BaseArea.Y + this.BaseArea.Height / 2.0) - IndicatorSize / 2.0;
                IndHeadingLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingLeft, EManipulationDirection.Left));

                PosX = this.BaseArea.X + this.BaseArea.Width - IndicatorSize / 2.0;
                IndHeadingRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingRight, EManipulationDirection.Right));
            }

            // Corners...
            if (UniqueDirection == null)
            {
                PosX = this.BaseArea.X - IndicatorSize / 2.0;
                PosY = this.BaseArea.Y - IndicatorSize / 2.0;
                IndHeadingTopLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingTopLeft, EManipulationDirection.TopLeft));

                PosX = this.BaseArea.X + this.BaseArea.Width - IndicatorSize / 2.0;
                IndHeadingTopRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingTopRight, EManipulationDirection.TopRight));

                PosX = this.BaseArea.X - IndicatorSize / 2.0;
                PosY = this.BaseArea.Y + this.BaseArea.Height - IndicatorSize / 2.0;
                IndHeadingBottomLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingBottomLeft, EManipulationDirection.BottomLeft));

                PosX = this.BaseArea.X + this.BaseArea.Width - IndicatorSize / 2.0;
                IndHeadingBottomRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                StandardIndicators.Add(Tuple.Create(IndHeadingBottomRight, EManipulationDirection.BottomRight));
            }

            return StandardIndicators;
        }

        /// <summary>
        /// For Callout and Quote complements, updates it position in relation to the targeted symbol.
        /// </summary>
        public void UpdatePositionRelativeToTarget(double DeltaX, double DeltaY, bool ApplyRender = true, bool IsResizingTarget = false)
        {
            double NewX = 0, NewY = 0;

            if (this.CanGroup)
            {
                if (IsResizingTarget)
                    return;

                NewX = this.BaseCenter.X + DeltaX;
                NewY = this.BaseCenter.Y + DeltaY;
            }
            else
                if (this.Target.OwnerLocal != null)
                {
                    var Quadrant = this.GetPropertyField<EVecinityQuadrant>(PROP_FIELD_QUADRANT);
                    var OffsetX = this.GetPropertyField<double>(PROP_FIELD_OFFSETX);
                    var OffsetY = this.GetPropertyField<double>(PROP_FIELD_OFFSETY);

                    if (Quadrant == EVecinityQuadrant.RightDown
                        || Quadrant == EVecinityQuadrant.RightUp)
                        NewX = this.Target.OwnerLocal.TotalArea.Right + OffsetX;
                    else
                        NewX = this.Target.OwnerLocal.TotalArea.Left + OffsetX;

                    if (Quadrant == EVecinityQuadrant.RightDown
                        || Quadrant == EVecinityQuadrant.LeftDown)
                        NewY = this.Target.OwnerLocal.TotalArea.Bottom + OffsetY;
                    else
                        NewY = this.Target.OwnerLocal.TotalArea.Top + OffsetY;
                }

            this.BaseCenter = new Point(NewX, NewY);

            if (ApplyRender)
                this.Render();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void StoreFloaterInfo(Rect TargetTotalArea,   // Must pass VisualSymbol.TotalArea
                                     Point CalloutCenter)
        {
            var TargetCenter = new Point(TargetTotalArea.X + TargetTotalArea.Width / 2.0,
                                         TargetTotalArea.Y + TargetTotalArea.Height / 2.0);
            EVecinityQuadrant Quadrant = default(EVecinityQuadrant);
            double OffsetX = 0, OffsetY = 0;

            if (CalloutCenter.X > TargetCenter.X)
            {
                OffsetX = CalloutCenter.X - TargetTotalArea.Right;

                if (CalloutCenter.Y > TargetCenter.Y)
                {
                    OffsetY = CalloutCenter.Y - TargetTotalArea.Bottom;
                    Quadrant = EVecinityQuadrant.RightDown;
                }
                else
                {
                    OffsetY = CalloutCenter.Y - TargetTotalArea.Top;
                    Quadrant = EVecinityQuadrant.RightUp;
                }
            }
            else
            {
                OffsetX = CalloutCenter.X - TargetTotalArea.Left;

                if (CalloutCenter.Y > TargetCenter.Y)
                {
                    OffsetY = CalloutCenter.Y - TargetTotalArea.Bottom;
                    Quadrant = EVecinityQuadrant.LeftDown;
                }
                else
                {
                    OffsetY = CalloutCenter.Y - TargetTotalArea.Top;
                    Quadrant = EVecinityQuadrant.LeftUp;
                }
            }

            this.SetPropertyField(PROP_FIELD_QUADRANT, Quadrant);
            this.SetPropertyField(PROP_FIELD_OFFSETX, OffsetX);
            this.SetPropertyField(PROP_FIELD_OFFSETY, OffsetY);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static DrawingGroup GenerateImage(VisualComplement Complement)
        {
            var Result = new DrawingGroup();

            var ContainedPicture = Complement.GetPropertyField<ImageSource>(PROP_FIELD_IMAGE);
            if (ContainedPicture != null)
                Result.Children.Add(new ImageDrawing(ContainedPicture, Complement.BaseArea));

            return Result;
        }

        public static DrawingGroup GenerateNoteGraphic(VisualComplement Complement)
        {
            var Result = new DrawingGroup();
            Geometry Shape = new RectangleGeometry(Complement.BaseArea);
            Rect ContentZone = Rect.Empty;
            GeometryDrawing GeomDrawing = null;

            var Text = Complement.GetPropertyField<string>(PROP_FIELD_TEXT);
            var LabelFormat = Complement.GetPropertyField<TextFormat>(PROP_FIELD_TEXTFORMAT);
            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);

            var FoldSize = 8.0;
            var GeomGroup = new GeometryGroup();
            GeomGroup.Children.Add(Display.PathFigureGeometry(new Point(Complement.BaseLeft, Complement.BaseTop),
                                                                new Point(Complement.BaseLeft + Complement.BaseWidth, Complement.BaseTop),
                                                                new Point(Complement.BaseLeft + Complement.BaseWidth, Complement.BaseTop + Complement.BaseHeight - FoldSize),
                                                                new Point(Complement.BaseLeft + Complement.BaseWidth - FoldSize, Complement.BaseTop + Complement.BaseHeight),
                                                                new Point(Complement.BaseLeft, Complement.BaseTop + Complement.BaseHeight)));
            GeomGroup.Children.Add(new LineGeometry(new Point(Complement.BaseLeft + Complement.BaseWidth - FoldSize, Complement.BaseTop + Complement.BaseHeight - FoldSize),
                                                    new Point(Complement.BaseLeft + Complement.BaseWidth - FoldSize, Complement.BaseTop + Complement.BaseHeight)));
            GeomGroup.Children.Add(new LineGeometry(new Point(Complement.BaseLeft + Complement.BaseWidth - FoldSize, Complement.BaseTop + Complement.BaseHeight - FoldSize),
                                                    new Point(Complement.BaseLeft + Complement.BaseWidth, Complement.BaseTop + Complement.BaseHeight - FoldSize)));
            Shape = GeomGroup;
            ContentZone = new Rect(Complement.BaseLeft + FoldSize / 2.0, Complement.BaseTop + FoldSize / 2.0,
                                    Complement.BaseWidth - FoldSize, Complement.BaseHeight - FoldSize * 1.5);

            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH);

            GeomDrawing = new GeometryDrawing(Background, Pencil, Shape);

            Result.Children.Add(GeomDrawing);

            using (var Context = Result.Append())
                Context.DrawText(LabelFormat.GenerateFormattedText(Text, ContentZone.Width, ContentZone.Height),
                                    ContentZone.TopLeft);

            //Result.Transform = new RotateTransform(-7.5, this.BaseCenter.X, this.BaseCenter.Y);

            return Result;
        }

        public static DrawingGroup GenerateTextGraphic(VisualComplement Complement)
        {
            var Result = new DrawingGroup();
            Geometry Shape = new RectangleGeometry(Complement.BaseArea);
            Rect ContentZone = Rect.Empty;
            GeometryDrawing GeomDrawing = null;

            var Text = Complement.GetPropertyField<string>(PROP_FIELD_TEXT);
            var LabelFormat = Complement.GetPropertyField<TextFormat>(PROP_FIELD_TEXTFORMAT);
            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);

            ContentZone = new Rect(Complement.BaseLeft + 2, Complement.BaseTop + 2,
                                    Complement.BaseWidth - 4, Complement.BaseHeight - 4);

            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH);

            GeomDrawing = new GeometryDrawing(Background, Pencil, Shape);

            Result.Children.Add(GeomDrawing);

            using (var Context = Result.Append())
            {
                var FmtText = LabelFormat.GenerateFormattedText(Text, ContentZone.Width, ContentZone.Height);
                var Top = (ContentZone.Top + ContentZone.Height / 2.0) - FmtText.Height / 2.0;
                Context.DrawText(FmtText, new Point(ContentZone.Left, Top));
            }

            return Result;
        }

        public static DrawingGroup GenerateStampGraphic(VisualComplement Complement)
        {
            var Result = new DrawingGroup();
            Geometry Shape = new RectangleGeometry(Complement.BaseArea, 5, 5);
            Rect ContentZone = Rect.Empty;
            GeometryDrawing GeomDrawing = null;

            var Text = Complement.GetPropertyField<string>(PROP_FIELD_TEXT);
            var LabelFormat = Complement.GetPropertyField<TextFormat>(PROP_FIELD_TEXTFORMAT);
            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);

            ContentZone = new Rect(Complement.BaseLeft + 2, Complement.BaseTop + 2,
                                    Complement.BaseWidth - 4, Complement.BaseHeight - 4);

            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH);

            GeomDrawing = new GeometryDrawing(Background, Pencil, Shape);

            Result.Children.Add(GeomDrawing);

            using (var Context = Result.Append())
            {
                var FmtText = LabelFormat.GenerateFormattedText(Text, ContentZone.Width, ContentZone.Height);
                var Top = (ContentZone.Top + ContentZone.Height / 2.0) - FmtText.Height / 2.0;
                Context.DrawText(FmtText, new Point(ContentZone.Left, Top));
            }

            Result.Transform = new RotateTransform(-15, Complement.BaseCenter.X, Complement.BaseCenter.Y);

            return Result;
        }

        public static DrawingGroup GenerateVisualSymbolFloater(View OwnerView, VisualComplement Complement, VisualSymbol Target)
        {
            var Result = new DrawingGroup();

            var Text = Complement.GetPropertyField<string>(PROP_FIELD_TEXT);
            var LabelFormat = Complement.GetPropertyField<TextFormat>(PROP_FIELD_TEXTFORMAT);
            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);

            var ContentZone = (Complement.IsComplementCallout
                               ? new Rect(Complement.BaseLeft + 4, Complement.BaseTop + 4,
                                          Complement.BaseWidth - 8, Complement.BaseHeight - 8)
                               : new Rect((Complement.BaseLeft + (Complement.BaseWidth / 2.0)) - (Complement.BaseWidth * 0.7) / 2.0,
                                          (Complement.BaseTop + (Complement.BaseHeight / 2.0)) - (Complement.BaseHeight * 0.7) / 2.0,
                                          Complement.BaseWidth * 0.7, Complement.BaseHeight * 0.7));

            var FloaterGeometry = (Complement.IsComplementCallout
                                   ? (Geometry)(new RectangleGeometry(Complement.BaseArea))
                                   : new EllipseGeometry(Complement.BaseArea));
            var ContactPoint = Target.BaseCenter.DetermineNearestIntersectingPoint(Complement.BaseCenter, OwnerView.Presenter, Target.Graphic, OwnerView.VisualHitTestFilter);

            var TrianglesBaseSize = Math.Min(Complement.BaseWidth, Complement.BaseHeight) / 3.0;
            Point TriangleBasePoint1, TriangleBasePoint2;

            if ((ContactPoint.X > Complement.BaseCenter.X && ContactPoint.Y < Complement.BaseCenter.Y) ||
                (ContactPoint.X < Complement.BaseCenter.X && ContactPoint.Y > Complement.BaseCenter.Y))
            {
                TriangleBasePoint1 = new Point(Complement.BaseCenter.X - TrianglesBaseSize / 2.0, Complement.BaseCenter.Y - TrianglesBaseSize / 2.0);
                TriangleBasePoint2 = new Point(Complement.BaseCenter.X + TrianglesBaseSize / 2.0, Complement.BaseCenter.Y + TrianglesBaseSize / 2.0);
            }
            else
            {
                TriangleBasePoint1 = new Point(Complement.BaseCenter.X + TrianglesBaseSize / 2.0, Complement.BaseCenter.Y - TrianglesBaseSize / 2.0);
                TriangleBasePoint2 = new Point(Complement.BaseCenter.X - TrianglesBaseSize / 2.0, Complement.BaseCenter.Y + TrianglesBaseSize / 2.0);
            }

            var GeomTip = new GeometryGroup();
            GeomTip.FillRule = FillRule.Nonzero;
            GeomTip.Children.Add(Display.PathFigureGeometry(TriangleBasePoint1, TriangleBasePoint2, ContactPoint));

            var Combination = new CombinedGeometry();
            Combination.Geometry1 = GeomTip;
            Combination.Geometry2 = FloaterGeometry;

            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH);

            var GeomDrawing = new GeometryDrawing(Background, Pencil, Combination);

            /*T var Grp = new DrawingGroup();
            Grp.Children.Add(GeomDrawing);
            Grp.Children.Add(new GeometryDrawing(Brushes.Gray, new Pen(Brushes.Blue, 0.5), new LineGeometry(TriangleBasePoint1, TriangleBasePoint2)));
            Grp.Children.Add(new GeometryDrawing(Brushes.Gray, new Pen(Brushes.Red, 0.5), new EllipseGeometry(ContactPoint, 3, 3)));
            return Grp; */

            Result.Children.Add(GeomDrawing);

            using (var Context = Result.Append())
                Context.DrawText(LabelFormat.GenerateFormattedText(Text, ContentZone.Width, ContentZone.Height),
                                 ContentZone.TopLeft);

            return Result;
        }

        public static DrawingGroup GenerateVisualSymbolGroupRegion(View OwnerView, VisualComplement Complement, VisualSymbol Target)
        {
            var Result = new DrawingGroup();

            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);
            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH);

            var Shape = new RectangleGeometry(Complement.BaseArea, 3, 3);

            /*- this is not the problem
            // To allow hit-testing of non-solid dash-styles.
            if (Pencil.DashStyle != DashStyles.Solid)
            {
                var SolidPen = new Pen(Brushes.Transparent, Pencil.Thickness);
                SolidPen.DashStyle = DashStyles.Solid;
                Result.Children.Add(new GeometryDrawing(null, SolidPen, Shape));
            } */

            Result.Children.Add(new GeometryDrawing(Background, Pencil, Shape));

            // Confuses fill-brush selection: Result.Opacity = 0.5;

            return Result;
        }

        public static DrawingGroup GenerateVisualSymbolGroupLine(View OwnerView, VisualComplement Complement, VisualSymbol Target)
        {
            var Result = new DrawingGroup();

            var Foreground = Complement.GetPropertyField<Brush>(PROP_FIELD_FOREGROUND);
            var Background = Complement.GetPropertyField<Brush>(PROP_FIELD_BACKGROUND);
            var Direction = Complement.GetPropertyField<Orientation>(PROP_FIELD_ORIENTATION);

            var StartPoint = (Direction == Orientation.Vertical
                              ? new Point(Complement.BaseCenter.X, Complement.BaseTop)
                              : new Point(Complement.BaseLeft, Complement.BaseCenter.Y));
            var EndPoint = (Direction == Orientation.Vertical
                            ? new Point(Complement.BaseCenter.X, Complement.BaseArea.Bottom)
                            : new Point(Complement.BaseArea.Right, Complement.BaseCenter.Y));

            var Pencil = new Pen(Foreground, Complement.GetPropertyField<double>(VisualComplement.PROP_FIELD_LINETHICK));
            Pencil.DashStyle = Complement.GetPropertyField<DashStyle>(VisualComplement.PROP_FIELD_LINEDASH);
            Pencil.StartLineCap = PenLineCap.Flat;
            Pencil.EndLineCap = PenLineCap.Flat;

            var Shape = new LineGeometry(StartPoint, EndPoint);

            Result.Children.Add(new GeometryDrawing(Background, Pencil, Shape));

            // Confuses fill-brush selection: Result.Opacity = 0.5;

            return Result;
        }

        /// <summary>
        /// Gets the initial position and width for a new Group Region Complement to be appended to the specified Target Symbol.
        /// </summary>
        public static Tuple<Point, double> GetGroupRegionInitialParams(VisualSymbol TargetSymbol)
        {
            var Placement = TargetSymbol.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.InitialGroupRegionPlacementHorizontal;

            var InitialWidth = (Placement == EPlacementOnBorderHorizontal.SameWidth
                                ? TargetSymbol.BaseWidth
                                : TargetSymbol.BaseWidth * GROUPREGION_INI_WIDTH_FACTOR);

            var PosX = TargetSymbol.BaseCenter.X;
            var PosY = (TargetSymbol.BaseCenter.Y +
                                    (TargetSymbol.TotalHeight * VisualComplement.GROUPREGION_INI_TOTALHEIGHT_FACTOR) / 2.0);

            // var Position = new Point(PosX, PosY);
            var Position = new Point((Placement.IsOneOf(EPlacementOnBorderHorizontal.CenterMiddle, EPlacementOnBorderHorizontal.CenterInward,
                                                        EPlacementOnBorderHorizontal.CenterOutward, EPlacementOnBorderHorizontal.SameWidth)
                                      ? PosX
                                      : (Placement.IsOneOf(EPlacementOnBorderHorizontal.LeftMiddle, EPlacementOnBorderHorizontal.LeftInward,
                                                           EPlacementOnBorderHorizontal.LeftOutward)
                                         ? (PosX - InitialWidth / 2.0) + TargetSymbol.BaseWidth / 2.0
                                         : (PosX + InitialWidth / 2.0) - TargetSymbol.BaseWidth / 2.0)),
                                     (Placement.IsOneOf(EPlacementOnBorderHorizontal.CenterMiddle, EPlacementOnBorderHorizontal.SameWidth,
                                                        EPlacementOnBorderHorizontal.LeftMiddle, EPlacementOnBorderHorizontal.RightMiddle)
                                      ? PosY
                                      : (Placement.IsOneOf(EPlacementOnBorderHorizontal.CenterInward, EPlacementOnBorderHorizontal.LeftInward,
                                                           EPlacementOnBorderHorizontal.RightInward)
                                         ? PosY - TargetSymbol.BaseHeight / 2.0
                                         : PosY + TargetSymbol.BaseHeight / 2.0)));

            // IMPORTANT: Do not snap to grid to avoid misplacing respect owner symbol.

            var Result = Tuple.Create(Position, InitialWidth);
            return Result;
        }

        /// <summary>
        /// Gets the initial position for a new Group Line Complement to be appended to the specified Target Symbol.
        /// </summary>
        public static Point GetGroupLineInitialPosition(VisualSymbol TargetSymbol)
        {
            var Previous = TargetSymbol.AttachedComplements.LastOrDefault(comp => comp.IsComplementGroupLine);

            var Direction = (Previous == null ? Orientation.Vertical : (Previous.GetPropertyField<Orientation>(PROP_FIELD_ORIENTATION) == Orientation.Vertical
                                                                        ? Orientation.Horizontal : Orientation.Vertical));

            var Result = default(Point);

            if (Direction == Orientation.Vertical)
                Result = new Point(TargetSymbol.BaseCenter.X,
                                   (TargetSymbol.BaseCenter.Y +
                                    (Math.Min(TargetSymbol.TotalHeight, TargetSymbol.BaseWidth)
                                     * VisualComplement.GROUPLINE_INI_LENGTH_FACTOR) / 2.0));
            else
                Result = new Point((TargetSymbol.BaseCenter.X +
                                    (Math.Min(TargetSymbol.BaseWidth, TargetSymbol.TotalHeight)
                                     * VisualComplement.GROUPLINE_INI_LENGTH_FACTOR) / 2.0),
                                   TargetSymbol.BaseCenter.Y);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Edits the specified Complement's content.
        /// </summary>
        public static void Edit(VisualComplement Complement, bool IsCreating = false)
        {
            if (IsCreating && Complement.Kind.TechName.IsOneOf(Domain.ComplementDefGroupRegion.TechName,
                                                               Domain.ComplementDefGroupLine.TechName,
                                                               Domain.ComplementDefInfoCard.TechName,
                                                               Domain.ComplementDefLegend.TechName))
                return;

            if (Complement.Kind.TechName == Domain.ComplementDefGroupRegion.TechName
                || Complement.Kind.TechName == Domain.ComplementDefGroupLine.TechName)
            {
                Complement.GetDisplayingView().EditPropertiesOfVisualRepresentation(Complement.Target.OwnerLocal.OwnerRepresentation);
                return;
            }

            if (Complement.Kind.TechName == Domain.ComplementDefInfoCard.TechName)
            {
                Complement.GetDisplayingView().OwnerCompositeContainer.OwnerComposition.Engine.EditCompositionProperties();
                return;
            }

            if (Complement.Kind.TechName == Domain.ComplementDefLegend.TechName)
            {
                DomainServices.DomainEdit(Complement.GetDisplayingView().OwnerCompositeContainer.OwnerComposition.CompositeContentDomain);
                return;
            }

            Complement.GetDisplayingView().EditEngine.StartCommandVariation("Edit Complement");

            if (Complement.IsComplementImage)
            {
                var Picture = Display.DialogGetImageFromFile();
                if (Picture == null)
                {
                    Complement.GetDisplayingView().EditEngine.DiscardCommandVariation();
                    return;
                }

                Complement.BaseWidth = Picture.GetWidth();
                Complement.BaseHeight = Picture.GetHeight();
                Complement.SetPropertyField(PROP_FIELD_IMAGE, Picture);
                Complement.Render();
            }
            else
                if (Complement.IsComplementInfoCard)
                {
                    if (CompositionEngine.EditViewProperties(Complement.GetDisplayingView()))
                        Complement.Render();
                }
                else
                    if (Complement.IsComplementLegend)
                    {
                        if (DomainServices.DomainEdit(Complement.GetDisplayingView().OwnerCompositeContainer.CompositeContentDomain))
                            Complement.Render();
                    }
                    else
                    {
                        if (!Complement.Target.IsGlobal)
                            Complement.GetDisplayingView().Manipulator.ApplySelection(Complement.Target.OwnerLocal);

                        Complement.GetDisplayingView().EditInPlace(Complement, true);
                    }

            Complement.GetDisplayingView().UpdateVersion();

            Complement.GetDisplayingView().EditEngine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Changes the Axis of the Complement.
        /// </summary>
        public static void ChangeAxis(VisualComplement Complement)
        {
            if (!Complement.IsComplementGroupLine)
                return;

            Complement.GetDisplayingView().EditEngine.StartCommandVariation("Change Complement Axis");

            var Direction = Complement.GetPropertyField<Orientation>(PROP_FIELD_ORIENTATION);

            if (Direction == Orientation.Vertical)
            {
                Direction = Orientation.Horizontal;

                var OrigThickness = Complement.BaseWidth;
                var OrigSeparation = (Complement.BaseCenter.Y - Complement.Target.OwnerLocal.BaseCenter.Y);
                var OrigOffset = (Complement.BaseCenter.X - Complement.Target.OwnerLocal.BaseCenter.X);
                var NewCenter = new Point(Complement.Target.OwnerLocal.BaseCenter.X + OrigSeparation,
                                          Complement.Target.OwnerLocal.BaseCenter.Y + OrigOffset);

                Complement.BaseWidth = Complement.BaseHeight;
                Complement.BaseHeight = OrigThickness;
                Complement.BaseCenter = NewCenter;
            }
            else
            {
                Direction = Orientation.Vertical;

                var OrigThickness = Complement.BaseHeight;
                var OrigSeparation = (Complement.BaseCenter.X - Complement.Target.OwnerLocal.BaseCenter.X);
                var OrigOffset = (Complement.BaseCenter.Y - Complement.Target.OwnerLocal.BaseCenter.Y);
                var NewCenter = new Point(Complement.Target.OwnerLocal.BaseCenter.X + OrigOffset,
                                          Complement.Target.OwnerLocal.BaseCenter.Y + OrigSeparation);

                Complement.BaseHeight = Complement.BaseWidth;
                Complement.BaseWidth = OrigThickness;
                Complement.BaseCenter = NewCenter;
            }

            Complement.SetPropertyField(PROP_FIELD_ORIENTATION, Direction);

            Complement.Render();

            Complement.GetDisplayingView().UpdateVersion();

            Complement.GetDisplayingView().EditEngine.CompleteCommandVariation();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public static void DrawLabel(DrawingContext Context, string Text, TextFormat Format,
                                    double Left, double Right, double Top, double LabelHeight, double Margin)
        {
            var LabelWidth = (Right - Left) - Margin * 2.0;
            if (LabelWidth < 1)
                return;

            var FmtText = Format.GenerateFormattedText(Text, LabelWidth, LabelHeight);
            Context.DrawText(FmtText, new Point(Left + Margin, Top + Margin));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates and returns a duplicate of this Complement instance.
        /// </summary>
        public VisualComplement GenerateIndependentDuplicate(Ownership<View, VisualSymbol> NewTarget)
        {
            var NewComplement = this.CreateClone(ECloneOperationScope.Slight, null);

            // Reassign global-id
            NewComplement.GlobalId = Guid.NewGuid();

            NewComplement.Target = NewTarget;

            /*- OLD: if (this.Content != null)
                NewComplement.Content = this.Content.GenerateDeepClone(); */

            return NewComplement;
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public string ContentAsText
        {
            get
            {
                var Text = this.GetPropertyField<string>(PROP_FIELD_TEXT);
                return Text;
            }
        }
        public override string ToString()
        {
            var Text = (this.Kind != null ? this.Kind.Name + " " : "") + "Complement";

            if (this.Kind != null
                && this.Kind.TechName.IsOneOf(Domain.ComplementDefText.TechName, Domain.ComplementDefStamp.TechName, Domain.ComplementDefNote.TechName,
                                              Domain.ComplementDefCallout.TechName, Domain.ComplementDefQuote.TechName))
                Text = Text + ": " + ContentAsText;
            else
                Text = Text + ".";

            return Text;
        }
    }
}