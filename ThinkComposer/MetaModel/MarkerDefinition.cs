// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : Marker.cs
// Object : Instrumind.ThinkComposer.MetaModel.Marker (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.12.20 Néstor Sánchez A.  Creation
//

using System;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Defines a simple classificator.
    /// </summary>
    [Serializable]
    public class MarkerDefinition : SimplePresentationElement, IClusterableDef, IModelEntity, IModelClass<MarkerDefinition>
    {
        /// <summary>
        /// Code for user-defined markers.
        /// </summary>
        public const string USERDEF_CODE = "UserDef";

        /// <summary>
        /// Standar size for marker icons.
        /// </summary>
        public static readonly Size StandardMarkerIconSize = new Size(16, 16);

        /// <summary>
        /// Static constructor
        /// </summary>
        static MarkerDefinition()
        {
            __ClassDefinitor = new ModelClassDefinitor<MarkerDefinition>("MarkerDefinition", SimplePresentationElement.__ClassDefinitor, "Marker Definition",
                                                                         "Defines a simple classificator.");
            __ClassDefinitor.DeclareProperty(__ClusterKey);
            __ClassDefinitor.DeclareProperty(__Background);
            __ClassDefinitor.DeclareProperty(__Foreground);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Name of the Marker.</param>
        /// <param name="TechName">Technical Name of the Marker.</param>
        /// <param name="Summary">Summary of the Marker.</param>
        /// <param name="Pictogram">Image representing the Marker.</param>
        /// <param name="ClusterKey">Key of the Cluster this Marker belongs to.</param>
        /// <param name="Background">Background brush.</param>
        /// <param name="Foreground">Foreground brush.</param>
        public MarkerDefinition(string Name, string TechName, string Summary = "", ImageSource Pictogram = null,
                                string ClusterKey = USERDEF_CODE, Brush Background = null, Brush Foreground = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            this.ClusterKey = ClusterKey;
            this.Background = Background;
            this.Foreground = Foreground;
        }

        /// <summary>
        /// Protected Constructor for Agent descendants and cloning.
        /// </summary>
        public MarkerDefinition()
        {
        }

        /// <summary>
        /// Key of the associated Cluster (tipically refers to the Tech-Name of the Cluster).
        /// </summary>
        public string ClusterKey { get { return __ClusterKey.Get(this); } set { __ClusterKey.Set(this, value); } }
        protected string ClusterKey_;
        public static readonly ModelPropertyDefinitor<MarkerDefinition, string> __ClusterKey =
                   new ModelPropertyDefinitor<MarkerDefinition, string>("ClusterKey", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ClusterKey_, (ins, val) => ins.ClusterKey_ = val, false, false,
                                                                        "Cluster Key", "Key of the associated Cluster (tipically refers to the Tech-Name of the Cluster).");

        /// <summary>
        /// Color brush for the Marker's background.
        /// </summary>
        public Brush Background { get { return __Background.Get(this).NullDefault(Brushes.White /*DodgerBlue*/); } set { __Background.Set(this, value); } }
        protected StoreBox<Brush> Background_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<MarkerDefinition, Brush> __Background =
                   new ModelPropertyDefinitor<MarkerDefinition, Brush>("Background", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Background_, (ins, stb) => ins.Background_ = stb, false, false,
                                                                       "Background", "Color brush for the Marker's background.");

        /// <summary>
        /// Color brush for the Marker's foreground.
        /// </summary>
        public Brush Foreground { get { return __Foreground.Get(this).NullDefault(Brushes.Black /*GhostWhite*/); } set { __Foreground.Set(this, value); } }
        protected StoreBox<Brush> Foreground_ = StoreBox.Store<Brush>(null);
        public static readonly ModelPropertyDefinitor<MarkerDefinition, Brush> __Foreground =
                   new ModelPropertyDefinitor<MarkerDefinition, Brush>("Foreground", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Foreground_, (ins, stb) => ins.Foreground_ = stb, false, false,
                                                                       "Foreground", "Color brush for the Marker's foreground.");
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<MarkerDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<MarkerDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<MarkerDefinition> __ClassDefinitor = null;

        public new MarkerDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((MarkerDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public MarkerDefinition PopulateFrom(MarkerDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}