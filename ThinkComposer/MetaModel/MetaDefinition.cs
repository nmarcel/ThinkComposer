// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : MetaDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.MetaDefinition (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Start
//

using System;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

/// Metadata shared abstractions which conform a Domain definition: Primitives for Composition creation.
namespace Instrumind.ThinkComposer.MetaModel
{
    /// <summary>
    /// Represents, at a metalevel of abstraction, the definition of the data structure upon which create schema objects of a type.
    /// </summary>
    [Serializable]
    public abstract class MetaDefinition : FormalPresentationElement, IModelEntity, IModelClass<MetaDefinition>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>.
        static MetaDefinition()
        {
            __ClassDefinitor = new ModelClassDefinitor<MetaDefinition>("MetaDefinition", FormalPresentationElement.__ClassDefinitor, "Meta-Definition",
                                                                       "Represents, at a metalevel of abstraction, the definition of the data structure upon which create schema objects of a type.");
            __ClassDefinitor.DeclareProperty(__MetaId);
            __ClassDefinitor.DeclareProperty(__Alterability);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Name of the MetaDefinition.</param>
        /// <param name="TechName">Technical Name of the MetaDefinition.</param>
        /// <param name="Summary">Summary of the MetaDefinition.</param>
        /// <param name="Pictogram">Image representing the MetaDefinition.</param>
        /// <param name="IsVersioned">Indicates whether to store versioning information.</param>
        public MetaDefinition(string Name, string TechName, string Summary = "", ImageSource Pictogram = null, bool IsVersioned = false)
            : base(Name, TechName, Summary, Pictogram, false, false, IsVersioned)
        {
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected MetaDefinition()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simple identifier for indirectly associate created objects with definitions.
        /// </summary>
        public int MetaId { get { return __MetaId.Get(this); } set { __MetaId.Set(this, value); } }
        protected int MetaId_;
        public static readonly ModelPropertyDefinitor<MetaDefinition, int> __MetaId =
                   new ModelPropertyDefinitor<MetaDefinition, int>("MetaId", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MetaId_, (ins, val) => ins.MetaId_ = val, false, true,
                                                                                    "Meta-Id", "Simple identifier for indirectly associate created objects with definitions.");

        /// <summary>
        /// Indicates at what level this definition can be changed.
        /// </summary>
        public EAlterability Alterability { get { return __Alterability.Get(this); } set { __Alterability.Set(this, value); } }
        protected EAlterability Alterability_ = EAlterability.Any;
        public static readonly ModelPropertyDefinitor<MetaDefinition, EAlterability> __Alterability =
                   new ModelPropertyDefinitor<MetaDefinition, EAlterability>("Alterability", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Alterability_, (ins, val) => ins.Alterability_ = val, false, true,
                                                                                    "Alterability", "Indicates at what level this definition can be changed.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<MetaDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<MetaDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<MetaDefinition> __ClassDefinitor = null;

        public new MetaDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((MetaDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public MetaDefinition PopulateFrom(MetaDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

    }
}