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
// File   : Concept.cs
// Object : Instrumind.ThinkComposer.Model.GraphModel.Concept (Class)
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

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.GraphModel
{
    /// <summary>
    // Concrete Idea, topic or subject, either atomic or conformed by other concepts.
    // Usable as container package or folder for organize the Composition by nesting.
    /// Concrete object, subtype of Idea, which can be associated to others through Relationships.
    /// </summary>
    [Serializable]
    public class Concept : Idea, IModelEntity, IModelClass<Concept>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Concept()
        {
            __ClassDefinitor = new ModelClassDefinitor<Concept>("Concept", Idea.__ClassDefinitor, "Concept",
                                                                "Concrete object, subtype of Idea, which can be associated to others through Relationships.");
            __ClassDefinitor.DeclareProperty(__ConceptType);
            __ClassDefinitor.DeclareProperty(__ConceptDefinitor);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="OwnerComposition">Composition owning this Concept.</param>
        /// <param name="Definitor">Definitor of the Concept.</param>
        /// <param name="Name">Name of the Concept.</param>
        /// <param name="TechName">Technical Name of the Concept.</param>
        /// <param name="Summary">Summary of the Concept.</param>
        /// <param name="Pictogram">Image representing the Concept.</param>
        public Concept(Composition OwnerComposition, ConceptDefinition Definitor,
                       string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(OwnerComposition, Name, TechName, Summary, Pictogram)
        {
            this.ConceptDefinitor = Definitor.Assign();

            this.CompositeContentDomain = Definitor.CompositeContentDomain ?? Definitor.OwnerDomain;

            if (this.OwnerComposition != null && this.OwnerComposition.UsedDomains != null)  // UsedDomains will only be null when creating the Composition (which is a Concept also)
                this.OwnerComposition.UsedDomains.AddNew(Definitor.OwnerDomain);
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected Concept()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tipification of the concept.
        /// </summary>
        public EConceptType ConceptType { get { return __ConceptType.Get(this); } set { __ConceptType.Set(this, value); } }
        protected EConceptType ConceptType_ = EConceptType.Individual;
        public static readonly ModelPropertyDefinitor<Concept, EConceptType> __ConceptType =
                   new ModelPropertyDefinitor<Concept, EConceptType>("ConceptType", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ConceptType_, (ins, val) => ins.ConceptType_ = val, false, true,
                                                                                    "Concept Type", "Tipification of the concept.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ConceptDefinition definitor of this Concept.
        /// </summary>
        public Assignment<ConceptDefinition> ConceptDefinitor { get { return __ConceptDefinitor.Get(this); } set { __ConceptDefinitor.Set(this, value); } }
        protected Assignment<ConceptDefinition> ConceptDefinitor_ = new Assignment<ConceptDefinition>();
        public static readonly ModelPropertyDefinitor<Concept, Assignment<ConceptDefinition>> __ConceptDefinitor =
                   new ModelPropertyDefinitor<Concept, Assignment<ConceptDefinition>>("ConceptDefinitor", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.ConceptDefinitor_, (ins, val) => ins.ConceptDefinitor_ = val, false, true,
                                                                                      "Concept Definition", "Concept Definition on which this Concept is based.");

        public override IdeaDefinition IdeaDefinitor { get { return this.ConceptDefinitor.Value; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Concept> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Concept> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Concept> __ClassDefinitor = null;

        public new Concept CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Concept)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Concept PopulateFrom(Concept SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Concept GenerateIndependentConceptDuplicate()
        {
            var Result = (Concept)this.GenerateIdeaIndependentDuplicate();

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Changes the underlying definition to the specified one and propagate the necessary changes.
        /// </summary>
        public void ApplyConceptDefinitionChange(ConceptDefinition NewDefinition)
        {
            var PreviousDefinitor = this.ConceptDefinitor.Value;
            if (PreviousDefinitor.IsEqual(NewDefinition))
                return;

            this.ConceptDefinitor = NewDefinition.Assign();

            this.ApplyIdeaDefinitionChange(PreviousDefinitor);  // Must be last, because updates graphics
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}