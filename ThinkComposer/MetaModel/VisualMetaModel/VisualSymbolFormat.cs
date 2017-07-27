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
// File   : VisualSymbolFormat.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.VisualSymbolFormat (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.31 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Specifies format for a visual symbol, considering shape, size and front board parts.
    /// </summary>
    [Serializable]
    public class VisualSymbolFormat : VisualElementFormat, IModelEntity, IModelClass<VisualSymbolFormat>
    {
        /// <summary>
        /// Collection of text formats to be used when none is explicitly assigned.
        /// </summary>
        public static readonly Dictionary<ETextPurpose, TextFormat> DefaultTextFormats;

        public static Brush DefaultDetailHeadingForeground = Brushes.LightGray;
        public static Brush DefaultDetailHeadingBackground = Brushes.DimGray;

        public static Brush DefaultDetailCaptionForeground = Brushes.LightGray;      // Used to draw grid lines (so make it soft)
        public static Brush DefaultDetailCaptionBackground = Brushes.Gainsboro;

        public static Brush DefaultDetailContentForeground = Brushes.LightGray;     // Used to draw grid lines (so make it soft)
        public static Brush DefaultDetailContentBackground = Brushes.WhiteSmoke;

        public const double SYMBOL_MIN_INI_SIZE = 2.0;

        public const double SYMBOL_STD_INI_WIDTH = 120;
        public const double SYMBOL_STD_INI_HEIGHT = 30;

        public const double SYMBOL_MAX_INI_WIDTH = 500.0;
        public const double SYMBOL_MAX_INI_HEIGHT = 100.0;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualSymbolFormat()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualSymbolFormat>("VisualSymbolFormat", VisualElementFormat.__ClassDefinitor, "Visual Symbol Format",
                                                                           "Specifies format for a visual symbol, considering shape, size and front board parts.");

            /* POSTPONED... __ClassDefinitor.DeclareProperty(__MantainAspectRatio);
            __ClassDefinitor.DeclareProperty(__CanResizeHorizontally);
            __ClassDefinitor.DeclareProperty(__CanResizeVertically);
            __ClassDefinitor.DeclareProperty(__IsAutoadjustable); */
            __ClassDefinitor.DeclareProperty(__ShowGlobalDetailsFirst);
            //? __ClassDefinitor.DeclareProperty(__SupressDetailsPoster);
            __ClassDefinitor.DeclareProperty(__SubtitleVisualDisposition);
            __ClassDefinitor.DeclareProperty(__UseNameAsMainTitle);
            __ClassDefinitor.DeclareProperty(__InPlaceEditingIsMultiline);
            __ClassDefinitor.DeclareProperty(__PictogramVisualDisposition);
            // POSTPONED: __ClassDefinitor.DeclareProperty(__PictogramStretch);
            __ClassDefinitor.DeclareProperty(__UseDefinitorPictogramAsNullDefault);
            __ClassDefinitor.DeclareProperty(__UsePictogramAsSymbol);
            __ClassDefinitor.DeclareProperty(__DetailsPosterIsHanging);
            __ClassDefinitor.DeclareProperty(__IncludeDetailsSeparators);
            __ClassDefinitor.DeclareProperty(__DetailHeadingForeground);
            __ClassDefinitor.DeclareProperty(__DetailHeadingBackground);
            __ClassDefinitor.DeclareProperty(__DetailCaptionForeground);
            __ClassDefinitor.DeclareProperty(__DetailCaptionBackground);
            __ClassDefinitor.DeclareProperty(__DetailContentForeground);
            __ClassDefinitor.DeclareProperty(__DetailContentBackground);
            __ClassDefinitor.DeclareProperty(__InitialWidth);
            __ClassDefinitor.DeclareProperty(__InitialHeight);
            __ClassDefinitor.DeclareProperty(__HasFixedWidth);
            __ClassDefinitor.DeclareProperty(__HasFixedHeight);
            __ClassDefinitor.DeclareProperty(__InitiallyFlippedHorizontally);
            __ClassDefinitor.DeclareProperty(__InitiallyFlippedVertically);
            __ClassDefinitor.DeclareProperty(__InitiallyTilted);
            __ClassDefinitor.DeclareProperty(__AsMultiple);
            __ClassDefinitor.DeclareProperty(__RegionBackground);
            __ClassDefinitor.DeclareProperty(__RegionForeground);
            __ClassDefinitor.DeclareProperty(__RegionDash);
            __ClassDefinitor.DeclareProperty(__RegionThickness);
            __ClassDefinitor.DeclareProperty(__InitialGroupRegionPlacementHorizontal);

            __InitialWidth.RangeMin = SYMBOL_MIN_INI_SIZE;
            __InitialWidth.RangeMax = SYMBOL_MAX_INI_WIDTH;
            __InitialWidth.RangeIntervals = General.CreateArray<double>(2, 4, 5, 8, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100,
                                                                        110, 120, 130, 140, 150, 160, 170, 180, 190, 200,
                                                                        210, 220, 230, 240, 250, 260, 270, 280, 290, 300,
                                                                        310, 320, 330, 340, 350, 360, 370, 380, 390, 400,
                                                                        410, 420, 430, 440, 450, 460, 470, 480, 490, 500);
            // __InitialWidth.RangeStep = 5.0; // (SYMBOL_MIN_INI_SIZE / 2.0);

            __InitialHeight.RangeMin = SYMBOL_MIN_INI_SIZE;
            __InitialHeight.RangeMax = SYMBOL_MAX_INI_HEIGHT;
            __InitialHeight.RangeIntervals = General.CreateArray<double>(2, 4, 5, 8, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100);
            __InitialHeight.RangeStep = 5.0; // (SYMBOL_MIN_INI_SIZE / 2.0);

            __InitialGroupRegionPlacementHorizontal.ItemsSourceGetter = ((ctx) => VisualSymbolFormat.AvailableRegionBorderPlacements);
            __InitialGroupRegionPlacementHorizontal.ItemsSourceSelectedValuePath = "Item1";
            __InitialGroupRegionPlacementHorizontal.ItemsSourceDisplayMemberPath = "Item2";
            __InitialGroupRegionPlacementHorizontal.BindingValueConverter =
                new GenericConverter<EPlacementOnBorderHorizontal, Tuple<EPlacementOnBorderHorizontal, string, string>>
                    (placm => VisualSymbolFormat.AvailableRegionBorderPlacements.First(pl => pl.Item1 == placm),
                     pltup => pltup.Item1);

            __RegionDash.ItemsSourceGetter = VisualElementFormat.__LineDash.ItemsSourceGetter;
            __RegionDash.ItemsSourceSelectedValuePath = VisualElementFormat.__LineDash.ItemsSourceSelectedValuePath;
            __RegionDash.ItemsSourceDisplayMemberPath = VisualElementFormat.__LineDash.ItemsSourceDisplayMemberPath;
            __RegionDash.BindingValueConverter = VisualElementFormat.__LineDash.BindingValueConverter;

            __RegionThickness.ItemsSourceGetter = VisualElementFormat.__LineThickness.ItemsSourceGetter;
            __RegionThickness.ItemsSourceSelectedValuePath = VisualElementFormat.__LineThickness.ItemsSourceSelectedValuePath;
            __RegionThickness.ItemsSourceDisplayMemberPath = VisualElementFormat.__LineThickness.ItemsSourceDisplayMemberPath;
            __RegionThickness.BindingValueConverter = VisualElementFormat.__LineThickness.BindingValueConverter;

            __ClassDefinitor.DeclareCollection(__TextFormats);

            DefaultTextFormats = new Dictionary<ETextPurpose,TextFormat>
                { { ETextPurpose.Title, new TextFormat("Arial", 12.0, Brushes.Black, true, false, true, TextAlignment.Center) },
                  { ETextPurpose.Subtitle, new TextFormat("Arial", 10.0, Brushes.Black, true, true, false, TextAlignment.Center) },
                  { ETextPurpose.Paragraph, new TextFormat("Arial", 9.0, Brushes.Black, false, false, false, TextAlignment.Left) },
                  { ETextPurpose.Extra, new TextFormat("Arial", 8.0, Brushes.Black, false, false, false, TextAlignment.Center) },
                  { ETextPurpose.DetailHeading, new TextFormat("Arial", 10.0, Brushes.White, true, false, false, TextAlignment.Left) },
                  { ETextPurpose.DetailCaption, new TextFormat("Arial", 9.0, Brushes.Black, false, false, false, TextAlignment.Center) },
                  { ETextPurpose.DetailContent, new TextFormat("Arial", 8.0, Brushes.Black, false, false, false, TextAlignment.Left) } };

            var Placements = Enum.GetValues(typeof(EPlacementOnBorderHorizontal)).Cast<EPlacementOnBorderHorizontal>();
            foreach (var Placement in Placements)
                AvailableRegionBorderPlacements.Add(Tuple.Create(Placement, Placement.GetFieldName(), (string)null));
        }

        public static readonly List<Tuple<EPlacementOnBorderHorizontal, string, string>> AvailableRegionBorderPlacements =
                            new List<Tuple<EPlacementOnBorderHorizontal,string,string>>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualSymbolFormat(Brush BackgroundBrush = null, Brush LineBrush = null, double LineThickness = DEFAULT_LINE_THICKNESS, double Opacity = 1.0,
                                  DashStyle LineDash = null, PenLineCap LineCap = PenLineCap.Round, PenLineJoin LineJoin = PenLineJoin.Round)
            : base(BackgroundBrush, LineBrush, LineThickness, Opacity, LineDash, LineCap, LineJoin)
        {
            this.InitializeTextFormats();

            // Predefined format values...
            this.TextFormats[ETextPurpose.Title] = new TextFormat("Arial", 12, Brushes.Black, true, false, false, TextAlignment.Center);
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        public VisualSymbolFormat(VisualSymbolFormat Original)
            : this()
        {
            this.PopulateFrom(Original, null, ECloneOperationScope.Deep);
        }

        /// <summary>
        /// Parameterless constructor for the descendats copy constructors.
        /// </summary>
        protected VisualSymbolFormat()
        {
            this.InitializeTextFormats();
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized]
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (this.RegionDash_ == null)
                this.RegionDash_ = StoreBox.Store<DashStyle>(null);
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initialization of text formats.
        /// </summary>
        protected void InitializeTextFormats()
        {
            this.TextFormats = new EditableDictionary<ETextPurpose, TextFormat>(__TextFormats.TechName, this);
            // At least the title text format must be present.
            this.TextFormats[ETextPurpose.Title] = new TextFormat();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /* POSTPONED...
        /// <summary>
        /// Indicates whether to mantain the aspect-ratio at resizing.
        /// </summary>
        public bool MantainAspectRatio { get { return __MantainAspectRatio.Get(this); } set { __MantainAspectRatio.Set(this, value); } }
        protected bool MantainAspectRatio_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __MantainAspectRatio =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("MantainAspectRatio", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MantainAspectRatio_, (ins, val) => ins.MantainAspectRatio_ = val, false, true, false,
                                                                        "Mantain Aspect-Ratio", "Indicates whether to mantain the aspect-ratio at resizing.");

        /// <summary>
        /// Indicates if the symbol should automatically adjust its size to properly surround its content.
        /// </summary>
        public bool IsAutoadjustable { get { return __IsAutoadjustable.Get(this); } set { __IsAutoadjustable.Set(this, value); } }
        protected bool IsAutoadjustable_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __IsAutoadjustable =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("IsAutoadjustable", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsAutoadjustable_, (ins, val) => ins.IsAutoadjustable_ = val, false, true, false,
                                                                        "Is Autoadjustable", "Indicates if the symbol should automatically adjust its size to properly surround its content.");
        */

        /// <summary>
        /// Indicates whether to show first the global/shared details, else the local/exclusive details are shown first.
        /// </summary>
        public bool ShowGlobalDetailsFirst { get { return __ShowGlobalDetailsFirst.Get(this); } set { __ShowGlobalDetailsFirst.Set(this, value); } }
        protected bool ShowGlobalDetailsFirst_ = true;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __ShowGlobalDetailsFirst =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("ShowGlobalDetailsFirst", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ShowGlobalDetailsFirst_, (ins, val) => ins.ShowGlobalDetailsFirst_ = val, true, false,
                                                                        "Show Global Details First", "Indicates whether to show first the global/shared details, else the local/exclusive details are shown first.");
        public static bool GetShowGlobalDetailsFirst(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __ShowGlobalDetailsFirst.TechName); }
        public static void SetShowGlobalDetailsFirst(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __ShowGlobalDetailsFirst.TechName, Value); }

        /*? /// <summary>
        /// Indicates that the symbol will never show a details poster underneath.
        /// </summary>
        public bool SupressDetailsPoster { get { return __SupressDetailsPoster.Get(this); } set { __SupressDetailsPoster.Set(this, value); } }
        protected bool SupressDetailsPoster_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __SupressDetailsPoster =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("SupressDetailsPoster", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.SupressDetailsPoster_, (ins, val) => ins.SupressDetailsPoster_ = val, true, false,
                                                                        "Supress Details Poster", "Indicates that the symbol will never show a details poster underneath."); */

        /// <summary>
        /// Indicates the Vertical positioning of the Subtitle (which can be the Name or Tech-Name) respect the Title.
        /// </summary>
        public EVisualDispositionMonodimensional SubtitleVisualDisposition { get { return __SubtitleVisualDisposition.Get(this); } set { __SubtitleVisualDisposition.Set(this, value); } }
        protected EVisualDispositionMonodimensional SubtitleVisualDisposition_ = EVisualDispositionMonodimensional.Hidden;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, EVisualDispositionMonodimensional> __SubtitleVisualDisposition =
                   new ModelPropertyDefinitor<VisualSymbolFormat, EVisualDispositionMonodimensional>("SubtitleVisualDisposition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.SubtitleVisualDisposition_, (ins, val) => ins.SubtitleVisualDisposition_ = val, true, false,
                                                                                                     "Subtitle Visual Disposition", "Indicates the Vertical positioning of the Subtitle (which can be the Name or Tech-Name) respect the Title.");
        public static EVisualDispositionMonodimensional GetSubtitleVisualDisposition(VisualSymbol Instance)
        { return (EVisualDispositionMonodimensional)GetValue(Instance, __SubtitleVisualDisposition.TechName); }
        public static void SetSubtitleVisualDisposition(VisualSymbol Instance, EVisualDispositionMonodimensional Value)
        { SetValue(Instance, __SubtitleVisualDisposition.TechName, Value); }

        /// <summary>
        /// Indicates whether to use the Name as main title hence the Tech-Name as subtitle, else the Tech-Name as main title and the Name as subtitle.
        /// </summary>
        public bool UseNameAsMainTitle { get { return __UseNameAsMainTitle.Get(this); } set { __UseNameAsMainTitle.Set(this, value); } }
        protected bool UseNameAsMainTitle_ = true;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __UseNameAsMainTitle =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("UseNameAsMainTitle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.UseNameAsMainTitle_, (ins, val) => ins.UseNameAsMainTitle_ = val, true, false,
                                                                        "Use Name as main Title", "Indicates whether to use the Name as main title hence the Tech-Name as subtitle, else the Tech-Name as main title and the Name as subtitle.");
        public static bool GetUseNameAsMainTitle(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __UseNameAsMainTitle.TechName); }
        public static void SetUseNameAsMainTitle(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __UseNameAsMainTitle.TechName, Value); }

        /// <summary>
        /// Indicates whether the in-place editing of the symbol main title is multiline, so it accepts carriage returns inside.
        /// </summary>
        public bool InPlaceEditingIsMultiline { get { return __InPlaceEditingIsMultiline.Get(this); } set { __InPlaceEditingIsMultiline.Set(this, value); } }
        protected bool InPlaceEditingIsMultiline_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __InPlaceEditingIsMultiline =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("InPlaceEditingIsMultiline", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InPlaceEditingIsMultiline_, (ins, val) => ins.InPlaceEditingIsMultiline_ = val, true, false,
                                                                        "In-Place Editing is Multiline", "Indicates whether the in-place editing of the symbol main title is multiline, so it accepts [Enter] inside.");
        public static bool GetInPlaceEditingIsMultiline(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __InPlaceEditingIsMultiline.TechName); }
        /*- public static void SetInPlaceEditingIsMultiline(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __InPlaceEditingIsMultiline.TechName, Value); } */

        /// <summary>
        /// Indicates the Horizontal or Vertical positioning of the Pictogram respect the Title/Subtitle in the visual symbol.
        /// </summary>
        public EVisualDispositionBidimensional PictogramVisualDisposition { get { return __PictogramVisualDisposition.Get(this); } set { __PictogramVisualDisposition.Set(this, value); } }
        protected EVisualDispositionBidimensional PictogramVisualDisposition_ = EVisualDispositionBidimensional.Right;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, EVisualDispositionBidimensional> __PictogramVisualDisposition =
                   new ModelPropertyDefinitor<VisualSymbolFormat, EVisualDispositionBidimensional>("PictogramVisualDisposition", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PictogramVisualDisposition_, (ins, val) => ins.PictogramVisualDisposition_ = val, true, false,
                                                                                                   "Pictogram Visual Disposition", "Indicates the Horizontal or Vertical positioning of the Pictogram respect the Title/Subtitle in the visual symbol.");
        public static EVisualDispositionBidimensional GetPictogramVisualDisposition(VisualSymbol Instance)
        { return (EVisualDispositionBidimensional)GetValue(Instance, __PictogramVisualDisposition.TechName); }
        public static void SetPictogramVisualDisposition(VisualSymbol Instance, EVisualDispositionBidimensional Value)
        { SetValue(Instance, __PictogramVisualDisposition.TechName, Value); }

        /* POSTOPONED:
        /// <summary>
        /// Indicates the way the Pictogram is shown in the available space.
        /// </summary>
        public Stretch PictogramStretch { get { return __PictogramStretch.Get(this); } set { __PictogramStretch.Set(this, value); } }
        protected Stretch PictogramStretch_ = Stretch.Uniform;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Stretch> __PictogramStretch =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Stretch>("PictogramStretch", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PictogramStretch_, (ins, val) => ins.PictogramStretch_ = val, true, true, false,
                                                                           "Pictogram Stretch", "Indicates the way the Pictogram is shown in the available space."); */

        /// <summary>
        /// Indicates to use the Definitor Pictogram when that of the Idea is empty.
        /// </summary>
        public bool UseDefinitorPictogramAsNullDefault { get { return __UseDefinitorPictogramAsNullDefault.Get(this); } set { __UseDefinitorPictogramAsNullDefault.Set(this, value); } }
        protected bool UseDefinitorPictogramAsNullDefault_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __UseDefinitorPictogramAsNullDefault =
                           new ModelPropertyDefinitor<VisualSymbolFormat, bool>("UseDefinitorPictogramAsNullDefault", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.UseDefinitorPictogramAsNullDefault_, (ins, val) => ins.UseDefinitorPictogramAsNullDefault_ = val, true, false,
                                                                                "Use Definitor Pictogram as Empty-Default", "Indicates to use the Pictogram of the Definition when that of the Idea is empty.");
        public static bool GetUseDefinitorPictogramAsNullDefault(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __UseDefinitorPictogramAsNullDefault.TechName); }
        public static void SetUseDefinitorPictogramAsNullDefault(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __UseDefinitorPictogramAsNullDefault.TechName, Value); }

        /// <summary>
        /// Show the Pictogram instead of the Symbol.
        /// </summary>
        public bool UsePictogramAsSymbol { get { return __UsePictogramAsSymbol.Get(this); } set { __UsePictogramAsSymbol.Set(this, value); } }
        protected bool UsePictogramAsSymbol_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __UsePictogramAsSymbol =
                           new ModelPropertyDefinitor<VisualSymbolFormat, bool>("UsePictogramAsSymbol", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.UsePictogramAsSymbol_, (ins, val) => ins.UsePictogramAsSymbol_ = val, true, false,
                                                                                "Use Pictogram as Symbol", "Show the Pictogram instead of the Symbol.");
        public static bool GetUsePictogramAsSymbol(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __UsePictogramAsSymbol.TechName); }
        public static void SetUsePictogramAsSymbol(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __UsePictogramAsSymbol.TechName, Value); }

        /* PENDING...
        /// <summary>
        /// Defines format styles for each Front Board part.
        /// </summary>
        public MultiDict<EVerticalPlacement, EHorizontalPlacement, VisualElementFormat> HeadingBoardSetting;
         */

        /// <summary>
        /// Indicates whether the Details Poster appears separated and hanging from the head with a triangular hook, else appears joined.
        /// </summary>
        public bool DetailsPosterIsHanging { get { return __DetailsPosterIsHanging.Get(this); } set { __DetailsPosterIsHanging.Set(this, value); } }
        protected bool DetailsPosterIsHanging_ = true;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __DetailsPosterIsHanging =
                           new ModelPropertyDefinitor<VisualSymbolFormat, bool>("DetailsPosterIsHanging", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailsPosterIsHanging_, (ins, val) => ins.DetailsPosterIsHanging_ = val, true, false,
                                                                                "Details Poster Is Hanging", "Indicates whether the Details Poster appears separated and hanging from the head with a triangular hook, else appears joined.");
        public static bool GetDetailsPosterIsHanging(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __DetailsPosterIsHanging.TechName); }
        public static void SetDetailsPosterIsHanging(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __DetailsPosterIsHanging.TechName, Value); }

        /// <summary>
        /// Indicates whether to insert a separator line between shown Details.
        /// </summary>
        public bool IncludeDetailsSeparators { get { return __IncludeDetailsSeparators.Get(this); } set { __IncludeDetailsSeparators.Set(this, value); } }
        protected bool IncludeDetailsSeparators_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __IncludeDetailsSeparators =
                           new ModelPropertyDefinitor<VisualSymbolFormat, bool>("IncludeDetailsSeparators", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IncludeDetailsSeparators_, (ins, val) => ins.IncludeDetailsSeparators_ = val, true, false,
                                                                                "Separate with Lines", "Indicates whether to insert a separator line between shown Details.");
        public static bool GetIncludeDetailsSeparators(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __IncludeDetailsSeparators.TechName); }
        public static void SetIncludeDetailsSeparators(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __IncludeDetailsSeparators.TechName, Value); }

        /// <summary>
        /// Brush for the detail heading foreground.
        /// </summary>
        public Brush DetailHeadingForeground { get { return __DetailHeadingForeground.Get(this); } set { __DetailHeadingForeground.Set(this, value); } }
        protected StoreBox<Brush> DetailHeadingForeground_ = DefaultDetailHeadingForeground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __DetailHeadingForeground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("DetailHeadingForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailHeadingForeground_, (ins, stb) => ins.DetailHeadingForeground_ = stb, false, false,
                                                                         "Detail Heading Foreground", "Brush for the detail heading foreground.");
        public static Brush GetDetailHeadingForeground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __DetailHeadingForeground.TechName); }
        public static void SetDetailHeadingForeground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __DetailHeadingForeground.TechName, Value); }

        /// <summary>
        /// Brush for the detail heading background.
        /// </summary>
        public Brush DetailHeadingBackground { get { return __DetailHeadingBackground.Get(this); } set { __DetailHeadingBackground.Set(this, value); } }
        protected StoreBox<Brush> DetailHeadingBackground_ = DefaultDetailHeadingBackground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __DetailHeadingBackground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("DetailHeadingBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailHeadingBackground_, (ins, stb) => ins.DetailHeadingBackground_ = stb, false, false,
                                                                         "Detail Heading Background", "Brush for the detail heading background.");
        public static Brush GetDetailHeadingBackground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __DetailHeadingBackground.TechName); }
        public static void SetDetailHeadingBackground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __DetailHeadingBackground.TechName, Value); }

        /// <summary>
        /// Brush for the detail caption foreground.
        /// </summary>
        public Brush DetailCaptionForeground { get { return __DetailCaptionForeground.Get(this); } set { __DetailCaptionForeground.Set(this, value); } }
        protected StoreBox<Brush> DetailCaptionForeground_ = DefaultDetailCaptionForeground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __DetailCaptionForeground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("DetailCaptionForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailCaptionForeground_, (ins, stb) => ins.DetailCaptionForeground_ = stb, false, false,
                                                                         "Detail Caption Foreground", "Brush for the detail caption foreground.");
        public static Brush GetDetailCaptionForeground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __DetailCaptionForeground.TechName); }
        public static void SetDetailCaptionForeground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __DetailCaptionForeground.TechName, Value); }

        /// <summary>
        /// Brush for the detail caption background.
        /// </summary>
        public Brush DetailCaptionBackground { get { return __DetailCaptionBackground.Get(this); } set { __DetailCaptionBackground.Set(this, value); } }
        protected StoreBox<Brush> DetailCaptionBackground_ = DefaultDetailCaptionBackground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __DetailCaptionBackground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("DetailCaptionBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailCaptionBackground_, (ins, stb) => ins.DetailCaptionBackground_ = stb, false, false,
                                                                         "Detail Caption Background", "Brush for the detail caption background.");
        public static Brush GetDetailCaptionBackground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __DetailCaptionBackground.TechName); }
        public static void SetDetailCaptionBackground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __DetailCaptionBackground.TechName, Value); }

        /// <summary>
        /// Brush for the detail content foreground.
        /// </summary>
        public Brush DetailContentForeground { get { return __DetailContentForeground.Get(this); } set { __DetailContentForeground.Set(this, value); } }
        protected StoreBox<Brush> DetailContentForeground_ = DefaultDetailContentForeground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __DetailContentForeground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("DetailContentForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailContentForeground_, (ins, stb) => ins.DetailContentForeground_ = stb, false, false,
                                                                         "Detail Content Foreground", "Brush for the detail content foreground.");
        public static Brush GetDetailContentForeground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __DetailContentForeground.TechName); }
        public static void SetDetailContentForeground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __DetailContentForeground.TechName, Value); }

        /// <summary>
        /// Brush for the detail content background.
        /// </summary>
        public Brush DetailContentBackground { get { return __DetailContentBackground.Get(this); } set { __DetailContentBackground.Set(this, value); } }
        protected StoreBox<Brush> DetailContentBackground_ = DefaultDetailContentBackground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __DetailContentBackground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("DetailContentBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DetailContentBackground_, (ins, stb) => ins.DetailContentBackground_ = stb, false, false,
                                                                         "Detail Content Background", "Brush for the detail content background.");
        public static Brush GetDetailContentBackground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __DetailContentBackground.TechName); }
        public static void SetDetailContentBackground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __DetailContentBackground.TechName, Value); }

        /// <summary>
        /// Formats for texts, classified by its purpose.
        /// </summary>
        protected EditableDictionary<ETextPurpose, TextFormat> TextFormats { get; set; }
        public static ModelDictionaryDefinitor<VisualSymbolFormat, ETextPurpose, TextFormat> __TextFormats =
                   new ModelDictionaryDefinitor<VisualSymbolFormat, ETextPurpose, TextFormat>("TextFormats", EEntityMembership.InternalCoreExclusive, ins => ins.TextFormats, (ins, coll) => ins.TextFormats = coll,
                                                                                              "Text Formats", "Formats for texts, classified by its purpose.");

        /// <summary>
        /// Initial width of the symbols.
        /// </summary>
        public double InitialWidth { get { return __InitialWidth.Get(this); } set { __InitialWidth.Set(this, value); } }
        protected double InitialWidth_ = SYMBOL_STD_INI_WIDTH;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, double> __InitialWidth =
                   new ModelPropertyDefinitor<VisualSymbolFormat, double>("InitialWidth", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitialWidth_, (ins, val) => ins.InitialWidth_ = (val == 0 ? 0 : val.EnforceRange(SYMBOL_MIN_INI_SIZE, SYMBOL_MAX_INI_WIDTH)), true, false,
                                                                          "Initial Width", "Initial width of the symbol.");
        /*- public static double GetInitialWidth(VisualSymbol Instance)
        { return (double)GetValue(Instance, __InitialWidth.TechName); }
        public static void SetInitialWidth(VisualSymbol Instance, double Value)
        { SetValue(Instance, __InitialWidth.TechName, Value); } */

        /// <summary>
        /// Initial height of the symbols.
        /// </summary>
        public double InitialHeight { get { return __InitialHeight.Get(this); } set { __InitialHeight.Set(this, value); } }
        protected double InitialHeight_ = SYMBOL_STD_INI_HEIGHT;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, double> __InitialHeight =
                   new ModelPropertyDefinitor<VisualSymbolFormat, double>("InitialHeight", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitialHeight_, (ins, val) => ins.InitialHeight_ = (val == 0 ? 0 : val.EnforceRange(SYMBOL_MIN_INI_SIZE, SYMBOL_MAX_INI_HEIGHT)), true, false,
                                                                          "Initial Height", "Initial height of the symbol.");
        /*- public static double GetInitialHeight(VisualSymbol Instance)
        { return (double)GetValue(Instance, __InitialHeight.TechName); }
        public static void SetInitialHeight(VisualSymbol Instance, double Value)
        { SetValue(Instance, __InitialHeight.TechName, Value); } */

        /// <summary>
        /// Indicates that the symbol has a fixed (initial) width.
        /// </summary>
        public bool HasFixedWidth { get { return __HasFixedWidth.Get(this); } set { __HasFixedWidth.Set(this, value); } }
        protected bool HasFixedWidth_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __HasFixedWidth =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("HasFixedWidth", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HasFixedWidth_, (ins, val) => ins.HasFixedWidth_ = val, true, false,
                                                                        "Has Fixed Width", "Indicates that the symbol has a fixed (initial) width that cannot be resized.");
        public static bool GetHasFixedWidth(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __HasFixedWidth.TechName); }
        public static void SetHasFixedWidth(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __HasFixedWidth.TechName, Value); }

        /// <summary>
        /// Indicates that the symbol has a fixed (initial) height.
        /// </summary>
        public bool HasFixedHeight { get { return __HasFixedHeight.Get(this); } set { __HasFixedHeight.Set(this, value); } }
        protected bool HasFixedHeight_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __HasFixedHeight =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("HasFixedHeight", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HasFixedHeight_, (ins, val) => ins.HasFixedHeight_ = val, true, false,
                                                                        "Has Fixed Height", "Indicates that the symbol has a fixed (initial) height that cannot be resized.");
        public static bool GetHasFixedHeight(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __HasFixedHeight.TechName); }
        public static void SetHasFixedHeight(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __HasFixedHeight.TechName, Value); }

        /// <summary>
        /// Indicates that the symbol is created flipped on its horizontal axis.
        /// </summary>
        public bool InitiallyFlippedHorizontally { get { return __InitiallyFlippedHorizontally.Get(this); } set { __InitiallyFlippedHorizontally.Set(this, value); } }
        protected bool InitiallyFlippedHorizontally_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __InitiallyFlippedHorizontally =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("InitiallyFlippedHorizontally", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitiallyFlippedHorizontally_, (ins, val) => ins.InitiallyFlippedHorizontally_ = val, true, false,
                                                                        "Flipped Horizontally", "Indicates that the symbol is shown flipped on its horizontal axis.");
        public static bool GetInitiallyFlippedHorizontally(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __InitiallyFlippedHorizontally.TechName); }
        public static void SetInitiallyFlippedHorizontally(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __InitiallyFlippedHorizontally.TechName, Value); }
        
        /// <summary>
        /// Indicates that the symbol is created flipped on its vertical axis.
        /// </summary>
        public bool InitiallyFlippedVertically { get { return __InitiallyFlippedVertically.Get(this); } set { __InitiallyFlippedVertically.Set(this, value); } }
        protected bool InitiallyFlippedVertically_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __InitiallyFlippedVertically =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("InitiallyFlippedVertically", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitiallyFlippedVertically_, (ins, val) => ins.InitiallyFlippedVertically_ = val, true, false,
                                                                        "Flipped Vertically", "Indicates that the symbol is shown flipped on its vertical axis.");
        public static bool GetInitiallyFlippedVertically(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __InitiallyFlippedVertically.TechName); }
        public static void SetInitiallyFlippedVertically(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __InitiallyFlippedVertically.TechName, Value); }

        /// <summary>
        /// Indicates that the symbol is created tilted 90° clockwise (flip it to change orientation).
        /// </summary>
        public bool InitiallyTilted { get { return __InitiallyTilted.Get(this); } set { __InitiallyTilted.Set(this, value); } }
        protected bool InitiallyTilted_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __InitiallyTilted =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("InitiallyTilted", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitiallyTilted_, (ins, val) => ins.InitiallyTilted_ = val, true, false,
                                                                        "Tilted", "Indicates that the symbol is shown tilted 90° clockwise (flip it to change orientation).");
        public static bool GetInitiallyTilted(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __InitiallyTilted.TechName); }
        public static void SetInitiallyTilted(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __InitiallyTilted.TechName, Value); }

        /// <summary>
        /// Indicates that the symbol initially will be shown as multiple ones stacked.
        /// </summary>
        public bool AsMultiple { get { return __AsMultiple.Get(this); } set { __AsMultiple.Set(this, value); } }
        protected bool AsMultiple_ = false;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, bool> __AsMultiple =
                   new ModelPropertyDefinitor<VisualSymbolFormat, bool>("AsMultiple", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.AsMultiple_, (ins, val) => ins.AsMultiple_ = val, true, false,
                                                                        "As Multiple", "Indicates that the symbol will be shown as multiple ones stacked.");
        public static bool GetAsMultiple(VisualSymbol Instance)
        { return (bool)GetValue(Instance, __AsMultiple.TechName); }
        public static void SetAsMultiple(VisualSymbol Instance, bool Value)
        { SetValue(Instance, __AsMultiple.TechName, Value); }

        /// <summary>
        /// Brush for a Group Region Complement fill background.
        /// </summary>
        public Brush RegionBackground { get { return __RegionBackground.Get(this); } set { __RegionBackground.Set(this, value); } }
        protected StoreBox<Brush> RegionBackground_ = new StoreBox<Brush>();    // Starts wil NULL, meaning use the brush of the targeted symbol.
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __RegionBackground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("RegionBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RegionBackground_, (ins, stb) => ins.RegionBackground_ = stb, false, false,
                                                                         "Region/Line Background", "Brush for a Group Region Complement fill background.");
        public static Brush GetRegionBackground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __RegionBackground.TechName); }
        /*- public static void SetRegionBackground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __RegionBackground.TechName, Value); } */

        /// <summary>
        /// Brush for a Group Region Complement line foreground.
        /// </summary>
        public Brush RegionForeground { get { return __RegionForeground.Get(this); } set { __RegionForeground.Set(this, value); } }
        protected StoreBox<Brush> RegionForeground_ = new StoreBox<Brush>();    // Starts wil NULL, meaning use the brush of the targeted symbol.
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, Brush> __RegionForeground =
                   new ModelPropertyDefinitor<VisualSymbolFormat, Brush>("RegionForeground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RegionForeground_, (ins, stb) => ins.RegionForeground_ = stb, false, false,
                                                                         "Region/Line Foreground", "Brush for a Group Region/Line Complement line foreground.");
        public static Brush GetRegionForeground(VisualSymbol Instance)
        { return (Brush)GetValue(Instance, __RegionForeground.TechName); }
        /*- public static void SetRegionForeground(VisualSymbol Instance, Brush Value)
        { SetValue(Instance, __RegionForeground.TechName, Value); } */

        /// <summary>
        /// Dash style of the Group Region/Lines.
        /// </summary>
        public DashStyle RegionDash { get { return __RegionDash.Get(this); } set { __RegionDash.Set(this, value); } }
        protected StoreBox<DashStyle> RegionDash_ = StoreBox.Store<DashStyle>(null);   // to be initially replaced by the complement-target value
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, DashStyle> __RegionDash =
                   new ModelPropertyDefinitor<VisualSymbolFormat, DashStyle>("RegionDash", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RegionDash_, (ins, stb) => ins.RegionDash_ = stb, false, false,
                                                                             "Region/Line Dash", "Dash style of the Group Region/Line.");
        public static DashStyle GetRegionDash(VisualSymbol Instance)
        { return (DashStyle)GetValue(Instance, __RegionDash.TechName); }

        /// <summary>
        /// Thickness of the Group Region/Line.
        /// </summary>
        public double RegionThickness { get { return __RegionThickness.Get(this); } set { __RegionThickness.Set(this, value); } }
        protected double RegionThickness_ = 0;  // to be initially replaced by the complement-target value
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, double> __RegionThickness =
                   new ModelPropertyDefinitor<VisualSymbolFormat, double>("RegionThickness", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RegionThickness_, (ins, val) => ins.RegionThickness_ = val, false, false,
                                                                          "Region/Line Thickness", "Thickness of the Group Region/Line.");
        public static double GetRegionThickness(VisualSymbol Instance)
        { return (double)GetValue(Instance, __RegionThickness.TechName); }

        /// <summary>
        /// Initial placement for the top-border of the Group Region, respect its owning Symbol.
        /// </summary>
        public EPlacementOnBorderHorizontal InitialGroupRegionPlacementHorizontal { get { return __InitialGroupRegionPlacementHorizontal.Get(this); } set { __InitialGroupRegionPlacementHorizontal.Set(this, value); } }
        protected EPlacementOnBorderHorizontal InitialGroupRegionPlacementHorizontal_ = EPlacementOnBorderHorizontal.CenterMiddle;
        public static readonly ModelPropertyDefinitor<VisualSymbolFormat, EPlacementOnBorderHorizontal> __InitialGroupRegionPlacementHorizontal =
                   new ModelPropertyDefinitor<VisualSymbolFormat, EPlacementOnBorderHorizontal>("InitialGroupRegionPlacementHorizontal", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitialGroupRegionPlacementHorizontal_, (ins, val) => ins.InitialGroupRegionPlacementHorizontal_ = val, true, false,
                                                                                                "Top-Border Region Place", "Initial placement for the top-border of the Group Region, respect its owning Symbol.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the text format related to the specified purpose.
        /// Automatically returns a default format if none was explicitly set.
        /// </summary>
        public TextFormat GetTextFormat(ETextPurpose Purpose)
        {
            if (this.TextFormats.ContainsKey(Purpose))
            {
                //T Console.WriteLine("Getting TxtFmt from Local-coll[{0}] of inst[{1}].", this.TextFormats.GetHashCode(), this.GetHashCode());
                return this.TextFormats[Purpose];
            }

            if (DefaultTextFormats.ContainsKey(Purpose))
            {
                //T Console.WriteLine("Getting TxtFmt from Global-coll[{0}] of inst[{1}].", this.TextFormats.GetHashCode(), this.GetHashCode());
                return DefaultTextFormats[Purpose];
            }

            //T Console.WriteLine("Getting First TxtFmt from Global-coll[{0}] of inst[{1}].", this.TextFormats.GetHashCode(), this.GetHashCode());
            return DefaultTextFormats[ETextPurpose.DetailCaption];
        }
        public static TextFormat GetTextFormat(VisualSymbol Instance, ETextPurpose Purpose)
        { return (TextFormat)GetValue(Instance, TEXTFORMAT_PREFIX + Enum.GetName(typeof(ETextPurpose), Purpose)); }

        /// <summary>
        /// Sets the text format of the specified purpose to the supplied format.
        /// </summary>
        public void SetTextFormat(ETextPurpose Purpose, TextFormat Format)
        {
            this.TextFormats[Purpose] = Format;
            this.NotifyPropertyChange(__TextFormats.TechName);
        }
        public static void SetTextFormat(VisualSymbol Instance, ETextPurpose Purpose, TextFormat Value)
        { SetValue(Instance, TEXTFORMAT_PREFIX + Enum.GetName(typeof(ETextPurpose), Purpose), Value); }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualSymbolFormat> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualSymbolFormat> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualSymbolFormat> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualSymbolFormat CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualSymbolFormat)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualSymbolFormat PopulateFrom(VisualSymbolFormat SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}