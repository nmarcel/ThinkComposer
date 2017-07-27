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
// File   : ExternalLanguageDeclaration.cs
// Object : Instrumind.ThinkComposer.MetaModel.ExternalLanguageDeclaration (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2013.03.23 Néstor Sánchez A.  Start
//

using System;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

/// Provides configuration information for meta-models.
namespace Instrumind.ThinkComposer.MetaModel.Configurations
{
    /// <summary>
    /// Stores information about a declared External Language.
    /// </summary>
    [Serializable]
    public class ExternalLanguageDeclaration : FormalPresentationElement, IModelEntity, IModelClass<ExternalLanguageDeclaration>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>.
        static ExternalLanguageDeclaration()
        {
            __ClassDefinitor = new ModelClassDefinitor<ExternalLanguageDeclaration>("ExternalLanguageDeclaration", FormalPresentationElement.__ClassDefinitor, "Meta-Definition",
                                                                       "Represents, at a metalevel of abstraction, the definition of the data structure upon which create schema objects of a type.");
            __ClassDefinitor.DeclareProperty(__MetaId);
            __ClassDefinitor.DeclareProperty(__Alterability);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Name of the ExternalLanguageDeclaration.</param>
        /// <param name="TechName">Technical Name of the ExternalLanguageDeclaration.</param>
        /// <param name="Summary">Summary of the ExternalLanguageDeclaration.</param>
        /// <param name="Pictogram">Image representing the ExternalLanguageDeclaration.</param>
        /// <param name="IsVersioned">Indicates whether to store versioning information.</param>
        public ExternalLanguageDeclaration(string Name, string TechName, string Summary = "", ImageSource Pictogram = null, bool IsVersioned = false)
            : base(Name, TechName, Summary, Pictogram, false, false, IsVersioned)
        {
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected ExternalLanguageDeclaration()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Simple identifier for indirectly associate created objects with definitions.
        /// </summary>
        public int MetaId { get { return __MetaId.Get(this); } set { __MetaId.Set(this, value); } }
        protected int MetaId_;
        public static readonly ModelPropertyDefinitor<ExternalLanguageDeclaration, int> __MetaId =
                   new ModelPropertyDefinitor<ExternalLanguageDeclaration, int>("MetaId", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MetaId_, (ins, val) => ins.MetaId_ = val, false, true,
                                                                                    "Meta-Id", "Simple identifier for indirectly associate created objects with definitions.");

        /// <summary>
        /// Indicates at what level this definition can be changed.
        /// </summary>
        public EAlterability Alterability { get { return __Alterability.Get(this); } set { __Alterability.Set(this, value); } }
        protected EAlterability Alterability_ = EAlterability.Any;
        public static readonly ModelPropertyDefinitor<ExternalLanguageDeclaration, EAlterability> __Alterability =
                   new ModelPropertyDefinitor<ExternalLanguageDeclaration, EAlterability>("Alterability", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.Alterability_, (ins, val) => ins.Alterability_ = val, false, true,
                                                                                    "Alterability", "Indicates at what level this definition can be changed.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<ExternalLanguageDeclaration> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<ExternalLanguageDeclaration> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<ExternalLanguageDeclaration> __ClassDefinitor = null;

        public new ExternalLanguageDeclaration CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ExternalLanguageDeclaration)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public ExternalLanguageDeclaration PopulateFrom(ExternalLanguageDeclaration SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

    }
}