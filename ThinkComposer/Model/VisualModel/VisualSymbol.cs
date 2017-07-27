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
// File   : VisualSymbol.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.VisualSymbol (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer.ComposerUI;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using System.Runtime.Serialization;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Base ancestor for visual symbols such as vector-based drawings, images and texts.
    /// </summary>
    [Serializable]
    public abstract partial class VisualSymbol : VisualElement, IModelEntity, IModelClass<VisualSymbol>
    {
        /// <summary>
        /// Initialization height for details poster.
        /// </summary>
        public const double INITIAL_DETAILS_POSTER_HEIGHT = 100.0;

        /// <summary>
        /// Height of visual labels.
        /// </summary>
        public const double LABELING_HEIGHT = 14;

        /// <summary>
        /// Font size for labeling.
        /// </summary>
        public const double LABELING_FONT_SIZE = 9;

        /// <summary>
        /// Size used for hiding symbols
        /// </summary>
        public const double HIDING_SIZE = 0.1;

        /// <summary>
        /// Radius for rounded-rectangles
        /// </summary>
        public const double ROUNDEDRECT_RADIUS = 3.0;

        /// <summary>
        /// Minimum size for displaying a symbol.
        /// </summary>
        public static readonly Size MIN_SYMBOL_DISPLAY_SIZE = new Size(16, 16);

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualSymbol()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualSymbol>("VisualSymbol", VisualElement.__ClassDefinitor, "Visual Symbol",
                                                                     "Base ancestor for visual symbols such as vector-based drawings, images and texts.");
            __ClassDefinitor.DeclareProperty(__OwnerRepresentation);
            __ClassDefinitor.DeclareProperty(__BaseCenter);
            __ClassDefinitor.DeclareProperty(__BaseWidth);
            __ClassDefinitor.DeclareProperty(__BaseHeight);
            __ClassDefinitor.DeclareProperty(__IsAutoPositionable);
            __ClassDefinitor.DeclareProperty(__BaseContentArea);
            __ClassDefinitor.DeclareProperty(__DetailsContentArea);
            __ClassDefinitor.DeclareProperty(__AreDetailsShown);
            __ClassDefinitor.DeclareProperty(__ShowCompositeContentAsDetails);
            __ClassDefinitor.DeclareProperty(__DetailsPosterHeight);
            __ClassDefinitor.DeclareProperty(__CurrentDetailZones);
            __ClassDefinitor.DeclareProperty(__IsHorizontallyFlipped);
            __ClassDefinitor.DeclareProperty(__IsVerticallyFlipped);
            __ClassDefinitor.DeclareProperty(__IsTilted);
            __ClassDefinitor.DeclareProperty(__ShowAsMultiple);

            __ClassDefinitor.DeclareCollection(__TargetConnections);
            __ClassDefinitor.DeclareCollection(__OriginConnections);
            __ClassDefinitor.DeclareCollection(__Complements);

            MaxTableRecordsShown = AppExec.GetConfiguration("IdeaEditing", "Details.MaxTableRecordsShown", MaxTableRecordsShown);
            ShowAllDetailHeaders = AppExec.GetConfiguration("IdeaEditing", "Details.ShowAllDetailHeaders", ShowAllDetailHeaders);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualSymbol(VisualRepresentation OwnerRepresentation, EVisualRepresentationPart VisualRepresentationPart,
                            Point CenterPosition, double AreaWidth, double AreaHeight)
             : base(VisualRepresentationPart)
        {
            General.ContractRequiresNotNull(OwnerRepresentation);
            General.ContractRequires(VisualRepresentationPart == EVisualRepresentationPart.ConceptBodySymbol ||
                                     VisualRepresentationPart == EVisualRepresentationPart.RelationshipCentralSymbol);

            this.OwnerRepresentation = OwnerRepresentation;

            this.TargetConnections = new EditableList<VisualConnector>(__TargetConnections.TechName, this);
            this.OriginConnections = new EditableList<VisualConnector>(__OriginConnections.TechName, this);
            // Do not create the Complements collection unless necessary.
            
            this.BaseCenter = CenterPosition;
            this.BaseWidth = AreaWidth;
            this.BaseHeight = AreaHeight;

            //- this.IsHorizontallyFlipped = OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.InitiallyFlippedHorizontally;
            //- this.IsVerticallyFlipped = OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.InitiallyFlippedVertically;
            //- this.IsTilted = OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.InitiallyTilted;
            //- this.ShowAsMultiple = OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat.AsMultiple;
        }

        protected VisualSymbol()
                : base()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the owning visual representator.
        /// </summary>
        public override VisualRepresentation OwnerRepresentation { get { return __OwnerRepresentation.Get(this); } set { __OwnerRepresentation.Set(this, value); } }
        protected VisualRepresentation OwnerRepresentation_;
        public static readonly ModelPropertyDefinitor<VisualSymbol, VisualRepresentation> __OwnerRepresentation =
                   new ModelPropertyDefinitor<VisualSymbol, VisualRepresentation>("OwnerRepresentation", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerRepresentation_, (ins, val) => ins.OwnerRepresentation_ = val, false, false,
                                                                                  "Owner Representation", "References the owning visual representator.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of targeted connectors whose origin is this symbol.
        /// </summary>
        public EditableList<VisualConnector> TargetConnections { get; protected set; }
        public static ModelListDefinitor<VisualSymbol, VisualConnector> __TargetConnections =
                   new ModelListDefinitor<VisualSymbol, VisualConnector>("TargetConnections", EEntityMembership.InternalBulk, ins => ins.TargetConnections, (ins, coll) => ins.TargetConnections = coll,
                                                                         "Target Connections", "List of targeted connectors whose origin is this symbol.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// List of sourcing connectors whose destination is this symbol.
        /// </summary>
        public EditableList<VisualConnector> OriginConnections { get; protected set; }
        public static ModelListDefinitor<VisualSymbol, VisualConnector> __OriginConnections =
                   new ModelListDefinitor<VisualSymbol, VisualConnector>("OriginConnections", EEntityMembership.InternalBulk, ins => ins.OriginConnections, (ins, coll) => ins.OriginConnections = coll,
                                                                         "Origin Connections", "List of originating connectors whose destination is this symbol.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Attached visual complements, such as Callouts.
        /// </summary>
        public EditableList<VisualComplement> Complements { get; set; }
        public static ModelListDefinitor<VisualSymbol, VisualComplement> __Complements =
                   new ModelListDefinitor<VisualSymbol, VisualComplement>("Complements", EEntityMembership.InternalBulk, ins => ins.Complements, (ins, coll) => ins.Complements = coll,
                                                                          "Complements", "Attached visual complements, such as Callouts.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Central position where the symbol is displayed around.
        /// </summary>
        public override Point BaseCenter { get { return __BaseCenter.Get(this); } set { __BaseCenter.Set(this, value); } }
        protected Point BaseCenter_ = Display.NULL_POINT;
        public static readonly ModelPropertyDefinitor<VisualSymbol, Point> __BaseCenter =
                   new ModelPropertyDefinitor<VisualSymbol, Point>("BaseCenter", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseCenter_, (ins, val) => ins.BaseCenter_ = val, false, false,
                                                                   "Base Center", "Central position where the symbol is displayed around.");

        /// <summary>
        /// Horizontal left position of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.
        /// </summary>
        [Description("Horizontal left position of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular. " +
                     "For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.")]
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
        /// Vertical top position of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.
        /// </summary>
        [Description("Vertical top position of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular. " +
                     "For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.")]
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
        /// Width of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular.
        /// For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.
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
        public static readonly ModelPropertyDefinitor<VisualSymbol, double> __BaseWidth =
                   new ModelPropertyDefinitor<VisualSymbol, double>("BaseWidth", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseWidth_, (ins, val) => ins.BaseWidth_ = val, false, false,
                                                                    "Base Width", "Width of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular. " +
                                                                                  "For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.");

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
        public static readonly ModelPropertyDefinitor<VisualSymbol, double> __BaseHeight =
                   new ModelPropertyDefinitor<VisualSymbol, double>("BaseHeight", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseHeight_, (ins, val) => ins.BaseHeight_ = val, false, false,
                                                                    "Base Height", "Height of the symbol bounds rectangle area, containing the actual geometry which maybe is not rectangular. " +
                                                                                   "For Concepts, this refers to the body; for Relationships, this refers to the main-symbol.");

        /// <summary>
        /// Height of the symbol plus its details poster, if shown.
        /// </summary>
        [Description("Height of the symbol plus its details poster, if shown.")]
        public double TotalHeight
        {
            get
            {
                return __BaseHeight.Get(this) + (this.AreDetailsShown ? this.DetailsPosterHeight : 0);
            }
        }

        /// <summary>
        /// Gets the symbol's heading rectangle.
        /// </summary>
        [Description("Gets the symbol's heading rectangle.")]
        public override Rect BaseArea { get { return new Rect(this.BaseLeft, this.BaseTop, this.BaseWidth, this.BaseHeight); } }

        /// <summary>
        /// Gets the symbol's detail poster area.
        /// </summary>
        [Description("Gets the symbol's detail poster area.")]
        public Rect DetailsArea { get { return new Rect(this.BaseLeft, this.BaseTop + this.BaseHeight, this.BaseWidth, this.DetailsPosterHeight); } }

        /// <summary>
        /// Area for the content to be shown in the heading of the symbol.
        /// </summary>
        public Rect BaseContentArea { get { return __BaseContentArea.Get(this); } protected set { __BaseContentArea.Set(this, value); } }
        protected Rect BaseContentArea_ = Rect.Empty;
        public static readonly ModelPropertyDefinitor<VisualSymbol, Rect> __BaseContentArea =
                   new ModelPropertyDefinitor<VisualSymbol, Rect>("BaseContentArea", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.BaseContentArea_, (ins, val) => ins.BaseContentArea_ = val, false, false,
                                                                  "Base Content Area", "Area for the content to be shown in the heading of the symbol.");

        /// <summary>
        /// Indicates whether this visual object can be positioned without explicit user interaction.
        /// This is the default behavior for Relationship center-symbols.
        /// </summary>
        public bool IsAutoPositionable { get { return __IsAutoPositionable.Get(this); } internal set { __IsAutoPositionable.Set(this, value); } }
        protected bool IsAutoPositionable_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool> __IsAutoPositionable =
                   new ModelPropertyDefinitor<VisualSymbol, bool>("IsAutoPositionable", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsAutoPositionable_, (ins, val) => ins.IsAutoPositionable_ = val, false, false,
                                                                  "Is Auto-Positionable", "Indicates whether this visual object can be positioned without explicit user interaction.");

        /// <summary>
        /// Indicates whether the details are currently being shown on the view.
        /// </summary>
        public bool AreDetailsShown { get { return __AreDetailsShown.Get(this); } internal set { __AreDetailsShown.Set(this, value); } }
        protected bool AreDetailsShown_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool> __AreDetailsShown =
                   new ModelPropertyDefinitor<VisualSymbol, bool>("AreDetailsShown", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AreDetailsShown_, (ins, val) => ins.AreDetailsShown_ = val, false, false,
                                                                  "Are Details Shown", "Indicates whether the details are currently being shown on the view.");

        /// <summary>
        /// Indicates to show composite-content as (instead of) details.
        /// </summary>
        public bool ShowCompositeContentAsDetails { get { return __ShowCompositeContentAsDetails.Get(this); } internal set { __ShowCompositeContentAsDetails.Set(this, value); } }
        protected bool ShowCompositeContentAsDetails_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool> __ShowCompositeContentAsDetails =
                   new ModelPropertyDefinitor<VisualSymbol, bool>("ShowCompositeContentAsDetails", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowCompositeContentAsDetails_, (ins, val) => ins.ShowCompositeContentAsDetails_ = val, false, false,
                                                                  "Show Composite-Content as Details", "Indicates to show composite-content as (instead of) details.");

        /// <summary>
        /// Current details poster height, even if not shown.
        /// </summary>
        public double DetailsPosterHeight { get { return __DetailsPosterHeight.Get(this); } internal set { __DetailsPosterHeight.Set(this, value); } }
        protected double DetailsPosterHeight_ = INITIAL_DETAILS_POSTER_HEIGHT;
        public static readonly ModelPropertyDefinitor<VisualSymbol, double> __DetailsPosterHeight =
                   new ModelPropertyDefinitor<VisualSymbol, double>("DetailsPosterHeight", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailsPosterHeight_, (ins, val) => ins.DetailsPosterHeight_ = val, false, false,
                                                                    "Details Poster Height", "Current details poster height, even if not shown.");

        /// <summary>
        /// Indidcates that the symbol is horizontally flipped.
        /// </summary>
        public bool? IsHorizontallyFlipped { get { return __IsHorizontallyFlipped.Get(this); } internal set { __IsHorizontallyFlipped.Set(this, value); } }
        protected bool? IsHorizontallyFlippedValue_ = null;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool?> __IsHorizontallyFlipped =
                   new ModelPropertyDefinitor<VisualSymbol, bool?>("IsHorizontallyFlipped", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsHorizontallyFlippedValue_, (ins, val) => ins.IsHorizontallyFlippedValue_ = val, false, false,
                                                                   "Is Horizontally Flipped", "Indidcates that the symbol is horizontally flipped.");
        protected bool IsHorizontallyFlipped_;    // Original serialized value in legacy Composition files

        /// <summary>
        /// Indidcates that the symbol is vertically flipped.
        /// </summary>
        public bool? IsVerticallyFlipped { get { return __IsVerticallyFlipped.Get(this); } internal set { __IsVerticallyFlipped.Set(this, value); } }
        protected bool? IsVerticallyFlippedValue_ = null;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool?> __IsVerticallyFlipped =
                   new ModelPropertyDefinitor<VisualSymbol, bool?>("IsVerticallyFlipped", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsVerticallyFlippedValue_, (ins, val) => ins.IsVerticallyFlippedValue_ = val, false, false,
                                                                   "Is Vertically Flipped", "Indidcates that the symbol is vertically flipped.");
        protected bool IsVerticallyFlipped_;    // Original serialized value in legacy Composition files

        public void Apply_ModelRev10_UpdateSymbolsFlippingProperties()
        {
            this.IsHorizontallyFlippedValue_ = this.IsHorizontallyFlipped_;
            this.IsVerticallyFlippedValue_ = this.IsVerticallyFlipped_;
        }

        /// <summary>
        /// Indidcates that the symbol is tilted.
        /// </summary>
        // NOTE: Based on Nullable<bool> in order to support users already using the related property from the Symbol-Format.
        public bool? IsTilted { get { return __IsTilted.Get(this); } internal set { __IsTilted.Set(this, value); } }
        protected bool? IsTilted_ = null;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool?> __IsTilted =
                   new ModelPropertyDefinitor<VisualSymbol, bool?>("IsTilted", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsTilted_, (ins, val) => ins.IsTilted_ = val, false, false,
                                                                   "Is Tilted", "Indidcates that the symbol is tilted.");

        /// <summary>
        /// Indicates to show the symbol as multiple stacked ones.
        /// </summary>
        // NOTE: Based on Nullable<bool> in order to support users already using the related property from the Symbol-Format.
        public bool? ShowAsMultiple { get { return __ShowAsMultiple.Get(this); } internal set { __ShowAsMultiple.Set(this, value); } }
        protected bool? ShowAsMultiple_ = null;
        public static readonly ModelPropertyDefinitor<VisualSymbol, bool?> __ShowAsMultiple =
                   new ModelPropertyDefinitor<VisualSymbol, bool?>("ShowAsMultiple", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowAsMultiple_, (ins, val) => ins.ShowAsMultiple_ = val, false, false,
                                                                   "Show As Multiple", "Indicates to show the symbol as multiple stacked ones.");

        /// <summary>
        /// Last calculated Labeling area, which may vary depending on calculated label size,
        /// plus possible union with name-as-label when shown instead of a Relationship's hidden central symbol.
        /// </summary>
        public Rect LabelingArea
        {
            get { return this.LabelingArea_; }
            protected set { this.LabelingArea_ = value; }
        }
        [NonSerialized]
        private Rect LabelingArea_ = Rect.Empty;

        /// <summary>
        /// Last calculated Marking area.
        /// </summary>
        public Rect MarkingArea
        {
            get { return this.MarkingArea_;  }
            protected set { this.MarkingArea_ = value;  }
        }
        [NonSerialized]
        private Rect MarkingArea_ = Rect.Empty;

        /// <summary>
        /// Area for the content to be shown in the details poster of the symbol.
        /// </summary>
        public Rect DetailsContentArea { get { return __DetailsContentArea.Get(this); } protected set { __DetailsContentArea.Set(this, value); } }
        protected Rect DetailsContentArea_ = Rect.Empty;
        public static readonly ModelPropertyDefinitor<VisualSymbol, Rect> __DetailsContentArea =
                   new ModelPropertyDefinitor<VisualSymbol, Rect>("DetailsContentArea", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailsContentArea_, (ins, val) => ins.DetailsContentArea_ = val, false, false,
                                                                  "Details Content Area", "Area for the content to be shown in the details poster of the symbol.");

        /// <summary>
        /// Indicates that the symbol is not shown.
        /// </summary>
        public bool IsHidden
        {
            get { return this.IsHidden_;  }
            protected set { this.IsHidden_ = value;  }
        }
        [NonSerialized]
        private bool IsHidden_ = false;

        /// <summary>
        /// Gets the current content area, considering the Heading and Details (if displayed).
        /// </summary>
        [Description("Gets the current content area, considering the Heading and Details (if displayed).")]
        public override Rect TotalArea
        {
            get
            {
                var Result = (this.AreDetailsShown
                              ? new Rect(this.BaseArea.X, this.BaseArea.Y, this.BaseArea.Width, this.TotalHeight)
                              : this.BaseArea);

                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Draws and returns a set of indicator adorners, based on supplied
        /// Indicator Size, Stroke, Pencil and optional Geometry-Creator, for mark the selection of this visual element.
        /// </summary>
        public override List<Tuple<Drawing, bool, EManipulationDirection>> GenerateSelectionIndicators(double IndicatorSize, Brush IndStroke, Pen IndPencil, Func<Rect, Geometry> GeometryCreator = null)
        {
            GeometryCreator = GeometryCreator.NullDefault((rect) => new RectangleGeometry(rect));

            var Result = new List<Tuple<Drawing, bool, EManipulationDirection>>();

            var HasFixedWidth = VisualSymbolFormat.GetHasFixedWidth(this);
            var HasFixedHeight = VisualSymbolFormat.GetHasFixedHeight(this);

            if (!(HasFixedWidth && HasFixedHeight))
            {
                double PosX = 0, PosY = 0;
                Drawing IndHeadingTop = null, IndHeadingBottom = null, IndHeadingLeft = null, IndHeadingRight = null,
                        IndHeadingTopLeft = null, IndHeadingTopRight = null, IndHeadingBottomLeft = null, IndHeadingBottomRight = null;
                Drawing IndDetailsBottom = null, IndDetailsRight = null, IndDetailsLeft = null, IndDetailsBottomLeft = null, IndDetailsBottomRight = null;

                // Centrals...
                if (this.BaseArea.Width >= IndicatorSize * 3.0)
                {
                    if (!HasFixedHeight)
                    {
                        PosX = (this.BaseArea.X + this.BaseArea.Width / 2.0) - IndicatorSize / 2.0;
                        PosY = this.BaseArea.Y - IndicatorSize / 2.0;
                        IndHeadingTop = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                        Result.Add(Tuple.Create(IndHeadingTop, true, EManipulationDirection.Top));

                        PosY = this.BaseArea.Y + this.BaseArea.Height - IndicatorSize / 2.0;
                        IndHeadingBottom = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                        Result.Add(Tuple.Create(IndHeadingBottom, true, EManipulationDirection.Bottom));
                    }

                    PosX = (this.DetailsArea.X + this.DetailsArea.Width / 2.0) - IndicatorSize / 2.0;
                    PosY = this.DetailsArea.Y + this.DetailsArea.Height - IndicatorSize / 2.0;
                    IndDetailsBottom = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                    if (this.AreDetailsShown)
                        Result.Add(Tuple.Create(IndDetailsBottom, false, EManipulationDirection.Bottom));
                }

                if (this.BaseArea.Height >= IndicatorSize * 3.0 && !HasFixedWidth)
                {
                    PosX = this.BaseArea.X - IndicatorSize / 2.0;
                    PosY = (this.BaseArea.Y + this.BaseArea.Height / 2.0) - IndicatorSize / 2.0;
                    IndHeadingLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                    Result.Add(Tuple.Create(IndHeadingLeft, true, EManipulationDirection.Left));

                    PosX = this.BaseArea.X + this.BaseArea.Width - IndicatorSize / 2.0;
                    IndHeadingRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                    Result.Add(Tuple.Create(IndHeadingRight, true, EManipulationDirection.Right));

                    PosX = this.DetailsArea.X - IndicatorSize / 2.0;
                    PosY = (this.DetailsArea.Y + this.DetailsArea.Height / 2.0) - IndicatorSize / 2.0;
                    IndDetailsLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                    if (this.AreDetailsShown)
                        Result.Add(Tuple.Create(IndDetailsLeft, false, EManipulationDirection.Left));

                    PosX = this.DetailsArea.X + this.DetailsArea.Width - IndicatorSize / 2.0;
                    IndDetailsRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                    if (this.AreDetailsShown)
                        Result.Add(Tuple.Create(IndDetailsRight, false, EManipulationDirection.Right));
                }

                // Corners...
                PosX = this.BaseArea.X - IndicatorSize / 2.0;
                PosY = this.BaseArea.Y - IndicatorSize / 2.0;
                IndHeadingTopLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                Result.Add(Tuple.Create(IndHeadingTopLeft, true, EManipulationDirection.TopLeft));

                PosX = this.BaseArea.X + this.BaseArea.Width - IndicatorSize / 2.0;
                IndHeadingTopRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                Result.Add(Tuple.Create(IndHeadingTopRight, true, EManipulationDirection.TopRight));

                PosX = this.BaseArea.X - IndicatorSize / 2.0;
                PosY = this.BaseArea.Y + this.BaseArea.Height - IndicatorSize / 2.0;
                IndHeadingBottomLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                Result.Add(Tuple.Create(IndHeadingBottomLeft, true, EManipulationDirection.BottomLeft));

                PosX = this.BaseArea.X + this.BaseArea.Width - IndicatorSize / 2.0;
                IndHeadingBottomRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                Result.Add(Tuple.Create(IndHeadingBottomRight, true, EManipulationDirection.BottomRight));

                PosX = this.DetailsArea.X - IndicatorSize / 2.0;
                PosY = this.DetailsArea.Y + this.DetailsArea.Height - IndicatorSize / 2.0;
                IndDetailsBottomLeft = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                if (this.AreDetailsShown)
                    Result.Add(Tuple.Create(IndDetailsBottomLeft, false, EManipulationDirection.BottomLeft));

                PosX = this.DetailsArea.X + this.DetailsArea.Width - IndicatorSize / 2.0;
                IndDetailsBottomRight = (new GeometryDrawing(IndStroke, IndPencil, GeometryCreator(new Rect(PosX, PosY, IndicatorSize, IndicatorSize))));
                if (this.AreDetailsShown)
                    Result.Add(Tuple.Create(IndDetailsBottomRight, false, EManipulationDirection.BottomRight));
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualElement> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualSymbol> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualSymbol> __ClassDefinitor = null;

        public new VisualSymbol CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualSymbol)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualSymbol PopulateFrom(VisualSymbol SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return "Symbol of '" + (this.OwnerRepresentation == null ? "" : this.OwnerRepresentation.RepresentedIdea.Name) + "'"; //T ", Id=[" + this.GlobalId.ToString() + "]";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Recreates and returns the Graphic of this visual object descendant and its children.
        /// </summary>
        public override ContainerVisual GenerateGraphic(UIElement PresentationContext, bool ShowManipulationAdorners)
        {
            this.Graphic = this.CreateDraw(PresentationContext, ShowManipulationAdorners).RenderToDrawingVisual();
            return this.Graphic;
        }

        /// <summary>
        /// Creates and returns a new draw implementing this visual shape for an optional presentation context.
        /// </summary>
        public override DrawingGroup CreateDraw(UIElement PresentationContext, bool ShowManipulationAdorners)
        {
            DrawingGroup Result = null;

            var PresentationWidth = this.BaseWidth;
            var PresentationHeight = this.BaseHeight;
            bool ShowAll = true;

            var RelationshipDefinitor = this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor as RelationshipDefinition;
            if (RelationshipDefinitor != null
                && RelationshipDefinitor.IsSimple && RelationshipDefinitor.HideCentralSymbolWhenSimple)
            {
                // NOTE: As an alternative to hiding or reduce-to-almost-nothing, consider to show a little circle.
                PresentationWidth = HIDING_SIZE;
                PresentationHeight = HIDING_SIZE;
                ShowAll = false;

                this.IsHidden = true;
            }
            else
                this.IsHidden = false;

            // Draw the Body symbol and its poster if details are to be shown
            var WorkingLineBrush = VisualSymbolFormat.GetLineBrush(this);
            var WorkingLineThickness = VisualSymbolFormat.GetLineThickness(this);
            var WorkingLineDash = VisualSymbolFormat.GetLineDash(this);

            var SymbolResult = MasterDrawer.CreateDrawingSymbol(this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.RepresentativeShape,
                                                                this.OwnerRepresentation.RepresentedIdea.Pictogram,
                                                                this.OwnerRepresentation.RepresentedIdea.Definitor.Pictogram,
                                                                (this.IsHidden ? null : WorkingLineBrush),
                                                                WorkingLineThickness, WorkingLineDash,
                                                                VisualSymbolFormat.GetLineJoin(this),
                                                                VisualSymbolFormat.GetLineCap(this),
                                                                !this.IsHidden && VisualSymbolFormat.GetUsePictogramAsSymbol(this),
                                                                (this.IsHidden ? null : VisualSymbolFormat.GetMainBackground(this)),
                                                                VisualSymbolFormat.GetDetailsPosterIsHanging(this),
                                                                VisualSymbolFormat.GetOpacity(this),
                                                                this.BaseCenter, PresentationWidth, PresentationHeight,
                                                                (this.AreDetailsShown ? this.DetailsPosterHeight : 0),
                                                                this.OwnerRepresentation.MainSymbol.ShowAsMultiple.NullDefaultTo(VisualSymbolFormat.GetAsMultiple(this)),
                                                                this.OwnerRepresentation.MainSymbol.IsHorizontallyFlipped.NullDefaultTo(VisualSymbolFormat.GetInitiallyFlippedHorizontally(this)),
                                                                this.OwnerRepresentation.MainSymbol.IsVerticallyFlipped.NullDefaultTo(VisualSymbolFormat.GetInitiallyFlippedVertically(this)),
                                                                this.OwnerRepresentation.MainSymbol.IsTilted.NullDefaultTo(VisualSymbolFormat.GetInitiallyTilted(this)));
            Result = SymbolResult.Item1;

            // Adorn as shortcut if necessary
            if (this.OwnerRepresentation.IsShortcut
                && this.OwnerRepresentation.DisplayingView.ShowIndicators)
                using (var Context = Result.Append())
                {
                    var ShortcutImage = Display.GetAppImage("shortcut_lit.png");
                    var Zone = new Rect(this.BaseLeft, this.BaseTop + this.BaseHeight - ShortcutImage.GetHeight(),
                                        ShortcutImage.GetWidth(), ShortcutImage.GetHeight());
                    Context.DrawImage(ShortcutImage, Zone);
                }

            // Determine presentation areas
            this.BaseContentArea = SymbolResult.Item2;

            // Set initial labeling-area, later adjusted when text is finally determined and shown
            this.LabelingArea = new Rect(this.BaseLeft + this.BaseWidth * 0.1,
                                         (this.BaseTop - (LABELING_HEIGHT / 2.0)) - 2,
                                         this.BaseWidth * 0.8, LABELING_HEIGHT);

            this.MaxDefinitionLabelWidth = this.LabelingArea.Width * 0.9;

            this.MarkingArea = new Rect(this.BaseLeft,
                                        this.BaseTop - (MarkerDefinition.StandardMarkerIconSize.Height + 2.0),
                                        this.BaseWidth,
                                        MarkerDefinition.StandardMarkerIconSize.Height);

            this.DetailsContentArea = SymbolResult.Item3;

            // ..........................................
            // Determine brushes for superimposed content
            Brush SuperimposingBackground = VisualSymbolFormat.GetLineBrush(this).GetSolidBrush()
                                                .SubstituteFor(Brushes.Transparent, Brushes.White);
            Brush SuperimposingForeground = VisualSymbolFormat.GetMainBackground(this).GetSolidBrush(false)
                                                .SubstituteFor(Brushes.Transparent, Brushes.White);

            if (SuperimposingBackground.IsLike(SuperimposingForeground))
            {
                SuperimposingBackground = VisualSymbolFormat.GetTextFormat(this, ETextPurpose.Title)
                                                .ForegroundBrush.SubstituteFor(Brushes.Transparent, Brushes.White);
                SuperimposingForeground = VisualSymbolFormat.GetMainBackground(this)
                                                .SubstituteFor(Brushes.Transparent, Brushes.White);
            }

            if (ShowAll)
            {
                // ..........................................
                // Draw Heading Content
                if (this.BaseContentArea.Width >= MIN_SYMBOL_DISPLAY_SIZE.Width
                    && this.BaseContentArea.Height >= MIN_SYMBOL_DISPLAY_SIZE.Height)
                {
                    var WorkingPictogram = this.OwnerRepresentation.RepresentedIdea.Pictogram
                                    .NullDefault(VisualSymbolFormat.GetUseDefinitorPictogramAsNullDefault(this)
                                                 ? this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.Pictogram
                                                 : null);

                    // PENDING: Adjust-to-text (only in symbol creation?)
                    MasterDrawer.GenerateHeadingContent(Result, this.BaseContentArea,
                                                        WorkingPictogram, VisualSymbolFormat.GetUsePictogramAsSymbol(this), VisualSymbolFormat.GetPictogramVisualDisposition(this),
                                                        VisualSymbolFormat.GetTextFormat(this, ETextPurpose.Title), VisualSymbolFormat.GetTextFormat(this, ETextPurpose.Subtitle),
                                                        VisualSymbolFormat.GetSubtitleVisualDisposition(this), VisualSymbolFormat.GetUseNameAsMainTitle(this),
                                                        this.OwnerRepresentation.RepresentedIdea.Name, this.OwnerRepresentation.RepresentedIdea.TechName);
                }

                // ..........................................
                // Draw Details
                this.CurrentDetailZones = null;

                if (this.DetailsContentArea.Width >= MIN_SYMBOL_DISPLAY_SIZE.Width
                    && this.DetailsContentArea.Height >= MIN_SYMBOL_DISPLAY_SIZE.Height)
                {
                    using (var Context = Result.Append())
                        if (this.ShowCompositeContentAsDetails)
                            DrawCompositeContent(Context, this.DetailsContentArea);
                        else
                        {
                            var SeparatorPen = new Pen(WorkingLineBrush, WorkingLineThickness);
                            SeparatorPen.DashStyle = WorkingLineDash;
                            this.CurrentDetailZones = DrawDetails(Context, this.DetailsContentArea,
                                                                  VisualSymbolFormat.GetIncludeDetailsSeparators(this),
                                                                  PresentationWidth, SeparatorPen);
                        }
                }

                // ..........................................
                // Draw Markings
                this.CurrentMarkingZones = null;

                if (this.OwnerRepresentation.DisplayingView.ShowMarkers && this.MarkingArea.Width >= MarkerDefinition.StandardMarkerIconSize.Width
                    && this.MarkingArea.Height >= MarkerDefinition.StandardMarkerIconSize.Height)
                    using (var Context = Result.Append())
                        this.CurrentMarkingZones = this.GenerateMarkings(Context, this.MarkingArea);

                // ..........................................
                // Draw Decorators

                if (this.OwnerRepresentation.DisplayingView.ShowIndicators
                    && this.BaseWidth >= DECORATOR_SIZE * 4.2 && this.BaseHeight >= DECORATOR_SIZE * 1.2)
                {
                    double PosX, PosY;
                    Point PointTopLeft, PointMediumLeft, PointBottomLeft,
                            PointTopRight, PointMediumRight, PointBottomRight;

                    // Top-Left
                    PosX = (this.BaseLeft + (this.BaseWidth / 5.0));
                    PosY = this.BaseTop - DECORATOR_SIZE / 2.0;
                    PointTopLeft = new Point(PosX, PosY);

                    // Medium-Left
                    PosX = this.BaseLeft - DECORATOR_SIZE / 2.0;
                    PosY = (this.BaseTop + this.BaseHeight / 2.0) - DECORATOR_SIZE / 2.0;
                    PointMediumLeft = new Point(PosX, PosY);

                    // Bottom-Left
                    PosX = (this.BaseLeft + (this.BaseWidth / 5.0));
                    PosY = this.BaseTop + this.BaseHeight - DECORATOR_SIZE / 2.0;
                    PointBottomLeft = new Point(PosX, PosY);

                    // Top-Right
                    PosX = (this.BaseLeft + (this.BaseWidth / 5.0) * 4.0) - DECORATOR_SIZE;
                    PosY = this.BaseTop - DECORATOR_SIZE / 2.0;
                    PointTopRight = new Point(PosX, PosY);

                    // Medium-Right
                    PosX = (this.BaseLeft + this.BaseWidth) - DECORATOR_SIZE / 2.0;
                    PosY = (this.BaseTop + this.BaseHeight / 2.0) - DECORATOR_SIZE / 2.0;
                    PointMediumRight = new Point(PosX, PosY);

                    // Bottom-Right
                    PosX = (this.BaseLeft + (this.BaseWidth / 5.0) * 4.0) - DECORATOR_SIZE;
                    PosY = this.BaseTop + this.BaseHeight - DECORATOR_SIZE / 2.0;
                    PointBottomRight = new Point(PosX, PosY);

                    var StateDecorators = new List<Drawing>();
                    GeometryGroup DecGeom = null;

                    if (this.OwnerRepresentation.RepresentedIdea.IsComposite)
                    {
                        DecGeom = Display.GetResource<GeometryGroup>(SymbolDrawer.RES_PFX_DECORATORS + DECO_Composite, true);
                        StateDecorators.Add(SymbolDrawer.CreateDecorator(DecGeom, SuperimposingBackground,
                                                                                  SuperimposingForeground,
                                                                                  SuperimposingForeground,
                                                                                  SuperimposingBackground,
                                                                                  PointBottomLeft, DECORATOR_SIZE, DECORATOR_SIZE));
                    }

                    if (!this.OwnerRepresentation.AreRelatedOriginsShown)
                    {
                        // NOTE: Do not share this geometry because is frozen
                        DecGeom = Display.GetResource<GeometryGroup>(SymbolDrawer.RES_PFX_DECORATORS + DECO_Related, true);
                        StateDecorators.Add(SymbolDrawer.CreateDecorator(DecGeom, SuperimposingBackground,
                                                                         SuperimposingBackground,
                                                                         SuperimposingForeground,
                                                                         SuperimposingBackground,
                                                                         PointMediumLeft, DECORATOR_SIZE, DECORATOR_SIZE, 180.0));
                    }

                    if (!this.OwnerRepresentation.AreRelatedTargetsShown)
                    {
                        DecGeom = Display.GetResource<GeometryGroup>(SymbolDrawer.RES_PFX_DECORATORS + DECO_Related, true);
                        StateDecorators.Add(SymbolDrawer.CreateDecorator(DecGeom, SuperimposingBackground,
                                                                         SuperimposingBackground,
                                                                         SuperimposingForeground,
                                                                         SuperimposingBackground,
                                                                         PointMediumRight, DECORATOR_SIZE, DECORATOR_SIZE));
                    }

                    if (!this.AreDetailsShown && (this.OwnerRepresentation.RepresentedIdea.HasDetailedContent
                                                  || this.OwnerRepresentation.RepresentedIdea.HasLinksToExistentContent))
                    {
                        DecGeom = Display.GetResource<GeometryGroup>(SymbolDrawer.RES_PFX_DECORATORS + DECO_Details, true);
                        StateDecorators.Add(SymbolDrawer.CreateDecorator(DecGeom, SuperimposingBackground,
                                                                         SuperimposingForeground,
                                                                         SuperimposingForeground,
                                                                         SuperimposingBackground,
                                                                         PointBottomRight, DECORATOR_SIZE, DECORATOR_SIZE));
                    }

                    if (StateDecorators.Count > 0)
                        using (var Context = Result.Append())
                            foreach (var DecoDrawing in StateDecorators)
                                Context.DrawDrawing(DecoDrawing);
                }

                // PENDING: Register periferic decorators (such as callouts, notes, etc. Not to be confused with Text decorations)

                // ..........................................
                // Register Selection Indicators for drawing
                if (ShowManipulationAdorners)
                    if (this.OwnerRepresentation.IsSelected)
                    {
                        var SizeFactor = (this.OwnerRepresentation is ConceptVisualRepresentation
                                          || this.GetDisplayingView().SelectedObjects.Contains(this)
                                          ? 1.0 : 0.5);
                        this.OwnerRepresentation.DisplayingView.AttachAdorner(this, GenerateSelectionIndicators(INDICATOR_SIZE * SizeFactor, SelectionIndicatorBackground,
                                                                                                                SelectionIndicatorForeground, SelectionIndicatorGeometryCreator).Select(tup => tup.Item1));
                    }
                    else
                        this.OwnerRepresentation.DisplayingView.DetachAdorner(this);
            }

            // IMPORTANT: The next two "puts" are overriden by simple-and-without-mainsymbol relationships.
            // So, they are drawn again in the VisualConnector rendering method.

            // Draw name over the center, when hiding symbol
            Rect? AreaForName = null;

            if (this.CanShowNameOverCenter)
                AreaForName = PutNameOverCenter(Result);

            // Draw Labeling
            //- if (PresentationWidth > HIDING_SIZE && PresentationHeight > HIDING_SIZE)
            var Offset = (PresentationWidth <= HIDING_SIZE || PresentationHeight <= HIDING_SIZE ? 10 : 0);

            Rect? AreaForDefinition = null;
            if (this.CanShowDefinitionOverTop)
                AreaForDefinition = PutDefinitionOverTop(Result, SuperimposingForeground, SuperimposingBackground,
                                                          ((ShowAll ? 0 : 4) + Offset) - (this.CanShowNameOverCenter ? 11 : 0));

            // Recalculate labeling-area, which later will be used to extend drawing area of selection (yellow) adorner.
            if (AreaForName != null || AreaForDefinition != null)
                this.LabelingArea = (AreaForName != null && AreaForDefinition != null
                                     ? Rect.Union(AreaForName.Value, AreaForDefinition.Value)
                                     : (AreaForName != null ? AreaForName.Value : AreaForDefinition.Value));

            // Vanish if required (e.g. when cutting)
            if (this.OwnerRepresentation.IsVanished)
                Result.Opacity = VisualRepresentation.SELECTION_VANISHING_OPACITY;

            return Result;
        }

        /// <summary>
        /// Gets the maximum width for definition labels.
        /// </summary>
        [NonSerialized]
        public double MaxDefinitionLabelWidth;

        /// <summary>
        /// Indicates whether the Idea Name can be shown on top of this symbol.
        /// </summary>
        public bool CanShowNameOverCenter
        {
            get
            {
                var RelationshipDefinitor = this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor as RelationshipDefinition;
                var Result = (RelationshipDefinitor != null && RelationshipDefinitor.IsSimple
                              && RelationshipDefinitor.HideCentralSymbolWhenSimple
                              && RelationshipDefinitor.ShowNameIfHidingCentralSymbol);
                return Result;
            }
        }

        /// <summary>
        /// Indicates whether the Idea Definition Name can be shown on top of this symbol.
        /// </summary>
        public bool CanShowDefinitionOverTop
        {
            get
            {
                var Result = ((this.IsHidden || (this.LabelingArea.Width >= this.MaxDefinitionLabelWidth && this.LabelingArea.Height >= LABELING_HEIGHT))
                              && (((this.OwnerRepresentation is ConceptVisualRepresentation) && this.OwnerRepresentation.DisplayingView.ShowConceptDefinitionLabels)
                                  || ((this.OwnerRepresentation is RelationshipVisualRepresentation) && this.OwnerRepresentation.DisplayingView.ShowRelationshipDefinitionLabels)));
                return Result;
            }
        }

        /// <summary>
        /// Puts a Definition (name) label on top of this symbol.
        /// Returns used area.
        /// </summary>
        public Rect PutDefinitionOverTop(DrawingGroup Target, Brush SuperimposingForeground, Brush SuperimposingBackground, double VerticalAdjustment = 0)
        {
            var Area = Rect.Empty;

            using (var Context = Target.Append())
            {
                var Format = new TextFormat("Arial", 8.0, SuperimposingForeground);
                var LabelText = Format.GenerateFormattedText(this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.Name,
                                                             this.MaxDefinitionLabelWidth - 6, LABELING_HEIGHT - 3);

                Area = new Rect((this.LabelingArea.X + (this.LabelingArea.Width / 2.0)) - ((LabelText.Width + 6) / 2.0),
                                    this.LabelingArea.Y + VerticalAdjustment, LabelText.Width + 6, this.LabelingArea.Height);
                Context.DrawRoundedRectangle(SuperimposingBackground, null, Area, 3.0, 3.0);

                Context.DrawText(LabelText, new Point((this.LabelingArea.X + (this.LabelingArea.Width / 2.0)) - LabelText.Width / 2.0,
                                                        this.LabelingArea.Y + 2 + VerticalAdjustment));
            }

            return Area;
        }

        /// <summary>
        /// Puts a Name label on top of this symbol.
        /// Returns the used area.
        /// </summary>
        public Rect PutNameOverCenter(DrawingGroup Target)
        {
            var Area = Rect.Empty;

            using (var Context = Target.Append())
            {
                var MaxDecoratorWidth = this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat
                                            .InitialWidth.SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Width);
                var MaxDecoratorHeight = this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat
                                            .InitialHeight.SubstituteFor(0, ProductDirector.DefaultRelationshipCentralSymbolSize.Height);

                var Format = VisualSymbolFormat.GetTextFormat(this, ETextPurpose.Title);
                var Text = Format.GenerateFormattedText(this.OwnerRepresentation.RepresentedIdea.Name,
                                                        MaxDecoratorWidth, MaxDecoratorHeight, TextAlignment.Left, 0, 0, 0, 8.0);

                MaxDecoratorWidth = Text.Width + 6.0;
                MaxDecoratorHeight = Text.Height + 4.0;
                Area = new Rect(this.BaseCenter.X - (MaxDecoratorWidth / 2.0), this.BaseCenter.Y - (MaxDecoratorHeight / 2.0),
                                MaxDecoratorWidth, MaxDecoratorHeight);

                var RoundedRectGeom = new RectangleGeometry(Area);
                RoundedRectGeom.RadiusX = ROUNDEDRECT_RADIUS;
                RoundedRectGeom.RadiusY = ROUNDEDRECT_RADIUS;
                Context.DrawGeometry(VisualSymbolFormat.GetMainBackground(this),
                                     new Pen(VisualSymbolFormat.GetLineBrush(this), 1.0),
                                     RoundedRectGeom);

                Context.DrawText(Text, new Point(Area.X + 3.0, Area.Y + 2.0));
            }

            return Area;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Draws the related Idea Composite-Content on the specified drawing Context and Available Area.
        /// </summary>
        public void DrawCompositeContent(DrawingContext Context, Rect AvailableArea, bool PutBackgroundBrush = true)
        {
            if (this.OwnerRepresentation.RepresentedIdea.CompositeActiveView == null)
                return;

            this.OwnerRepresentation.RepresentedIdea.CompositeActiveView.DrawContent(Context, AvailableArea, !PutBackgroundBrush);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the default text foreground and background brushes for labels.
        /// </summary>
        public Tuple<Brush, Brush> GetDefaultLabelBrushes()
        {
            var Format = VisualSymbolFormat.GetTextFormat(this, ETextPurpose.Title);
            var Result = Tuple.Create(Format.ForegroundBrush, VisualSymbolFormat.GetMainBackground(this));
            return Result;
        }

        /// <summary>
        /// References, per detail designator, the current individual detail manipulation areas (title-designate, title-expand, content-assign and content-edit),
        /// which were created in the last render.
        /// </summary>
        // IMPORTANT: This property is declared just to be available for undo/redo operations, so not serialize it;
        public Dictionary<DetailDesignator, Tuple<Rect, Rect, Rect, Rect>> CurrentDetailZones { get { return __CurrentDetailZones.Get(this); } protected set { __CurrentDetailZones.Set(this, value); } }
        [NonSerialized]
        protected Dictionary<DetailDesignator, Tuple<Rect, Rect, Rect, Rect>> CurrentDetailZones_ = null;
        public static readonly ModelPropertyDefinitor<VisualSymbol, Dictionary<DetailDesignator, Tuple<Rect, Rect, Rect, Rect>>> __CurrentDetailZones =
                   new ModelPropertyDefinitor<VisualSymbol, Dictionary<DetailDesignator, Tuple<Rect, Rect, Rect, Rect>>>("CurrentDetailZones", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CurrentDetailZones_, (ins, val) => ins.CurrentDetailZones_ = val, false, false,
                                                                                                                         "Current Detail Zones", "References, per detail designator, the current individual detail manipulation areas (title-designate, title-expand, content-assign and content-edit), which were created in the last render.");

        /// <summary>
        /// References, per marker assignment, the current individual marking areas (just the icon),
        /// which were created in the last render.
        /// </summary>
        // IMPORTANT: This property is declared just to be available for undo/redo operations, so not serialize it;
        public List<Tuple<MarkerAssignment, Rect>> CurrentMarkingZones { get { return __CurrentMarkingZones.Get(this); } protected set { __CurrentMarkingZones.Set(this, value); } }
        [NonSerialized]
        protected List<Tuple<MarkerAssignment, Rect>> CurrentMarkingZones_ = null;
        public static readonly ModelPropertyDefinitor<VisualSymbol, List<Tuple<MarkerAssignment, Rect>>> __CurrentMarkingZones =
                   new ModelPropertyDefinitor<VisualSymbol, List<Tuple<MarkerAssignment, Rect>>>("CurrentMarkingZones", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CurrentMarkingZones_, (ins, val) => ins.CurrentMarkingZones_ = val, false, false,
                                                                                                 "Current Marking Zones", "References, per marker assignment, the current individual marking areas (just the icon),");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// Draws the related Idea assigned Markings on the specified drawing Context and Available Area.
        /// Returns the the areas of the markers, or null when none was generated.
        public List<Tuple<MarkerAssignment, Rect>> GenerateMarkings(DrawingContext Context, Rect AvailableArea)
        {
            // For Testing: Draw marking-area rectangle
            //var Stroke = Brushes.WhiteSmoke.Clone();
            //Stroke.Opacity = 0.5;
            //Context.DrawDrawing(new GeometryDrawing(Stroke, new Pen(Brushes.LightGray, 0.5), new RectangleGeometry(AvailableArea)));

            // Calculate quantity of showable markers
            int ShowableMarkersQuantity = Math.Min(this.OwnerRepresentation.RepresentedIdea.Markings.Count,
                                                   Convert.ToInt32(Math.Round(AvailableArea.Width / MarkerDefinition.StandardMarkerIconSize.Width)));
            if (ShowableMarkersQuantity < 1)
                return null;

            var Result = new List<Tuple<MarkerAssignment, Rect>>();

            // Draw markings from right to left (thus, delaying touching of some symbol parts such as Actor's head or Folder's label)
            var Position = (AvailableArea.Left + AvailableArea.Width) - (MarkerDefinition.StandardMarkerIconSize.Width * ShowableMarkersQuantity);

            for (int Index = 0; Index < ShowableMarkersQuantity; Index++)
            {
                var Zone = new Rect(Position, AvailableArea.Top, MarkerDefinition.StandardMarkerIconSize.Width, MarkerDefinition.StandardMarkerIconSize.Height);

                if (Index >= ShowableMarkersQuantity - 1 && this.OwnerRepresentation.RepresentedIdea.Markings.Count > ShowableMarkersQuantity)
                    Context.DrawImage(Display.GetAppImage("text_ellipsis.png"), Zone);
                else
                {
                    var Marker = this.OwnerRepresentation.RepresentedIdea.Markings[Index];

                    // If marker has descriptor, then put its Name/Title in an oblique way
                    if (this.OwnerRepresentation.DisplayingView.ShowMarkersTitles && Marker.Descriptor != null)
                    {
                        var Pencil = new Pen(Brushes.LightGray, 1);

                        var Format = new TextFormat("Arial", 9.0, Brushes.Black);
                        var LabelText = Format.GenerateFormattedText(Marker.Descriptor.Name, 100.0, 14.0);

                        var Start = new Point(Zone.X + (Zone.Width / 2.0), Zone.Y + (Zone.Height / 2.0));
                        var TextSize = (LabelText.Width * 0.75) + 2.0;
                        var Label = Display.PathFigureGeometry(Start,
                                                               new Point(Start.X + TextSize + 8.0, Start.Y - (TextSize + 8.0)),
                                                               new Point(Start.X + TextSize, Start.Y - (TextSize + 16)),
                                                               new Point(Start.X + 0, Start.Y - 16));

                        Context.DrawGeometry(Brushes.WhiteSmoke, Pencil, Label);

                        Context.PushTransform(new TranslateTransform(Start.X + 2.0, Start.Y - 16.0));
                        Context.PushTransform(new RotateTransform(-45.0));
                        Context.DrawText(LabelText, new Point(0, -1));
                        Context.Pop();
                        Context.Pop();
                    }

                    // Draw the appropriate icon
                    var Icon = (Marker.Descriptor == null || Marker.Descriptor.Pictogram == null ? Marker.Definitor.Pictogram : Marker.Descriptor.Pictogram);
                    Context.DrawImage(Icon, Zone);

                    Result.Add(Tuple.Create(Marker, Zone));
                }

                Position += MarkerDefinition.StandardMarkerIconSize.Width;
            }

            return Result;
        }

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
        /// Returns the symbols grouped by this symbol either by attached Group Regions/Lines or by the symbol itself.
        /// Note: Objects reported can appear repeated, so apply Distinct before use.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<VisualSymbol> GetGroupedSymbols()
        {
            var Result = Enumerable.Empty<VisualSymbol>();
            var DispView = this.GetDisplayingView();

            if (this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.CanGroupIntersectingObjects)
                Result = Result.Concat(DispView.GetVisualObjectsInside(this.TotalArea, true, false).CastAs<VisualSymbol,VisualObject>());

            foreach (var Grouper in this.GroupingComplements)
            {
                var FullInside = Grouper.IsComplementGroupRegion &&
                                    !this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.CanGroupIntersectingObjects;
                Result = Result.Concat(DispView.GetVisualObjectsInside(Grouper.TotalArea, true, FullInside).CastAs<VisualSymbol, VisualObject>());
            }

            Result = Result.Except(this.IntoEnumerable());

            return Result;
        }

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

            if (this.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.CanGroupIntersectingObjects)
                Result = Result.Concat(this.GetDisplayingView().GetVisualObjectsInside(this.TotalArea, true, false)
                                        .Select(obj => Tuple.Create(obj,false)));

            foreach(var Grouper in this.GroupingComplements)
                Result = Result.Concat(Grouper.GetMovableMembers(IncludeRelatedOrigins, IncludeRelatedTargets, IsForVisualization));

            // IMPORTANT: *NOTICE* that this algorithm MOVES the entire super/sub-trees (either of Origins or Targets) associated to a SYMBOL WITH COLLAPSED ORIGIN OR TARGETS,
            //            therefore the directly related symbols are not visible.
            //            Remember that parts of these super/sub-trees can still be visible when part of an opposite hierarchy from other symbol,
            //            hence from that side, they can appear as moving along with the moved TEMPORARY (VISUALLY) UNCONNECTED symbol.
            // IMPORTANT: Do not apply .Distinct() to give the caller a chance to detect duplicates
            Result = Result.Concat(GetOriginSymbolsHierarchy(vsym => (IncludeRelatedOrigins && vsym.IsRelatedVisible)
                                                                      || (!IsForVisualization && !IncludeRelatedOrigins && !vsym.IsRelatedVisible))
                                                                      .Select(obj => Tuple.Create((VisualObject)obj, false)))
                           .Concat(GetTargetSymbolsHierarchy(vsym => (IncludeRelatedTargets && vsym.IsRelatedVisible)
                                                                      || (!IsForVisualization && !IncludeRelatedTargets && !vsym.IsRelatedVisible))
                                                                      .Select(obj => Tuple.Create((VisualObject)obj, false)));

            return Result;
        }

        /// <summary>
        /// Moves the bse figure to the specified coordinates.
        /// </summary>
        public override void MoveTo(double PosX, double PosY, bool LockNewPosition = false, bool IsResizing = false)
        {
            this.MoveTo(PosX, PosY, LockNewPosition, null, null, null, IsResizing);
        }

        /// <summary>
        /// Moves the symbol to the specified central coordinates and acknowledges the indication of lock new position, original symbol moved
        /// and ongoing moving representations. Plus, rearranges any connected relationships.
        /// </summary>
        public void MoveTo(double PosX, double PosY, bool LockNewPosition,
                           VisualObject OriginalObjectMoved = null, IEnumerable<VisualSymbol> OngoingMovingSymbols = null,
                           IList<VisualObject> RegionContainedObjects = null, bool IsResizing = false)
        {
            double DeltaX = PosX - this.BaseCenter.X;
            double DeltaY = PosY - this.BaseCenter.Y;

            this.BaseCenter = new Point(PosX, PosY);

            // This makes the symbol not automatically-adjustable after the lock indication.
            var OwnerRelationshipVisRep = this.OwnerRepresentation as RelationshipVisualRepresentation;

            if (LockNewPosition
                && !(OwnerRelationshipVisRep != null && (OwnerRelationshipVisRep.RepresentedRelationship.RelationshipDefinitor.Value.HideCentralSymbolWhenSimple
                                                         && OwnerRelationshipVisRep.RepresentedRelationship.RelationshipDefinitor.Value.IsSimple)))
                this.IsAutoPositionable = false;

            this.RenderElement();

            /* Alternative, but must change many other things
            var VisPrim = this.OwnerRepresentation.DisplayingView.Presenter.GetVisualContainerOf(this);
            VisPrim.Offset = new Vector(DeltaX, DeltaY); */

            /*T InfiniteLoopCounter++;
            if (InfiniteLoopCounter > 20)
                Console.Write(""); */

            this.UpdateDependents(DeltaX, DeltaY, OriginalObjectMoved, OngoingMovingSymbols, RegionContainedObjects, IsResizing);
        }
        //T private static int InfiniteLoopCounter=0;

        /// <summary>
        /// Resizes the symbol to the specified dimensions.
        /// Plus, rearranges any connected visual elements.
        /// Returns indication of valid resizing respect the minimum allowed.
        /// </summary>
        public override bool ResizeTo(double Width, double Height)
        {
            if (Width < ApplicationProduct.ProductDirector.DefaultMinBaseFigureSize.Width ||
                Height < ApplicationProduct.ProductDirector.DefaultMinBaseFigureSize.Height)
                return false;

            // IMPORTANT: Do not miss RenderElement here, because it's required to redraw and/or clear selection indicators.

            var HasFixedWidth = VisualSymbolFormat.GetHasFixedWidth(this);
            var HasFixedHeight = VisualSymbolFormat.GetHasFixedHeight(this);

            double DeltaWidth = (HasFixedWidth ? 0.0 : Width - this.BaseWidth);
            double DeltaHeight = (HasFixedHeight ? 0.0 : Height - this.BaseHeight);

            this.BaseWidth = Width;
            this.BaseHeight = Height;

            this.RenderElement();

            ResizingArrange(DeltaWidth, DeltaHeight);

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies the neccesary proportional offsets, in reaction to a Resize operation and recreates the related connectors and complements.
        /// </summary>
        public void ResizingArrange(double DeltaWidth, double DeltaHeight, bool IsForDetailsPoster = false)
        {
            if (DeltaWidth != 0 || DeltaHeight != 0)
            {
                double PosX, PosY, OffsetX, OffsetY;
                double OriginalWidth = this.BaseWidth - DeltaWidth;
                double OriginalHeight = this.TotalHeight - DeltaHeight;

                //T Console.WriteLine("==> Resizing Arrange... CurrentLeft={0}, CurrentTop={1}", this.SymbolLeft, this.SymbolTop);
                //T Console.WriteLine("OriginalWidth={0}, OriginalHeight={1}", OriginalWidth, OriginalHeight);
                //T Console.WriteLine("Current-Width={0}, Current-Height={1}", this.SymbolWidth, this.SymbolHeight);

                foreach (var Connector in this.TargetConnections)
                {
                    if (!(IsForDetailsPoster && this.BaseArea.Contains(Connector.OriginPosition)))
                    {
                        OffsetX = Connector.OriginPosition.X - this.BaseLeft;
                        PosX = this.BaseLeft + OffsetX * this.BaseWidth / OriginalWidth;
                        //T Console.WriteLine("Connector.OriginPosition.X={0}, OffsetX={1}, NewPosX={2}", Connector.OriginPosition.X, OffsetX, PosX);

                        OffsetY = Connector.OriginPosition.Y - this.BaseTop;
                        PosY = this.BaseTop + OffsetY * this.TotalHeight / OriginalHeight;
                        //T Console.WriteLine("Connector.OriginPosition.Y={0}, OffsetY={1}, NewPosY={2}", Connector.OriginPosition.Y, OffsetY, PosY);

                        var CalculatedPosition = new Point(PosX.EnforceRange(this.BaseLeft, (this.BaseLeft + this.BaseWidth)),
                                                           PosY.EnforceRange(this.BaseTop, (this.BaseTop + this.TotalHeight)));

                        if (!this.OwnerRepresentation.DisplayingView.Presenter.ContainsObjectWithPoint(this.Graphic, CalculatedPosition))
                            CalculatedPosition = this.BaseCenter.FindBoundary(CalculatedPosition, this.OwnerRepresentation.DisplayingView.Presenter,
                                                                              this.Graphic, true);

                        Connector.OriginPosition = CalculatedPosition;
                    }

                    Connector.RenderElement();  // Render always, despite not being updated
                }

                foreach (var Connector in this.OriginConnections)
                {
                    if (!(IsForDetailsPoster && this.BaseArea.Contains(Connector.TargetPosition)))
                    {
                        OffsetX = Connector.TargetPosition.X - this.BaseLeft;
                        PosX = this.BaseLeft + OffsetX * this.BaseWidth / OriginalWidth;
                        //T Console.WriteLine("Connector.OriginPosition.X={0}, OffsetX={1}, NewPosX={2}", Connector.OriginPosition.X, OffsetX, PosX);

                        OffsetY = Connector.TargetPosition.Y - this.BaseTop;
                        PosY = this.BaseTop + OffsetY * this.TotalHeight / OriginalHeight;
                        //T Console.WriteLine("Connector.OriginPosition.Y={0}, OffsetY={1}, NewPosY={2}", Connector.OriginPosition.Y, OffsetY, PosY);

                        var CalculatedPosition = new Point(PosX.EnforceRange(this.BaseLeft, (this.BaseLeft + this.BaseWidth)),
                                                           PosY.EnforceRange(this.BaseTop, (this.BaseTop + this.TotalHeight)));

                        if (!this.OwnerRepresentation.DisplayingView.Presenter.ContainsObjectWithPoint(this.Graphic, CalculatedPosition))
                            CalculatedPosition = this.BaseCenter.FindBoundary(CalculatedPosition, this.OwnerRepresentation.DisplayingView.Presenter,
                                                                              this.Graphic, true);

                        Connector.TargetPosition = CalculatedPosition;
                    }

                    Connector.RenderElement();  // Render always, despite not being updated
                }
            }

            this.UpdateDependents(0, 0, null, null, null, true);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Recreates the associated Connectors, Complements and grouped (by Regions) objects, applying the neccesary linear offsets, in reaction to a Move operation.
        /// </summary>
        internal void UpdateDependents(double MoveDeltaX = 0, double MoveDeltaY = 0, VisualObject ParallelMoveObject = null,
                                       IEnumerable<VisualSymbol> OngoingMovingSymbols = null, IList<VisualObject> RegionContainedObjects = null, bool IsResizing = false)
        {
            if (OngoingMovingSymbols == null)
                OngoingMovingSymbols = Enumerable.Empty<VisualSymbol>();

            var AutoReferences = new RelationshipVisualRepresentation[0];

            var ParallelMoveSymbolRelRep = (ParallelMoveObject != null && ParallelMoveObject is VisualSymbol
                                            && ((VisualSymbol)ParallelMoveObject).OwnerRepresentation is RelationshipVisualRepresentation
                                            ? (RelationshipVisualRepresentation)((VisualSymbol)ParallelMoveObject).OwnerRepresentation
                                            : null);

            var CanDoParallelMove = (ParallelMoveObject != null
                                     && (ParallelMoveSymbolRelRep == null || ParallelMoveSymbolRelRep.MainSymbol != ParallelMoveObject)
                                     && (MoveDeltaX != 0 || MoveDeltaY != 0));

            if (CanDoParallelMove)
                AutoReferences = this.TargetConnections.Where(conn => conn.OwnerRelationshipRepresentation.RepresentedRelationship.IsAutoReference)
                                        .Select(conn => conn.OwnerRelationshipRepresentation)
                                            .Concat(this.OriginConnections.Where(targconn => targconn.OwnerRelationshipRepresentation.RepresentedRelationship.IsAutoReference)
                                                        .Select(conn => conn.OwnerRelationshipRepresentation))
                                        .Distinct().ToArray();

            if (CanDoParallelMove && AutoReferences.Length > 0)
                foreach (var AutoRef in AutoReferences)
                    AutoRef.DoParallelMove(this, MoveDeltaX, MoveDeltaY, RegionContainedObjects);

            var ConnectorsToRender = new List<VisualConnector>();

            // For when connection is from/to exact locations (no symbol centers)...
            // IMPORTANT: This must be before calls to UpdateConnectorMiddleSymbolOfSimpleRel because updating of precise Origin and Target Positions.
            if (MoveDeltaX != 0 || MoveDeltaY != 0)
            {
                foreach (var Connector in this.TargetConnections)
                {
                    Connector.OriginPosition = new Point(Connector.OriginPosition.X + MoveDeltaX, Connector.OriginPosition.Y + MoveDeltaY);
                    ConnectorsToRender.AddNew(Connector);
                    //- this.UpdateConnectorVisualization(Connector, true);
                }

                foreach (var Connector in this.OriginConnections)
                {
                    Connector.TargetPosition = new Point(Connector.TargetPosition.X + MoveDeltaX, Connector.TargetPosition.Y + MoveDeltaY);
                    ConnectorsToRender.AddNew(Connector);
                    //- this.UpdateConnectorVisualization(Connector, false);
                }
            }

            foreach (var Connector in this.TargetConnections.Where(conn => !conn.OwnerRelationshipRepresentation.IsIn(AutoReferences)))
                if (this.UpdateConnectorMiddleSymbolOfSimpleRel(Connector, true, OngoingMovingSymbols))
                    ConnectorsToRender.AddNew(Connector);

            foreach (var Connector in this.OriginConnections.Where(conn => !conn.OwnerRelationshipRepresentation.IsIn(AutoReferences)))
                if (this.UpdateConnectorMiddleSymbolOfSimpleRel(Connector, false, OngoingMovingSymbols))
                    ConnectorsToRender.AddNew(Connector);

            if (this.Complements != null)
                foreach (var Complement in this.Complements.Where(comp => !comp.CanGroup))    // Exclude regions and rods (they are moved apart)
                    Complement.UpdatePositionRelativeToTarget(MoveDeltaX, MoveDeltaY, true, IsResizing);

            foreach (var Connector in ConnectorsToRender)
                Connector.RenderElement();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the specified visual connector's middle symbol of simple relationship, which is tied to this symbol.
        /// Plus, indications of origination (else targeting) and ongoing moving symbols are passed.
        /// Returns indication of required rendering.
        /// </summary>
        internal bool UpdateConnectorMiddleSymbolOfSimpleRel(VisualConnector Connector, bool IsOriginated, IEnumerable<VisualSymbol> OngoingMovingSymbols)
        {
            // Determine whether automatic repositioning aplies
            var IntermediateSymbol = (IsOriginated ? Connector.TargetSymbol : Connector.OriginSymbol);

            var DoAutomaticReposition = (Connector.IntermediatePosition == Display.NULL_POINT
                                         && IntermediateSymbol.IsAutoPositionable
                                         && (IntermediateSymbol.OwnerRepresentation is RelationshipVisualRepresentation)
                                         && (IntermediateSymbol != this)
                                         && (IntermediateSymbol.TargetConnections.Count == 1
                                             && IntermediateSymbol.OriginConnections.Count == 1)
                                         && !Connector.OwnerRelationshipRepresentation.RepresentedRelationship.IsAutoReference
                                         && !IntermediateSymbol.IsIn(OngoingMovingSymbols));

            if (DoAutomaticReposition)
            {
                var TargetingConnector = IntermediateSymbol.TargetConnections.First();
                var OriginatingConnector = IntermediateSymbol.OriginConnections.First();
                var LocalPosition = (IsOriginated ? Connector.OriginPosition : Connector.TargetPosition);
                Point OppositePosition;
                VisualSymbol OppositeSymbol = null;

                if (TargetingConnector == Connector)
                {
                    OppositePosition = OriginatingConnector.OriginPosition;
                    OppositeSymbol = OriginatingConnector.OriginSymbol;
                }
                else
                {
                    OppositePosition = TargetingConnector.TargetPosition;
                    OppositeSymbol = TargetingConnector.TargetSymbol;
                }

                var LocalEdgePoint = LocalPosition.DetermineNearestIntersectingPoint(OppositePosition, this.OwnerRepresentation.DisplayingView.Presenter,
                                                                                     this.Graphic, this.OwnerRepresentation.DisplayingView.VisualHitTestFilter);

                var OppositeEdgePoint = OppositePosition.DetermineNearestIntersectingPoint(LocalPosition, this.OwnerRepresentation.DisplayingView.Presenter,
                                                                                           OppositeSymbol.Graphic, this.OwnerRepresentation.DisplayingView.VisualHitTestFilter);

                // PENDING: Avoid to move inside a connected symbol
                IntermediateSymbol.MoveTo((LocalEdgePoint.X + OppositeEdgePoint.X) / 2.0,
                                          (LocalEdgePoint.Y + OppositeEdgePoint.Y) / 2.0);

                return true;
            }

            return false;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supertree of originated visual Symbols.
        /// Optionally a filtering condition can be specified.
        /// </summary>
        public IEnumerable<VisualSymbol> GetOriginSymbolsHierarchy(Func<VisualSymbol, bool> Filter = null)
        {
            var Result = new List<VisualSymbol>();

            foreach (var Origin in this.OriginConnections.Select(conn => conn.OriginSymbol))
                Origin.TravelOriginSymbolsHierarchy(Result, Filter);

            return Result;
        }

        private void TravelOriginSymbolsHierarchy(IList<VisualSymbol> Trace, Func<VisualSymbol, bool> Filter = null)
        {
            if (Trace.Contains(this) || (Filter != null && !Filter(this)))
                return;

            Trace.Add(this);

            foreach (var Origin in this.OriginConnections.Select(conn => conn.OriginSymbol))
                Origin.TravelOriginSymbolsHierarchy(Trace);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the subtree of targeted visual Symbols.
        /// Optionally a filtering condition can be specified.
        /// </summary>
        public IEnumerable<VisualSymbol> GetTargetSymbolsHierarchy(Func<VisualSymbol,bool> Filter = null)
        {
            var Result = new List<VisualSymbol>();

            foreach (var Target in this.TargetConnections.Select(conn => conn.TargetSymbol))
                Target.TravelTargetSymbolsHierarchy(Result, Filter);

            return Result;
        }

        private void TravelTargetSymbolsHierarchy(IList<VisualSymbol> Trace, Func<VisualSymbol, bool> Filter = null)
        {
            if (Trace.Contains(this) || (Filter != null && !Filter(this)))
                return;

            Trace.Add(this);

            foreach (var Target in this.TargetConnections.Select(conn => conn.TargetSymbol))
                Target.TravelTargetSymbolsHierarchy(Trace, Filter);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the attached complements
        /// </summary>
        public IEnumerable<VisualComplement> AttachedComplements
        {
            get
            {
                if (this.Complements == null)
                    return Enumerable.Empty<VisualComplement>();

                return this.Complements;
            }
        }

        /// <summary>
        /// Adds the supplied Complement to this symbol.
        /// </summary>
        public void AddComplement(VisualComplement Complement)
        {
            if (this.Complements == null)
                this.Complements = new EditableList<VisualComplement>(__Complements.TechName, this);

            this.Complements.Add(Complement);
        }

        /// <summary>
        /// Removes the specified Complement from this symbol.
        /// </summary>
        public void RemoveComplement(VisualComplement Complement)
        {
            if (this.Complements == null)
                return;

            this.Complements.Remove(Complement);
        }

        /// <summary>
        /// If assigned, gets the grouping Region/Line Visual Complements.
        /// </summary>
        public IEnumerable<VisualComplement> GroupingComplements
        {
            get
            {
                if (this.Complements == null)
                    return Enumerable.Empty<VisualComplement>();

                var Result = this.Complements.Where(vcomp => vcomp.CanGroup);
                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}