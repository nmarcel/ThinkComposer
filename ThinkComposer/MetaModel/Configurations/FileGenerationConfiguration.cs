//
// Copyright (C) 2011-2015 Néstor Marcel Sánchez Ahumada.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : FileGenerationConfiguration.cs
// Object : Instrumind.ThinkComposer.MetaModel.Configurations.FileGenerationConfiguration (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2013.02.07 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Printing;
using System.Text;
using System.Windows.Media;
using System.Windows;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Provides configuration information for meta-models.
namespace Instrumind.ThinkComposer.MetaModel.Configurations
{
    /// <summary>
    /// Stores report generation parameters.
    /// </summary>
    [Serializable]
    public class FileGenerationConfiguration : IModelEntity, IModelClass<FileGenerationConfiguration>
    {
        public const string DEF_COMPOCONTENT_SUBDIR_SUFFIX = "_content";

        /// <summary>
        /// Static constructor
        /// </summary>
        static FileGenerationConfiguration()
        {
            __ClassDefinitor = new ModelClassDefinitor<FileGenerationConfiguration>("FileGenerationConfiguration", null, "File Generation Configuration",
                                                                                    "Stores file generation parameters.");

            __ClassDefinitor.DeclareProperty(__TargetDirectory);
            __ClassDefinitor.DeclareProperty(__Language);
            __ClassDefinitor.DeclareProperty(__CreateCompositionRootDirectory);
            __ClassDefinitor.DeclareProperty(__UseIdeaTechNameForFileNaming);
            __ClassDefinitor.DeclareProperty(__UseTechNamesAsProgramIdentifiers);
            __ClassDefinitor.DeclareProperty(__CompositeContentSubdirSuffix);
            // __ClassDefinitor.DeclareProperty(__GenerateFilesForConcepts);
            __ClassDefinitor.DeclareProperty(__GenerateFilesForRelationships);
            // __ClassDefinitor.DeclareProperty(__CreateCompositesSubdirs);

            __ClassDefinitor.DeclareCollection(__ExcludedIdeasGlobalIds);
        }

        public FileGenerationConfiguration()
        {
            this.ExcludedIdeasGlobalIds = new EditableList<string>("SelectedIdeasGlobalIds", this);
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization.
        /// </summary>
        [OnDeserialized]
        protected void Initialize(StreamingContext context = default(StreamingContext))
        {
            if (this.ExcludedIdeasGlobalIds == null)
                this.ExcludedIdeasGlobalIds = new EditableList<string>("ExcludedIdeasGlobalIds", this);
        }
        /// <summary>
        /// Target directory
        /// </summary>
        public string TargetDirectory { get { return __TargetDirectory.Get(this); } set { __TargetDirectory.Set(this, value); } }
        protected string TargetDirectory_ = "";
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, string> __TargetDirectory =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, string>("TargetDirectory", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.TargetDirectory_, (ins, val) => ins.TargetDirectory_ = val, false, false,
                                                                                           "Generate files for Concepts", "Generates files for Concepts of the Composition.");

        /// <summary>
        /// External Language of the files to be generated.
        /// </summary>
        public ExternalLanguageDeclaration Language { get { return __Language.Get(this); } set { __Language.Set(this, value); } }
        protected ExternalLanguageDeclaration Language_ = null;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, ExternalLanguageDeclaration> __Language =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, ExternalLanguageDeclaration>("Language", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.Language_, (ins, val) => ins.Language_ = val, false, false,
                                                                                                                "Language", "External Language of the files to be generated.");

        /// <summary>
        /// Indicates wether to create the Composition Root directory inside the target directory (e.g.: 'c:\\MyProjects\\MyTargetDir\\MyCompoRoot').
        /// </summary>
        public bool CreateCompositionRootDirectory { get { return __CreateCompositionRootDirectory.Get(this); } set { __CreateCompositionRootDirectory.Set(this, value); } }
        protected bool CreateCompositionRootDirectory_ = false;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, bool> __CreateCompositionRootDirectory =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, bool>("CreateCompositionRootDirectory", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CreateCompositionRootDirectory_, (ins, val) => ins.CreateCompositionRootDirectory_ = val, false, false,
                                                                                         "Create Composition Root Directory", "Indicates wether to create the Composition Root directory inside the target directory (e.g.: 'c:\\MyProjects\\MyTargetDir\\MyCompoRoot').");

        /// <summary>
        /// Indicates to use the Idea's Tech-Name for naming the generated files for that Idea, else uses the Name.
        /// </summary>
        public bool UseIdeaTechNameForFileNaming { get { return __UseIdeaTechNameForFileNaming.Get(this); } set { __UseIdeaTechNameForFileNaming.Set(this, value); } }
        protected bool UseIdeaTechNameForFileNaming_ = true;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, bool> __UseIdeaTechNameForFileNaming =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, bool>("UseIdeaTechNameForFileNaming", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.UseIdeaTechNameForFileNaming_, (ins, val) => ins.UseIdeaTechNameForFileNaming_ = val, false, false,
                                                                                         "Use Idea Tech-Name for file naming", "Indicates to use the Idea's Tech-Name for naming the generated files for that Idea, else uses the Name.");

        /// <summary>
        /// Text suffix to be appended to directories containing Idea's Composite-Content (e.g.: 'MyIdea_content').
        /// </summary>
        // Not probable: If you generate source-code, it is highly recommended to use this feature in order to avoid a Namespace/Package having the same name of a Class.
        public string CompositeContentSubdirSuffix { get { return __CompositeContentSubdirSuffix.Get(this); } set { __CompositeContentSubdirSuffix.Set(this, value); } }
        protected string CompositeContentSubdirSuffix_ = "";    // Not really necessary: DEF_COMPOCONTENT_SUBDIR_SUFFIX;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, string> __CompositeContentSubdirSuffix =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, string>("CompositeContentSubdirSuffix", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CompositeContentSubdirSuffix_, (ins, val) => ins.CompositeContentSubdirSuffix_ = val, false, false,
                                                                                           "Composite-content Subdirectory Suffix", "Text suffix to be optionally appended to directories containing Idea's Composite-Content (e.g.: use '_content' to get 'MyIdea_content').");

        /// <summary>
        /// Indicates that Tech-Names will be used as Programming language identifiers instead of file names.
        /// (So, they will be enforced to be composed of letter, digits or '_', and to start with a letter or '_').
        /// </summary>
        public bool UseTechNamesAsProgramIdentifiers { get { return __UseTechNamesAsProgramIdentifiers.Get(this); } set { __UseTechNamesAsProgramIdentifiers.Set(this, value); } }
        protected bool UseTechNamesAsProgramIdentifiers_ = false;
        private static ModelPropertyDefinitor<FileGenerationConfiguration, bool> __UseTechNamesAsProgramIdentifiers =
                   new ModelPropertyDefinitor<FileGenerationConfiguration, bool>("UseTechNamesAsProgramIdentifiers", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.UseTechNamesAsProgramIdentifiers_, (ins, val) => ins.UseTechNamesAsProgramIdentifiers_ = val, false, false,
                                                                                 "Use Tech-Names as programming language identifiers", "Indicates that Tech-Names will be used as Programming language identifiers instead of file names. " +
                                                                                                                                       "(So, they will be enforced to be composed of letter, digits or '_', and to start with a letter or '_').");

        /* CANCELLED: Always files will be generated for Concepts (only Relationships' files are optional)
        /// <summary>
        /// Generates files for Concepts of the Composition.
        /// </summary>
        public bool GenerateFilesForConcepts { get { return __GenerateFilesForConcepts.Get(this); } set { __GenerateFilesForConcepts.Set(this, value); } }
        protected bool GenerateFilesForConcepts_ = true;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, bool> __GenerateFilesForConcepts =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, bool>("GenerateFilesForConcepts", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.GenerateFilesForConcepts_, (ins, val) => ins.GenerateFilesForConcepts_ = val, false, false,
                                                                                         "Generate files for Concepts", "Generates files for Concepts of the Composition."); */

        /// <summary>
        /// Generates files for Relationships of the Composition.  Normally not necessary, due to inclusion of Relationship as Concept's generation output.
        /// </summary>
        public bool GenerateFilesForRelationships { get { return __GenerateFilesForRelationships.Get(this); } set { __GenerateFilesForRelationships.Set(this, value); } }
        protected bool GenerateFilesForRelationships_ = false;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, bool> __GenerateFilesForRelationships =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, bool>("GenerateFilesForRelationships", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.GenerateFilesForRelationships_, (ins, val) => ins.GenerateFilesForRelationships_ = val, false, false,
                                                                                         "Generate files for Relationships", "Generates files for Relationships of the Composition. Normally not necessary, due to inclusion of Relationship as Concept's generation output.");

        /* CANCELLED: The path could become too long and crash the generation
        /// <summary>
        /// Creates subdirectories for the content of composite Ideas.
        /// Otherwise, files will be named prefixing their hierarchical containment path (e.g. 'company.division.department.employee.txt').
        /// </summary>
        public bool CreateCompositesSubdirs { get { return __CreateCompositesSubdirs.Get(this); } set { __CreateCompositesSubdirs.Set(this, value); } }
        protected bool CreateCompositesSubdirs_;
        public static readonly ModelPropertyDefinitor<FileGenerationConfiguration, bool> __CreateCompositesSubdirs =
                           new ModelPropertyDefinitor<FileGenerationConfiguration, bool>("CreateCompositesSubdirs", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.CreateCompositesSubdirs_, (ins, val) => ins.CreateCompositesSubdirs_ = val, false, false,
                                                                                         "Create subdirectories for composite/nested content", "Creates subdirectories for the content of composite Ideas. Otherwise, files will be named prefixing their hierarchical containment path (e.g. 'company.division.department.employee.txt')."); */

        /// <summary>
        /// Collection of Globa-Ids of the Ideas explicitly excluded for generation.
        /// </summary>
        public EditableList<string> ExcludedIdeasGlobalIds { get; protected set; }
        public static ModelListDefinitor<FileGenerationConfiguration, string> __ExcludedIdeasGlobalIds =
                   new ModelListDefinitor<FileGenerationConfiguration, string>("ExcludedIdeasGlobalIds", EEntityMembership.InternalCoreExclusive, ins => ins.ExcludedIdeasGlobalIds, (ins, coll) => ins.ExcludedIdeasGlobalIds = coll,
                                                                               "Excluded Ideas Global-Ids", "Collection of Globa-Ids of the Ideas explicitly excluded for generation.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<FileGenerationConfiguration> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<FileGenerationConfiguration> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<FileGenerationConfiguration> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public FileGenerationConfiguration CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((FileGenerationConfiguration)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public FileGenerationConfiguration PopulateFrom(FileGenerationConfiguration SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelEntity Members

        public EntityEditEngine EditEngine { get { return EntityEditEngine.ObtainEditEngine(this, EditEngine_); } set { EditEngine_ = value; } }
        [NonSerialized]
        private EntityEditEngine EditEngine_ = null;

        public virtual void RefreshEntity() { }

        public virtual MEntityInstanceController Controller { get { return this.Controller_; } set { this.Controller_ = value; } }
        [NonSerialized]
        private MEntityInstanceController Controller_ = null;

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied definitor, has changed.
        /// </summary>
        public void NotifyPropertyChange(MModelPropertyDefinitor PropertyDefinitor)
        {
            this.NotifyPropertyChange(PropertyDefinitor.TechName);
        }

        /// <summary>
        /// Notifies all entity subscriptors that a property, identified by the supplied name, has changed.
        /// </summary>
        public void NotifyPropertyChange(string PropertyName)
        {
            var Handler = PropertyChanged;

            if (Handler != null)
                Handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public IdeaSelection CurrentSelection { get { return this.CurrentSelection_; } set { this.CurrentSelection_ = value; } }
        [NonSerialized]
        private IdeaSelection CurrentSelection_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}
