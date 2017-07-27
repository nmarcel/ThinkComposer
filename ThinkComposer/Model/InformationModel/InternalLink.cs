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
// File   : InternalLink.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.InternalLink (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.03.16 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

using Instrumind.Common;
using Instrumind.Common.Visualization;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.InformationModel
{
    /// <summary>
    /// References an internal property.
    /// </summary>
    [Serializable]
    public class InternalLink : Link, IModelEntity, IModelClass<InternalLink>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static InternalLink()
        {
            __ClassDefinitor = new ModelClassDefinitor<InternalLink>("InternalLink", Link.__ClassDefinitor, "Internal Link",
                                                                     "References an internal property.");
            __ClassDefinitor.DeclareProperty(__AssignedDesignator);
            __ClassDefinitor.DeclareProperty(__TargetProperty);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public InternalLink(Idea OwnerContainer, Assignment<DetailDesignator>/*L*/ Designator)
             : base(OwnerContainer)
        {
            this.AssignedDesignator = Designator;
        }

        /// <summary>
        /// Internal-Link assigned designator.
        /// </summary>
        public override Assignment<DetailDesignator> AssignedDesignator { get { return __AssignedDesignator.Get(this); } set { __AssignedDesignator.Set(this, value); } }
        protected Assignment<DetailDesignator> AssignedDesignator_;
        public static readonly ModelPropertyDefinitor<InternalLink, Assignment<DetailDesignator>> __AssignedDesignator =
                  new ModelPropertyDefinitor<InternalLink, Assignment<DetailDesignator>>("AssignedDesignator", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AssignedDesignator_, (ins, val) => ins.AssignedDesignator_ = val, false, true,
                                                                                         "Assigned Designator", "Link assigned designator");
        public override MAssignment ContentDesignator { get { return this.AssignedDesignator; } }
        [Description("Internal-Link assigned designator.")]
        public LinkDetailDesignator Designator { get { return (LinkDetailDesignator)this.ContentDesignator.AssignedValue; } }

        /// <summary>
        /// Returns the underlying reference.
        /// </summary>
        public override object Target { get { return this.TargetProperty; } }

        /// <summary>
        /// Reference to the target property
        /// </summary>
        public MModelPropertyDefinitor TargetProperty { get { return __TargetProperty.Get(this); } set { __TargetProperty.Set(this, value); } }
        protected StoreBox<MModelPropertyDefinitor> TargetProperty_ = new StoreBox<MModelPropertyDefinitor>();
        public static readonly ModelPropertyDefinitor<InternalLink, MModelPropertyDefinitor> __TargetProperty =
                   new ModelPropertyDefinitor<InternalLink, MModelPropertyDefinitor>("TargetProperty", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.TargetProperty_, (ins, stb) => ins.TargetProperty_ = stb, false, true, "Target Property", "Reference to the target property.");

        /// <summary>
        /// Address of the resource.
        /// </summary>
        public override string TargetAddress { get { return this.TargetProperty.QualifiedTechName; } }

        /// <summary>
        /// Indicates whether the contained/referenced detail is non empty and not references nothing.
        /// </summary>
        public override bool ValueExists
        {
            get
            {
                if (TargetProperty == null)
                    return false;

                var Value = TargetProperty.Read(this.OwnerIdea);
                if (Value == null)
                    return false;

                return !Value.ToString().IsAbsent();
            }
        }

        /// <summary>
        /// Updates the related Designator identification information based on the Content.
        /// </summary>
        public override void UpdateDesignatorIdentification()
        {
            if (this.AssignedDesignator == null)
                return;

            this.Designation.Name = this.TargetProperty.Name;
            this.Designation.TechName = this.TargetProperty.Name.TextToIdentifier();
        }

        public override string ToString()
        {
            return ("Detail-Type" + Idea.SYNOP_SEPARATOR + "Link(Internal)" + Idea.SYNOP_SEPARATOR +
                    "Target-Property" + Idea.SYNOP_SEPARATOR + (this.TargetProperty == null ? General.STR_NULLREF : this.TargetProperty.Name));
        }


        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<InternalLink> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<InternalLink> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<InternalLink> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new InternalLink CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((InternalLink)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public InternalLink PopulateFrom(InternalLink SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}