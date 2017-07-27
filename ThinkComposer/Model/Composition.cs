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
// File   : Composition.cs
// Object : Instrumind.ThinkComposer.Model.Composition (Class)
//
// Date       Author             Comments
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.11 Néstor Sánchez A.  Start
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.ApplicationProduct;
using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.MetaModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.Model.VisualModel;

/// Business entity schema for Compositions based upon Domain definitions(metadefinitions).
namespace Instrumind.ThinkComposer.Model
{
    /// <summary>
    /// Semantic, informational and visual set of ideas, expressing knowledge about a subject.
    /// This document or book is conformed by the hierarchical nesting of containers (logical documents based on a Composite Concept), starting from a root.
    /// </summary>
    [Serializable]
    public class Composition : Concept, IModelEntity, IModelClass<Composition>, ISphereModel
    {
        /// <summary>
        /// File extension of Composition file names.
        /// </summary>
        public const string FILE_EXTENSION_COMPOSITION = "tcom";

        /// <summary>
        /// Document suffix for ThinkComposer Composition Graph.
        /// </summary>
        public const string DOCSUF_COMPOSITION = "TCG";

        /// <summary>
        /// Default pictogram of this class.
        /// </summary>
        public static readonly ImageSource DefaultPictogram = Display.GetAppImage("page_white.png");

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Composition()
        {
            __ClassDefinitor = new ModelClassDefinitor<Composition>("Composition", Concept.__ClassDefinitor, "Composition",
                                                                    "Semantic, informational and visual set of ideas, expressing knowledge about a subject. " +
                                                                    "This document or book is conformed by the hierarchical nesting of containers (logical documents based on a Composite Concept), starting from a root.");
            __ClassDefinitor.DeclareCollection(__UsedDomains);
            __ClassDefinitor.DeclareCollection(__RegisteredIdeas);

            __ClassDefinitor.DeclareProperty(__CompositionDefinitor);
            __ClassDefinitor.DeclareProperty(__ActiveView);
            __ClassDefinitor.DeclareProperty(__ViewsPrefix);
            __ClassDefinitor.DeclareProperty(__CentralizedStoreBoxSharedReferences);

            // Register filters for code-generation
            DotLiquid.Template.RegisterFilter(typeof(Instrumind.ThinkComposer.Composer.Generation.TemplateFilters));

            // Register classes exposed to code-generation
            var ExposedTypes = new List<Tuple<Type, bool, string[], string[]>>();

            // ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(Object)));
            // ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(Type)));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(ModelDefinition)));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(ImageSource)));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(MOwnership), true));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(MAssignment), true));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(typeof(StoreBoxBase), true));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(UniqueElement.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(SimpleElement.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(FormalElement.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(SimplePresentationElement.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(FormalPresentationElement.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Domain.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Composition.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(VersionCard.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(MetaDefinition.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(IdeaDefinition.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(ConceptDefinition.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(RelationshipDefinition.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(LinkRoleDefinition.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Idea.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Concept.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Relationship.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(TableDefinition.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(FieldDefinition.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(DetailDesignator.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(AttachmentDetailDesignator.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(TableDetailDesignator.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(LinkDetailDesignator.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(ContainedDetail.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Attachment.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Link.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(InternalLink.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(ResourceLink.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(Table.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(TableRecord.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(VisualRepresentation.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(ConceptVisualRepresentation.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(RelationshipVisualRepresentation.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(VisualElement.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(VisualSymbol.__ClassDefinitor));
            ExposedTypes.Add(ExposeForTemplateBasedExtraction(VisualConnector.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(MarkerAssignment.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(RoleBasedLink.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(View.__ClassDefinitor));

            ExposedTypes.Add(ExposeForTemplateBasedExtraction(VisualComplement.__ClassDefinitor, false, "Content"));

            // For Documentation generation:
            //T PrintAllExposedApi(ExposedTypes);

            /* IMPORTANT: THE EXPOSED CLASSES AND MEMEBERS MUST SUPPORT THE NEXT TEMPLATE...
                ====
                Class template for {{Name}}

                {% for Detail in Details  %}

                Detail: {{Detail}}
                Detail-Designation: {{Detail.Designation}}
                Detail-Designation.Name: {{Detail.Designation.Name}}

                {% endfor -%}
                ====
             */
        }

        /// <summary>
        /// Register a model-class type as exposed to template-based extraction (such as code generation).
        /// Accepts a list of explicit Members to be exposed.
        /// </summary>
        internal static Tuple<Type, bool, string[], string[]> ExposeForTemplateBasedExtraction(MModelClassDefinitor ClassDefinitor,
                                                                                               bool IsGenericBase = false, params string[] ExplicitMembers)
        {
            return ExposeForTemplateBasedExtraction(ClassDefinitor.DeclaringType, IsGenericBase, ExplicitMembers);
        }

        /// <summary>
        /// Register a type as exposed to template-based extraction (such as code generation),
        /// storing the Type, generic-base indication and explicit members.
        /// </summary>
        internal static Tuple<Type, bool, string[], string[]> ExposeForTemplateBasedExtraction(Type DeclaringType,
                                                                                               bool IsGenericBase = false, params string[] ExplicitMembers)
        {
            if (DeclaringType == null)
                return null;

            var AllowedMembers = DeclaringType.GetProperties().Select(pi => pi.Name).ToArray();
            AllowedMembers = AllowedMembers.Concat(ExplicitMembers == null
                                                   ? Enumerable.Empty<string>()
                                                   : ExplicitMembers.Except(AllowedMembers) // Exclusion to avoid duplicate the explicit/required members
                                                   ).OrderBy(m => m).ToArray();

            var Duplicates = AllowedMembers.Duplicates().ToArray();

            // Force exclusion of duplicates (instance properties hiding others with the "new" C# modifier)
            if (Duplicates.Length > 0)
                AllowedMembers = AllowedMembers.Except(Duplicates).ToArray();

            /* Alternative (problem is that the properties not declared in the class-definitor, such as Idea.Definitor, must be included manually)
            var AllowedMembers = ClassDefinitor.Properties.Select(p => p.TechName)
                                                    .Concat(Members.NullDefault(Enumerable.Empty<string>())).Distinct().ToArray(); */

            DotLiquid.Template.RegisterSafeType(DeclaringType, AllowedMembers);

            return Tuple.Create(DeclaringType, IsGenericBase, AllowedMembers, ExplicitMembers);
        }

        public static void PrintAllExposedApi(IList<Tuple<Type, bool, string[], string[]>> RegTypes)
        {
            Func<Type, bool> IsExposed =
                ((type) =>
                    type.IsPrimitive || (type.IsValueType && !type.IsEnum) //-? || PropInfo.PropertyType.IsArray
                    || type.FullName == "System.String"
                    || RegTypes.Any(rtype => (rtype.Item2 ? rtype.Item1.IsAssignableFrom(type) : rtype.Item1 == type))
                );

            foreach (var RegType in RegTypes.OrderBy(rt => rt.Item1.Name))
            {
                var DeclaringType = RegType.Item1;
                var AllowedMembers = RegType.Item3;

                // Supress ancestor's memebers
                var Definitor = MModelClassDefinitor.DeclaredClassDefinitors.FirstOrDefault(dd => dd.TechName == DeclaringType.Name);

                if (Definitor != null)
                {
                    Console.WriteLine("Class\t" + DeclaringType.Name + "\t" +
                                      (Definitor.AncestorDefinitor == null ? "[NONE]" : Definitor.AncestorDefinitor.TechName) + "\t" +
                                      (Definitor == null ? "[System]" : Definitor.Summary) + "\t" +
                                      "");

                    foreach (var Member in AllowedMembers)
                    {
                        var MembKind = "Property";
                        var PropInfo = DeclaringType.GetProperty(Member);
                        var PropName = PropInfo.Name;
                        var PropDesc = "[Internal]";

                        // Only show non-ancestor properties, primitive and exposed types.
                        if (Definitor != null && PropInfo.DeclaringType.Name == Definitor.TechName
                            && (IsExposed(PropInfo.PropertyType) || PropName.IsIn(RegType.Item4)
                                || ((PropInfo.PropertyType.Name.StartsWith("EditableList")
                                     || PropInfo.PropertyType.Name.StartsWith("IEnumerable")) && PropInfo.PropertyType.IsGenericType
                                    && IsExposed(PropInfo.PropertyType.GetGenericArguments()[0]))
                                || (PropInfo.PropertyType.Name.StartsWith("EditableDictionary") && PropInfo.PropertyType.IsGenericType
                                    && IsExposed(PropInfo.PropertyType.GetGenericArguments()[0])
                                    && IsExposed(PropInfo.PropertyType.GetGenericArguments()[1]))))
                        {
                            var MembDef = Definitor.Members.FirstOrDefault(m => m.TechName == PropName);
                            var IsUsed = true;

                            if (MembDef == null)
                                PropDesc = PropInfo.GetDescription();
                            else
                            {
                                if (MembDef is MModelCollectionDefinitor)
                                    MembKind = "Collection";

                                PropDesc = MembDef.Summary;
                                IsUsed = !MembDef.HasPendingImplementation;
                            }

                            if (IsUsed && !PropDesc.IsAbsent())
                                Console.WriteLine(MembKind + "\t" + PropName + "\t" +
                                                  PropInfo.PropertyType.GetSimplifiedName() + "\t" + PropDesc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="EntityEditor">Edit engine constructing this Composition.</param>
        /// <param name="MainDefinitor">Main domain defining the Composition.</param>
        /// <param name="Name">Name of the Composition.</param>
        /// <param name="Tech-Name">Technical Name of the Composition.</param>
        /// <param name="Summary">Summary of the Composition.</param>
        /// <param name="Pictogram">Image representing the Composition.</param>
        public Composition(EntityEditEngine EntityEditor, Domain MainDefinitor, string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(null, MainDefinitor, Name, TechName, Summary, Pictogram)
        {
            General.ContractRequiresNotNull(EntityEditor, MainDefinitor);

            this.EditEngine = EntityEditor;

            this.GlobalId = EntityEditor.GlobalId;

            this.CompositionDefinitor = MainDefinitor;
            this.ConceptDefinitor = MainDefinitor.Assign<ConceptDefinition>();
            this.ConceptType = EConceptType.Individual;

            this.UsedDomains = new EditableList<Domain>(__UsedDomains.TechName, this);
            this.UsedDomains.AddNew(MainDefinitor);

            this.RegisteredIdeas = new EditableList<Idea>(__RegisteredIdeas.TechName, this);

            this.Version = new VersionCard();

            // MIME-Type: "application/x-instrumind-thinkcomposer-composition"
            this.Classification = new ClassificationCard(FileDataType.FileTypeComposition.MimeType);
        }

        /// <summary>
        /// Protected Constructor for Agent descendants.
        /// </summary>
        protected Composition()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        public void Initialize()
        {
            StoreBox.RegisterSharedReferencesCentralizer(this.GlobalId, this.CentralizedStoreBoxSharedReferences);

            if (this.RootView != null)
                this.RootView.Initialize();

            this.IdentificationScopeController = new IdentificationController(General.IsValidIdentifier, General.IsValidText,
                                                                              txt => General.TextToIdentifier(txt, true),
                                                                              idn => General.IdentifierToText(idn));
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Starts the interactive visual editing of the Composition.
        /// </summary>
        public void Start()
        {
            if (this.RootView == null)
                this.RootView = this.OpenCompositeView(this.CompositionDefinitor);

            this.RootView.Initialize();
            this.RootView.ShowAll();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Graphic representation of the object.
        /// </summary>
        public override ImageSource Pictogram
        {
            // Gets the stored image only if it is Bitmap-based (assuming an external source) and is not that of the Domain.
            get
            {
                var Result = (base.Pictogram == null || base.Pictogram is DrawingImage || base.Pictogram == Domain.DefaultPictogram
                              ? DefaultPictogram : base.Pictogram);

                /* ALTERNATIVE (But snapshot is too little):
                ImageSource Result = base.Pictogram;

                if (base.Pictogram == null || base.Pictogram is DrawingImage || base.Pictogram == Domain.DefaultPictogram)
                    if (this.ActiveView != null)
                    {
                        var Snapshot = this.ActiveView.ToSnapshot(DocumentEngine.PART_SNAPSHOT_WIDTH,
                                                                  DocumentEngine.PART_SNAPSHOT_HEIGHT);
                        if (Snapshot.Item1 == null)
                            Result = DefaultPictogram;
                        else
                            Result = Snapshot.Item1.ToDrawingImage();
                    }
                    else
                        Result = DefaultPictogram; */

                return Result;
            }
            set { base.Pictogram = value; }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Centralized references dictionary, associated with this Composition.
        /// This allows the Serialization of Store-Box contained objects which are required to be shared (such as Images, hence saving resources).
        /// </summary>
        public Dictionary<Guid, byte[]> CentralizedStoreBoxSharedReferences { get { return __CentralizedStoreBoxSharedReferences.Get(this); } private set { __CentralizedStoreBoxSharedReferences.Set(this, value); } }
        protected Dictionary<Guid, byte[]> CentralizedStoreBoxSharedReferences_ = new Dictionary<Guid, byte[]>();
        public static readonly ModelPropertyDefinitor<Composition, Dictionary<Guid, byte[]>> __CentralizedStoreBoxSharedReferences =
                           new ModelPropertyDefinitor<Composition, Dictionary<Guid, byte[]>>("CentralizedStoreBoxSharedReferences", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CentralizedStoreBoxSharedReferences_, (ins, val) => ins.CentralizedStoreBoxSharedReferences_ = val, false, true,
                                                                                    "Centralized Store-Box Shared References", "References a centralized references dictionary, associated with this Composition. " +
                                                                                                                                        "This allows the Serialization of Store-Box contained objects which are required to be shared (such as Images, hence saving resources).");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initial and central View of the Composition.
        /// </summary>
        [Description("Initial and central View of the Composition.")]
        public View RootView
        {
            get { return (View)this.CompositeActiveView; }
            set
            {
                this.CompositeActiveView = value;

                if (this.ActiveView == null)
                    this.ActiveView = value;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Active View of the composition.
        /// </summary>
        public View ActiveView { get { return __ActiveView.Get(this); } internal set { __ActiveView.Set(this, value); } }
        protected View ActiveView_ = null;
        public static readonly ModelPropertyDefinitor<Composition, View> __ActiveView =
                           new ModelPropertyDefinitor<Composition, View>("ActiveView", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.ActiveView_, (ins, val) => ins.ActiveView_ = val, false, true,
                                                                         "Active View", "Active View of the composition.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public override string ToString()
        {
            return this.Name + " [" + this.CompositionDefinitor.ToStringAlways() + "]";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Domain definition of this Composition.
        /// </summary>
        public Domain CompositionDefinitor { get { return __CompositionDefinitor.Get(this); } set { __CompositionDefinitor.Set(this, value); } }
        protected Domain CompositionDefinitor_;
        public static readonly ModelPropertyDefinitor<Composition, Domain> __CompositionDefinitor =
                           new ModelPropertyDefinitor<Composition, Domain>("CompositionDefinitor", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.CompositionDefinitor_, (ins, val) => ins.CompositionDefinitor_ = val, false, true,
                                                                           "Composition Definitor", "Domain definition of this Composition.");

        public override MetaDefinition Definitor { get { return this.CompositionDefinitor; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Prefix for naming related views of this document.
        /// </summary>
        public string ViewsPrefix { get { return __ViewsPrefix.Get(this); } set { __ViewsPrefix.Set(this, value); } }
        protected string ViewsPrefix_ = "View";
        public static readonly ModelPropertyDefinitor<Composition, string> __ViewsPrefix =
                           new ModelPropertyDefinitor<Composition, string>("ViewsPrefix", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ViewsPrefix_, (ins, val) => ins.ViewsPrefix_ = val, false, true,
                                                                           "Views Prefix", "Prefix for naming related views of this document.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Collection of used Domains in this Composition.
        /// </summary>
        public EditableList<Domain> UsedDomains { get; protected set; }
        public static ModelListDefinitor<Composition, Domain> __UsedDomains =
                   new ModelListDefinitor<Composition, Domain>("UsedDomains", EEntityMembership.External, ins => ins.UsedDomains, (ins, coll) => ins.UsedDomains = coll,
                                                               "Used Domains", "Collection of used Domains in this Composition.");

        /// <summary>
        /// Centralized collection of Ideas.
        /// </summary>
        protected EditableList<Idea> RegisteredIdeas { get; set; }
        public static ModelListDefinitor<Composition, Idea> __RegisteredIdeas =
                   new ModelListDefinitor<Composition, Idea>("RegisteredIdeas", EEntityMembership.InternalCoreExclusive, ins => ins.RegisteredIdeas, (ins, coll) => ins.RegisteredIdeas = coll,
                                                             "Registered Ideas", "Centralized collection of Ideas.");

        /// <summary>
        /// Returns all the current declared/registered Ideas of this Composition.
        /// </summary>
        public IEnumerable<Idea> DeclaredIdeas { get { return RegisteredIdeas; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Registers the supplied Idea as declared on this Composition.
        /// </summary>
        public void RegisterIdea(Idea Idea)
        {
            this.RegisteredIdeas.AddNew(Idea);
        }

        /// <summary>
        /// Unegisters the supplied Idea as undeclared from this Composition.
        /// </summary>
        public void UnregisterIdea(Idea Idea)
        {
            this.RegisteredIdeas.Remove(Idea);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns a collection of the available Table-Definitions of each referenced Domain.
        /// </summary>
        public IEnumerable<TableDefinition> GetTableDefinitions()
        {
            return this.UsedDomains.SelectMany(dom => dom.TableDefinitions);
            /*-
            return General.SetEmptyVariable(ref this.TableDefinitionsEnumerable,
                                            () => new ExtendedEnumerable<TableDefinition,TableDefinition,Domain>
                                                  (tdef => Tuple.Create<bool,TableDefinition>(true,tdef),
                                                   dom => dom.TableDefinitions,
                                                   this.UsedDomains),
                                            extenum => extenum.UpdateCollectionContainmentAssignments(this.UsedDomains)); */
        }
        /*- [NonSerialized]
        private ExtendedEnumerable<TableDefinition, TableDefinition, Domain> TableDefinitionsEnumerable = null; */
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IPersistable Members

        public EExistenceStatus ExistenceStatus
        {
            get { return this.EditEngine.ExistenceStatus; }
        }

        public Uri Location
        {
            get
            {
                if (this.Location_ == null)
                    if (!Uri.TryCreate("/" + this.TechName + "." + DOCSUF_COMPOSITION, UriKind.Relative, out Location_))
                        throw new UsageAnomaly("Cannot create Composition-Graph location from key.", this);

                return this.Location_;
            }
        }
        [NonSerialized]
        protected Uri Location_ = null;

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IIdentificationScope Members

        public IdentificationController IdentificationScopeController { get { return this.IdentificationScopeController_; } protected set { this.IdentificationScopeController_ = value; } }
        [NonSerialized]
        private IdentificationController IdentificationScopeController_ = null;

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Composition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Composition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Composition> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new Composition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Composition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Composition PopulateFrom(Composition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public CompositionEngine Engine { get { return (CompositionEngine)this.EditEngine; } }

        #region ISphereModel Members

        public string Title
        {
            get
            {
                if (this.Engine.IsForEditDomain)
                    return this.CompositeContentDomain.Title;

                return this.ToString() + (this.ExistenceStatus == EExistenceStatus.Modified ? General.STR_SIGNAL_MODIFICATION : "");
            }
        }

        public string Overview
        {
            get
            {
                if (this.Engine.IsForEditDomain)
                    return this.CompositeContentDomain.Overview;

                return this.Summary.AbsentDefault(this.Name);
            }
        }

        public ImageSource Icon
        {
            get
            {
                if (this.Engine.IsForEditDomain)
                    return this.CompositeContentDomain.Icon;

                return this.Pictogram;
            }
        }

        public IDocumentView ActiveDocumentView { get { return this.ActiveView; } }

        public DocumentEngine DocumentEditEngine { get { return this.Engine; } }

        public string SimplifiedLocation
        {
            get
            {
                if (this.Engine.IsForEditDomain)
                {
                    if (this.Engine.DomainLocation == null)
                        return "[Unsaved Document (No Folder)]";

                    string Route = Path.GetFileName(this.Engine.DomainLocation.LocalPath) + " (" +
                                   Path.GetDirectoryName(this.Engine.DomainLocation.LocalPath) + ")";
                    return Route;
                }

                return this.SimplifiedLocation_;
            }
            set
            {
                this.SimplifiedLocation_ = value;
                NotifyPropertyChange("SimplifiedLocation");
            }
        }
        private string SimplifiedLocation_ = "[Unsaved Document (No Folder)]";

        // IMPORTANT!: Here NavigableElements (without the final '_') was directly referenced to regenerate the extended-enumerable!
        //             In this case, the NavigableItems property is used to Refresh the entire items tree view.
        public System.Collections.IEnumerable NavigableItems { get { return this.NavigableElements; } }

        #endregion

        public bool ShowNavigableItemsOrderByName { get; set; }

        public bool ShowInterrelatedItemsOrderByName { get; set; }
    }
}