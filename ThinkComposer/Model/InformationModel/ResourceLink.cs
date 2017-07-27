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
// File   : ResourceLink.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.ResourceLink (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.02.09 Néstor Sánchez A.  Creation
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
    /// References a resource, such as a file, folder or web address.
    /// </summary>
    [Serializable]
    public class ResourceLink : Link, IModelEntity, IModelClass<ResourceLink>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static ResourceLink()
        {
            __ClassDefinitor = new ModelClassDefinitor<ResourceLink>("ResourceLink", Link.__ClassDefinitor, "Resource Link",
                                                                     "References a resource, such as a file, folder or web address.");
            __ClassDefinitor.DeclareProperty(__AssignedDesignator);
            __ClassDefinitor.DeclareProperty(__TargetLocation);
            //? __ClassDefinitor.DeclareProperty(__IsRelative);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ResourceLink(Idea OwnerContainer, Assignment<DetailDesignator>/*L*/ Designator)
             : base(OwnerContainer)
        {
            this.AssignedDesignator = Designator;
        }

        /// <summary>
        /// Reseource-Link designator.
        /// </summary>
        public override Assignment<DetailDesignator> AssignedDesignator { get { return __AssignedDesignator.Get(this); } set { __AssignedDesignator.Set(this, value); } }
        protected Assignment<DetailDesignator> AssignedDesignator_;
        public static readonly ModelPropertyDefinitor<ResourceLink, Assignment<DetailDesignator>> __AssignedDesignator =
                  new ModelPropertyDefinitor<ResourceLink, Assignment<DetailDesignator>>("AssignedDesignator", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AssignedDesignator_, (ins, val) => ins.AssignedDesignator_ = val, false, true,
                                                                                         "Assigned Designator", "Resource Link content designator");
        public override MAssignment ContentDesignator { get { return this.AssignedDesignator; } }
        [Description("Reseource-Link designator.")]
        public LinkDetailDesignator Designator { get { return (LinkDetailDesignator)this.ContentDesignator.AssignedValue; } }

        /// <summary>
        /// Returns the underlying reference.
        /// </summary>
        public override object Target { get { return this.TargetLocation; } }

        /// <summary>
        /// Gets the address of the resource.
        /// </summary>
        [Description("Address of the resource.")]
        public override string TargetAddress { get { return this.TargetLocation; } }

        /// <summary>
        /// Location-of/route-to the resource.
        /// </summary>
        public string TargetLocation { get { return __TargetLocation.Get(this); } set { __TargetLocation.Set(this, value); } }
        protected string TargetLocation_;
        public static readonly ModelPropertyDefinitor<ResourceLink, string> __TargetLocation =
                   new ModelPropertyDefinitor<ResourceLink, string>("TargetLocation", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.TargetLocation_, (ins, val) => ins.TargetLocation_ = val, false, false,
                                                                    "Target Location", "Location-of/route-to the resource.");

        /* PENDING: Determine how to restrict selection to only Composition's subdir related files, else how to relativize?
        /// <summary>
        /// Indicates that the target location is relative to the current Composition location.
        /// </summary>
        public bool IsRelative { get { return __IsRelative.Get(this); } set { __IsRelative.Set(this, value); } }
        protected bool IsRelative_ = false;
        public static readonly ModelPropertyDefinitor<ResourceLink, bool> __IsRelative =
                   new ModelPropertyDefinitor<ResourceLink, bool>("IsRelative", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsRelative_, (ins, val) => ins.IsRelative_ = val, false, false,
                                                                  "Is Relative", "Indicates that the target location is relative to the current Composition location."); */

        /// <summary>
        /// Updates the related Designator identification information based on the Content.
        /// </summary>
        public override void UpdateDesignatorIdentification()
        {
            if (this.AssignedDesignator == null)
                return;

            this.Designation.TechName = this.TargetLocation;
            this.Designation.Name = this.TargetLocation.GetSimplifiedResourceName();
        }

        public override string ToString()
        {
            return ("Detail-Type" + Idea.SYNOP_SEPARATOR + "Link(Resource)" + Idea.SYNOP_SEPARATOR +
                    "Target-Location" + Idea.SYNOP_SEPARATOR + this.TargetLocation.NullDefault(General.STR_NULLREF));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<ResourceLink> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<ResourceLink> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<ResourceLink> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new ResourceLink CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((ResourceLink)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public ResourceLink PopulateFrom(ResourceLink SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion
    }
}