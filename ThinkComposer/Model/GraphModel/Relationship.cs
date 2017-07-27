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
// File   : Relationship.cs
// Object : Instrumind.ThinkComposer.Model.GraphModel.Relationship (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.11 Néstor Sánchez A.  Start
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.GraphModel
{
    /// <summary>
    /// Association between multiple ideas, connected using link-roles, forming a nexus.
    /// </summary>
    [Serializable]
    public class Relationship : Idea, IModelEntity, IModelClass<Relationship>
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Relationship()
        {
            __ClassDefinitor = new ModelClassDefinitor<Relationship>("Relationship", Idea.__ClassDefinitor, "Relationship",
                                                                     "Association between multiple ideas, connected using link-roles, forming a nexus.");
            __ClassDefinitor.DeclareProperty(__RelationshipDefinitor);
            __ClassDefinitor.DeclareCollection(__Links);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="OwnerComposition">Composition owning this Relationship.</param>
        /// <param name="Definitor">Definitor of the Relationship.</param>
        /// <param name="Name">Name of the Relationship.</param>
        /// <param name="TechName">Technical Name of the Relationship.</param>
        /// <param name="Summary">Summary of the Relationship.</param>
        /// <param name="Pictogram">Image representing the Relationship.</param>
        public Relationship(Composition OwnerComposition, RelationshipDefinition Definitor,
                            string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
            : base(OwnerComposition, Name, TechName, Summary, Pictogram)
        {
            this.Links = new EditableList<RoleBasedLink>(__Links.TechName, this);

            this.RelationshipDefinitor = Definitor.Assign();

            this.CompositeContentDomain = Definitor.CompositeContentDomain ?? Definitor.OwnerDomain;

            this.OwnerComposition.UsedDomains.AddNew(Definitor.OwnerDomain);
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected Relationship()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// RelationshipDefinition definitor of this Relationship.
        /// </summary>
        public Assignment<RelationshipDefinition> RelationshipDefinitor { get { return __RelationshipDefinitor.Get(this); } set { __RelationshipDefinitor.Set(this, value); } }
        protected Assignment<RelationshipDefinition> RelationshipDefinitor_;
        public static readonly ModelPropertyDefinitor<Relationship, Assignment<RelationshipDefinition>> __RelationshipDefinitor =
                   new ModelPropertyDefinitor<Relationship, Assignment<RelationshipDefinition>>("RelationshipDefinitor", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.RelationshipDefinitor_, (ins, val) => ins.RelationshipDefinitor_ = val, false, true,
                                                                                                "Relationship Definition", "Relationship Definition on which this Relationship is based.");

        public override IdeaDefinition IdeaDefinitor { get { return (this.RelationshipDefinitor == null ? null : this.RelationshipDefinitor.Value); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of implemented Links.
        /// </summary>
        public EditableList<RoleBasedLink> Links { get; protected set; }
        public static ModelListDefinitor<Relationship, RoleBasedLink> __Links =
                   new ModelListDefinitor<Relationship, RoleBasedLink>("Links", EEntityMembership.InternalCoreExclusive, ins => ins.Links, (ins, coll) => ins.Links = coll, "Links", "Collection of implemented Links.");

        /// <summary>
        /// Links associating the origin (or participant) Ideas
        /// </summary>
        [Description("Links associating the origin (or participant) Ideas")]
        public IEnumerable<RoleBasedLink> OriginLinks { get { return this.Links.Where(link => link.RoleDefinitor.RoleType == ERoleType.Origin);  } }

        /// <summary>
        /// Links associating the target Ideas.
        /// </summary>
        [Description("Links associating the target Ideas.")]
        public IEnumerable<RoleBasedLink> TargetLinks { get { return this.Links.Where(link => link.RoleDefinitor.RoleType == ERoleType.Target); } }

        public void AddLink(RoleBasedLink Reference)
        {
            this.Links.Add(Reference);

            this.NotifyPropertyChange("DescriptiveCaption");
        }

        public void RemoveLink(RoleBasedLink Reference)
        {
            this.Links.Remove(Reference);
            Reference.AssociatedIdea.AssociatingLinks.Remove(Reference);

            this.NotifyPropertyChange("DescriptiveCaption");
        }

        /// <summary>
        /// Gets the Ideas from which this Relationship is Originted (includes Participants).
        /// </summary>
        [Description("Gets the Ideas from which this Relationship is Originted (includes Participants).")]
        public IEnumerable<Idea> OriginIdeas
        { get{ return this.OriginLinks.Select(link => link.AssociatedIdea); } }

        /// <summary>
        /// Gets the Ideas to which this Relationship is Targeted.
        /// </summary>
        [Description("Gets the Ideas to which this Relationship is Targeted.")]
        public IEnumerable<Idea> TargetIdeas
        { get{ return this.TargetLinks.Select(link => link.AssociatedIdea); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this represents an auto-reference for the connected Idea (non-excluvise).
        /// This means that can exists links pointing from/to another Ideas.
        /// </summary>
        [Description("Indicates whether this represents an auto-reference for the connected Idea (non-excluvise). " +
                     "This means that can exists links pointing from/to another Ideas.")]
        public bool IsAutoReference
        { get{ return this.OriginIdeas.Where(origin => this.TargetIdeas.Where(target => target == origin).Any()).Any(); } }

        /// <summary>
        /// Indicates whether this represents an exclusive auto-reference for the connected Idea.
        /// This means that all links points from/to the same Idea.
        /// </summary>
        [Description("Indicates whether this represents an exclusive auto-reference for the connected Idea. " +
                     "This means that all links points from/to the same Idea.")]
        public bool IsAutoReferenceExclusive
        { get { return this.OriginIdeas.All(origin => this.TargetIdeas.All(target => target == origin)); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets specialized (Idea descendant) content as text synopsis.
        /// </summary>
        public override string GetSpecializedTextSynopsis()
        {
            var Describer = new StringBuilder(base.GetSpecializedTextSynopsis());

            if (Links != null && Links.Count > 0)
            {
                Describer.AppendLine("Links...");
                foreach (var Link in this.Links)
                    Describer.AppendLine(SYNOP_SEPARATOR + "GlobalId" + SYNOP_SEPARATOR + Link.GlobalId.ToString() + SYNOP_SEPARATOR
                                                         + "Role-Definitor" + SYNOP_SEPARATOR + Link.RoleDefinitor.Name + SYNOP_SEPARATOR
                                                         + "Role-Variant" + SYNOP_SEPARATOR + Link.RoleVariant.ToString() + SYNOP_SEPARATOR
                                                         + "Associated-Idea" + SYNOP_SEPARATOR + Link.AssociatedIdea.ToString());
            }

            return Describer.ToString();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Relationship> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Relationship> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Relationship> __ClassDefinitor = null;

        public new Relationship CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Relationship)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Relationship PopulateFrom(Relationship SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Short text describing Relationship links.
        /// </summary>
        [Description("Short text describing Relationship links.")]
        public override string DescriptiveCaption
        {
            get
            {
                var OriginNames = this.OriginIdeas.Take(4).Select(idea => "`" + idea.Name + "`").ToArray();
                if (OriginNames.Length > 3)
                    OriginNames[2] = "...";

                var TargetNames = this.TargetIdeas.Take(4).Select(idea => "`" + idea.Name + "`").ToArray();
                if (TargetNames.Length > 3)
                    TargetNames[2] = "...";

                var Result = (this.RelationshipDefinitor.Value.IsDirectional
                              ? (OriginNames.Length > 0 || TargetNames.Length > 0
                                 ? ("[" + OriginNames.Take(3).GetConcatenation(null, ", ") + "] - " +
                                    "[" + TargetNames.Take(3).GetConcatenation(null, ", ") + "]")
                                 : "")
                              : "{" + OriginNames.Take(3).GetConcatenation(null, ", ") + "}")
                             .Replace("\n", " ").Replace("\r", " ");

                /*T if (OriginNames.Length < 1 && TargetNames.Length < 1 || this.Name.StartsWith("Membership") || Result.IsAbsent())
                    Console.WriteLine(); */

                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public Relationship GenerateIndependentRelationshipDuplicate()
        {
            // IMPORTANT: As this is used for copy+paste, Links cannot be duplicated here
            // because new (cloned) associated Ideas could still not exist.
            // Later these area created by the CompositionEngine.ReappointReferences method.

            var Result = (Relationship)this.GenerateIdeaIndependentDuplicate();
            Result.Links.Clear();

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Removes this relationship from its composite container.
        /// Optionally, indidcations for preserve non-sense relationships (those without both, sources and targets)
        /// and for preserve rich relationships (those with Composite-Content or Details) can be specified.
        /// </summary>
        public override void RemoveFromComposite(bool PreserveNonsenseRelationships = false, bool PreserveRichRelationships = false)
        {
            var Linkings = this.Links.ToArray();
            foreach (var Link in Linkings)
                this.RemoveLink(Link);

            base.RemoveFromComposite(PreserveNonsenseRelationships, PreserveRichRelationships);
        }

        /// <summary>
        /// Deletes the association implemented by this relationship to the Associated idea.
        /// Returns indication of deletion of relationship.
        /// Optionally, indidcations for preserve non-sense relationships (those without both, sources and targets)
        /// and for preserve rich relationships (those with Composite-Content or Details) can be specified.
        /// </summary>
        public bool DeleteRelationshipAssociation(Idea Associated, bool PreserveNonsenseRelationship = false, bool PreserveRichRelationship = false)
        {
            var LinksToDelete = this.Links.Where(link => link.AssociatedIdea == Associated).ToList();

            foreach (var AssocLink in LinksToDelete)
            {
                foreach (var Representator in AssocLink.OwnerRelationship.VisualRepresentators)
                {
                    // Determine visual connectors representating associating Links
                    var Connectors = Representator.VisualParts.CastAs<VisualConnector, VisualElement>()
                                                .Where(conn => conn.RepresentedLink.IsEqual(AssocLink))
                                                    .ToList();

                    // Remove visual connectors
                    foreach (var Connector in Connectors)
                    {
                        Connector.ClearElement();
                        Connector.Disconnect(); // Also removes the semantic link
                        Representator.VisualParts.Remove(Connector);
                    }
                }
            }

            // Do not delete nonsense relationship if required
            if (PreserveNonsenseRelationship
                || (this.OriginIdeas.Any() && this.TargetIdeas.Any()))
                return false;

            // Do not delete relationship with rich content if required
            if (PreserveRichRelationship
                && (this.IsComposite || this.HasDetailedContent))
                return false;

            /* Do not delete non simple relationships
            if (!Relation.RelationshipDefinitor.Value.IsSimple)
                return false; */

            // Delete the nonsense relationship
            this.RemoveFromComposite();

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Changes the underlying definition to the specified one and propagate the necessary changes.
        /// </summary>
        public void ApplyRelationshipDefinitionChange(RelationshipDefinition NewDefinition)
        {
            var PreviousDefinitor = this.RelationshipDefinitor.Value;
            if (PreviousDefinitor.IsEqual(NewDefinition))
                return;

            this.RelationshipDefinitor = NewDefinition.Assign();

            foreach (var Linking in this.Links)
            {
                if (Linking.RoleDefinitor.RoleType == ERoleType.Origin)
                    Linking.RoleDefinitor = NewDefinition.OriginOrParticipantLinkRoleDef;
                else
                    Linking.RoleDefinitor = NewDefinition.TargetLinkRoleDef;

                Linking.RoleVariant = Linking.RoleDefinitor.AllowedVariants.FirstOrDefault();
            }

            this.ApplyIdeaDefinitionChange(PreviousDefinitor);  // Must be last, because updates graphics
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}