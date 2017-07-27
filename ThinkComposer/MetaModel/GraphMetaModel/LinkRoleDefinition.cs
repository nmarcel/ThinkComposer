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
// File   : LinkRoleDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.LinkRoleDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.16 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Represents the definition of a Role Based Link.
    /// </summary>
    [Serializable]
    public class LinkRoleDefinition : MetaDefinition, IModelEntity, IModelClass<LinkRoleDefinition>, IVersionUpdater
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static LinkRoleDefinition()
        {
            __ClassDefinitor = new ModelClassDefinitor<LinkRoleDefinition>("LinkRoleDefinition", MetaDefinition.__ClassDefinitor, "Link Role Definition",
                                                                           "Represents the definition of a Role Link.");
            __ClassDefinitor.DeclareCollection(__AllowedVariants);
            __ClassDefinitor.DeclareCollection(__AssociableIdeaDefs);

            __ClassDefinitor.DeclareProperty(__OwnerRelationshipDef);
            __ClassDefinitor.DeclareProperty(__RoleType);
            __ClassDefinitor.DeclareProperty(__MaxConnections);
            __ClassDefinitor.DeclareProperty(__RelatedIdeasAreOrdered);

            __AllowedVariants.ItemsSourceGetter = ((ctx) =>
                                                    {
                                                        if (ctx == null || ctx.EditEngine == null)
                                                            return null;

                                                        return ((CompositionEngine)ctx.EditEngine).TargetComposition.CompositeContentDomain.LinkRoleVariants;
                                                    });

            __AssociableIdeaDefs.CanCollectionBeEmpty = true;
            __AssociableIdeaDefs.EmptyCollectionTitle = "All Idea Definitions are linkable.";
            __AssociableIdeaDefs.ItemsSourceGetter = ((ctx) =>
                                                        {
                                                            if (ctx == null || ctx.EditEngine == null)
                                                                return null;

                                                            return ((CompositionEngine)ctx.EditEngine).TargetComposition.CompositeContentDomain.Definitions;
                                                        });

            LinkRoleTypes = Enum.GetValues(typeof(ERoleType));
            __RoleType.ItemsSourceGetter = ((ctx) => LinkRoleDefinition.LinkRoleTypes);

            /* Alternative.... Plus: A generic converter must be used in order to bind properly,
              LinkRoleTypes = General.GetEnumMembers<ERoleType>().ToArray();
            __RoleType.ItemsSourceGetter = ((ctx, editor) => LinkRoleDefinition.LinkRoleTypes);
            __RoleType.ItemsSourceSelectedValuePath = "Item1";
            __RoleType.ItemsSourceDisplayMemberPath = "Item2"; */
        }

        public static readonly Array LinkRoleTypes = null;
        //Alt.: public static readonly IEnumerable<Tuple<ERoleType, string, string>> LinkRoleTypes = null;

        /// <summary>
        /// Constructor for a previously existing Relationship Definition.
        /// </summary>
        /// <param name="OwnerRelationshipDef">Relationship Definition owning this link-role def.</param>
        /// <param name="RoleType">Type of the relationship link-role definition.</param>
        /// <param name="Name">Name of the link-role definition.</param>
        /// <param name="TechName">Technical Name of the link-role definition.</param>
        /// <param name="Summary">Summary of the link-role definition.</param>
        /// <param name="Pictogram">Image representing the link-role definition.</param>
        /// <param name="AllowedVariants">Allowed link-role variants of the link-role definition.</param>
        public LinkRoleDefinition(RelationshipDefinition OwnerRelationshipDef, ERoleType RoleType, string Name, string TechName, string Summary = "", ImageSource Pictogram = null,
                                  params SimplePresentationElement[] AllowedVariants)
            : base(Name, TechName, Summary, Pictogram)
        {
            this.OwnerRelationshipDef = OwnerRelationshipDef;   // Can be null
            this.RoleType = RoleType;

            this.AllowedVariants = new EditableList<SimplePresentationElement>(__AllowedVariants.TechName, this);
            if (AllowedVariants != null && AllowedVariants.Length > 0)
                this.AllowedVariants.AddRange(AllowedVariants);

            /*T this.AllowedVariants.CollectionChanged += ((sdr, args) =>
                {
                    if (!args.Action.IsOneOf(System.Collections.Specialized.NotifyCollectionChangedAction.Remove,
                                             System.Collections.Specialized.NotifyCollectionChangedAction.Reset))
                        return;

                    Console.WriteLine("Deleting Variant.");
                }); */

            this.PropertyChanged += ((sender, args) =>
                {
                    if (args.PropertyName != __OwnerRelationshipDef.TechName || this.OwnerRelationshipDef == null
                        || this.AllowedVariants.Count > 0)
                        return;

                    var Variant = this.OwnerRelationshipDef.OwnerDomain.LinkRoleVariants.FirstOrDefault();
                    if (Variant != null)
                        this.AllowedVariants.Add(Variant);
                });

            this.AssociableIdeaDefs = new EditableList<IdeaDefinition>(__AssociableIdeaDefs.TechName, this);
        }

        /// <summary>
        /// Constructor for a not-created-yet Relationship Definition.
        /// </summary>
        /// <param name="RoleType">Type of the relationship link-role definition.</param>
        /// <param name="Name">Name of the link-role definition.</param>
        /// <param name="TechName">Technical Name of the link-role definition.</param>
        /// <param name="Summary">Summary of the link-role definition.</param>
        /// <param name="Pictogram">Image representing the link-role definition.</param>
        /// <param name="AllowedVariants">Allowed link-role variants of the link-role definition.</param>
        public LinkRoleDefinition(ERoleType RoleType, string Name, string TechName, string Summary = "", ImageSource Pictogram = null,
                                  params SimplePresentationElement[] AllowedVariants)
            : this(null, RoleType, Name, TechName, Summary, Pictogram, AllowedVariants)
        {
        }

        /// <summary>
        /// Internal Constructor for Agents and Cloning.
        /// </summary>
        internal LinkRoleDefinition()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Relationship definition owning this link role definition.
        /// </summary>
        public RelationshipDefinition OwnerRelationshipDef { get { return __OwnerRelationshipDef.Get(this); } set { __OwnerRelationshipDef.Set(this, value); } }
        protected RelationshipDefinition OwnerRelationshipDef_;
        public static readonly ModelPropertyDefinitor<LinkRoleDefinition, RelationshipDefinition> __OwnerRelationshipDef =
                   new ModelPropertyDefinitor<LinkRoleDefinition, RelationshipDefinition>("OwnerRelationshipDef", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerRelationshipDef_, (ins, val) => ins.OwnerRelationshipDef_ = val, false, true,
                                                                                          "Owner Relationship Definition", "Relationship definition owning this link role definition.");

        /// <summary>
        /// Type of the relationship link role definition.
        /// </summary>
        public ERoleType RoleType { get { return __RoleType.Get(this); } set { __RoleType.Set(this, value); } }
        protected ERoleType RoleType_;
        public static readonly ModelPropertyDefinitor<LinkRoleDefinition, ERoleType> __RoleType =
                   new ModelPropertyDefinitor<LinkRoleDefinition, ERoleType>("RoleType", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RoleType_, (ins, val) => ins.RoleType_ = val, true, false,
                                                                             "Role Type", "Type of the relationship link role definition.");

        /// <summary>
        /// Number of maximum Ideas that can be linked by the role.
        /// Zero for unlimited. The default is one.
        /// </summary>
        public uint MaxConnections { get { return __MaxConnections.Get(this); } set { __MaxConnections.Set(this, value); } }
        protected uint MaxConnections_ = 1;
        public static readonly ModelPropertyDefinitor<LinkRoleDefinition, uint> __MaxConnections =
                   new ModelPropertyDefinitor<LinkRoleDefinition, uint>("MaxConnections", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MaxConnections_, (ins, val) => ins.MaxConnections_ = val, true, false,
                                                                        "Maximum Connections", "Number of maximum Ideas that can be linked by the role. Zero for unlimited. The default is one.");

        /// <summary>
        /// Set of rules for defining visual connector format.
        /// </summary>
        //? public VisualConnectorFormatDefinitor ConnectorFormatDefinitor = null;

        /// <summary>
        /// Pictogram representing this Idea Definition.
        /// If no image is stored, then a generated sample-drawing is returned.
        /// </summary>
        public override ImageSource Pictogram
        {
            get
            {
                var Result = base.Pictogram;
                if (Result == null)
                    Result = new DrawingImage(MasterDrawer.CreateDrawingSample(this));
                return Result;
            }
            set { base.Pictogram = value; }
        }

        /// <summary>
        /// Allowed link-role variants and related plug style for the relationship link role.
        /// </summary>
        public EditableList<SimplePresentationElement> AllowedVariants { get; protected set; }
        public static ModelListDefinitor<LinkRoleDefinition, SimplePresentationElement> __AllowedVariants =
                   new ModelListDefinitor<LinkRoleDefinition, SimplePresentationElement>("AllowedVariants", EEntityMembership.External, ins => ins.AllowedVariants, (ins, coll) => ins.AllowedVariants = coll,
                                                                                         "Allowed Variants", "Allowed link-role variants and related plug style for the relationship link role.");

        /// <summary>
        /// Indicates whether the related Ideas for the relationship link role are free or follows an order.
        /// </summary>
        public bool RelatedIdeasAreOrdered { get { return __RelatedIdeasAreOrdered.Get(this); } set { __RelatedIdeasAreOrdered.Set(this, value); } }
        protected bool RelatedIdeasAreOrdered_;
        public static readonly ModelPropertyDefinitor<LinkRoleDefinition, bool> __RelatedIdeasAreOrdered =
                   new ModelPropertyDefinitor<LinkRoleDefinition, bool>("RelatedIdeasAreOrdered", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RelatedIdeasAreOrdered_, (ins, val) => ins.RelatedIdeasAreOrdered_ = val, true, false,
                                                                        "Related Ideas Are Ordered", "Indicates whether the related Ideas for the relationship link role are free or follows an order.");

        /// <summary>
        /// List of associable ideas by the link roles defined.
        /// If empty, then indicates linking to any Idea type.
        /// </summary>
        public EditableList<IdeaDefinition> AssociableIdeaDefs { get; protected set; }
        public static ModelListDefinitor<LinkRoleDefinition, IdeaDefinition> __AssociableIdeaDefs =
                   new ModelListDefinitor<LinkRoleDefinition, IdeaDefinition>("AssociableIdeaDefs", EEntityMembership.External, ins => ins.AssociableIdeaDefs, (ins, coll) => ins.AssociableIdeaDefs = coll,
                                                                              "Linkable Idea Definitions", "List of linkable idea definitions. If none is assigned, then all can be linked.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<LinkRoleDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<LinkRoleDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<LinkRoleDefinition> __ClassDefinitor = null;

        public new LinkRoleDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((LinkRoleDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public LinkRoleDefinition PopulateFrom(LinkRoleDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void UpdateVersion()
        {
            if (this.Version != null)
                this.Version.Update();

            if (this.OwnerRelationshipDef != null)
                this.OwnerRelationshipDef.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}