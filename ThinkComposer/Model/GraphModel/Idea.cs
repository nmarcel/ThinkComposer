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
// File   : Idea.cs
// Object : Instrumind.ThinkComposer.Model.GraphModel.Idea (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Start
//

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using DotLiquid;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Base abstractions for the conformation of the Graph schema
namespace Instrumind.ThinkComposer.Model.GraphModel
{
    /// <summary>
    /// Idea is the most basic Composition element of the Instrumind's Graph schema.
    /// Combines on a single -derived- instance the attributes of Graph existence, visual representation and information storage.
    /// </summary>
    [Serializable]
    public abstract class Idea : FormalPresentationElement, IDefined, IModelEntity, IModelClass<Idea>,
                                 IVersionUpdater, IRecognizableComposite, IIndexable
    {
        public const string TOKEN_ROUTE_ROOT = "\\";

        public const string TOKEN_ROUTE_NODE_SEP = "\\";

        public const string TOKEN_ROUTE_NODE_PART_SEP = "|";

        public const string MAIN_VIEW_PREFIX = "Main";

        //- public static string IdeaDefinitorFieldTechName = "Definition";
        //- public static string IdeaDefinitorFieldName = "Definition";

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Idea()
        {
            __ClassDefinitor = new ModelClassDefinitor<Idea>("Idea", FormalPresentationElement.__ClassDefinitor, "Idea",
                                                             "The most basic Composition element of the Instrumind's Graph schema, from which Concepts and Relationships are descendants. " +
                                                             "Combines on a single -derived- instance the attributes of Graph existence, visual representation and information storage.");

            // IMPORTANT: Propagate changes about these memebers to the Copy creators
            //       SEE: ConceptCreationCommand.CreateConceptCopy and RelationshipCreationCommand.CreateRelationshipCopy

            __ClassDefinitor.DeclareCollection(__VisualRepresentators);
            __ClassDefinitor.DeclareCollection(__AssociatingLinks);
            __ClassDefinitor.DeclareCollection(__CompositeViews);
            __ClassDefinitor.DeclareCollection(__CompositeIdeas);
            __ClassDefinitor.DeclareCollection(__Details);
            __ClassDefinitor.DeclareCollection(__DetailsCustomLooks);
            __ClassDefinitor.DeclareCollection(__Markings);

            __ClassDefinitor.DeclareProperty(__OwnerComposition);
            __ClassDefinitor.DeclareProperty(__OwnerContainer);
            __ClassDefinitor.DeclareProperty(__DefinitionIsShared);
            __ClassDefinitor.DeclareProperty(__CompositeContentDomain);
            __ClassDefinitor.DeclareProperty(__CompositeActiveView);

            /* POSTPONED:
            __ClassDefinitor.DeclareProperty(__CompositeMainIdea);
            __ClassDefinitor.DeclareProperty(__DetailsMain); */
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="OwnerComposition">Composition owning this Idea.</param>
        /// <param name="Name">Name of the Idea.</param>
        /// <param name="TechName">Technical Name of the Idea.</param>
        /// <param name="Summary">Summary of the Idea.</param>
        /// <param name="Pictogram">Image representing the Idea.</param>
        public Idea(Composition OwnerComposition, string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(Name, TechName, Summary, Pictogram)
        {
            this.VisualRepresentators = new EditableList<VisualRepresentation>(__VisualRepresentators.TechName, this);
            this.AssociatingLinks = new EditableList<RoleBasedLink>(__AssociatingLinks.TechName, this);

            this.OwnerComposition = (this is Composition ? (Composition)this : OwnerComposition);

            this.CompositeViews = new EditableList<View>(__CompositeViews.TechName, this);
            this.CompositeIdeas = new EditableList<Idea>(__CompositeIdeas.TechName, this);

            this.Details = new EditableList<ContainedDetail>(__Details.TechName, this);
            this.DetailsCustomLooks = new EditableDictionary<DetailDesignator, EditableDictionary<VisualSymbol, DetailAppearance>>(__DetailsCustomLooks.TechName, this);

            this.Markings = new EditableList<MarkerAssignment>(__Markings.TechName, this);
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected Idea()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Short text describing the Idea.
        /// </summary>
        [Description("Short text describing the Idea.")]
        public virtual string DescriptiveCaption { get { return null; } }

        /// <summary>
        /// Composition owning this Idea.
        /// Could be the Composition itself.
        /// </summary>
        public Composition OwnerComposition { get { return __OwnerComposition.Get(this); } protected set { __OwnerComposition.Set(this, value); } }
        protected Composition OwnerComposition_ = null;
        public static readonly ModelPropertyDefinitor<Idea, Composition> __OwnerComposition =
                   new ModelPropertyDefinitor<Idea, Composition>("OwnerComposition", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerComposition_, (ins, val) => ins.OwnerComposition_ = val, false, false,
                                                                 "Owner Composition", "Composition owning this Idea.");

        /// <summary>
        /// Gets the related Composition document.
        /// Almost the same as OwnerComposition, but returning "this" for the Composition itself.
        /// </summary>
        public ISphereModel RelatedDocument { get { return (Composition)OwnerComposition; } }

        /// <summary>
        /// Gets the definition of this Idea from its descendant specialization.
        /// </summary>
        public abstract IdeaDefinition IdeaDefinitor { get; }

        /// <summary>
        /// Indicates whether the Idea is currently selected for editing.
        /// </summary>
        public bool IsSelected
        {
            get { return this.IsSelected_; }
            set
            {
                if (this.IsSelected_ == value)
                    return;

                this.IsSelected_ = value;
                this.NotifyPropertyChange("IsSelected");
            }
        }
        [NonSerialized]
        internal bool IsSelected_ = false;

        /// <summary>
        /// Indicates whether the Idea is currently vanished for deletion.
        /// </summary>
        public bool IsVanished
        {
            get { return this.IsVanished_; }
            set
            {
                if (this.IsVanished_ == value)
                    return;

                this.IsVanished_ = value;
                this.NotifyPropertyChange("IsVanished");
            }
        }
        [NonSerialized]
        internal bool IsVanished_ = false;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the assigned Idea definition is Shared (owned by its Domain), or Local (owned by this Idea).
        /// </summary>
        public bool DefinitionIsShared { get { return __DefinitionIsShared.Get(this); } set { __DefinitionIsShared.Set(this, value); } }
        protected bool DefinitionIsShared_ = true;
        public static readonly ModelPropertyDefinitor<Idea, bool> __DefinitionIsShared =
                   new ModelPropertyDefinitor<Idea, bool>("DefinitionIsShared", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DefinitionIsShared_, (ins, val) => ins.DefinitionIsShared_ = val, false, true,
                                                          "Definition Is Shared", "Indicates whether the assigned Idea definition is Shared (owned by its Domain), or Local (owned by this Idea).");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Container directly owning this Idea, which is composing a dominant one.
        /// </summary>
        public Idea OwnerContainer { get { return __OwnerContainer.Get(this); } set { __OwnerContainer.Set(this, value); } }
        protected Idea OwnerContainer_;
        public static readonly ModelPropertyDefinitor<Idea, Idea> __OwnerContainer =
                   new ModelPropertyDefinitor<Idea, Idea>("OwnerContainer", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerContainer_, (ins, val) => ins.OwnerContainer_ = val, false, true,
                                                          "Owner Container", "Container owning this Idea, which is composing a dominant one.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of visual representations for this Idea.
        /// </summary>
        public EditableList<VisualRepresentation> VisualRepresentators { get; protected set; }
        public static ModelListDefinitor<Idea, VisualRepresentation> __VisualRepresentators =
                   new ModelListDefinitor<Idea, VisualRepresentation>("VisualRepresentators", EEntityMembership.InternalBulk, ins => ins.VisualRepresentators, (ins, coll) => ins.VisualRepresentators = coll,
                                                                      "Visual Representators", "Collection of visual representations for this Idea.");

        /// <summary>
        /// Gets the primary Visual Representator of this Idea.
        /// </summary>
        [Description("Gets the primary Visual Representator of this Idea.")]
        public VisualRepresentation MainRepresentator { get { return this.VisualRepresentators.FirstOrDefault(); } }

        /// <summary>
        /// Gets the Main Symbol of the primary Visual Representator of this Idea.
        /// </summary>
        [Description("Gets the Main Symbol of the primary Visual Representator of this Idea.")]
        public VisualSymbol MainSymbol { get { return (this.MainRepresentator == null ? null : this.MainRepresentator.MainSymbol); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of links which associate this Idea to a Relationship.
        /// </summary>
        [Description("Collection of links which associate this Idea to a Relationship.")]
        public EditableList<RoleBasedLink> AssociatingLinks { get; protected set; }
        public static ModelListDefinitor<Idea, RoleBasedLink> __AssociatingLinks =
                   new ModelListDefinitor<Idea, RoleBasedLink>("AssociatingLinks", EEntityMembership.InternalBulk, ins => ins.AssociatingLinks, (ins, coll) => ins.AssociatingLinks = coll,
                                                               "Associating Links", "Collection of links which associate this Idea to a Relationship.");

        /// <summary>
        /// Links targeting to this Idea (at the target side).
        /// </summary>
        [Description("Links targeting to this Idea (at the target side).")]
        public IEnumerable<RoleBasedLink> IncomingLinks { get { return this.AssociatingLinks.Where(rbl => rbl.RoleDefinitor.RoleType == ERoleType.Target); } }

        /// <summary>
        /// Links, opposite to incoming-links and of the same Relationships, pointed from origin Ideas.
        /// </summary>
        [Description("Links, opposite to incoming-links and of the same Relationships, pointed from origin Ideas.")]
        public IEnumerable<RoleBasedLink> OppositeOriginLinks { get { return this.LinkedFrom.SelectMany(rel => rel.OriginLinks).Distinct(); } }

        /// <summary>
        /// Links originating from this Idea (at the origin side).
        /// </summary>
        [Description("Links originating from this Idea (at the origin side).")]
        public IEnumerable<RoleBasedLink> OutgoingLinks { get { return this.AssociatingLinks.Where(rbl => rbl.RoleDefinitor.RoleType == ERoleType.Origin); } }

        /// <summary>
        /// Links, opposite to outgoing-links and of the same Relationships, pointing to target Ideas.
        /// </summary>
        [Description("Links, opposite to outgoing-links and of the same Relationships, pointing to target Ideas.")]
        public IEnumerable<RoleBasedLink> OppositeTargetLinks { get { return this.LinkingTo.SelectMany(rel => rel.TargetLinks).Distinct(); } }

        /// <summary>
        /// Incoming Relationships linking-to/whose-target-is this Idea.
        /// </summary>
        [Description("Incoming Relationships linking-to/whose-target-is this Idea.")]
        public IEnumerable<Relationship> LinkedFrom { get { return this.IncomingLinks.Select(lnk => lnk.OwnerRelationship).Distinct(); } }

        /// <summary>
        /// Outgoing Relationships linked-from/whose-origin-is this Idea.
        /// </summary>
        [Description("Outgoing Relationships linked-from/whose-origin-is this Idea.")]
        public IEnumerable<Relationship> LinkingTo { get { return this.OutgoingLinks.Select(lnk => lnk.OwnerRelationship).Distinct(); } }

        /// <summary>
        /// Ideas pointing to this Idea (through incoming Relationships)
        /// </summary>
        [Description("Ideas pointing to this Idea (through incoming Relationships)")]
        public IEnumerable<Idea> RelatedFrom { get { return this.IncomingLinks.SelectMany(lnk => lnk.OwnerRelationship.OriginIdeas).Distinct(); } }

        /// <summary>
        /// Ideas pointed by this Idea (through outgoing Relationships)
        /// </summary>
        [Description("Ideas pointed by this Idea (through outgoing Relationships)")]
        public IEnumerable<Idea> RelatingTo { get { return this.OutgoingLinks.SelectMany(lnk => lnk.OwnerRelationship.TargetIdeas).Distinct(); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the Idea is composed of others, else is atomic.
        /// </summary>
        [Description("Indicates whether the Idea is composed of others, else is atomic.")]
        public bool IsComposite { get { return (this.CompositeIdeas.Count > 0); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the Idea has detailed content (tables, attachments or non-internal links).
        /// </summary>
        [Description("Indicates whether the Idea has detailed content (tables, attachments or non-internal links).")]
        public bool HasDetailedContent { get { return (this.Details.Count > 0); } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the Idea has (through its definition) links to existent content (such as non-null and non-emtpy string variables)
        /// </summary>
        public bool HasLinksToExistentContent
        {
            get
            {
                foreach (var LinkDesignator in this.IdeaDefinitor.DetailDesignators.Where(dsn => dsn is LinkDetailDesignator))
                {
                    var Reference = LinkDesignator.GetFinalContent(this);
                    if (Reference != null && Reference.ValueExists)
                        return true;
                }

                return false;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Opens a composite View, either by creating a new one or using the existing active.
        /// Returns the opened View or null if not possible.
        /// </summary>
        /// <param name="ContentDomain">The Domain for the Composite-Content.</param>
        public View OpenCompositeView(Domain ContentDomain = null)
        {
            ContentDomain = ContentDomain ?? (this.OwnerContainer == null ? this.OwnerComposition.CompositionDefinitor : this.OwnerContainer.CompositeContentDomain);

            View TargetView = null;

            if (this.CompositeViews.Count > 0)
                TargetView = this.CompositeActiveView ?? this.CompositeViews[0];
            else
            {
                var MaxQuota = AppExec.CurrentLicenseEdition.TechName.SelectCorresponding(LicensingConfig.ComposabilityLevelsQuotas);

                if (!ProductDirector.ValidateEditionLimit(this.CompositeDepthLevel, MaxQuota, "compose", "Composability Depth Levels"))
                    return null;

                // Previous: this.Name + " [" + this.OwnerComposition.ViewsPrefix + (this.CompositeViews.Count + 1).ToString() + "]";
                var ViewName = (this.CompositeDepthLevel <= 1
                                ? MAIN_VIEW_PREFIX + " " + this.OwnerComposition.ViewsPrefix +
                                  (this.CompositeViews.Count > 0
                                   ? " " + (this.CompositeViews.Count + 1).ToString()
                                   : "")
                                : this.Name + (this.CompositeViews.Count > 0
                                               ? " [" + this.OwnerComposition.ViewsPrefix + " " + (this.CompositeViews.Count + 1).ToString() + "]"
                                               : ""));

                TargetView = new View(this, ViewName, ViewName.TextToIdentifier());

                this.CompositeViews.Add(TargetView);

                if (this.CompositeViews.Count == 1)
                    this.CompositeActiveView = TargetView;
            }

            this.OwnerComposition.Engine.ShowView(TargetView);

            return TargetView;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the containment route/path of this Idea, identifying containers by Global-Id, separated by back-slash and without the Composition root.
        /// </summary>
        public string ContainmentRouteByGlobalId { get { return this.GetContainmentRoute(__GlobalId.TechName); } }

        /// <summary>
        /// Gets the containment route/path of this Idea, identifying containers by Name, separated by back-slash and without the Composition root.
        /// </summary>
        public string ContainmentRouteByName { get { return this.GetContainmentRoute(__Name.TechName); } }

        /// <summary>
        /// Gets the containment route/path of this Idea, identifying containers by Tech-Name, separated by back-slash and without the Composition root.
        /// </summary>
        public string ContainmentRouteByTechName { get { return this.GetContainmentRoute(__TechName.TechName); } }

        /// <summary>
        /// Gets the containment route, optionally including the Composition root node, of this Idea using the specified property.
        /// Only "Name", "TechName" and "GlobalId" are supported.
        /// </summary>
        public string GetContainmentRoute(string PropertyName, bool IncludeRootNode = false)
        {
            if (PropertyName == Idea.__TechName.TechName)
                return GetContainmentRoute(false, true, false, true, TOKEN_ROUTE_NODE_SEP, IncludeRootNode, false);

            if (PropertyName == Idea.__GlobalId.TechName)
                return GetContainmentRoute(false, false, true, true, TOKEN_ROUTE_NODE_SEP, IncludeRootNode, false);

            return GetContainmentRoute(true, false, false, true, TOKEN_ROUTE_NODE_SEP, IncludeRootNode, false);
        }

        /// <summary>
        /// Gets the containment route of this Idea.
        /// </summary>
        public string GetContainmentRoute(bool IncludeName = true, bool IncludeTechName = false, bool IncludeGlobalId = false,
                                          bool UseNameCaptionInsteadOfName = true, string NodeSeparator = TOKEN_ROUTE_NODE_SEP,
                                          bool IncludeRootNode = true, bool StartWithRootToken = true)
        {
            var Nodes = new List<string>();

            var Target = this;
            while (Target != null)
            {
                var NodeText = (IncludeName ? (UseNameCaptionInsteadOfName ? Target.NameCaption : Target.Name) : "");

                if (IncludeTechName)
                    NodeText = NodeText + (NodeText.Length > 0 ? TOKEN_ROUTE_NODE_PART_SEP : "") + Target.TechName;

                if (IncludeGlobalId)
                    NodeText = NodeText + (NodeText.Length > 0 ? TOKEN_ROUTE_NODE_PART_SEP : "") + Target.GlobalId;

                Nodes.Add(NodeText);
                Target = Target.OwnerContainer;
            }

            if (!IncludeRootNode && Nodes.Count > 0)
                Nodes.RemoveAt(Nodes.Count - 1);

            var Builder = new StringBuilder(StartWithRootToken ? TOKEN_ROUTE_ROOT : "");
            Nodes.Reverse<string>()
                .ForEachIndexing((node, index) =>
                                 Builder.Append(node + (index < (Nodes.Count - 1)
                                                        ? NodeSeparator
                                                        : "")));

            var Result = Builder.ToString();
            return Result;
        }

        /// <summary>
        /// Gets the containment route, optionally including the Composition root node, of this Idea.
        /// </summary>
        public IList<IRecognizableComposite> GetContainmentNodes(bool IncludeRootNode = false)
        {
            var Nodes = new List<IRecognizableComposite>();

            var Target = this;
            while (Target != null)
            {
                Nodes.Add(Target);
                Target = Target.OwnerContainer;
            }

            if (!IncludeRootNode && Nodes.Count > 0)
                Nodes.RemoveAt(Nodes.Count - 1);

            Nodes.Reverse();
            return Nodes;
        }

        /// <summary>
        /// Gets the level of compositional depth of this Idea.
        /// </summary>
        [Description("Gets the level of compositional depth of this Idea.")]
        public int CompositeDepthLevel
        {
            get
            {
                int Level = 0;

                var Target = this;
                while (Target != null)
                {
                    Level++;
                    Target = Target.OwnerContainer;
                }

                return Level;
            }
        }

        /// <summary>
        /// Gets the count of dependent composite sublevels
        /// </summary>
        public int GetCompositeSubLevelsCount()
        {
            if (this.CompositeIdeas == null || this.CompositeIdeas.Count < 1)
                return 0;

            var Count = 1;  // Start counting this level
            foreach (var CompoIdea in this.CompositeIdeas)
                Count = Math.Max(Count, 1 + CompoIdea.GetCompositeSubLevelsCount());

            return Count;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether the supplied Idea is a component of this Idea (vis its composite container).
        /// </summary>
        public bool HasComponent(Idea Target)
        {
            return this.CompositeIdeas.Contains(Target);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Domain which rules the content of this Idea.
        /// NOTE: Initially, this is populated by the descendants Concept and Relationship constructors, after assign the Definitor.
        /// </summary>
        public Domain CompositeContentDomain
        {
            get { return __CompositeContentDomain.Get(this); }
            set { __CompositeContentDomain.Set(this, value); }
        }
        protected Domain CompositeContentDomain_;
        public static readonly ModelPropertyDefinitor<Idea, Domain> __CompositeContentDomain =
                   new ModelPropertyDefinitor<Idea, Domain>("CompositeContentDomain", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.CompositeContentDomain_, (ins, val) => ins.CompositeContentDomain_ = val, false, true,
                                                            "Composite-Content Domain", "Domain which rules the content of this Idea.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Current active view of the composite Idea.
        /// </summary>
        public View CompositeActiveView
        {
            get
            {
                var Value = __CompositeActiveView.Get(this);
                if (Value == null)
                    Value = this.CompositeViews.FirstOrDefault();

                return Value;
            }
            set
            {
                if (value != null && !this.CompositeViews.Contains(value))
                    throw new UsageAnomaly("Cannot activate an unregistered View.", value);

                __CompositeActiveView.Set(this, value);
            }
        }
        protected View CompositeActiveView_;
        public static readonly ModelPropertyDefinitor<Idea, View> __CompositeActiveView =
                   new ModelPropertyDefinitor<Idea, View>("CompositeActiveView", EEntityMembership.InternalCoreShared, null, EPropertyKind.Common, ins => ins.CompositeActiveView_, (ins, val) => ins.CompositeActiveView_ = val, false, true,
                                                          "Composite Active View", "Current active view of the composite Idea.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Views for contained children Ideas when this is composite.
        /// </summary>
        public EditableList<View> CompositeViews { get; protected set; }
        public static ModelListDefinitor<Idea, View> __CompositeViews =
                   new ModelListDefinitor<Idea, View>("CompositeViews", EEntityMembership.InternalCoreExclusive, ins => ins.CompositeViews, (ins, coll) => ins.CompositeViews = coll,
                                                      "Composite Views", "Views for contained children Ideas when this is composite.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// The collection of components of the composite Idea.
        /// </summary>
        public EditableList<Idea> CompositeIdeas { get; protected set; }
        public static ModelListDefinitor<Idea, Idea> __CompositeIdeas =
                   new ModelListDefinitor<Idea, Idea>("CompositeIdeas", EEntityMembership.InternalBulk, ins => ins.CompositeIdeas, (ins, coll) => ins.CompositeIdeas = coll,
                                                      "CompositeIdeas", "The collection of composing Ideas of this one.");

        public IEnumerable<Concept> CompositeConcepts { get { return this.CompositeIdeas.CastAs<Concept, Idea>(); } }

        public IEnumerable<Relationship> CompositeRelationships { get { return this.CompositeIdeas.CastAs<Relationship, Idea>(); } }

        /// <summary>
        /// Returns all composite children at all subtree levels (includes root).
        /// </summary>
        public List<Idea> GetSubgraphChildren()
        {
            var Result = new List<Idea>();
            
            GetInternalChildren(this, Result);

            return Result;
        }

        private void GetInternalChildren(Idea Child, List<Idea> DetectedChildren)
        {
            DetectedChildren.Add(Child);

            foreach (var Composite in Child.CompositeIdeas)
                GetInternalChildren(Composite, DetectedChildren);
        }

        /// <summary>
        /// Gets a list of navigable elements (suchas as Composite-Ideas, Views and so on), to be shown on a list or tree control.
        /// </summary>
        // IMPORTANT!: NEVER REFERENCE NavigableElements (without the final '_') because regenerates the extended-enumerable!
        //             That property was intended to be used in XAML and binded once.
        public ExtendedEnumerable<IRecognizableElement, IRecognizableElement> NavigableElements
        {
            get
            {
                if (this.NavigableElements_ != null)
                    this.NavigableElements_.UnregisterAllCollections(false);

                this.NavigableElements_ = new ExtendedEnumerable<IRecognizableElement,IRecognizableElement>(
                                                (item) => Tuple.Create(true,item),
                                                (this.OwnerComposition.ShowInterrelatedItemsOrderByName
                                                ? this.CompositeViews.OrderBy(cv => cv.NameCaption)
                                                : (IEnumerable<View>)this.CompositeViews),
                                                (this.OwnerComposition.ShowNavigableItemsOrderByName
                                                ? this.CompositeIdeas.OrderBy(ci => ci.NameCaption)
                                                : (IEnumerable<Idea>)this.CompositeIdeas));

                return this.NavigableElements_;

                // DO NOT EXPOSE DETAILS FROM HERE
                /* foreach(var CompoView in this.CompositeViews)
                    yield return CompoView;

                foreach(var CompoIdea in this.CompositeIdeas)
                    yield return CompoIdea; */
            }
        }
        [NonSerialized]
        public ExtendedEnumerable<IRecognizableElement, IRecognizableElement> NavigableElements_ = null;

        /// <summary>
        /// Gets a list of referenceable composite Ideas order by type (first Concepts, then Relationships) and Name.
        /// </summary>
        public IEnumerable<IRecognizableComposite> CompositeMembers
        {
            get
            {
                var Result = this.CompositeIdeas.OrderBy(idea => (idea is Concept ? "C" : "R") + idea.Name);
                return Result;
            }
        }

        /// <summary>
        /// Returns the parent composite container, if exists.
        /// </summary>
        public IRecognizableComposite CompositeParent
        {
            get { return this.OwnerContainer; }
        }

        // Notice the explicit implmentation of the interface to not clash with the other Definitor property.
        IRecognizableElement IRecognizableComposite.Definitor
        {
            get { return this.IdeaDefinitor; }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of contained details.
        /// </summary>
        public EditableList<ContainedDetail> Details { get; protected set; }
        public static ModelListDefinitor<Idea, ContainedDetail> __Details =
                  new ModelListDefinitor<Idea, ContainedDetail>("Details", EEntityMembership.InternalBulk, ins => ins.Details, (ins, coll) => ins.Details = coll,
                                                                "Details", "Collection of contained details.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all detail designators associated to this Idea, including these of the Idea Definition plus these of the Custom Details.
        /// </summary>
        public IEnumerable<DetailDesignator> GetAllDetailDesignators()
        {
            foreach (var Designator in this.IdeaDefinitor.DetailDesignators)
                yield return Designator;

            foreach (var Detail in this.GetCustomDetails())
                yield return Detail.Designation;
        }

        /// <summary>
        /// Gets all detail designators with possibly stored content, associated to this Idea, including these of the Idea Definition plus these of the Custom Details.
        /// If there is no content, then it attempts to generate default content from designator's initializer, if any.
        /// </summary>
        public IEnumerable<Tuple<DetailDesignator, ContainedDetail>> GetAllDetailDesignatorsAndFinalContents()
        {
            var Result = new List<Tuple<DetailDesignator, ContainedDetail>>();

            foreach (var Designator in this.IdeaDefinitor.DetailDesignators)
                if (!this.Details.Any(det => det.Designation.IsEqual(Designator)))
                    Result.Add(Tuple.Create<DetailDesignator, ContainedDetail>(Designator, Designator.GetFinalContent(this)));

            foreach (var Detail in this.Details)
                Result.Add(Tuple.Create<DetailDesignator, ContainedDetail>(Detail.Designation, Detail));

            Result = Result.OrderBy(tup =>
                        {
                            var Index = this.IdeaDefinitor.DetailDesignators.IndexOf(tup.Item1);
                            var Key = (Index >= 0
                                       ? "A" + ("00" + Index.ToString()).GetRight(3)
                                       : "Z" + ("00" + (this.Details.IndexOfMatch(det => tup.Item1 == det.Designation)).ToString()).GetRight(3));
                            return Key;
                        }).ToList();

            return Result;
        }

        /// <summary>
        /// Gets the custom details, which are those not related to a designated detail in the Idea Definitor.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ContainedDetail> GetCustomDetails()
        {
            foreach (var Detail in this.Details)
                if (Detail.IsCustomDetail)
                    yield return Detail;
        }

        /// <summary>
        /// Gets the appearance for the specified detail Designator and Symbol,
        /// returning either a explicitly customized one or the default of that Designator.
        /// </summary>
        public DetailAppearance GetDetailLook(DetailDesignator Designator, VisualSymbol Symbol)
        {
            if (Symbol != null && this.DetailsCustomLooks.ContainsKey(Designator))
                if (this.DetailsCustomLooks[Designator].ContainsKey(Symbol))
                    return this.DetailsCustomLooks[Designator][Symbol];

            return Designator.DetailLook;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of detail designators indicating custom look/appearance by symbol (if any).
        /// </summary>
        public EditableDictionary<DetailDesignator, EditableDictionary<VisualSymbol, DetailAppearance>> DetailsCustomLooks { get; protected set; }
        public static ModelDictionaryDefinitor<Idea, DetailDesignator, EditableDictionary<VisualSymbol, DetailAppearance>> __DetailsCustomLooks =
                   new ModelDictionaryDefinitor<Idea, DetailDesignator, EditableDictionary<VisualSymbol, DetailAppearance>>("DetailsCustomLooks", EEntityMembership.InternalCoreExclusive, ins => ins.DetailsCustomLooks, (ins, coll) => ins.DetailsCustomLooks = coll,
                                                                                                                            "Details Custom Looks", "Collection of detail designators indicating custom look/appeareance by symbol (if any).");

        /// <summary>
        /// Updates the relation between specified detail-designator, symbol and custom look/appearance.
        /// </summary>
        public void UpdateCustomLookFor(DetailDesignator RefDesignator, VisualSymbol RefSymbol, DetailAppearance RefLook)
        {
            General.ContractRequiresNotNull(RefDesignator, RefSymbol);

            bool IsCustomLook = RefDesignator.DetailLook.SettingsDiffersFrom(RefLook);

            if (IsCustomLook)
            {
                // Add or update custom look
                EditableDictionary<VisualSymbol, DetailAppearance> CustomLooks = null;

                if (this.DetailsCustomLooks.ContainsKey(RefDesignator))
                    CustomLooks = this.DetailsCustomLooks[RefDesignator];
                else
                {
                    CustomLooks = new EditableDictionary<VisualSymbol,DetailAppearance>("SymbolCustomLooks", this);
                    this.DetailsCustomLooks.Add(RefDesignator, CustomLooks);
                }

                CustomLooks[RefSymbol] = RefLook;
            }
            else
            {
                // Delete if previous custom look is present
                var CustomFormats = this.DetailsCustomLooks.GetValueOrDefault(RefDesignator);

                if (CustomFormats != null && CustomFormats.ContainsKey(RefSymbol))
                    CustomFormats.Remove(RefSymbol);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /* POSTPONED
        /// <summary>
        /// The central Idea of the compound group (usable as mind map root concept).
        /// </summary>
        public Idea CompositeMainIdea { get { return __CompositeMainIdea.Get(this); } set { __CompositeMainIdea.Set(this, value); } }
        protected Idea CompositeMainIdea_;
        public static readonly ModelPropertyDefinitor<Idea, Idea> __CompositeMainIdea =
                   new ModelPropertyDefinitor<Idea, Idea>("CompositeMainIdea", EEntityMembership.InternalCoreShared, null, EPropertyKind.Common, ins => ins.CompositeMainIdea_, (ins, val) => ins.CompositeMainIdea_ = val, false, true,
                                                          "Composite Main Idea", "The central Idea of the compound group (usable as mind map root concept).");

        /// <summary>
        /// References the detail designated as main.
        /// </summary>
        public ContainedDetail DetailsMain { get { return __DetailsMain.Get(this); } protected set { __DetailsMain.Set(this, value); } }
        protected ContainedDetail DetailsMain_ = null;
        public static readonly ModelPropertyDefinitor<Idea, ContainedDetail> __DetailsMain =
                   new ModelPropertyDefinitor<Idea, ContainedDetail>("DetailsMain", EEntityMembership.InternalCoreShared, null, EPropertyKind.Common, ins => ins.DetailsMain_, (ins, val) => ins.DetailsMain_ = val, false, false,
                                                                     "Main Detail", "References the detail designated as main."); */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of assigned markers.
        /// </summary>
        public EditableList<MarkerAssignment> Markings { get; protected set; }
        public static ModelListDefinitor<Idea, MarkerAssignment> __Markings =
                  new ModelListDefinitor<Idea, MarkerAssignment>("Markings", EEntityMembership.InternalCoreExclusive, ins => ins.Markings, (ins, coll) => ins.Markings = coll,
                                                                 "Markings", "Collection of assigned markers.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns an enumerator with the current collection of Attachment details.
        /// </summary>
        public IEnumerable<Attachment> GetAttachments()  { return this.Details.CastAs<Attachment, ContainedDetail>(); }
        /*- {
            return General.SetEmptyVariable(ref this.AttachmentsEnumerable,
                                            () => new ExtendedEnumerable<ContainedDetail, Attachment>
                                                  (condet => Tuple.Create<bool, Attachment>(condet is Attachment, condet as Attachment),
                                                   this.Details),
                                            extenum => extenum.UpdateCollectionAssignments(this.Details));
        }
        [NonSerialized]
        private ExtendedEnumerable<ContainedDetail, Attachment> AttachmentsEnumerable = null; */

        /// <summary>
        /// Returns an enumerator with the current collection of Link details.
        /// </summary>
        public IEnumerable<Link> GetLinks()  { return this.Details.CastAs<Link, ContainedDetail>(); }
        /*- {
            return General.SetEmptyVariable(ref this.LinksEnumerable,
                                            () => new ExtendedEnumerable<ContainedDetail, Link>
                                                  (condet => Tuple.Create<bool, Link>(condet is Link, condet as Link),
                                                   this.Details),
                                            extenum => extenum.UpdateCollectionAssignments(this.Details));
        }
        [NonSerialized]
        private ExtendedEnumerable<ContainedDetail, Link> LinksEnumerable = null; */

        /// <summary>
        /// Returns an enumerator with the current collection of Table details.
        /// Optionally, all nested stored Tables (stored in Field values) can be included also.
        /// </summary>
        public IEnumerable<Table> GetTables(bool IncludeNestedTables = false)
        {
            var Result = this.Details.CastAs<Table, ContainedDetail>();

            if (IncludeNestedTables)
            {
                var Extension = new List<Table>();

                foreach (var StoredTable in Result)
                    Extension.AddRange(StoredTable.GetStoredTables(IncludeNestedTables));

                Result = Result.Concat(Extension);
            }

            return Result;
        }
        /*- public IEnumerable<Table> GetTables()
        {
            return General.SetEmptyVariable(ref this.TablesEnumerable,
                                            () => new ExtendedEnumerable<ContainedDetail, Table>
                                                  (condet => Tuple.Create<bool, Table>(condet is Table, condet as Table),
                                                   this.Details),
                                            extenum => extenum.UpdateCollectionAssignments(this.Details));
        }
        [NonSerialized]
        private ExtendedEnumerable<ContainedDetail, Table> TablesEnumerable = null; */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates any dependent visual representator to reflect reccent changes.
        /// </summary>
        public void UpdateVisualRepresentators()
        {
            foreach (var Representator in this.VisualRepresentators)
                Representator.Render();
        }

        /// <summary>
        /// Clears and disconnect any dependent visual representator to reflect reccent changes.
        /// </summary>
        public void ClearVisualRepresentators()
        {
            foreach (var Representator in this.VisualRepresentators)
                Representator.Clear();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Separator used for synopsis for indenting or between properties and values.
        /// </summary>
        public const string SYNOP_SEPARATOR = "\t";

        /// <summary>
        /// Generates and returns a text synopsis of the Idea (for export).
        /// </summary>
        /// <returns></returns>
        public string GetTextSynopsis()
        {
            var Describer = new StringBuilder("\\[IDEA:" + this.GetType().Name.ToUpper() + "]");
            Describer.AppendLine();

            Describer.AppendLine("GlobalId" + SYNOP_SEPARATOR + this.GlobalId.ToString());
            Describer.AppendLine("Type" + SYNOP_SEPARATOR + this.Definitor.Name);
            Describer.AppendLine("Name" + SYNOP_SEPARATOR + this.Name);
            Describer.AppendLine("TechName" + SYNOP_SEPARATOR + this.TechName);
            Describer.AppendLine("Summary" + SYNOP_SEPARATOR + this.Summary);
            //? Describer.AppendLine();

            var SpecializedSynopsis = GetSpecializedTextSynopsis();
            if (!SpecializedSynopsis.IsAbsent())
            {
                Describer.AppendLine(SpecializedSynopsis);
                //? Describer.AppendLine();
            }

            if (Details != null && Details.Count > 0)
            {
                Describer.AppendLine("Details...");
                foreach (var Detail in this.Details)
                    Describer.AppendLine(SYNOP_SEPARATOR + "GlobalId" + SYNOP_SEPARATOR + Detail.Designation.GlobalId.ToString() + SYNOP_SEPARATOR
                                                         + "Type" + SYNOP_SEPARATOR + Detail.GetType().Name + SYNOP_SEPARATOR
                                                         + "Name" + SYNOP_SEPARATOR + Detail.Designation.Name + SYNOP_SEPARATOR
                                                         + "Content:" + SYNOP_SEPARATOR + Detail.ToString());

                //? Describer.AppendLine();
            }

            Describer.AppendLine("\\[IDEA-END]");

            return Describer.ToString();
        }

        /// <summary>
        /// Gets specialized (Idea descendant) content as text synopsis.
        /// </summary>
        public virtual string GetSpecializedTextSynopsis() { return "";  }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return this.NameCaption + (this.IdeaDefinitor == null ? "" : " [" + this.IdeaDefinitor.NameCaption + "]");
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        // See CompositionsManager.CommandApplyFormat_Execution
        public ImageSource RepresentativePicture
        {
            get
            {
                var Result = new DrawingImage(this.MainSymbol == null
                                              ? MasterDrawer.CreateDrawingSample(this.IdeaDefinitor)
                                              : MasterDrawer.CreateDrawingSample(this.IdeaDefinitor, false, false, 0.0, 0.0,
                                                                                 VisualSymbolFormat.GetMainBackground(this.MainSymbol),
                                                                                 VisualSymbolFormat.GetLineBrush(this.MainSymbol),
                                                                                 VisualSymbolFormat.GetLineThickness(this.MainSymbol),
                                                                                 VisualSymbolFormat.GetLineDash(this.MainSymbol),
                                                                                 VisualSymbolFormat.GetOpacity(this.MainSymbol)));
                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IDefined Members

        public virtual MetaDefinition Definitor { get { return this.IdeaDefinitor; } }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Idea> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Idea> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Idea> __ClassDefinitor = null;

        public new Idea CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Idea)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Idea PopulateFrom(Idea SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the kind (Composition, Concept or Relationship) of this idea final instance type.
        /// </summary>
        public ModelDefinition SelfKind
        {
            get
            {
                ModelDefinition Result = Idea.__ClassDefinitor;

                if (this is Composition)
                    Result = Composition.__ClassDefinitor;
                else
                    if (this is Concept)
                        Result = Concept.__ClassDefinitor;
                    else
                        if (this is Relationship)
                            Result = Relationship.__ClassDefinitor;

                return Result;
            }
        }

        /// <summary>
        /// Gets the kind (Domain, ConceptDefintion or RelationshipDefinition) of the definitor of this idea final instance type.
        /// </summary>
        public ModelDefinition BaseKind
        {
            get
            {
                ModelDefinition Result = IdeaDefinition.__ClassDefinitor;

                if (this is Composition)
                    Result = Domain.__ClassDefinitor;
                else
                    if (this is Concept)
                        Result = ConceptDefinition.__ClassDefinitor;
                    else
                        if (this is Relationship)
                            Result = RelationshipDefinition.__ClassDefinitor;

                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected Idea GenerateIdeaIndependentDuplicate()
        {
            // PEDING: Clone composite subgraph.
            // Complex, since must devinculate from the rest of the Composition
            // and reappoint every link to the new subgraph clone.

            var NewIdea = this.CreateClone(ECloneOperationScope.Slight, null);

            // Reassign global-id
            NewIdea.GlobalId = Guid.NewGuid();

            // Reassign collections: Reset all composites or interrelations,
            // plus generate clones of detailed content.
            NewIdea.AssociatingLinks.Clear();
            NewIdea.VisualRepresentators.Clear();
            NewIdea.CompositeViews.Clear();
            NewIdea.CompositeIdeas.Clear();
            NewIdea.DetailsCustomLooks.Clear();
            NewIdea.Markings = this.Markings.CloneFor(NewIdea, null, ECloneOperationScope.Deep);

            NewIdea.Details = new EditableList<ContainedDetail>(__Details.TechName, NewIdea, this.Details.Count);

            foreach (var Detail in this.Details)
                if (Detail is Table)
                {
                    var SourceTable = (Table)Detail;
                    var Designator = SourceTable.AssignedDesignator;

                    if (!Designator.Value.Owner.IsGlobal)
                        Designator = SourceTable.Designation.CreateClone(ECloneOperationScope.Deep, null)
                                                        .Assign(SourceTable.AssignedDesignator.IsLocal);

                    var ClonedTable = Table.CreateTableFrom(Designator, SourceTable, NewIdea);
                    NewIdea.Details.Add(ClonedTable);
                }
                else
                {
                    var NewDetail = Detail.CreateClone(ECloneOperationScope.Deep, null);
                    NewDetail.OwnerIdea = NewIdea;
                    NewIdea.Details.Add(NewDetail);
                }

            // Reset properties
            NewIdea.CompositeActiveView = null;

            /*POSTPONED
            NewIdea.CompositeMainIdea = null;
            NewIdea.DetailsMain = null; */

            return NewIdea;
        }

        protected Idea GenerateIndependentDuplicateForExternalComposition(Composition ExternalComposition)
        {
            // PEDING: Clone composite subgraph.
            // Complex, since must devinculate from the rest of the Composition
            // and reappoint every link to the new subgraph clone.

            var ExternalNewIdea = this.CreateClone(ECloneOperationScope.Slight, null);
            ExternalNewIdea.OwnerComposition = ExternalComposition;
            ExternalNewIdea.CompositeContentDomain = this.CompositeContentDomain;

            // Reassign global-id
            ExternalNewIdea.GlobalId = Guid.NewGuid();

            // Reassign collections: Reset all composites or interrelations,
            // plus generate clones of detailed content.
            ExternalNewIdea.AssociatingLinks.Clear();
            ExternalNewIdea.VisualRepresentators.Clear();
            ExternalNewIdea.CompositeViews.Clear();
            ExternalNewIdea.CompositeIdeas.Clear();
            ExternalNewIdea.DetailsCustomLooks.Clear();
            ExternalNewIdea.Markings = CreateExternalClonesForInternalMarkings(ExternalNewIdea, this.Markings);
            ExternalNewIdea.Details = CreateExternalClonesForInternalDetails(ExternalNewIdea, this.Details);

            // Reset properties
            ExternalNewIdea.CompositeActiveView = null;
            /* POSTPONED:
            ExternalNewIdea.CompositeMainIdea = null;
            ExternalNewIdea.DetailsMain = null; */

            return ExternalNewIdea;
        }

        public EditableList<MarkerAssignment> CreateExternalClonesForInternalMarkings(Idea ExternalTarget, IList<MarkerAssignment> InternalMarkings)
        {
            var Result = new EditableList<MarkerAssignment>(__Markings.TechName, ExternalTarget);

            foreach (var InternalMarker in InternalMarkings)
            {
                var ExternalMarkerDef = ExternalTarget.OwnerComposition.CompositeContentDomain.MarkerDefinitions
                                                .FirstOrDefault(mkdef => mkdef.TechName == InternalMarker.Definitor.TechName);
                if (ExternalMarkerDef == null)
                {
                    ExternalMarkerDef = InternalMarker.Definitor.CreateClone(ECloneOperationScope.Core, null);
                    ExternalTarget.OwnerComposition.CompositeContentDomain.MarkerDefinitions.Add(ExternalMarkerDef);
                }

                var Descriptor = (InternalMarker.MarkerHasDescriptor
                                  ? InternalMarker.Descriptor.CreateClone(ECloneOperationScope.Core, null)
                                  : null);

                var ExternalMarker = new MarkerAssignment(ExternalTarget.EditEngine, ExternalMarkerDef,
                                                          Descriptor);

                Result.Add(ExternalMarker);
            }

            return Result;
        }

        public EditableList<ContainedDetail> CreateExternalClonesForInternalDetails(Idea ExternalTarget, IList<ContainedDetail> InternalDetails)
        {
            var Result = new EditableList<ContainedDetail>(__Details.TechName, ExternalTarget, this.Details.Count);

            foreach (var Detail in this.Details)
            {
                ContainedDetail ClonedDetail = null;

                // NOTE: All designators must be local (it would be very complicated to detect and replicate the shared ones)
                if (Detail is Table)
                {
                    var SourceTable = (Table)Detail;

                    var LocalTableDef = SourceTable.Designator.DeclaringTableDefinition.GenerateIndependentDuplicateForExternalDomain(
                                                                                    ExternalTarget.OwnerComposition.CompositeContentDomain);

                    var LocalTableLook = SourceTable.Designator.TableLook.CreateClone(ECloneOperationScope.Core, null);

                    var LocalDesignator = (TableDetailDesignator)SourceTable.Designation.CreateClone(ECloneOperationScope.Deep, null);
                    LocalDesignator.DeclaringTableDefinition = LocalTableDef;
                    LocalDesignator.TableLook = LocalTableLook;

                    ClonedDetail = Table.CreateTableFrom(((DetailDesignator)LocalDesignator).Assign(true), SourceTable, ExternalTarget);
                    Result.Add(ClonedDetail);
                }
                else
                {
                    ClonedDetail = Detail.CreateClone(ECloneOperationScope.Deep, null);

                    var LocalDesignator = Detail.Designation.CreateClone(ECloneOperationScope.Deep, null);

                    ClonedDetail.AssignedDesignator = LocalDesignator.Assign(true);

                    Result.Add(ClonedDetail);
                }
            }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override void RefreshEntity()
        {
            foreach (var Representation in this.VisualRepresentators)
                Representation.Render();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the supplied New Composite idea to this idea's composite container.
        /// </summary>
        private void AddComponent(Idea NewComposite)
        {
            NewComposite.OwnerContainer = this;
            this.CompositeIdeas.AddNew(NewComposite);
            this.OwnerComposition.RegisterIdea(NewComposite);
        }

        /// <summary>
        /// Removes the specified Existend Composite idea from this idea's composite container.
        /// </summary>
        private void RemoveComponent(Idea ExistentComposite)
        {
            this.CompositeIdeas.Remove(ExistentComposite);
            this.OwnerComposition.UnregisterIdea(ExistentComposite);
            ExistentComposite.OwnerContainer = null;
        }

        /// <summary>
        /// Adds this idea to the specified Composite Container
        /// </summary>
        public virtual void AddToComposite(Idea CompositeContainer)
        {
            CompositeContainer.AddComponent(this);
        }

        /// <summary>
        /// Removes this idea from its composite container.
        /// Optionally, indidcations for preserve non-sense relationships (those without both, sources and targets)
        /// and for preserve rich relationships (those with Composite-Content or Details) can be specified.
        /// </summary>
        public virtual void RemoveFromComposite(bool PreserveNonsenseRelationships = false, bool PreserveRichRelationships = false)
        {
            var SubgraphChildren = this.GetSubgraphChildren();
            this.DeleteNonSubgraphAssociations(SubgraphChildren, PreserveNonsenseRelationships, PreserveRichRelationships);
            var RemainingRelationships = this.DeleteAssociations(PreserveNonsenseRelationships, PreserveRichRelationships);

            // Delete the target
            this.ClearVisualRepresentators();

            if (this.OwnerContainer != null)
            {
                this.OwnerContainer.UpdateVersion();

                var RemovedOwner = this.OwnerContainer;

                this.OwnerContainer.RemoveComponent(this);  // last time Idea has an owner-container

                // Called at the very last to refresh content-tree
                // IMPORTANT!: NEVER REFERENCE NavigableElements (without the final '_') because regenerates the extended-enumerable!
                //             That property was intended to be used in XAML and binded once.
                if (RemovedOwner.NavigableElements_ != null)
                    System.Windows.Application.Current.MainWindow.PostCall(wnd => RemovedOwner.NavigableElements_.NotifyReset(), true);
            }

            // Update affected relationships
            foreach (var Relation in RemainingRelationships)
                Relation.UpdateVisualRepresentators();
        }

        /// <summary>
        /// Gets all nested composite Ideas from this one, optionally including only these with composite Views.
        /// </summary>
        public IEnumerable<Idea> GetNestedCompositeIdeas(bool IncludeOnlyIdeasWithCompositeViews = false)
        {
            var Result = new List<Idea>();
            GetNestedCompositeIdeas(this, Result, IncludeOnlyIdeasWithCompositeViews);
            return Result;
        }

        private static void GetNestedCompositeIdeas(Idea Source, IList<Idea> Route, bool IncludeOnlyIdeasWithCompositeViews = false)
        {
            if (IncludeOnlyIdeasWithCompositeViews && Source.CompositeViews.Count < 1)
                return;

            Route.Add(Source);

            foreach (var Child in Source.CompositeIdeas)
                GetNestedCompositeIdeas(Child, Route);
        }

        /// <summary>
        /// Deletes associations between the Target Idea Composite-Content (children) which are to/from outside the subgraph.
        /// </summary>
        public void DeleteNonSubgraphAssociations(List<Idea> AllChildren, bool PreserveNonsenseRelationships = false, bool PreserveRichRelationships = false)
        {
            var LinksToDelete = this.AssociatingLinks.Where(link => !link.AssociatedIdea.IsIn(AllChildren)).ToList();

            foreach (var Link in LinksToDelete)
                Link.OwnerRelationship.DeleteRelationshipAssociation(Link.AssociatedIdea, PreserveNonsenseRelationships, PreserveRichRelationships);

            foreach (var Composite in this.CompositeIdeas)
                Composite.DeleteNonSubgraphAssociations(AllChildren);
        }

        /// <summary>
        /// Delete association to/from this idea.
        /// Returns non deleted relationships
        /// </summary>
        public IEnumerable<Relationship> DeleteAssociations(bool PreserveNonsenseRelationships = false, bool PreserveRichRelationships = false)
        {
            var Result = new List<Relationship>();

            // Delete connections with target
            var AssocLinks = this.AssociatingLinks.ToList();
            var AssocRelationships = AssocLinks.Select(link => link.OwnerRelationship).Distinct();

            foreach (var AssocRel in AssocRelationships)
                if (!AssocRel.DeleteRelationshipAssociation(this, PreserveNonsenseRelationships, PreserveRichRelationships))
                    Result.Add(AssocRel);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Updates the version information (increases version number), propagating it to composite owner.
        /// </summary>
        public void UpdateVersion()
        {
            if (this.IdeaDefinitor.IsVersionable && this.Version == null)
                this.Version = new VersionCard();

            if (this.Version != null)
                this.Version.Update();

            if (this.OwnerContainer != null)
                this.OwnerContainer.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies the necessary changes for an executed base Idea-Definition change.
        /// </summary>
        protected void ApplyIdeaDefinitionChange(IdeaDefinition PreviousIdeaDef)
        {
            UpdateVisualRepresentators();
            UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Alias of the Custom-Fields record.
        /// </summary>
        // For be fast written in file generation Templates.
        [Description("Alias of the Custom-Fields record.")]
        public TableRecord _ { get { return this.CustomFields; } }

        /// <summary>
        /// Gets the Custom-Fields record or null when absent.
        /// </summary>
        // Already exposed: [Description("Gets the Custom-Fields record or null when absent.")]
        public TableRecord CustomFields { get { return (this.CustomFieldsTable == null || this.CustomFieldsTable.Count < 1
                                                        ? null : this.CustomFieldsTable[0]); } }

        /// <summary>
        /// Returns the detail Table containing the Idea's Custom-Fields, or null if not-present.
        /// </summary>
        // Already exposed: [Description("Returns the detail Table containing the Idea's Custom-Fields, or null if not-present.")]
        public Table CustomFieldsTable
        {
            get
            {
                if (this.CustomFieldsTable_ == null)
                    this.CustomFieldsTable_ = this.Details.CastAs<Table, ContainedDetail>()
                                                .FirstOrDefault(tbl => tbl.Designator.Alterability == EAlterability.System && tbl.Designator.TableDefIsOwned
                                                                && tbl.Designator.TechName == IdeaDefinition.SYSNAME_CUSTOM_FIELDS);

                return this.CustomFieldsTable_;
            }
        }
        [NonSerialized]
        private Table CustomFieldsTable_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this Idea (To support access to Details via indexer).
        /// </summary>
        [Description("Returns this Idea (To support access to Details thru indexer).")]
        public Idea This { get { return this; } }

        #region IIndexable Members

        /// <summary>
        /// Returns the Detail designated by the specified Designator Tech-Name.
        /// </summary>
        // Used with 'object' in order to implement IIndexable
        public object this[object DesignatorTechName]
        {
            get
            {
                var Result = this.Details.FirstOrDefault(det => det.Designation.TechName == DesignatorTechName.ToStringAlways());
                return Result;
            }
        }

        /// <summary>
        /// Indicates whether exists a Detail designated by the specified Designator Tech-Name.
        /// </summary>
        // Used with 'object' in order to implement IIndexable
        public bool ContainsKey(object DesignatorTechName)
        {
            var Result = Details.Any(det => det.Designation.TechName == DesignatorTechName.ToStringAlways());
            return Result;
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}