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
// File   : TextFormat.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.TextFormat (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.24 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Specifies format to be applied on text.
    /// </summary>
    [Serializable]
    public class TextFormat : IModelEntity, IModelClass<TextFormat>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static TextFormat()
        {
            __ClassDefinitor = new ModelClassDefinitor<TextFormat>("TextFormat", null, "Text Format",
                                                                   "Specifies format to be applied on text.");
            __ClassDefinitor.DeclareProperty(__Alignment);
            __ClassDefinitor.DeclareProperty(__ForegroundBrush);
            __ClassDefinitor.DeclareProperty(__IsBold);
            __ClassDefinitor.DeclareProperty(__IsItalic);
            __ClassDefinitor.DeclareProperty(__IsUnderline);
            __ClassDefinitor.DeclareProperty(__IsStrikethrough);
            __ClassDefinitor.DeclareProperty(__FontFamilyName);
            __ClassDefinitor.DeclareProperty(__FontSize);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public TextFormat(string FontFamilyName = "Arial", double FontSize = 12.0, Brush ForegroundBrush = null,
                          bool IsBold = false, bool IsItalic = false, bool IsUnderline = false,
                          TextAlignment Alignment = TextAlignment.Left,
                          bool IsStrikethrough = false)
            : this()
        {
            this.FontFamilyName = FontFamilyName;
            this.FontSize = FontSize;
            this.ForegroundBrush = ForegroundBrush ?? Brushes.Black;
            this.IsBold = IsBold;
            this.IsItalic = IsItalic;
            this.IsUnderline = IsUnderline;
            this.Alignment = Alignment;
            this.IsStrikethrough = IsStrikethrough;
        }

        /// <summary>
        /// Empty constructor for the descendats copy constructors and initialization of text formats.
        /// </summary>
        protected TextFormat()
        {
            // Initialize cached objects.
            this.Initialize();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Copy Constructor.
        /// </summary>
        public TextFormat(TextFormat Original)
             : base()
        {
            this.PopulateFrom(Original, null, ECloneOperationScope.Deep);
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized]
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            this.RecreateCurrentDecorations();
            this.RecreateCurrentTypeface();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this text-format properties makes this equivalent with the specified Other format.
        /// </summary>
        public bool IsEquivalentTo(TextFormat Other)
        {
            var Result = (this.Alignment == Other.Alignment
                          && this.ForegroundBrush.IsLike(Other.ForegroundBrush, true)
                          && this.IsBold == Other.IsBold
                          && this.IsItalic == Other.IsItalic
                          && this.IsUnderline == Other.IsUnderline
                          && this.IsStrikethrough == Other.IsStrikethrough
                          && this.FontFamilyName == Other.FontFamilyName
                          && this.FontSize == Other.FontSize);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Alignment of the text (left, right, center or justify).
        /// </summary>
        public TextAlignment Alignment { get { return __Alignment.Get(this); } set { __Alignment.Set(this, value); } }
        protected TextAlignment Alignment_ = TextAlignment.Center;
        public static readonly ModelPropertyDefinitor<TextFormat, TextAlignment> __Alignment =
                   new ModelPropertyDefinitor<TextFormat, TextAlignment>("Alignment", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Alignment_, (ins, val) => ins.Alignment_ = val, false, false,
                                                                                                                 "Alignment", "Alignment of the text (left, right, center or justify).");

        /// <summary>
        /// Brush for the text.
        /// </summary>
        public Brush ForegroundBrush
        {
            get { return __ForegroundBrush.Get(this); }
            set
            {
                //T Console.WriteLine("Setting ForegroundBrush of Format=[{0}] with Color=[{1}]", this.GetHashCode(), (value is SolidColorBrush ? ((SolidColorBrush)value).Color : Colors.Transparent));
                __ForegroundBrush.Set(this, value);
            }
        }
        protected StoreBox<Brush> ForegroundBrush_ = Brushes.Black.Store<Brush>();
        public static readonly ModelPropertyDefinitor<TextFormat, Brush> __ForegroundBrush =
                   new ModelPropertyDefinitor<TextFormat, Brush>("ForegroundBrush", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ForegroundBrush_, (ins, stb) => ins.ForegroundBrush_ = stb, false, false,
                                                                                  "Foreground Brush", "Brush for the text.");

        /// <summary>
        /// Indicates whether the text is hardened or normal.
        /// </summary>
        public bool IsBold
        {
            get
            {
                return __IsBold.Get(this);
            }
            set
            {
                if (__IsBold.Set(this, value))
                    RecreateCurrentTypeface();
            }
        }
        protected bool IsBold_ = false;
        public static readonly ModelPropertyDefinitor<TextFormat, bool> __IsBold =
                   new ModelPropertyDefinitor<TextFormat, bool>("IsBold", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsBold_, (ins, val) => ins.IsBold_ = val, false, false,
                                                                                                                 "Is Bold", "Indicates whether the text is hardened or normal.");

        /// <summary>
        /// Indicates whether the text is obliqued or normal.
        /// </summary>
        public bool IsItalic
        {
            get
            {
                return __IsItalic.Get(this);
            }
            set
            {
                if (__IsItalic.Set(this, value))
                    RecreateCurrentTypeface();
            }
        }
        protected bool IsItalic_ = false;
        public static readonly ModelPropertyDefinitor<TextFormat, bool> __IsItalic =
                   new ModelPropertyDefinitor<TextFormat, bool>("IsItalic", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsItalic_, (ins, val) => ins.IsItalic_ = val, false, false,
                                                                                                                 "Is Italic", "Indicates whether the text is obliqued or normal.");

        /// <summary>
        /// Indicates wheteher the text has a line at its bottom or is normal.
        /// </summary>
        public bool IsUnderline
        {
            get
            {
                return __IsUnderline.Get(this);
            }
            set
            {
                if (__IsUnderline.Set(this, value))
                    RecreateCurrentDecorations();
            }
        }
        protected bool IsUnderline_ = false;
        public static readonly ModelPropertyDefinitor<TextFormat, bool> __IsUnderline =
                   new ModelPropertyDefinitor<TextFormat, bool>("IsUnderline", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsUnderline_, (ins, val) => ins.IsUnderline_ = val, false, false,
                                                                                                                 "Is Underlined", "Indicates wheteher the text has a line at its bottom or is normal.");

        /// <summary>
        /// Indicates wheteher the text is crossed by a line or is normal.
        /// </summary>
        public bool IsStrikethrough
        {
            get
            {
                return __IsStrikethrough.Get(this);
            }
            set
            {
                if (__IsStrikethrough.Set(this, value))
                    RecreateCurrentDecorations();
            }
        }
        protected bool IsStrikethrough_ = false;
        public static readonly ModelPropertyDefinitor<TextFormat, bool> __IsStrikethrough =
                   new ModelPropertyDefinitor<TextFormat, bool>("IsStrikethrough", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsStrikethrough_, (ins, val) => ins.IsStrikethrough_ = val, false, false,
                                                                                                                 "Is Strikethrough", "Indicates wheteher the text is crossed by a line or is normal.");

        /// <summary>
        /// Name of the font family of the text.
        /// </summary>
        public string FontFamilyName
        {
            get
            {
                return __FontFamilyName.Get(this);
            }
            set
            {
                if (__FontFamilyName.Set(this, value))
                    RecreateCurrentTypeface();
            }
        }
        protected string FontFamilyName_ = "Arial";
        public static readonly ModelPropertyDefinitor<TextFormat, string> __FontFamilyName =
                   new ModelPropertyDefinitor<TextFormat, string>("FontFamilyName", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FontFamilyName_, (ins, val) => ins.FontFamilyName_ = val, false, false,
                                                                                                                 "Font Family Name", "Name of the font family of the text.");

        /// <summary>
        /// Font size of the text.
        /// </summary>
        public double FontSize { get { return __FontSize.Get(this); } set { __FontSize.Set(this, value); } }
        protected double FontSize_ = 12.0;
        public static readonly ModelPropertyDefinitor<TextFormat, double> __FontSize =
                   new ModelPropertyDefinitor<TextFormat, double>("FontSize", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.FontSize_, (ins, val) => ins.FontSize_ = val, false, false,
                                                                                                                 "Font Size", "Font size of the text.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Typeface CurrentTypeface
        {
            // NOTE: This is not neccesary to be cloned.
            get
            {
                if (this.CurrentTypeface_ == null)
                    this.RecreateCurrentTypeface();

                return this.CurrentTypeface_;
            }
            protected set
            {
                if (this.CurrentTypeface_ == value)
                    return;

                this.CurrentTypeface_ = value;
            }
        }
        [NonSerialized]
        private Typeface CurrentTypeface_ = null;

        private void RecreateCurrentTypeface()
        {
            FontFamily Family = null;

            try
            {
                Family = new FontFamily(this.FontFamilyName);
                //T Console.WriteLine("Creating Font-Family '" + this.FontFamilyName + "' " + DateTime.Now.ToString("hh:mm:ss.fff"));
            }
            catch (Exception Problem)
            {
                Console.WriteLine("Cannot create FontFamily named '" + this.FontFamilyName + "'. Problem: " + Problem.Message);
                Family = new FontFamily("Arial");
            }

            this.CurrentTypeface = new Typeface(Family,
                                                (this.IsItalic ? FontStyles.Italic : FontStyles.Normal),
                                                (this.IsBold ? FontWeights.Bold : FontWeights.Normal),
                                                FontStretches.Normal);
        }

        /// <summary>
        /// Gets a clone of the current decorations
        /// </summary>
        public TextDecorationCollection GetCurrentDecorations()
        {
            return (this.CurrentDecorations == null ? null : this.CurrentDecorations.Clone());
        }

        [NonSerialized]
        private TextDecorationCollection CurrentDecorations = null;

        private void RecreateCurrentDecorations()
        {
            this.CurrentDecorations = (this.IsUnderline || this.IsStrikethrough ? new TextDecorationCollection() : null);
            if (this.CurrentDecorations == null)
                return;

            if (this.IsUnderline)
                this.CurrentDecorations.Add(new TextDecoration(TextDecorationLocation.Underline, null, 0,
                                                               TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended));

            if (this.IsStrikethrough)
                this.CurrentDecorations.Add(new TextDecoration(TextDecorationLocation.Strikethrough, null, 0,
                                                               TextDecorationUnit.FontRecommended, TextDecorationUnit.FontRecommended));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<TextFormat> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<TextFormat> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<TextFormat> __ClassDefinitor = null;

        public object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public TextFormat CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((TextFormat)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public TextFormat PopulateFrom(TextFormat SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        protected EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied definitor, has changed.
        /// </summary>
        public void NotifyPropertyChange(MModelPropertyDefinitor PropertyDefinitor)
        {
            this.NotifyPropertyChange(PropertyDefinitor.TechName);
        }

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied name, has changed.
        /// </summary>
        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = PropertyChanged;

            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a new formatted text object for the supplied text, max width, max height and paddings, based on this text format object.
        /// </summary>
        public FormattedText GenerateFormattedText(string Text, double MaxWidth, double MaxHeight,
                                                   double VerticalPadding = 0, double LeftPadding = 0, double RightPadding = 0)
        {
            return this.GenerateFormattedText(Text, MaxWidth, MaxHeight, this.Alignment, VerticalPadding, LeftPadding, RightPadding);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Creates and returns a new formatted text object for the supplied text, max width, max height,
        /// explicit alignment, paddings and font-size, based on this text format object.
        /// </summary>
        public FormattedText GenerateFormattedText(string Text, double MaxWidth, double MaxHeight, TextAlignment ExplicitAlignment,
                                                   double VerticalPadding = 0, double LeftPadding = 0, double RightPadding = 0,
                                                   double ExplicitFontSize = double.NaN)
        {
            MaxWidth = (MaxWidth - (LeftPadding + RightPadding)).EnforceRange(1.0, MaxWidth);
            MaxHeight = (MaxHeight - (VerticalPadding * 2.0)).EnforceRange(1.0, MaxHeight);

            if (this.CurrentTypeface == null)
                this.RecreateCurrentTypeface();

            if (this.CurrentDecorations == null)
                this.RecreateCurrentDecorations();

            var Result = new FormattedText(Text, CultureInfo.CurrentUICulture,
                                           (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight),
                                           this.CurrentTypeface, ExplicitFontSize.NaNDefault(this.FontSize), this.ForegroundBrush,
                                           null, DefaultFormattingMode);

            Result.Trimming = TextTrimming.CharacterEllipsis;
            Result.TextAlignment = ExplicitAlignment;
            Result.MaxTextWidth = (MaxWidth == double.NegativeInfinity ? 1.0 : MaxWidth);
            Result.MaxTextHeight = (MaxHeight == double.NegativeInfinity ? 1.0 : MaxHeight);

            if (this.CurrentDecorations != null)
                Result.SetTextDecorations(this.CurrentDecorations);

            return Result;
        }

        public static TextFormattingMode DefaultFormattingMode = TextFormattingMode.Ideal;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return base.ToString() + ". HC={" + GetHashCode().ToString() + "}";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}