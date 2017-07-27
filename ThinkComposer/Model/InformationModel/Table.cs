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
// File   : Table.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.Table (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.08 Néstor Sánchez A.  Creation
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
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;
using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the Information enrichment of Graph entities
namespace Instrumind.ThinkComposer.Model.InformationModel
{
    /// Stores structured information associated to an Idea, containing multiple data records of the same type.
    /// Each Table is defined by a TableDefinition about its structure and multiplicity (allowed number of records).
    [Serializable]
    public class Table : ContainedDetail, IList<TableRecord>, IModelEntity, IModelClass<Table>, IRecognizableElement
    {
        /// <summary>
        /// Separator text between the records labels of the Tables's Records-Label property.
        /// </summary>
        public const string TABLE_RECORDS_LABEL_SEP = " | ";

        public const bool PRESERVE_COMPATIBLE_VALUES_DEFAULT = true;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static Table()
        {
            __ClassDefinitor = new ModelClassDefinitor<Table>("Table", ContainedDetail.__ClassDefinitor, "Table",
                                                              "Stores structured information, containing one or multiple data records of the same type.");
            __ClassDefinitor.DeclareProperty(__AssignedDesignator);
            __ClassDefinitor.DeclareCollection(__Records);
        }

        /// <summary>
        /// Creates and returns a table from the specified designator and supplied records (of another table) for the specified Owner Idea.
        /// </summary>
        public static Table CreateTableFrom(Assignment<DetailDesignator> Designator, IEnumerable<TableRecord> Data, Idea Owner)
        {
            return CreateTableFrom(Designator, Data.Select(rec => rec.GetValues()), Owner);
        }

        /// <summary>
        /// Creates and returns a table from the specified designator and supplied data for the specified Owner Idea.
        /// </summary>
        public static Table CreateTableFrom(Assignment<DetailDesignator> Designator, IEnumerable<IList<object>> Data, Idea Owner)
        {
            var DetailTable = new Table(Owner, Designator);

            Data.ForEach(record => DetailTable.Add(new TableRecord(DetailTable, record)));

            return DetailTable;
        }

        /// <summary>
        /// Updates the Target table from the supplied data.
        /// </summary>
        public static void UpdateTableFrom(Table Target, List<List<object>> Data, bool Reset = true)
        {
            var TempTable = CreateTableFrom(Target.Designation.Assign(true), Data, Target.OwnerIdea);

            if (Reset)
                Target.Clear();

            Target.UpdateContentFrom(TempTable);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public Table(Idea OwnerContainer, Assignment<DetailDesignator> Designator)
             : base(OwnerContainer)
        {
            General.ContractRequiresNotNull(Designator);

            this.AssignedDesignator = Designator;
            this.Records = new EditableList<TableRecord>(__Records.TechName, this);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Table designator.
        /// </summary>
        public override Assignment<DetailDesignator> AssignedDesignator
        {
            get { return __AssignedDesignator.Get(this); }
            set { __AssignedDesignator.Set(this, value); }
        }
        protected Assignment<DetailDesignator> AssignedDesignator_;
        public static readonly ModelPropertyDefinitor<Table, Assignment<DetailDesignator>> __AssignedDesignator =
                   new ModelPropertyDefinitor<Table, Assignment<DetailDesignator>>("AssignedDesignator", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.AssignedDesignator_, (ins, val) => ins.AssignedDesignator_ = val, false, true,
                                                                                    "Assigned Designator", "Table designator.");
        public override MAssignment ContentDesignator { get { return this.AssignedDesignator; } }
        [Description("Table designator.")]
        public TableDetailDesignator Designator { get { return (this.ContentDesignator == null ? null : (TableDetailDesignator)this.ContentDesignator.AssignedValue); } }

        /// <summary>
        /// Collection of records belonging to this Table.
        /// A Record is a collection of data Items, where each item has an object Value and its associated to a field by its position/index.
        /// </summary>
        public EditableList<TableRecord> Records { get; private set; }
        public static ModelListDefinitor<Table, TableRecord> __Records =
                   new ModelListDefinitor<Table, TableRecord>("Records", EEntityMembership.InternalCoreExclusive, ins => ins.Records, (ins, coll) => ins.Records = coll, "Records", "Collection of records belonging to this Table.");

        /// <summary>
        /// Gets the designated Table-Structure Definition.
        /// </summary>
        [Description("Gets the designated Table-Structure Definition.")]
        public TableDefinition Definition { get { return (this.Designator == null ? null : this.Designator.DeclaringTableDefinition); } }

        /// <summary>
        /// Updates the related Designator identification information based on the Content.
        /// </summary>
        public override void UpdateDesignatorIdentification()
        {
            // This does not apply for table content type.
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates the supplied record.
        /// Returns null if valid, or an error message list if validation was not passed.
        /// </summary>
        public IEnumerable<string> ValidateRecord(TableRecord Record)
        {
            // PENDING: VALIDATE AT LEAST UNIQUE-KEY NON DUPLICATES.
            return null;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Validates, for the specified field-definitor, the supplied field value.
        /// Returns null if valid, or an error message if validation was not passed.
        /// </summary>
        public string ValidateFieldValue(FieldDefinition FieldDefinitor, object Value)
        {
            // Validates at least data-type compatibility.
            return FieldDefinitor.FieldType.Validate(Value);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Modifies the stored Table data to accomplish a just performed structure change.
        /// Only type changes,  additions and deletions of data fields are performed (no moves).
        /// If desired, data is preserved for equivalent positioned fields with the same data-type.
        /// </summary>
        /// <param name="OriginalClonedFieldDefs">List of Field Definitions which describes the PREVIOUS Fields and WAS that valid previously.</param>
        /// <param name="AlterStorageFieldDefs">List of Field Definitions which describes the NEW/CURRENT Storage-Structure and IS that valid now.</param>
        /// <param name="AlterUniqueKeyFieldDefs">List of Field Definitions which together define the NEW/CURRENT Unique-Key of the Table.</param>
        /// <param name="PreserveCompatibleValues">Indicates whether to preserver data-type compatible values.</param>
        /// <param name="FieldsSetAlteration">Indicates that changes were made to the main Field Definitions set.</param>
        /// <param name="UniqueKeyAlteration">Indicates that changes were made to the Unique-Key Field Definitions set.</param>
        internal void ApplyStructuralAlter(IList<FieldDefinition> OriginalClonedFieldDefs, IList<FieldDefinition> AlterStorageFieldDefs,
                                           IList<FieldDefinition> AlterUniqueKeyFieldDefs, bool PreserveCompatibleValues,
                                           bool FieldsSetAlteration = true, bool UniqueKeyAlteration = true)
        {
            if (!FieldsSetAlteration && !UniqueKeyAlteration)
                return;

            // Remove data if no fields are defined
            if (AlterStorageFieldDefs.Count < 1)
            {
                this.Records.Clear();
                return;
            }

            // Remove unused fields (in reverse order to avoid move due to remove intermediates)
            var UnusedFieldDefs = OriginalClonedFieldDefs.Where(fd => !AlterStorageFieldDefs.Any(sf => sf.GlobalId == fd.GlobalId))
                                      .OrderByDescending(fd => fd.StorageIndex).ToList();

            if (UnusedFieldDefs.Count > 0)
                foreach (var Record in this.Records)
                    foreach (var FieldDef in UnusedFieldDefs)
                        Record.RemoveFieldAt(FieldDef.StorageIndex);

            //+ var DetectedFieldDefinitionChanges = OriginalClonedFieldDefs.GetDifferencesFrom(AlterStorageFieldDefs, (current, changed) => current.GlobalId == changed.GlobalId, General.DetermineDifferences, General.IsEquivalent);

            // Consider only necessary-to-compare fields, hence take the minimum between the fields count of the two field collections.
            int FieldCount = Math.Min(OriginalClonedFieldDefs.Count, AlterStorageFieldDefs.Count);

            // Set of flags that indicate, for each field, whether they must mantain its initial-value and they field-type are compatible.
            var PreserveOriginalData = new bool[FieldCount];

            if (FieldsSetAlteration && PreserveCompatibleValues)
                for (int FieldIndex = 0; FieldIndex < FieldCount; FieldIndex++)
                {
                    // IMPORTANT: Compare by Field-Equivalence (not by list index correlation)
                    //            because OriginalClonedFieldDefs may not be in the same order as AlterStorageFieldDefs
                    var CurrField = AlterStorageFieldDefs[FieldIndex];
                    var PrevField = OriginalClonedFieldDefs.FirstOrDefault(fld => fld.IsEquivalent(CurrField) /*-? fld.TechName == CurrField.TechName */);

                    if (PrevField != null)
                        PreserveOriginalData[FieldIndex] = CurrField.FieldType.IsEqual(PrevField.FieldType);
                }

            if (PreserveCompatibleValues)
                for (int recordindex = 0; recordindex < this.Records.Count; recordindex++)
                {
                    var Record = this.Records[recordindex];

                    // Changes relevant to storage structure alteration.
                    // IMPORTANT: Capacity will be increased only if necessary (i.e.: for string non-null values)
                    if (FieldsSetAlteration)
                        for (int FieldIndex = 0; FieldIndex < FieldCount; FieldIndex++)
                            if (!PreserveOriginalData[FieldIndex])
                            {
                                var FieldDef = AlterStorageFieldDefs[FieldIndex];

                                Record.WriteStoredValue(FieldIndex,
                                                        FieldDef.InitialStoreValue.NullDefault(FieldDef.ExplicitInitializer == null ? null : FieldDef.ExplicitInitializer()),
                                                        FieldDef.DefaultEmptyValue);
                            }

                    // Changes relevant to unique-key alteration.
                    if (UniqueKeyAlteration)
                    {
                        // PENDING...
                        // Compose Unique-Key values group.

                        // Detect any duplicates

                        // Mark record with detected errors + Define centralized standard table-records errors collection.
                    }
                }
            else
                this.Records.Clear();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Replaces by reference (not cloning) all the Data content from the supplied Source Table, which must have the same Table-Structure Definition.
        /// </summary>
        public void UpdateContentFrom(Table Source)
        {
            if (Source.Definition != this.Definition)
                throw new UsageAnomaly("Cannot replace data-content of Table because Source has a different Table-Structure Definition");

            this.Records.UpdateContentFrom(Source.Records);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Table Records sorted by the specified list of Field definitions or by its Key if none/null is specified.
        /// </summary>
        public IEnumerable<TableRecord> GetRecordsSorted(IEnumerable<FieldDefinition> SortingFieldDefs = null)
        {
            if (SortingFieldDefs == null || !SortingFieldDefs.Any())
                SortingFieldDefs = this.Definition.UniqueKeyFieldDefs;

            if (!SortingFieldDefs.Any())
                return this.Records;
            
            return this.Records.OrderBy(rec => rec.GetValues(SortingFieldDefs), General.SetsComparer);
        }

        /// <summary>
        /// Gets, in a hierarchical/nested arrangement based on the Dominant Reference Field definitions, the Table Records and their relative-to-the-root level number.
        /// Plus, it is sorted by the specified list of Field definitions or by its Key if none/null is specified.
        /// </summary>
        public IEnumerable<Tuple<TableRecord, int>> GetRecordsHierarchized(IEnumerable<FieldDefinition> SortingFieldDefs = null)
        {
            if (SortingFieldDefs == null || !SortingFieldDefs.Any())
                SortingFieldDefs = this.Definition.UniqueKeyFieldDefs;

            // PENDING
            return this.Records.Select(rec => Tuple.Create<TableRecord, int>(rec, 0));
        }

        /// <summary>
        /// Gets the Table Records into a collection of field values as strings for export.
        /// </summary>
        public IEnumerable<IEnumerable<string>> GetRecordsForExportAsString(bool Quoted = false, bool UniversalDate = true, bool NullAsEmpty = true)
        {
            foreach (var Record in this.Records)
                yield return Record.GetValuesForExport(null, Quoted, UniversalDate, NullAsEmpty);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IList<TableRecord> members

        /// <summary>
        /// Give access the record at the specified Index.
        /// </summary>
        public TableRecord this[int index]
        {
            get { return this.Records[index]; }
            set { this.Records[index] = value; }
        }

        /// <summary>
        /// Returns the index of the supplied Item in the contained records.
        /// </summary>
        public int IndexOf(TableRecord Item)
        {
            return this.Records.IndexOf(Item);
        }

        /// <summary>
        /// Inserts the supplied Item at the specified Index in the contained records.
        /// </summary>
        public void Insert(int index, TableRecord Item)
        {
            this.Records.Insert(index, Item);
        }

        /// <summary>
        /// Removes the contained item at the specified Index.
        /// </summary>
        public void RemoveAt(int index)
        {
            this.Records.RemoveAt(index);
        }

        /// <summary>
        /// Adds the supplied Item to the contained records.
        /// </summary>
        public void Add(TableRecord Item)
        {
            this.Records.Add(Item);
        }

        /// <summary>
        /// Clears all the contained items.
        /// </summary>
        public void Clear()
        {
            this.Records.Clear();
        }

        /// <summary>
        /// Returns indication of existence of the specified Item in the contained items.
        /// </summary>
        public bool Contains(TableRecord Item)
        {
            return this.Records.Contains(Item);
        }

        /// <summary>
        /// Copies the contained items into the supplied Array, at the specified ArrayIndex.
        /// </summary>
        public void CopyTo(TableRecord[] Array, int ArrayIndex)
        {
            this.Records.CopyTo(Array, ArrayIndex);
        }

        /// <summary>
        /// Returns the count of contained Table-Records.
        /// </summary>
        [Description("Returns the count of contained Table-Records.")]
        public int Count { get { return this.Records.Count; } }

        /// <summary>
        /// Idicates whether the contained items list is read-only.
        /// </summary>
        public bool IsReadOnly {  get { return this.Records.IsReadOnly; } }

        /// <summary>
        /// Removes the specified Item from the contained items.
        /// </summary>
        public bool Remove(TableRecord Item)
        {
            return this.Records.Remove(Item);
        }

        /// <summary>
        /// Gets the generic enumerator of the contained items.
        /// </summary>
        public IEnumerator<TableRecord> GetEnumerator()
        {
            return this.Records.GetEnumerator();
        }

        /// <summary>
        /// Gets the non-generic enumerator of the contained items.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Records.GetEnumerator();
        }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Tables stored in Fields of data-type Table.
        /// Optionally, all the subsequent nested tables can be included.
        /// </summary>
        public IEnumerable<Table> GetStoredTables(bool IncludeNestedTables = false)
        {
            var Result = new List<Table>();

            var FieldsStoringTables = this.Definition.FieldDefinitions.Where(fdef => fdef.FieldType is TableType).ToList();
            if (FieldsStoringTables.Count > 0)
                foreach (var Record in this.Records)
                    foreach (var FieldDef in FieldsStoringTables)
                    {
                        var Value = Record.GetStoredValue(FieldDef) as Table;
                        if (Value != null)
                        {
                            Result.Add(Value);

                            if (IncludeNestedTables)
                                Result.AddRange(Value.GetStoredTables(IncludeNestedTables));
                        }
                    }

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<Table> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<Table> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<Table> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new Table CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((Table)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public Table PopulateFrom(Table SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the kind of this detail.
        /// </summary>
        [Description("Returns the kind of this detail.")]
        public override ModelDefinition Kind { get { return __ClassDefinitor; } }

        public override string ToString()
        {
            return this.Name + (this.Definition == null ? "" : " (" + this.Definition.Name + ")");

            /* T
            return ("Detail-Type" + Idea.SYNOP_SEPARATOR + "Table" + Idea.SYNOP_SEPARATOR +
                    "Table-Definition" + Idea.SYNOP_SEPARATOR + this.Definition.Name + Idea.SYNOP_SEPARATOR + "Total-Records" + Idea.SYNOP_SEPARATOR + this.RecordsCount.ToString()); */
        }

        /* OLD:
        public override string ToString()
        {
            // Too verbose for users:
            // return "Table. Dsn='" + this.Designator.Name + "', Def='" + this.Definition.Name + "', Owner='" + this.OwnerIdea.ToString() + "'.";

            return this.Name;
        } */

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Text representation of the Table's Records data (only the first 3 records).
        /// </summary>
        [Description("Text representation of the Table's Records data (only the first 3 records).")]
        public string RecordsLabel
        {
            get
            {
                if (this.Records.Count < 1)
                    return "<Empty>";   // Do not return ""

                var Text = new StringBuilder(this.Records[0].Label);

                var Limit = 3;
                var Segments = 1;
                foreach (var Record in Records.Skip(1))
                {
                    if (Records.Count > Limit && Segments >= Limit)
                    {
                        Text.Append(TABLE_RECORDS_LABEL_SEP + "...");
                        break;
                    }

                    Text.Append(TABLE_RECORDS_LABEL_SEP + Record.Label);
                    Segments++;
                }

                var Result = Text.ToString();
                return Result;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public string Name { get { return (this.Designator == null ? null : this.Designator.Name); } set { if (this.Designator == null) return; this.Designator.Name = value; } }

        public string TechName { get { return (this.Designator == null ? null : this.Designator.TechName); } set { if (this.Designator == null) return; this.Designator.TechName = value; } }

        public string Summary { get { return (this.Designator == null ? null : this.Designator.Summary); } set { if (this.Designator == null) return; this.Designator.Summary = value; } }

        public ImageSource Pictogram { get { return (this.Designator == null ? null : this.Designator.Pictogram); } set { if (this.Designator == null) return; this.Designator.Pictogram = value; } }

        public int CompareTo(object obj) { return (this.Designator == null ? 0 : this.Designator.CompareTo(obj)); }
    }
}
