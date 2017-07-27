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
// File   : IdeaDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.GraphMetaModel.IdeaDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.14 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Definitor.DefinitorMaintenance;
using Instrumind.ThinkComposer.Definitor.DefinitorUI;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.MetaModel.VisualMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Composer.Generation;
using Instrumind.ThinkComposer.MetaModel.Configurations;

/// Base abstractions for the user metadefinition of Graph schemas
namespace Instrumind.ThinkComposer.MetaModel.GraphMetaModel
{
    /// <summary>
    /// Common ancestor for Metadefinitions about Concepts and Relationships. It can have attached user-defined Information and Visualization assignments.
    /// </summary>
    [Serializable]
    public abstract class IdeaDefinition : MetaDefinition, IModelEntity, IModelClass<IdeaDefinition>, IVersionUpdater
    {
        public const string SYSNAME_CUSTOM_FIELDS = "CustomFields";

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static IdeaDefinition()
        {
            __ClassDefinitor = new ModelClassDefinitor<IdeaDefinition>("IdeaDefinition", MetaDefinition.__ClassDefinitor, "IdeaDefinition",
                                                                       "Common ancestor for Metadefinitions about Concepts and Relationships. It can have attached user-defined Information and Visualization assignments.");
            __ClassDefinitor.DeclareProperty(__OwnerDefinitor);
            __ClassDefinitor.DeclareProperty(__CompositeContentDomain);
            __ClassDefinitor.DeclareProperty(__Cluster);

            __ClassDefinitor.DeclareProperty(__IsComposable);
            __ClassDefinitor.DeclareProperty(__IsVersionable);
            __ClassDefinitor.DeclareProperty(__CanAutomaticallyCreateRelatedConcepts);
            __ClassDefinitor.DeclareProperty(__CanGroupIntersectingObjects);
            __ClassDefinitor.DeclareProperty(__CanAutomaticallyCreateGroupedConcepts);
            __ClassDefinitor.DeclareProperty(__AutomaticGroupedConceptDef);
            __ClassDefinitor.DeclareProperty(__HasGroupRegion);
            __ClassDefinitor.DeclareProperty(__HasGroupLine);
            __ClassDefinitor.DeclareProperty(__RepresentativeShape);
            __ClassDefinitor.DeclareProperty(__PreciseConnectByDefault);
            __ClassDefinitor.DeclareProperty(__DefaultSymbolFormat);
            __ClassDefinitor.DeclareProperty(__CustomFieldsTableDef);

            __ClassDefinitor.DeclareCollection(__ConceptDefinitions);
            __ClassDefinitor.DeclareCollection(__RelationshipDefinitions);
            __ClassDefinitor.DeclareCollection(__DetailDesignators);
            __ClassDefinitor.DeclareCollection(__TableDefinitions);
            __ClassDefinitor.DeclareCollection(__OutputTemplates);

            __Cluster.ItemsSourceGetter = ((ctx) =>
                {
                    var IdeaDef = ctx as IdeaDefinition;
                    if (IdeaDef == null)
                        return null;

                    var Result = (IdeaDef is ConceptDefinition
                                  ? ConceptDefinition.ConceptDef_Cluster_None.IntoList().Concat(IdeaDef.OwnerDomain.ConceptDefClusters)
                                  : RelationshipDefinition.RelationshipDef_Cluster_None.IntoList().Concat(IdeaDef.OwnerDomain.RelationshipDefClusters));

                    return Result;
                });
            // NOTE: __Cluster.ItemsSourceSelectedValuePath must not be assigned in order to use the whole item
            __Cluster.ItemsSourceDisplayMemberPath = FormalElement.__Name.TechName;

            __AutomaticGroupedConceptDef.ItemsSourceGetter = ((ctx) =>
            {
                var IdeaDef = ctx as IdeaDefinition;
                if (IdeaDef == null)
                    return null;

                return IdeaDef.OwnerDomain.ConceptDefinitions;
            });
            __AutomaticGroupedConceptDef.ItemsSourceSelectedValuePath = FormalElement.__TechName.TechName;
            __AutomaticGroupedConceptDef.ItemsSourceDisplayMemberPath = FormalElement.__Name.TechName;

            __RepresentativeShape.ItemsSourceGetter =
                ((ctx) =>
                {
                    if (ctx == null || ctx.EditEngine == null)
                        return null;

                    var Result = ((CompositionEngine)ctx.EditEngine).TargetComposition.CompositeContentDomain.AvailableShapes.OrderBy(shp => shp.Name);
                    return Result;
                });
            __RepresentativeShape.ItemsSourceSelectedValuePath = SimpleElement.__TechName.TechName;
            __RepresentativeShape.ItemsSourceDisplayMemberPath = SimpleElement.__Name.TechName;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="OwnerComposite">Composite IdeaDefinition owning this new one.</param>
        /// <param name="Name">Name of the IdeaDefinition.</param>
        /// <param name="TechName">Technical Name of the IdeaDefinition.</param>
        /// <param name="RepresentativeShape">Shape visually representing the IdeaDefinition.</param>
        /// <param name="Summary">Summary of the IdeaDefinition.</param>
        /// <param name="Pictogram">Image representing the IdeaDefinition.</param>
        public IdeaDefinition(IdeaDefinition OwnerComposite, string Name, string TechName, string RepresentativeShape,
                              string Summary = "", ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram, true)
        {
            General.ContractRequires(OwnerComposite != null || this is Domain);
            General.ContractRequiresNotAbsent(RepresentativeShape);

            this.RepresentativeShape = RepresentativeShape;
            this.ConceptDefinitions = new EditableList<ConceptDefinition>(__ConceptDefinitions.TechName, this);
            this.RelationshipDefinitions = new EditableList<RelationshipDefinition>(__RelationshipDefinitions.TechName, this);
            this.DetailDesignators = new EditableList<DetailDesignator>(__DetailDesignators.TechName, this);
            this.TableDefinitions = new EditableList<TableDefinition>(__TableDefinitions.TechName, this);

            this.OwnerDefinitor = OwnerComposite;

            if (OwnerComposite != null)
                this.CompositeContentDomain = OwnerComposite.CompositeContentDomain;

            // Default details designations...

            // In this case, the default detail shown will be the Summary property.
            this.DetailDesignators.Add(new LinkDetailDesignator(Ownership.Create<IdeaDefinition, Idea>(this),
                                                                __Summary.Name, __Summary.TechName, __Summary.Summary, __Summary));

            // Table for custom fields...
            this.CustomFieldsTableDef = new TableDefinition(this.OwnerDomain, "Custom-Fields Designator", "CustomFieldsDesignator",
                                                            "User-defined fields for this particular Idea definition.");
            CustomFieldsTableDef.Alterability = EAlterability.System;
            CustomFieldsTableDef.AlterStructure();

            var CustomFieldsTableDsn = new TableDetailDesignator(Ownership.Create<IdeaDefinition,Idea>(this), CustomFieldsTableDef, true,
                                                                 "Custom-Fields", SYSNAME_CUSTOM_FIELDS);
            CustomFieldsTableDsn.Alterability = EAlterability.System;
            CustomFieldsTableDsn.TableLook = new TableAppearance(ShowTitle: false, IsMultiRecord: false, Layout: ETableLayoutStyle.Transposed);

            this.DetailDesignators.Add(CustomFieldsTableDsn);

            this.DeclareOutputTemplatesCollection();
        }

        /// <summary>
        /// Protected Constructor for Cloning.
        /// </summary>
        protected IdeaDefinition()
        {
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized]
        protected void Initialize(StreamingContext context = default(StreamingContext))
        {
            this.DeclareOutputTemplatesCollection();
        }

        public void DeclareOutputTemplatesCollection()
        {
            if (this.OutputTemplates == null)
                this.OutputTemplates = new EditableList<TextTemplate>(__OutputTemplates.TechName, this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Cluster to which this Idea-Definition is associated (used for better organization/grouping of the Definitions).
        /// </summary>
        // Key of the associated Cluster (tipically refers to the Tech-Name of the Cluster).
        public FormalPresentationElement Cluster { get { return __Cluster.Get(this); } set { __Cluster.Set(this, value); } }
        protected FormalPresentationElement Cluster_ = null;
        // IMPORTANT: The IDEADEF_CLUSTER_EMPTY_ID key represents the null/unassigned value.
        public static readonly ModelPropertyDefinitor<IdeaDefinition, FormalPresentationElement> __Cluster =
                   new ModelPropertyDefinitor<IdeaDefinition, FormalPresentationElement>("ClusterKey", EEntityMembership.External, null,
                                                                      // Notice the evaluation agains Absent-Default instead of null (some stock remaining ClusterKeys may be empty-string).
                                                                      EPropertyKind.Common, ins => ins.Cluster_.NullDefault(ins is ConceptDefinition ? ConceptDefinition.ConceptDef_Cluster_None : RelationshipDefinition.RelationshipDef_Cluster_None),
                                                                      (ins, val) => ins.Cluster_ = val.SubstituteFor((ins is ConceptDefinition ? ConceptDefinition.ConceptDef_Cluster_None : RelationshipDefinition.RelationshipDef_Cluster_None), null),
                                                                      false, false,
                                                                      "Cluster", "Cluster to which this Idea-Definition is associated (used for better organization/grouping of the Definitions).");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the composite Idea definition owning this one.
        /// </summary>
        public IdeaDefinition OwnerDefinitor { get { return __OwnerDefinitor.Get(this); } set { __OwnerDefinitor.Set(this, value); } }
        protected IdeaDefinition OwnerDefinitor_ = null;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, IdeaDefinition> __OwnerDefinitor =
                   new ModelPropertyDefinitor<IdeaDefinition, IdeaDefinition>("OwnerDefinitor", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerDefinitor_, (ins, val) => ins.OwnerDefinitor_ = val, false, true,
                                                                              "Owner Definitor", "References the composite Idea definition owning this one.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the ancestor Idea definition of this one.
        /// </summary>
        public abstract IdeaDefinition AncestorIdeaDef { get; }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
         /// <summary>
        /// If set, indicates the Domain which rules the content of the defined Idea.
        /// </summary>
        public Domain CompositeContentDomain { get { return __CompositeContentDomain.Get(this); } set { __CompositeContentDomain.Set(this, value); } }
        protected Domain CompositeContentDomain_ = null;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, Domain> __CompositeContentDomain =
                   new ModelPropertyDefinitor<IdeaDefinition, Domain>("CompositeContentDomain", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.CompositeContentDomain_, (ins, val) => ins.CompositeContentDomain_ = val, false, true,
                                                                      "Composite-Content Domain", "If set, indicates the Domain which rules the content of the defined Idea.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
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
                    Result = new DrawingImage(this.GetSampleDrawing());

                return Result;
            }
            set
            {
                base.Pictogram = value;
            }
        }

        /// <summary>
        /// Returns the predefined default pictogram of this definition.
        /// </summary>
        public abstract ImageSource DefaultPictogram { get; }

        /// <summary>
        /// Indicates whether the defined Ideas can be composed of others.
        /// </summary>
        public bool IsComposable { get { return __IsComposable.Get(this); } set { __IsComposable.Set(this, value); } }
        protected bool IsComposable_ = true;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __IsComposable =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("IsComposable", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsComposable_, (ins, val) => ins.IsComposable_ = val, false, false,
                                                                    "Is Composable", "Indicates whether the defined Ideas can be composed of others in a whole view/diagram contained inside.");

        /// <summary>
        /// Indicates whether the defined Ideas can maintain versioning information.
        /// </summary>
        public bool IsVersionable { get { return __IsVersionable.Get(this); } set { __IsVersionable.Set(this, value); } }
        protected bool IsVersionable_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __IsVersionable =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("IsVersionable", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsVersionable_, (ins, val) => ins.IsVersionable_ = val, false, false,
                                                                    "Is Versionable", "Indicates whether the defined Ideas can maintain versioning information.");

        /// <summary>
        /// Indicates whether the Ideas of this type will automatically create related Concepts in editing (by pressing [Enter], [Tab] or dropping Idea Definitions over them).
        /// </summary>
        public bool CanAutomaticallyCreateRelatedConcepts { get { return __CanAutomaticallyCreateRelatedConcepts.Get(this); } set { __CanAutomaticallyCreateRelatedConcepts.Set(this, value); } }
        protected bool CanAutomaticallyCreateRelatedConcepts_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __CanAutomaticallyCreateRelatedConcepts =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("CanAutomaticallyCreateRelatedConcepts", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.CanAutomaticallyCreateRelatedConcepts_, (ins, val) => ins.CanAutomaticallyCreateRelatedConcepts_ = val, false, false,
                                                                    "Can Automatically Create Related Concepts", "Indicates whether the Ideas of this type will automatically create related Concepts in editing (by pressing [Enter], [Tab] or dropping Idea Definitions over them).");

        /// <summary>
        /// Indicates whether the Ideas of this type will group objects intersecting its symbol or Group Region.
        /// </summary>
        public bool CanGroupIntersectingObjects { get { return __CanGroupIntersectingObjects.Get(this); } set { __CanGroupIntersectingObjects.Set(this, value); } }
        protected bool CanGroupIntersectingObjects_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __CanGroupIntersectingObjects =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("CanGroupIntersectingObjects", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.CanGroupIntersectingObjects_, (ins, val) => ins.CanGroupIntersectingObjects_ = val, false, false,
                                                                    "Can Group Intersecting Objects", "Indicates whether the Ideas of this type will group objects intersecting its symbol or Group Region.");

        /// <summary>
        /// Indicates whether the Ideas of this type will automatically create grouped Concepts when linking a Relationship into an appended Group Region/Line.
        /// </summary>
        public bool CanAutomaticallyCreateGroupedConcepts { get { return __CanAutomaticallyCreateGroupedConcepts.Get(this); } set { __CanAutomaticallyCreateGroupedConcepts.Set(this, value); } }
        protected bool CanAutomaticallyCreateGroupedConcepts_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __CanAutomaticallyCreateGroupedConcepts =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("CanAutomaticallyCreateGroupedConcepts", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.CanAutomaticallyCreateGroupedConcepts_, (ins, val) => ins.CanAutomaticallyCreateGroupedConcepts_ = val, false, false,
                                                                    "Can Automatically Create Grouped Concepts", "Indicates whether the Ideas of this type will automatically create grouped Concepts when linking a Relationship into an appended Group Region/Line.");

        /// <summary>
        /// Definition of the Concept to be automatically created onto an appended Group Region/Line.
        /// </summary>
        public ConceptDefinition AutomaticGroupedConceptDef
        {
            get { return __AutomaticGroupedConceptDef.Get(this); }
            set { __AutomaticGroupedConceptDef.Set(this, value); }
        }
        protected ConceptDefinition AutomaticGroupedConceptDef_ = null;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, ConceptDefinition> __AutomaticGroupedConceptDef =
                   new ModelPropertyDefinitor<IdeaDefinition, ConceptDefinition>("AutomaticGroupedConceptDef", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AutomaticGroupedConceptDef_, (ins, val) => { ins.AutomaticGroupedConceptDef_ = val; }, false, false,
                                                                                 "Automatic Grouped Concept Def.", "Definition of the Concept to be automatically created onto an appended Group Region/Line.");

        /// <summary>
        /// Indicates whether the defined Ideas are created with a Group Region Complement appended.
        /// </summary>
        public bool HasGroupRegion { get { return __HasGroupRegion.Get(this); } set { __HasGroupRegion.Set(this, value); } }
        protected bool HasGroupRegion_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __HasGroupRegion =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("HasGroupRegion", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HasGroupRegion_, (ins, val) => ins.HasGroupRegion_ = val, false, false,
                                                                    "Has Group Region", "Indicates whether the defined Ideas are created with a Group Region complement (a boundary) appended.");

        /// <summary>
        /// Indicates whether the defined Ideas are created with a Group Line complement (like a 'life line') appended.
        /// </summary>
        public bool HasGroupLine { get { return __HasGroupLine.Get(this); } set { __HasGroupLine.Set(this, value); } }
        protected bool HasGroupLine_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __HasGroupLine =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("HasGroupLine", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HasGroupLine_, (ins, val) => ins.HasGroupLine_ = val, false, false,
                                                                    "Has Group Line", "Indicates whether the defined Ideas are created with a Group Line complement (like a 'life line') appended.");

        /// <summary>
        /// Shape illustrating the definition, to be exposed as the visual symbol of the represented Ideas.
        /// </summary>
        public string RepresentativeShape { get { return __RepresentativeShape.Get(this); } set { __RepresentativeShape.Set(this, value); } }
        protected string RepresentativeShape_ = Shapes.Rectangle;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, string> __RepresentativeShape =
                   new ModelPropertyDefinitor<IdeaDefinition, string>("RepresentativeShape", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RepresentativeShape_, (ins, val) => { ins.RepresentativeShape_ = val; }, false, false,
                                                                      "Representative Shape", "Shape illustrating the definition, to be exposed as the visual symbol of the represented Ideas.");

        /// <summary>
        /// Indicates to connect from/to precise aimed positions inside the Symbol, by default, else from/to the Symbol center.
        /// </summary>
        public bool PreciseConnectByDefault { get { return __PreciseConnectByDefault.Get(this); } set { __PreciseConnectByDefault.Set(this, value); } }
        protected bool PreciseConnectByDefault_ = false;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, bool> __PreciseConnectByDefault =
                   new ModelPropertyDefinitor<IdeaDefinition, bool>("PreciseConnectByDefault", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.PreciseConnectByDefault_, (ins, val) => ins.PreciseConnectByDefault_ = val, false, false,
                                                                    "Precise Connect by default", "Indicates to connect from/to precise aimed positions inside the Symbol, by default, else from/to the Symbol center.");

        /// <summary>
        /// Format to be initially applied to the representations (main) visual symbol.
        /// For Concepts it represents its whole body, and for Relationships it represents the central connector symbol, if any (therefore can be null).
        /// </summary>
        public VisualSymbolFormat DefaultSymbolFormat { get { return __DefaultSymbolFormat.Get(this); } set { __DefaultSymbolFormat.Set(this, value); } }
        protected VisualSymbolFormat DefaultSymbolFormat_ = null;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, VisualSymbolFormat> __DefaultSymbolFormat =
                   new ModelPropertyDefinitor<IdeaDefinition, VisualSymbolFormat>("DefaultSymbolFormat", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DefaultSymbolFormat_, (ins, val) => ins.DefaultSymbolFormat_ = val, false, false,
                                                                                  "Default Symbol Format", "Format to be initially applied to the representations (main) visual symbol. For Concepts it represents its whole body, " +
                                                                                                           "and for Relationships it represents the central connector symbol, if any (therefore can be null).");

        /// <summary>
        /// Gets a sample format (e.g.: for palettes) based on the visualization formats established.
        /// </summary>
        public virtual Drawing GetSampleDrawing(bool IsForCursor = false, bool ShowUpperLeftPointer = false)
        {
            return MasterDrawer.CreateDrawingSample(this, IsForCursor, ShowUpperLeftPointer);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of Concept definitions which are part of this one.
        /// </summary>
        public EditableList<ConceptDefinition> ConceptDefinitions { get; protected set; }
        public static ModelListDefinitor<IdeaDefinition, ConceptDefinition> __ConceptDefinitions =
                   new ModelListDefinitor<IdeaDefinition, ConceptDefinition>("ConceptDefinitions", EEntityMembership.InternalCoreExclusive, ins => ins.ConceptDefinitions, (ins, coll) => ins.ConceptDefinitions = coll,
                                                                             "Concept Definitions", "Collection of Concept definitions which are part of this one.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of Relationship definitions which are part of this one.
        /// </summary>
        public EditableList<RelationshipDefinition> RelationshipDefinitions { get; protected set; }
        public static ModelListDefinitor<IdeaDefinition, RelationshipDefinition> __RelationshipDefinitions =
                   new ModelListDefinitor<IdeaDefinition, RelationshipDefinition>("RelationshipDefinitions", EEntityMembership.InternalCoreExclusive, ins => ins.RelationshipDefinitions, (ins, coll) => ins.RelationshipDefinitions = coll,
                                                                                  "Relationship Definitions", "Collection of Relationship definitions which are part of this one.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets all the declared children Idea Definitions of this one.
        /// </summary>
        public IEnumerable<IdeaDefinition> IdeaDefinitions
        {
            get
            {
                return this.ConceptDefinitions.Concat<IdeaDefinition>(this.RelationshipDefinitions);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the registered definitions (of Concepts and Relationships).
        /// </summary>
        public IEnumerable<IdeaDefinition> Definitions
        {
            get
            {
                foreach (var Def in this.ConceptDefinitions)
                    yield return Def;

                foreach (var Def in this.RelationshipDefinitions)
                    yield return Def;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the directly related implementing Ideas of this Definition.
        /// </summary>
        public IEnumerable<Idea> GetDependentIdeas()
        {
            var Result = Enumerable.Empty<Idea>();

            if (this.OwnerDomain.OwnerComposition != null)
                if (this is Domain)
                    Result = this.OwnerDomain.OwnerComposition.DeclaredIdeas;
                else
                    Result = this.OwnerDomain.OwnerComposition.DeclaredIdeas.Where(idea => idea.IdeaDefinitor.IsEquivalent(this));

            return Result;
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of Detail Designators declared for this Idea definition.
        /// </summary>
        public EditableList<DetailDesignator> DetailDesignators { get; protected set; }
        public static ModelListDefinitor<IdeaDefinition, DetailDesignator> __DetailDesignators =
                   new ModelListDefinitor<IdeaDefinition, DetailDesignator>("DetailDesignators", EEntityMembership.InternalCoreExclusive, ins => ins.DetailDesignators, (ins, coll) => ins.DetailDesignators = coll,
                                                                            "Detail Designators", "Collection of Detail Designators declared for this Idea definition.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of declared Table-Structure Definitions.
        /// This is a composition when Shared (the owner is the Idea Definition), else is an aggregation when Local (the owner is the Idea).
        /// </summary>
        public EditableList<TableDefinition> TableDefinitions { get; protected set; }
        public static ModelListDefinitor<IdeaDefinition, TableDefinition> __TableDefinitions =
                   new ModelListDefinitor<IdeaDefinition, TableDefinition>("TableDefinitions", EEntityMembership.InternalCoreExclusive, ins => ins.TableDefinitions, (ins, coll) => ins.TableDefinitions = coll,
                                                                           "Table-Structure Definitions", "Collection of declared Table-Structure Definitions.");

        /// <summary>
        /// Definition of Custom-Fields (based on a Table-Definition).
        /// </summary>
        public TableDefinition CustomFieldsTableDef { get { return __CustomFieldsTableDef.Get(this); } set { __CustomFieldsTableDef.Set(this, value); } }
        protected TableDefinition CustomFieldsTableDef_ = null;
        public static readonly ModelPropertyDefinitor<IdeaDefinition, TableDefinition> __CustomFieldsTableDef =
                   new ModelPropertyDefinitor<IdeaDefinition, TableDefinition>("CustomFieldsTableDef", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CustomFieldsTableDef_, (ins, val) => ins.CustomFieldsTableDef_ = val, false, false,
                                                                               "Custom-Fields Table-Structure Definition", "Definition of Custom-Fields (based on a Table-Structure Definition).");

        /// <summary>
        /// Text templates used for file generation.
        /// </summary>
        public EditableList<TextTemplate> OutputTemplates { get; protected set; }
        public static ModelListDefinitor<IdeaDefinition, TextTemplate> __OutputTemplates =
                   new ModelListDefinitor<IdeaDefinition, TextTemplate>("OutputTemplates", EEntityMembership.InternalCoreExclusive, ins => ins.OutputTemplates, (ins, coll) => ins.OutputTemplates = coll,
                                                                        "Output-Templates", "Text templates used for file generation.");
        public TextTemplate CurrentTemplate { get { return this.OutputTemplates.FirstOrDefault(tpl => tpl.Language == this.OwnerDomain.CurrentExternalLanguage); }}

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Domain owning this Idea definition.
        /// </summary>
        public Domain OwnerDomain
        {
            get
            {
                if (this is Domain)
                    return (Domain)this;

                return OwnerDefinitor.OwnerDomain;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this supplied Idea Definition is compatible with the specified one, either by being the same or a descendant of it.
        /// </summary>
        /// <returns></returns>
        public bool IsConsidered(IdeaDefinition EvaluatedAncestor)
        {
            if (this.IsEqual(EvaluatedAncestor) || this.AncestorIdeaDef.IsEqual(EvaluatedAncestor))
                return true;

            if (this.AncestorIdeaDef == null)
                return false;

            var Result = this.AncestorIdeaDef.IsConsidered(EvaluatedAncestor);

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this Idea can generate template based files.
        /// </summary>
        public bool CanGenerateFiles
        {
            get
            {
                var HasGlobalTemplate = false;

                if (this is ConceptDefinition)
                    HasGlobalTemplate = this.OwnerDomain.OutputTemplatesForConcepts.Any(tpl => tpl.Text.NullDefault("").Trim() != "");
                else
                    if (this is RelationshipDefinition)
                        HasGlobalTemplate = this.OwnerDomain.OutputTemplatesForRelationships.Any(tpl => tpl.Text.NullDefault("").Trim() != "");

                var Result = (HasGlobalTemplate || this.OutputTemplates.Any(tpl => tpl.Text.NullDefault("").Trim() != ""));
                return Result;
            }
        }

        /// <summary>
        /// Gets the complete file generation Template for the specified Language, based on the Domain's global Template for this kind of Idea Definition,
        /// concatenated to the local Template.
        // POSTPONED: Support multiple templates (from multiple sections in the template-text, separated by config-prefix).
        /// </summary>
        public string GetGenerationFinalTemplate(ExternalLanguageDeclaration Language, string ExplicitTemplateText = null, bool? ExplicitExtendsBaseTemplate = null)
        {
            var BaseTemplate = (this is Domain
                                ? null
                                : (this is ConceptDefinition
                                   ? this.OwnerDomain.OutputTemplatesForConcepts.FirstOrDefault(tpl => tpl.Language == Language)
                                   : this.OwnerDomain.OutputTemplatesForRelationships.FirstOrDefault(tpl => tpl.Language == Language)));

            var BaseTemplateText = (BaseTemplate == null ? "" : BaseTemplate.Text);

            // Notice that overwrite (non extension) is performed only when the template is not empty at the Idea-Def level.
            var Template = this.OutputTemplates.FirstOrDefault(tpl => tpl.Language == Language);
            var TemplateText = ExplicitTemplateText.NullDefault(Template == null ? "" : Template.Text);
            var ExtendsBaseTemplate = ExplicitExtendsBaseTemplate.NullDefaultTo(Template == null ? true : Template.ExtendsBaseTemplate);

            var ExtensionPlaceMark = GenerationManager.GENPAR_PREFIX + GenerationManager.GENKEY_POS_EXTENSION;
            var ExtensionPosition = TemplateText.IndexOf(ExtensionPlaceMark, StringComparison.OrdinalIgnoreCase);

            if (ExtendsBaseTemplate)
            {
                if (ExtensionPosition < 0)
                    TemplateText = BaseTemplateText + TemplateText;
                else
                    TemplateText = BaseTemplateText.ReplaceAt(ExtensionPosition, ExtensionPlaceMark.Length, TemplateText);
            }
            else
                if (ExtensionPosition >= 0)     // Don't forget to remove the place-mark
                    TemplateText = BaseTemplateText.ReplaceAt(ExtensionPosition, ExtensionPlaceMark.Length, "");

            return TemplateText;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<IdeaDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<IdeaDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<IdeaDefinition> __ClassDefinitor = null;

        public new IdeaDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((IdeaDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public IdeaDefinition PopulateFrom(IdeaDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the kind (Domain, ConceptDefinition or RelationshipDefinition) of this idea definition final instance type.
        /// </summary>
        public ModelDefinition DefKind
        {
            get
            {
                ModelDefinition Result = Idea.__ClassDefinitor;

                if (this is Domain)
                    Result = Domain.__ClassDefinitor;
                else
                    if (this is ConceptDefinition)
                        Result = ConceptDefinition.__ClassDefinitor;
                    else
                        if (this is RelationshipDefinition)
                            Result = RelationshipDefinition.__ClassDefinitor;

                return Result;
            }
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public virtual void UpdateVersion()
        {
            if (this.Version != null)
                this.Version.Update();

            if (this.OwnerDefinitor != null)
                this.OwnerDefinitor.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}