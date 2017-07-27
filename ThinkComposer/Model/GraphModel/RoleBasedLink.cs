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
// File   : RoleBasedLink.cs
// Object : Instrumind.ThinkComposer.Model.GraphModel.RoleBasedLink (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.GraphModel
{
    /// <summary>
    /// Links a Relationship with a related Idea, following a Relationship Link Role Definition.
    /// </summary>
    [Serializable]
    public class RoleBasedLink : UniqueElement, IModelEntity, IModelClass<RoleBasedLink>, IVersionUpdater
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static RoleBasedLink()
        {
            __ClassDefinitor = new ModelClassDefinitor<RoleBasedLink>("RoleBasedLink", UniqueElement.__ClassDefinitor, "Link",
                                                                      "Links a Relationship with a related Idea, following a Link Role Definition.");
            __ClassDefinitor.DeclareProperty(__OwnerRelationship);
            __ClassDefinitor.DeclareProperty(__Descriptor);
            __ClassDefinitor.DeclareProperty(__AssociatedIdea);
            __ClassDefinitor.DeclareProperty(__RoleDefinitor);
            __ClassDefinitor.DeclareProperty(__RoleVariant);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public RoleBasedLink(Relationship RelationshipOwner, Idea AssociatedIdea, LinkRoleDefinition RoleDefinitor, SimplePresentationElement RoleVariant)
             : base()
        {
            this.OwnerRelationship = RelationshipOwner;
            this.AssociatedIdea = AssociatedIdea;
            this.RoleDefinitor = RoleDefinitor;
            this.RoleVariant = RoleVariant;

            this.AssociatedIdea.AssociatingLinks.Add(this);
        }

        /// <summary>
        /// References the owning Relationship.
        /// </summary>
        public Relationship OwnerRelationship { get { return __OwnerRelationship.Get(this); } set { __OwnerRelationship.Set(this, value); } }
        protected Relationship OwnerRelationship_ = null;
        public static readonly ModelPropertyDefinitor<RoleBasedLink, Relationship> __OwnerRelationship =
                   new ModelPropertyDefinitor<RoleBasedLink, Relationship>("OwnerRelationship", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerRelationship_, (ins, val) => ins.OwnerRelationship_ = val, false, false,
                                                                           "Owner Relationship", "References the owning Relationship.");

        /// <summary>
        /// Optional description of this link.
        /// </summary>
        public SimplePresentationElement Descriptor { get { return __Descriptor.Get(this); } set { __Descriptor.Set(this, value); } }
        protected SimplePresentationElement Descriptor_ = null;
        public static readonly ModelPropertyDefinitor<RoleBasedLink, SimplePresentationElement> __Descriptor =
                   new ModelPropertyDefinitor<RoleBasedLink, SimplePresentationElement>("Descriptor", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.Descriptor_, (ins, val) => ins.Descriptor_ = val, true, false,
                                                                                        "Descriptor", "Optional description of this link.");

        /// <summary>
        /// References the associated Idea of this link.
        /// </summary>
        public Idea AssociatedIdea { get { return __AssociatedIdea.Get(this); } set { __AssociatedIdea.Set(this, value); } }
        protected Idea AssociatedIdea_ = null;
        public static readonly ModelPropertyDefinitor<RoleBasedLink, Idea> __AssociatedIdea =
                   new ModelPropertyDefinitor<RoleBasedLink, Idea>("AssociatedIdea", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AssociatedIdea_, (ins, val) => ins.AssociatedIdea_ = val, true, false,
                                                                   "Associated Idea", "References the associated Idea of this link.");

        /// <summary>
        /// Related Link-Role Definition.
        /// </summary>
        public LinkRoleDefinition RoleDefinitor { get { return __RoleDefinitor.Get(this); } set { __RoleDefinitor.Set(this, value); } }
        protected LinkRoleDefinition RoleDefinitor_ = null;
        public static readonly ModelPropertyDefinitor<RoleBasedLink, LinkRoleDefinition> __RoleDefinitor =
                   new ModelPropertyDefinitor<RoleBasedLink, LinkRoleDefinition>("RoleDefinitor", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.RoleDefinitor_, (ins, val) => ins.RoleDefinitor_ = val, true, false,
                                                                                 "Role Definitor", "Related Link-Role Definition.");

        /// <summary>
        /// Indicates the Link-Role Variant for this link.
        /// </summary>
        public SimplePresentationElement RoleVariant { get { return __RoleVariant.Get(this); } set { __RoleVariant.Set(this, value); } }
        protected SimplePresentationElement RoleVariant_ = null;
        public static readonly ModelPropertyDefinitor<RoleBasedLink, SimplePresentationElement> __RoleVariant =
                   new ModelPropertyDefinitor<RoleBasedLink, SimplePresentationElement>("RoleVariant", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RoleVariant_, (ins, val) => ins.RoleVariant_ = val, false, false,
                                                                                        "Role Variant", "Indicates the Link-Role Variant for this link.");

        /// <summary>
        /// References the optional subpoints linked.
        /// </summary>
        //? public List<SubPoint> SubOrigins { get; protected set; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<RoleBasedLink> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<RoleBasedLink> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<RoleBasedLink> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new RoleBasedLink CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((RoleBasedLink)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public RoleBasedLink PopulateFrom(RoleBasedLink SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            var OwnerRel = (this.OwnerRelationship == null ? "<Empty>" : this.OwnerRelationship.Name);
            var AssocIdea = (this.AssociatedIdea == null ? "<Empty>" : this.AssociatedIdea.Name);
            var RoleDef = (this.RoleDefinitor == null ? "<Empty>" : this.RoleDefinitor.Name);

            return "Role-Based Link of Relationship '" + OwnerRel + "' for Idea '" + AssocIdea + "' (Def.: '" + RoleDef + "')";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public bool DoEditDescriptor(Action<RoleBasedLink> SuccessCompletion, bool FinishSuccessfulCommand = true)
        {
            this.EditEngine.StartCommandVariation("Edit Link-Role Descriptor");

            this.Descriptor = this.Descriptor.NullDefault(new SimplePresentationElement("", ""));
            var InstanceController = EntityInstanceController.AssignInstanceController(this.Descriptor);
            InstanceController.StartEdit();

            var EditPanel = Display.CreateEditPanel(this.Descriptor, null, true, null, null, true, false, false, true);

            var Result = InstanceController.Edit(EditPanel, "Edit Link descriptor.").IsTrue();
            if (!Result)
            {
                this.EditEngine.CompleteCommandVariation();
                this.EditEngine.Undo();
                return false;
            }

            if (Descriptor.Name.IsAbsent() && this.Descriptor.TechName.IsAbsent() && this.Descriptor.Summary.IsAbsent())
                this.Descriptor = null;

            this.UpdateVersion();

            if (SuccessCompletion != null)
                SuccessCompletion(this);

            if (FinishSuccessfulCommand)
                this.EditEngine.CompleteCommandVariation();

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void UpdateVersion()
        {
            if (this.OwnerRelationship != null)
                this.OwnerRelationship.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}