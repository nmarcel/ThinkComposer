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
// File   : VisualElementFormat.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.VisualElementFormat (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.31 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Specifies format for visual elements, considering opacity, line and background styles.
    /// </summary>
    [Serializable]
    public abstract class VisualElementFormat : IModelEntity, IModelClass<VisualElementFormat>
    {
        public const double DEFAULT_LINE_THICKNESS = 0.5;

        public static Brush DefaultMainBackground = Brushes.LightGray;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualElementFormat()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualElementFormat>("VisualElementFormat", null, "Visual Element Format",
                                                                            "Specifies format for visual elements, considering opacity, line and background styles.");
            __ClassDefinitor.DeclareProperty(__Opacity);
            __ClassDefinitor.DeclareProperty(__MainBackground);
            __ClassDefinitor.DeclareProperty(__LineBrush);
            __ClassDefinitor.DeclareProperty(__LineDash);
            __ClassDefinitor.DeclareProperty(__LineCap);
            __ClassDefinitor.DeclareProperty(__LineJoin);
            __ClassDefinitor.DeclareProperty(__LineThickness);
            // POSTPONED: __ClassDefinitor.DeclareProperty(__HasShadow);

            __Opacity.RangeMin = 0;
            __Opacity.RangeMax = 1;
            __Opacity.RangeStep = 0.05;

            __LineThickness.ItemsSourceGetter = ((ctx) => MasterDrawer.AvailableThicknesses);
            __LineThickness.ItemsSourceSelectedValuePath = SimpleElement.__TechName.TechName;
            __LineThickness.ItemsSourceDisplayMemberPath = SimpleElement.__Name.TechName;
            __LineThickness.BindingValueConverter =
                new GenericConverter<double, SimplePresentationElement>(val => val == 0.0 ? null : MasterDrawer.AvailableThicknesses.First(tn => tn.TechName == val.ToString(CultureInfo.InvariantCulture.NumberFormat)),
                                                                        spe => spe == null ? 0.0 : double.Parse(spe.TechName, CultureInfo.InvariantCulture.NumberFormat));

            __LineDash.ItemsSourceGetter = ((ctx) => MasterDrawer.AvailableDashStyles);
            __LineDash.ItemsSourceSelectedValuePath = SimpleElement.__TechName.TechName;
            __LineDash.ItemsSourceDisplayMemberPath = SimpleElement.__Name.TechName;
            __LineDash.BindingValueConverter =
                new GenericConverter<DashStyle, SimplePresentationElement>(dash => dash == null ? null : MasterDrawer.AvailableDashStyles.First(ds => ds.TechName == Display.DeclaredDashStyles.First(reg => reg.Item1 == dash).Item2),
                                                                           spe => spe == null ? null : Display.DeclaredDashStyles.First(reg => reg.Item2 == spe.TechName).Item1);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualElementFormat(Brush BackgroundBrush, Brush LineBrush, double LineThickness = DEFAULT_LINE_THICKNESS, double Opacity = 1.0,
                                   DashStyle LineDash = null, PenLineCap LineCap = PenLineCap.Round, PenLineJoin LineJoin = PenLineJoin.Miter)
        {
            this.MainBackground = BackgroundBrush.NullDefault(Brushes.WhiteSmoke);

            this.LineBrush = LineBrush ?? Brushes.Black;
            this.Opacity = Opacity;
            this.LineThickness = LineThickness;
            this.LineDash = LineDash ?? DashStyles.Solid;
        }

        /// <summary>
        /// Empty constructor for the descendants copy constructors.
        /// </summary>
        protected VisualElementFormat()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public const string TEXTFORMAT_PREFIX = "^";

        /// <summary>
        /// Gets the Element-Format Property/Text-Format nullable value for the specified Instance and Key.
        /// </summary>
        public static TResult GetValueFrom<TResult>(VisualElement Instance, string PropertyKey)
                where TResult : struct
        {
            var Result = default(TResult);

            var Value = GetValue(Instance, PropertyKey);
            if (Value != null)
                if (Value.IsNullable())
                {
                    var Container = Value as Nullable<TResult>;
                    if (Container.HasValue)
                        Result = Container.Value;
                }
                else
                    if (Value is TResult)
                        Result = (TResult)Value;

            return Result;
        }

        /// <summary>
        /// Gets the Element-Format Property/Text-Format value for the specified Instance and Key.
        /// </summary>
        public static object GetValue(VisualElement Instance, string PropertyKey)
        {
            object Result = null;
            MModelPropertyDefinitor PropDef = null;
            VisualElementFormat DefaultFormat = null;
            var StorageKey = PropertyKey;

            if (!PropertyKey.StartsWith(TEXTFORMAT_PREFIX))
            {
                if (Instance is VisualSymbol)
                {
                    PropDef = VisualSymbolFormat.__ClassDefinitor.GetPropertyDef(PropertyKey);
                    DefaultFormat = Instance.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat;
                }
                else
                {
                    PropDef = VisualConnectorsFormat.__ClassDefinitor.GetPropertyDef(PropertyKey);
                    DefaultFormat = ((RelationshipVisualRepresentation)Instance.OwnerRepresentation).RepresentedRelationship
                                                                        .RelationshipDefinitor.Value.DefaultConnectorsFormat;
                }

                // Do not do this concatenation for Visual-Symbol to not loose users' (stock) already stored formatting.
                if (Instance is VisualConnector)
                    StorageKey = VisualConnector.__ClassDefinitor.TechName + "." + PropertyKey; // Avoids ambiguity between VisualElementFormat descendants
            }

            if (Instance.OwnerRepresentation.CustomFormatValues.ContainsKey(StorageKey))
            {
                Result = Instance.OwnerRepresentation.CustomFormatValues[StorageKey];

                if (Result != null && PropDef != null && PropDef.IsStoreBoxBased)
                    Result = ((StoreBoxBase)Result).StoredObject;
            }
            else
                if (PropertyKey.StartsWith(TEXTFORMAT_PREFIX))
                    Result = Instance.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat
                                .GetTextFormat((ETextPurpose)Enum.Parse(typeof(ETextPurpose), PropertyKey.Substring(1)));
                else
                    Result = PropDef.Read(DefaultFormat);

            return Result;
        }

        /// <summary>
        /// Sets the Symbol-Format Property/Text-Format Value for the specified Instance and Key.
        /// </summary>
        public static void SetValue(VisualElement Instance, string PropertyKey, object Value)
        {
            object GlobalValue = null;
            bool ValuesAreDifferent = false;
            MModelPropertyDefinitor PropDef = null;
            var StorageKey = PropertyKey;

            if (PropertyKey.StartsWith(TEXTFORMAT_PREFIX))
            {
                var TxtFormat = Instance.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat
                                    .GetTextFormat((ETextPurpose)Enum.Parse(typeof(ETextPurpose), PropertyKey.Substring(1)));
                GlobalValue = TxtFormat;
                ValuesAreDifferent = !TxtFormat.IsEquivalentTo((TextFormat)Value);
            }
            else
            {
                VisualElementFormat DefaultFormat = null;

                if (Instance is VisualSymbol)
                {
                    PropDef = VisualSymbolFormat.__ClassDefinitor.GetPropertyDef(PropertyKey);
                    DefaultFormat = Instance.OwnerRepresentation.RepresentedIdea.IdeaDefinitor.DefaultSymbolFormat;
                }
                else
                {
                    PropDef = VisualConnectorsFormat.__ClassDefinitor.GetPropertyDef(PropertyKey);
                    DefaultFormat = ((RelationshipVisualRepresentation)Instance.OwnerRepresentation).RepresentedRelationship
                                                                        .RelationshipDefinitor.Value.DefaultConnectorsFormat;
                }

                GlobalValue = PropDef.Read(DefaultFormat);
                ValuesAreDifferent = (GlobalValue != Value);
            }

            // Do not do this concatenation for Visual-Symbol to not loose users' (stock) already stored formatting.
            if (Instance is VisualConnector)
                StorageKey = VisualConnector.__ClassDefinitor.TechName + "." + PropertyKey; // Avoids ambiguity between VisualElementFormat descendants

            if (ValuesAreDifferent)
            {
                if (Value != null && PropDef != null && PropDef.IsStoreBoxBased)
                    Value = StoreBox.CreateStoreBoxForType(PropDef.DataType, Value);

                Instance.OwnerRepresentation.CustomFormatValues.AddOrReplace(StorageKey, Value);
            }
            else
                Instance.OwnerRepresentation.CustomFormatValues.Remove(StorageKey);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opacity factor for the element (0=Transparent).
        /// </summary>
        public double Opacity { get { return __Opacity.Get(this); } set { __Opacity.Set(this, value); } }
        protected double Opacity_ = 1.0;
        public static readonly ModelPropertyDefinitor<VisualElementFormat, double> __Opacity =
                   new ModelPropertyDefinitor<VisualElementFormat, double>("Opacity", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Opacity_, (ins, val) => ins.Opacity_ = val, false, false,
                                                                           "Opacity", "Opacity factor for the element (0=Transparent).");
        public static double GetOpacity(VisualElement Instance)
        { return (double)GetValue(Instance, __Opacity.TechName); }
        public static void SetOpacity(VisualElement Instance, double Value)
        { SetValue(Instance, __Opacity.TechName, Value); }

        /// <summary>
        /// Brush for the element main background.
        /// </summary>
        public Brush MainBackground { get { return __MainBackground.Get(this); } set { __MainBackground.Set(this, value); } }
        protected StoreBox<Brush> MainBackground_ = DefaultMainBackground.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualElementFormat, Brush> __MainBackground =
                   new ModelPropertyDefinitor<VisualElementFormat, Brush>("MainBackground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MainBackground_, (ins, stb) => ins.MainBackground_ = stb, false, false,
                                                                          "Main Background", "Brush for the main background of the object.");
        public static Brush GetMainBackground(VisualElement Instance)
        { return (Brush)GetValue(Instance, __MainBackground.TechName); }
        public static void SetMainBackground(VisualElement Instance, Brush Value)
        { SetValue(Instance, __MainBackground.TechName, Value); }

        /// <summary>
        /// Foreground Brush of the lines.
        /// </summary>
        public Brush LineBrush { get { return __LineBrush.Get(this); } set { __LineBrush.Set(this, value); } }
        protected StoreBox<Brush> LineBrush_ = Brushes.Black.Store<Brush>();
        public static readonly ModelPropertyDefinitor<VisualElementFormat, Brush> __LineBrush =
                   new ModelPropertyDefinitor<VisualElementFormat, Brush>("LineBrush", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LineBrush_, (ins, stb) => ins.LineBrush_ = stb, false, false,
                                                                          "Line Brush", "Foreground brush of the lines.");
        public static Brush GetLineBrush(VisualElement Instance)
        { return (Brush)GetValue(Instance, __LineBrush.TechName); }
        public static void SetLineBrush(VisualElement Instance, Brush Value)
        { SetValue(Instance, __LineBrush.TechName, Value); }

        /// <summary>
        /// Dash style of the lines.
        /// </summary>
        public DashStyle LineDash { get { return __LineDash.Get(this); } set { __LineDash.Set(this, value); } }
        protected StoreBox<DashStyle> LineDash_ = DashStyles.Solid.Store();
        public static readonly ModelPropertyDefinitor<VisualElementFormat, DashStyle> __LineDash =
                   new ModelPropertyDefinitor<VisualElementFormat, DashStyle>("LineDash", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LineDash_, (ins, stb) => ins.LineDash_ = stb, false, false,
                                                                              "Line Dash", "Dash style of the lines.");
        public static DashStyle GetLineDash(VisualElement Instance)
        { return (DashStyle)GetValue(Instance, __LineDash.TechName); }
        public static void SetLineDash(VisualElement Instance, DashStyle Value)
        { SetValue(Instance, __LineDash.TechName, Value); }

        /// <summary>
        /// Style of the start/end of the lines.
        /// </summary>
        public PenLineCap LineCap { get { return __LineCap.Get(this); } set { __LineCap.Set(this, value); } }
        protected PenLineCap LineCap_ = PenLineCap.Round;
        public static readonly ModelPropertyDefinitor<VisualElementFormat, PenLineCap> __LineCap =
                   new ModelPropertyDefinitor<VisualElementFormat, PenLineCap>("LineCap", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LineCap_, (ins, val) => ins.LineCap_ = val, false, false,
                                                                               "Line Cap", "Style of the start/end of the lines.");
        public static PenLineCap GetLineCap(VisualElement Instance)
        { return (PenLineCap)GetValue(Instance, __LineCap.TechName); }
        public static void SetLineCap(VisualElement Instance, PenLineCap Value)
        { SetValue(Instance, __LineCap.TechName, Value); }

        /// <summary>
        /// Style of the join between two lines.
        /// </summary>
        public PenLineJoin LineJoin { get { return __LineJoin.Get(this); } set { __LineJoin.Set(this, value); } }
        protected PenLineJoin LineJoin_ = PenLineJoin.Miter;
        public static readonly ModelPropertyDefinitor<VisualElementFormat, PenLineJoin> __LineJoin =
                   new ModelPropertyDefinitor<VisualElementFormat, PenLineJoin>("LineJoin", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LineJoin_, (ins, val) => ins.LineJoin_ = val, false, false,
                                                                                "Line Join", "Style of the join between two lines.");
        public static PenLineJoin GetLineJoin(VisualElement Instance)
        { return (PenLineJoin)GetValue(Instance, __LineJoin.TechName); }
        public static void SetLineJoin(VisualElement Instance, PenLineJoin Value)
        { SetValue(Instance, __LineJoin.TechName, Value); }

        /// <summary>
        /// Thickness of the lines.
        /// </summary>
        public double LineThickness { get { return __LineThickness.Get(this); } set { __LineThickness.Set(this, value); } }
        protected double LineThickness_ = 0.5;
        public static readonly ModelPropertyDefinitor<VisualElementFormat, double> __LineThickness =
                   new ModelPropertyDefinitor<VisualElementFormat, double>("LineThickness", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LineThickness_, (ins, val) => ins.LineThickness_ = val, false, false,
                                                                           "Line Thickness", "Thickness of the lines.");
        public static double GetLineThickness(VisualElement Instance)
        { return (double)GetValue(Instance, __LineThickness.TechName); }
        public static void SetLineThickness(VisualElement Instance, double Value)
        { SetValue(Instance, __LineThickness.TechName, Value); }

        /* POSTPONED:
        /// <summary>
        /// Indicates whether a shadow is cast under the visual element.
        /// </summary>
        public bool HasShadow { get { return __HasShadow.Get(this); } set { __HasShadow.Set(this, value); } }
        protected bool HasShadow_ = false;
        public static readonly ModelPropertyDefinitor<VisualElementFormat, bool> __HasShadow =
                   new ModelPropertyDefinitor<VisualElementFormat, bool>("HasShadow", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HasShadow_, (ins, val) => ins.HasShadow_ = val, false, true, false,
                                                                         "Has Shadow", "Indicates whether a shadow is cast under the visual element."); */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualElementFormat> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<VisualElementFormat> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<VisualElementFormat> __ClassDefinitor = null;

        public abstract object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner);
        public VisualElementFormat CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualElementFormat)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualElementFormat PopulateFrom(VisualElementFormat SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        private EntityEditEngine EditEngine_ = null;

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

    }
}