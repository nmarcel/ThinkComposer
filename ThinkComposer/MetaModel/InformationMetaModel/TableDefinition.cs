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
// File   : TableDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.TableDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.08 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.Model.InformationModel;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a table structure type.
    /// </summary>
    [Serializable]
    public class TableDefinition : MetaDefinition, IModelEntity, IModelClass<TableDefinition>, IVersionUpdater
    {
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static TableDefinition()
        {
            __ClassDefinitor = new ModelClassDefinitor<TableDefinition>("TableDefinition", MetaDefinition.__ClassDefinitor, "Table-Structure Definition",
                                                                        "Defines a Table-Structure type.");
            __ClassDefinitor.DeclareProperty(__OwnerDomain);
            __ClassDefinitor.DeclareCollection(__Categories);
            __ClassDefinitor.DeclareCollection(__StorageStructureFieldDefs);
            __ClassDefinitor.DeclareCollection(__FieldDefinitions);
            __ClassDefinitor.DeclareCollection(__UniqueKeyFieldDefs);
            __ClassDefinitor.DeclareCollection(__LabelFieldDefs);
            __ClassDefinitor.DeclareCollection(__DominantRefFieldDefs);

            __UniqueKeyFieldDefs.HasPendingImplementation = true;
            __DominantRefFieldDefs.HasPendingImplementation = true;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="OwnerDomain">Domain owning this TableDefinition.</param>
        /// <param name="Name">Name of the TableDefinition.</param>
        /// <param name="TechName">Technical Name of the TableDefinition.</param>
        /// <param name="Summary">Summary of the TableDefinition.</param>
        /// <param name="Pictogram">Image representing the TableDefinition.</param>
        public TableDefinition(Domain OwnerDomain, string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
             : base(Name, TechName, Summary, Pictogram.NullDefault(Display.GetAppImage("table.png")), true)
        {
            General.ContractRequiresNotNull(OwnerDomain);

            this.OwnerDomain = OwnerDomain;

            this.Categories = new EditableList<MetaCategory<TableDefinition>>(__Categories.TechName, this);

            this.StorageStructureFieldDefs = new EditableList<FieldDefinition>(__StorageStructureFieldDefs.TechName, this);

            this.FieldDefinitions = new EditableList<FieldDefinition>(__FieldDefinitions.TechName, this);
            this.UniqueKeyFieldDefs = new EditableList<FieldDefinition>(__UniqueKeyFieldDefs.TechName, this);
            this.LabelFieldDefs = new EditableList<FieldDefinition>(__LabelFieldDefs.TechName, this);
            this.DominantRefFieldDefs = new EditableList<FieldDefinition>(__DominantRefFieldDefs.TechName, this);
        }

        /// <summary>
        /// Internal Constructor for Agent descendants.
        /// </summary>
        internal TableDefinition()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Domain owning this Table-Structure Definition.
        /// </summary>
        public Domain OwnerDomain { get { return __OwnerDomain.Get(this); } internal set { __OwnerDomain.Set(this, value); } }
        protected Domain OwnerDomain_ = null;
        public static readonly ModelPropertyDefinitor<TableDefinition, Domain> __OwnerDomain =
                           new ModelPropertyDefinitor<TableDefinition, Domain>("OwnerDomain", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerDomain_, (ins, val) => ins.OwnerDomain_ = val, false, false,
                                                                               "Owner Domain", "Domain owning this Table-Structure Definition.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// References the categories related to this TableDefinition.
        /// </summary>
        public EditableList<MetaCategory<TableDefinition>> Categories { get; protected set; }
        public static ModelListDefinitor<TableDefinition, MetaCategory<TableDefinition>> __Categories =
                   new ModelListDefinitor<TableDefinition, MetaCategory<TableDefinition>>("Categories", EEntityMembership.External, ins => ins.Categories, (ins, coll) => ins.Categories = coll,
                                                                                          "Categories", "References the categories related to this TableDefinition.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the directly related Table detail designators, declaring this Table-Structure Definition as definitor for final data Tables.
        /// </summary>
        public IEnumerable<TableDetailDesignator> GetDependentDesignators(bool IncludeDomainDesignators = true)
        {
            var Retriever = this.OwnerDomain.Definitions.SelectMany(def => def.DetailDesignators.Where(dsn => dsn.Definitor == this))
                            .Concat(this.OwnerDomain.GetDependentIdeas()
                                        .SelectMany(idea => idea.Details.Where(det => det != null && det.Designation != null && det.Designation.Definitor == this)
                                                                        .Select(det => det.Designation)));

            if (IncludeDomainDesignators && this.OwnerDomain.BaseContentRoot != null)
                Retriever = Retriever.Concat(this.OwnerDomain.BaseContentRoot.GetAllDetailDesignators().Where(dsn => dsn.Definitor == this));

            var Result = Retriever.Distinct().Cast<TableDetailDesignator>().ToList();

            return Result;
        }

        /// <summary>
        /// Gets the indirectly related Tables whose structure is defined by this Table-Structure Definition, via intermediate Table detail designators.
        /// </summary>
        public IEnumerable<Table> GetDefinedTables(bool IncludeDomainTables = true, bool IncludeNestedTables = true)
        {
            var Result = this.GetDependentDesignators()
                                .SelectMany(tbldetdsn => tbldetdsn.GetDesignatedTables(IncludeDomainTables, IncludeNestedTables))
                                    .Distinct().ToList();
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Externallly accessible collection of Field Definitions conforming the Storage Structure.
        /// </summary>
        public IList<FieldDefinition> StorageFieldDefinitions { get { return this.StorageStructureFieldDefs; } }
        /* IMPORTANT: Do not use a read-only collection, because remains referencing the original instance after cloning!
        public ReadOnlyCollection<FieldDefinition> StorageFieldDefinitions
        {
            get
            {
                if (this.StorageFieldDefinitions_ == null)
                    this.StorageFieldDefinitions_ = new ReadOnlyCollection<FieldDefinition>(this.StorageStructureFieldDefs);

                return this.StorageFieldDefinitions_;
            }
        }
        [NonSerialized]
        private ReadOnlyCollection<FieldDefinition> StorageFieldDefinitions_ = null; */

        /// <summary>
        /// List of ordered field definitions in the actual positions on which the data is stored in each dependent Table Record.
        /// Because the costly and unnecessary move operations, only additions, updates and deletions are made on dependent storage and this collection.
        /// </summary>
        protected EditableList<FieldDefinition> StorageStructureFieldDefs { get; set; }
        public static ModelListDefinitor<TableDefinition, FieldDefinition> __StorageStructureFieldDefs =
                   new ModelListDefinitor<TableDefinition, FieldDefinition>("StorageStructureFieldDefs", EEntityMembership.InternalCoreExclusive, ins => ins.StorageStructureFieldDefs, (ins, coll) => ins.StorageStructureFieldDefs = coll,
                                                                            "Storage Structure Field Definitions", "List of ordered field definitions in the actual positions on which the data is stored in each dependent Table Record. " +
                                                                                                                   "Because the costly and unnecessary move operations, only additions, updates and deletions are made on dependent storage and this collection.");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /*? /// <summary>
        /// Gets the collection of declared Field definitions of this Table-Structure Definition.
        /// </summary>
        public IEnumerable<FieldDefinition> GetFieldDefinitions()
        {
            return this.FieldDefinitions.GetExtendedEnumerable();
        } */

        /// <summary>
        /// Collection of declared Field definitions of this Table-Structure Definition.
        /// </summary>
        public EditableList<FieldDefinition> FieldDefinitions { get; protected set; }
        public static ModelListDefinitor<TableDefinition, FieldDefinition> __FieldDefinitions =
                   new ModelListDefinitor<TableDefinition, FieldDefinition>("FieldDefinitions", EEntityMembership.InternalCoreExclusive, ins => ins.FieldDefinitions, (ins, coll) => ins.FieldDefinitions = coll,
                                                                            "Field Definitions", "Collection of declared Field definitions of this Table-Structure Definition.");

        // .........................................................................................
        /*? /// <summary>
        /// Gets the list of ordered field definitions used as unique key (for individual identification of the record).
        /// </summary>
        public IEnumerable<FieldDefinition> GetUniqueKeyFields()
        {
            return this.UniqueKeyFieldDefs.GetExtendedEnumerable();
        } */

        /// <summary>
        /// List of ordered field definitions used as unique key (for individual identification of the record).
        /// </summary>
        public EditableList<FieldDefinition> UniqueKeyFieldDefs { get; protected set; }
        public static ModelListDefinitor<TableDefinition, FieldDefinition> __UniqueKeyFieldDefs =
                   new ModelListDefinitor<TableDefinition, FieldDefinition>("UniqueKeyFieldDefs", EEntityMembership.InternalCoreShared, ins => ins.UniqueKeyFieldDefs, (ins, coll) => ins.UniqueKeyFieldDefs = coll,
                                                                            "Unique Key Field Definitions", "List of ordered field definitions used as unique key (for individual identification of the record).");

        // .........................................................................................
        /*? /// <summary>
        /// Gets the list of ordered field definitions used as Labels (for title usage).
        /// </summary>
        public IEnumerable<FieldDefinition> GetLabelFieldDefs()
        {
            return this.LabelFieldDefs.GetExtendedEnumerable();
        } */

        /// <summary>
        /// List of ordered field definitions used as Labels (for title usage).
        /// </summary>
        public EditableList<FieldDefinition> LabelFieldDefs { get; protected set; }
        public static ModelListDefinitor<TableDefinition, FieldDefinition> __LabelFieldDefs =
                   new ModelListDefinitor<TableDefinition, FieldDefinition>("LabelFieldDefs", EEntityMembership.InternalCoreShared, ins => ins.LabelFieldDefs, (ins, coll) => ins.LabelFieldDefs = coll,
                                                                            "Label Field Definitions", "List of ordered field definitions used as Labels (for title usage).");

        // .........................................................................................
        /*? /// <summary>
        /// Gets the list of ordered field definitions referencing a dominant record's key (for hierarchy linking).
        /// </summary>
        public IEnumerable<FieldDefinition> GetDominantRefFieldDefs()
        {
            return this.DominantRefFieldDefs.GetExtendedEnumerable();
        } */

        /// <summary>
        /// List of ordered field definitions referencing a dominant record's key (for hierarchy linking).
        /// </summary>
        public EditableList<FieldDefinition> DominantRefFieldDefs { get; protected set; }
        public static ModelListDefinitor<TableDefinition, FieldDefinition> __DominantRefFieldDefs =
                   new ModelListDefinitor<TableDefinition, FieldDefinition>("DominantRefFieldDefs", EEntityMembership.InternalCoreShared, ins => ins.DominantRefFieldDefs, (ins, coll) => ins.DominantRefFieldDefs = coll,
                                                                            "Dominant Reference Field Definitions", "List of ordered field definitions referencing a dominant record's key (for hierarchy linking).");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates (returning true) if this Table-Structure Definition is compatible,
        /// in field definitions (exact) types and positions, with the supplied alternate one.
        /// </summary>
        public bool IsCompatibleWith(TableDefinition AlternateDefinition, bool AlternateCanBeLarger = false, bool AlternateCanBeShorter = false)
        {
            return this.IsCompatibleWith(AlternateDefinition.FieldDefinitions.Select(fdef => fdef.FieldType.ContainerType).ToList(),
                                         AlternateCanBeLarger, AlternateCanBeShorter);
        }

        /// <summary>
        /// Indicates (returning true) if this Table-Structure Definition is compatible,
        /// in field definitions (exact) types and positions, with the supplied alternate structure.
        /// </summary>
        public bool IsCompatibleWith(IList<Type> AlternateStructure, bool AlternateCanBeLarger = false, bool AlternateCanBeShorter = false)
        {
            General.ContractRequiresNotNull(AlternateStructure);

            if ((AlternateStructure.Count > this.FieldDefinitions.Count && !AlternateCanBeLarger) ||
                (AlternateStructure.Count < this.FieldDefinitions.Count && !AlternateCanBeShorter))
                return false;

            var MaxIndex = Math.Min(this.FieldDefinitions.Count, AlternateStructure.Count);
            for (int FieldIndex = 0; FieldIndex <  MaxIndex; FieldIndex++)
                if (this.FieldDefinitions[FieldIndex].FieldType.ContainerType
                    != AlternateStructure[FieldIndex])
                    return false;

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Alters the internal structure of this Table-Structure Definition, optionally based on the supplied Previous Table-Structure Definition and compatible-values preservation indication.
        /// Plus, propagate the changes to the registered dependant Designators.
        /// </summary>
        public void AlterStructure(bool PreserveCompatibleValues = true, IList<FieldDefinition> OriginalStorageFieldDefs = null)
        {
            if (OriginalStorageFieldDefs == null)
                OriginalStorageFieldDefs = this.StorageStructureFieldDefs;

            // Update structure positions (only Delete and Add operations to mantain order and to avoid costly Move operations)
            // var PreviousStorageStructureFieldDefs = this.StorageStructureFieldDefs.CloneFor(null, ECloneOperationScope.Deep);
            this.StorageStructureFieldDefs.UpdateOnlyListContentFrom(this.FieldDefinitions);

            // Update the storage indexes
            var FieldIndex = 0;
            foreach(var StorageField in this.StorageStructureFieldDefs.OrderBy(fd => fd.StorageIndex))
            {
                StorageField.StorageIndex = FieldIndex;
                FieldIndex++;
            }
            /*- for (int FieldDefIndex = 0; FieldDefIndex < this.StorageStructureFieldDefs.Count; FieldDefIndex++)
                this.StorageStructureFieldDefs[FieldDefIndex].StorageIndex = FieldDefIndex; */

            // Propagate the Alteration to dependants
            var Dependents = this.GetDefinedTables();

            foreach(var Dependant in Dependents)
                Dependant.ApplyStructuralAlter(OriginalStorageFieldDefs,
                                               this.StorageStructureFieldDefs,
                                               this.UniqueKeyFieldDefs,
                                               PreserveCompatibleValues);
        }
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<TableDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<TableDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<TableDefinition> __ClassDefinitor = null;

        public new TableDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((TableDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public TableDefinition PopulateFrom(TableDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public TableDefinition GenerateIndependentDuplicateForExternalDomain(Domain ExternalDomain)
        {
            var Result = this.CreateClone(ECloneOperationScope.Core, null);

            Result.OwnerDomain = ExternalDomain;

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void UpdateVersion()
        {
            if (this.Version != null)
                this.Version.Update();

            if (this.OwnerDomain != null)
                this.OwnerDomain.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}