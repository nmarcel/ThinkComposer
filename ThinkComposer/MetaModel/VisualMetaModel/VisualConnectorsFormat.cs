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
// File   : VisualConnectorsFormat.cs
// Object : Instrumind.ThinkComposer.MetaModel.VisualMetaModel.VisualConnectorsFormat (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.08.31 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Base abstractions for the user metadefinition of Visual schemas
namespace Instrumind.ThinkComposer.MetaModel.VisualMetaModel
{
    /// <summary>
    /// Specifies format for connectors of a represented Relationship, considering its plugs and decorators.
    /// </summary>
    [Serializable]
    public class VisualConnectorsFormat : VisualElementFormat, IModelEntity, IModelClass<VisualConnectorsFormat>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static VisualConnectorsFormat()
        {
            __ClassDefinitor = new ModelClassDefinitor<VisualConnectorsFormat>("VisualConnectorsFormat", VisualElementFormat.__ClassDefinitor, "Visual Connectors Format",
                                                                               "Specifies format for connectors of a represented Relationship, considering its plugs and decorators.");
            __ClassDefinitor.DeclareCollection(__HeadPlugs);
            __ClassDefinitor.DeclareCollection(__TailPlugs);

            __ClassDefinitor.DeclareProperty(__PathStyle);
            __ClassDefinitor.DeclareProperty(__PathCorner);

            __ClassDefinitor.DeclareProperty(__LabelLinkVariant);
            __ClassDefinitor.DeclareProperty(__LabelLinkDefinitor);
            __ClassDefinitor.DeclareProperty(__LabelLinkDescriptor);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualConnectorsFormat(SimplePresentationElement DefaultTailVariant, string DefaultTailPlug,
                                      SimplePresentationElement DefaultHeadVariant, string DefaultHeadPlug,
                                      Brush LineBrush, DashStyle LineDash = null,
                                      double LineThickness = DEFAULT_LINE_THICKNESS, double Opacity = 1.0, Brush BackgroundBrush = null,
                                      EPathStyle PathStyle = EPathStyle.SinglelineStraight, EPathCorner PathCorner = EPathCorner.Rounded,
                                      PenLineCap LineCap = PenLineCap.Round, PenLineJoin LineJoin = PenLineJoin.Round)
            : base(BackgroundBrush, LineBrush, LineThickness, Opacity, LineDash, LineCap, LineJoin)
        {
            General.ContractRequiresNotNull(DefaultHeadVariant, DefaultTailVariant, LineBrush);
            General.ContractRequiresNotAbsent(DefaultHeadPlug, DefaultTailPlug);

            this.HeadPlugs = new EditableDictionary<SimplePresentationElement, string>(__HeadPlugs.TechName, this);
            this.HeadPlugs.Add(DefaultHeadVariant, DefaultHeadPlug);

            this.TailPlugs = new EditableDictionary<SimplePresentationElement, string>(__TailPlugs.TechName, this);
            this.TailPlugs.Add(DefaultTailVariant, DefaultTailPlug);

            this.PathStyle = PathStyle;
            this.PathCorner = PathCorner;
        }

        /// <summary>
        /// Copy Constructor.
        /// </summary>
        public VisualConnectorsFormat(VisualConnectorsFormat Original)
             : base()
        {
            this.PopulateFrom(Original, null, ECloneOperationScope.Deep);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Styles for the plug symbol of the connector end/destination point by link-role Variant (the default is the first one).
        /// </summary>
        public EditableDictionary<SimplePresentationElement, string> HeadPlugs { get; protected set; }
        public static ModelDictionaryDefinitor<VisualConnectorsFormat, SimplePresentationElement, string> __HeadPlugs =
                   new ModelDictionaryDefinitor<VisualConnectorsFormat, SimplePresentationElement, string>("HeadPlugs", EEntityMembership.InternalCoreExclusive, ins => ins.HeadPlugs, (ins, coll) => ins.HeadPlugs = coll,
                                                                                                           "Head Plugs", "Styles for the plug symbol of the connector end/destination point by link-role Variant (the default is the first one).");

        /// <summary>
        /// Styles for the plug symbol of the connector start/origin point by link-role Variant (the default is the first one).
        /// </summary>
        public EditableDictionary<SimplePresentationElement, string> TailPlugs { get; protected set; }
        public static ModelDictionaryDefinitor<VisualConnectorsFormat, SimplePresentationElement, string> __TailPlugs =
                   new ModelDictionaryDefinitor<VisualConnectorsFormat, SimplePresentationElement, string>("TailPlugs", EEntityMembership.InternalCoreExclusive, ins => ins.TailPlugs, (ins, coll) => ins.TailPlugs = coll,
                                                                                                           "Tail Plugs", "Styles for the plug symbol of the connector start/origin point by link-role Variant (the default is the first one).");

        /// <summary>
        /// Type of path style considering number of lines and angulation of them.
        /// </summary>
        public EPathStyle PathStyle { get { return __PathStyle.Get(this); } set { __PathStyle.Set(this, value); } }
        protected EPathStyle PathStyle_ = EPathStyle.SinglelineStraight;
        public static readonly ModelPropertyDefinitor<VisualConnectorsFormat, EPathStyle> __PathStyle =
                   new ModelPropertyDefinitor<VisualConnectorsFormat, EPathStyle>("PathStyle", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PathStyle_, (ins, val) => ins.PathStyle_ = val, false, false,
                                                                                  "Path Style", "Type of path style considering number of lines and angulation of them.");
        public static EPathStyle GetPathStyle(VisualConnector Instance)
        { return (EPathStyle)GetValue(Instance, __PathStyle.TechName); }
        public static void SetPathStyle(VisualConnector Instance, EPathStyle Value)
        { SetValue(Instance, __PathStyle.TechName, Value); }

        /// <summary>
        /// Type of corners for multiline paths.
        /// </summary>
        public EPathCorner PathCorner { get { return __PathCorner.Get(this); } set { __PathCorner.Set(this, value); } }
        protected EPathCorner PathCorner_ = EPathCorner.Rounded;
        public static readonly ModelPropertyDefinitor<VisualConnectorsFormat, EPathCorner> __PathCorner =
                   new ModelPropertyDefinitor<VisualConnectorsFormat, EPathCorner>("PathCorner", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PathCorner_, (ins, val) => ins.PathCorner_ = val, false, false,
                                                                                   "Path Corner", "Type of corners for multiline paths.");
        public static EPathCorner GetPathCorner(VisualConnector Instance)
        { return (EPathCorner)GetValue(Instance, __PathCorner.TechName); }
        public static void SetPathCorner(VisualConnector Instance, EPathCorner Value)
        { SetValue(Instance, __PathCorner.TechName, Value); }

        /// <summary>
        /// Indicates to label the Link (role) Variant over the connector.
        /// </summary>
        public bool LabelLinkVariant { get { return __LabelLinkVariant.Get(this); } set { __LabelLinkVariant.Set(this, value); } }
        protected bool LabelLinkVariant_ = false;
        public static ModelPropertyDefinitor<VisualConnectorsFormat, bool> __LabelLinkVariant =
                  new ModelPropertyDefinitor<VisualConnectorsFormat, bool>("LabelLinkVariant", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LabelLinkVariant_, (ins, val) => ins.LabelLinkVariant_ = val, true, false,
                                                                           "Label Link Variant", "Indicates to label the Link (role) with the Variant name over the connector.");
        public static bool GetLabelLinkVariant(VisualConnector Instance)
        { return (bool)GetValue(Instance, __LabelLinkVariant.TechName); }
        public static void SetLabelLinkVariant(VisualConnector Instance, bool Value)
        { SetValue(Instance, __LabelLinkVariant.TechName, Value); }

        /// <summary>
        /// Indicates to label the Link Definitor name over the connector.
        /// </summary>
        public bool LabelLinkDefinitor { get { return __LabelLinkDefinitor.Get(this); } set { __LabelLinkDefinitor.Set(this, value); } }
        protected bool LabelLinkDefinitor_ = false;
        public static ModelPropertyDefinitor<VisualConnectorsFormat, bool> __LabelLinkDefinitor =
                  new ModelPropertyDefinitor<VisualConnectorsFormat, bool>("LabelLinkDefinitor", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LabelLinkDefinitor_, (ins, val) => ins.LabelLinkDefinitor_ = val, true, false,
                                                                           "Label Link Definitor", "Indicates to label the Link with the Definition name over the connector.");
        public static bool GetLabelLinkDefinitor(VisualConnector Instance)
        { return (bool)GetValue(Instance, __LabelLinkDefinitor.TechName); }
        public static void SetLabelLinkDefinitor(VisualConnector Instance, bool Value)
        { SetValue(Instance, __LabelLinkDefinitor.TechName, Value); }

        /// <summary>
        /// Indicates to label the Link Descriptor name over the connector.
        /// </summary>
        public bool LabelLinkDescriptor { get { return __LabelLinkDescriptor.Get(this); } set { __LabelLinkDescriptor.Set(this, value); } }
        protected bool LabelLinkDescriptor_ = true;
        public static ModelPropertyDefinitor<VisualConnectorsFormat, bool> __LabelLinkDescriptor =
                  new ModelPropertyDefinitor<VisualConnectorsFormat, bool>("LabelLinkDescriptor", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.LabelLinkDescriptor_, (ins, val) => ins.LabelLinkDescriptor_ = val, true, false,
                                                                           "Label Link Descriptor", "Indicates to label the Link with the Descriptor name over the connector.");
        public static bool GetLabelLinkDescriptor(VisualConnector Instance)
        { return (bool)GetValue(Instance, __LabelLinkDescriptor.TechName); }
        public static void SetLabelLinkDescriptor(VisualConnector Instance, bool Value)
        { SetValue(Instance, __LabelLinkDescriptor.TechName, Value); }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<VisualConnectorFormat> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<VisualConnectorsFormat> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<VisualConnectorsFormat> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new VisualConnectorsFormat CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((VisualConnectorsFormat)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public VisualConnectorsFormat PopulateFrom(VisualConnectorsFormat SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}