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
// File   : ConceptVisualRepresentation.cs
// Object : Instrumind.ThinkComposer.Model.VisualModel.ConceptVisualRepresentation (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.09.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the visual representation of Graph entities
namespace Instrumind.ThinkComposer.Model.VisualModel
{
    /// <summary>
    /// Visually represents a Concept.
    /// </summary>
    [Serializable]
    public class ConceptVisualRepresentation : VisualRepresentation, IModelEntity, IModelClass<ConceptVisualRepresentation>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static ConceptVisualRepresentation()
        {
            __ClassDefinitor = new ModelClassDefinitor<ConceptVisualRepresentation>("ConceptVisualRepresentation", VisualRepresentation.__ClassDefinitor, "Concept Visual Representation",
                                                                                    "Visually represents a Concept.");
            __ClassDefinitor.DeclareProperty(__RepresentedConcept);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ConceptVisualRepresentation(Concept RepresentedConcept, View DisplayingView)
             : base(DisplayingView)
        {
            this.RepresentedConcept = RepresentedConcept;

            this.RepresentedConcept.VisualRepresentators.Add(this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Represented Idea by this visual element.
        /// </summary>
        public override Idea RepresentedIdea { get { return this.RepresentedConcept; } }

        /// <summary>
        /// Represented Concept by this visual element.
        /// </summary>
        public Concept RepresentedConcept { get { return __RepresentedConcept.Get(this); } set { __RepresentedConcept.Set(this, value); } }
        protected Concept RepresentedConcept_;
        public static readonly ModelPropertyDefinitor<ConceptVisualRepresentation, Concept> __RepresentedConcept =
                   new ModelPropertyDefinitor<ConceptVisualRepresentation, Concept>("RepresentedConcept", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.RepresentedConcept_, (ins, val) => ins.RepresentedConcept_ = val, false, false,
                                                                                    "Represented Concept", "Represented Concept by this visual element.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the graphic parts (drawing and z-order) of the visual depiction of this representation, plus any others needed to make it comprehensive.
        /// For example, a Relationship only is meaningful if, appart of showing its main-symbol and connectors, it also includes the connected symbols.
        /// </summary>
        public override IDictionary<Drawing,int> CreateIntegralGraphic()
        {
            var Result = new Dictionary<Drawing, int>();
            var SourceView = this.MainSymbol.GetDisplayingView();

            // Generate the symbol
            Result.Add(this.MainSymbol.CreateDraw(null, false), this.MainSymbol.ZOrder);

            // Generate complements
            foreach (var Complement in this.MainSymbol.AttachedComplements)
                Result.Add(Complement.CreateDraw(false), Complement.ZOrder);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<ConceptVisualRepresentation> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<ConceptVisualRepresentation> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<ConceptVisualRepresentation> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new ConceptVisualRepresentation CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ConceptVisualRepresentation)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public ConceptVisualRepresentation PopulateFrom(ConceptVisualRepresentation SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        public override string ToString()
        {
            return "Visual Representation of Concept '" + this.RepresentedIdea.ToStringAlways() + "' on View '" + this.DisplayingView.ToStringAlways() + "'";
        }
    }
}