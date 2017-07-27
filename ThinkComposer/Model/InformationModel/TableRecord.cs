// -------------------------------------------------------------------------------------------
// Instrumind ThinkComposer
//
// Copyright (C) Néstor Marcel Sánchez Ahumada. Santiago, Chile.
// http://thinkcomposer.codeplex.com
//
// This file is part of ThinkComposer, which is free software licensed under the GNU General Public License.
// It is provided without any warranty. You should find a copy of the license in the root directory of this software product.
// -------------------------------------------------------------------------------------------
//
// Project: Instrumind ThinkComposer v1.0
// File   : TableRecord.cs
// Object : Instrumind.ThinkComposer.Model.InformationModel.TableRecord (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.04.14 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Dynamic;
using System.Text;
using System.Runtime.Serialization;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

using Instrumind.ThinkComposer.MetaModel.InformationMetaModel;
using Instrumind.ThinkComposer.Model.GraphModel;

/// Base abstractions for the Information enrichment of Graph entities
namespace Instrumind.ThinkComposer.Model.InformationModel
{
    /// <summary>
    /// Groups variable data items that conforms a data entity, composing a Table with others of the same kind.
    /// </summary>
    // NOTE: Do not implement IEditableObject because editing is by cell, not by row.
    [Serializable]
    public class TableRecord : DynamicObject, IDynamicStore, IModelClass<TableRecord>, ISerializable
    {
        /// <summary>
        /// Separator text between fields of the Record's Label.
        /// </summary>
        public const string RECORD_LABEL_FIELD_SEP = " · "; // Or... " \\ ";

        /// <summary>
        /// Code to signal a field with a contained picture.
        /// </summary>
        public const string FIELD_PICTURE_INDICATOR = "[Picture]";

        /// <summary>
        /// Converter to populate decimal fields with a value from a table-record's compatible field.
        /// </summary>
        public static GenericConverter<decimal, TableRecord> TableRecordToNumericConverter =
            new GenericConverter<decimal, TableRecord>(
                dec => null,
                tbr =>
                {
                    decimal Result = 0;

                    var TableDef = tbr.OwnerTable.Definition;
                    var FirstCompat = (TableDef.LabelFieldDefs.Count == 1 && TableDef.LabelFieldDefs[0].FieldType is NumberType
                                        ? TableDef.LabelFieldDefs[0]
                                        : TableDef.FieldDefinitions.FirstOrDefault(fd => fd.FieldType is NumberType));
                    if (FirstCompat != null)
                        Result = (decimal)tbr.GetStoredValue(FirstCompat);
                    else
                        if (!decimal.TryParse(tbr.Label, out Result))
                        {
                            var FirstText = TableDef.FieldDefinitions.FirstOrDefault(fd => fd.FieldType.ContainerType == typeof(string));
                            if (FirstText != null)
                                decimal.TryParse(tbr.GetStoredValue(FirstText).ToStringAlways(), out Result);
                            else
                                Console.WriteLine("Table does not contain a numeric field or a convertible text field to get a number.");
                        }

                    return Result;
                });

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static TableRecord()
        {
            __ClassDefinitor = new ModelClassDefinitor<TableRecord>("TableRecord", null, "Table Record",
                                                                    "Groups variable data items that conforms a data entity, composing a Table with others of the same kind.");
            __ClassDefinitor.DeclareProperty(__OwnerTable);
            __ClassDefinitor.DeclareCollection(__Values);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        internal TableRecord(Table OwnerTable, IList<object> InitialValues = null)
        {
            General.ContractRequiresNotNull(OwnerTable);

            this.OwnerTable = OwnerTable;

            // IMPORTANT: The Values table has it content of type Model-Class treated as References, therefore they are non-cloneable.
            this.Values = new EditableList<object>(__Values.TechName, this.OwnerTable, this.OwnerTable.Definition.StorageFieldDefinitions.Count(), true);

            // Initialize values
            for (int FieldIndex = 0; FieldIndex < this.OwnerTable.Definition.FieldDefinitions.Count; FieldIndex++)
            {
                var FieldDef = this.OwnerTable.Definition.FieldDefinitions[FieldIndex];
                object InitialValue = null;

                if (InitialValues != null && FieldIndex < InitialValues.Count)
                    InitialValue = InitialValues[FieldIndex];
                else
                    InitialValue = FieldDef.InitialStoreValue.NullDefault(FieldDef.ExplicitInitializer == null ? null : FieldDef.ExplicitInitializer());

                var DefaultValue = FieldDef.FieldType.ContainerType.GetDefaultValue();

                if (InitialValue != null && InitialValue != DefaultValue)
                    this.SetStoredValue(FieldDef, InitialValue);
            }
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        protected TableRecord(SerializationInfo information, StreamingContext context)
        {
            this.OwnerTable = information.GetValue("OwnerTable", typeof(Table)) as Table;
            this.Values = information.GetValue("Values", typeof(EditableList<object>)) as EditableList<object>;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Table owning this table record.
        /// </summary>
        public Table OwnerTable { get { return __OwnerTable.Get(this); } protected set { __OwnerTable.Set(this, value); } }
        protected Table OwnerTable_ = null;
        public static readonly ModelPropertyDefinitor<TableRecord, Table> __OwnerTable =
                           new ModelPropertyDefinitor<TableRecord, Table>("OwnerTable", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerTable_, (ins, val) => ins.OwnerTable_ = val, false, false,
                                                                                                                 "Owner Table", "Table owning this table record");

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a set of values for the specified Selected Field Definitions, or all (in the standard field-defs order) if none is supplied.
        /// </summary>
        public IList<object> GetValues(IEnumerable<FieldDefinition> SelectedFieldDefs = null)
        {
            if (SelectedFieldDefs == null || !SelectedFieldDefs.Any())
                SelectedFieldDefs = this.OwnerTable.Definition.FieldDefinitions;

            var Result = SelectedFieldDefs.Select(fielddef => this.GetStoredValue(fielddef)).ToList();
            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a set of values for the specified Selected Field Definitions, or all (in the standard field-defs order) if none is supplied.
        /// </summary>
        public IList<string> GetValuesForExport(IEnumerable<FieldDefinition> SelectedFieldDefs = null,
                                                bool Quoted = false, bool UniversalDate = true, bool NullAsEmpty = true)
        {
            if (SelectedFieldDefs == null || !SelectedFieldDefs.Any())
                SelectedFieldDefs = this.OwnerTable.Definition.FieldDefinitions;

            var Result = SelectedFieldDefs.Select(fielddef => this.GetFieldValueForExport(fielddef, Quoted, UniversalDate, NullAsEmpty)).ToList();
            return Result;
        }

        /// <summary>
        /// Gets the field value associated to the specified field Definitor, as string for export.
        /// </summary>
        public string GetFieldValueForExport(FieldDefinition Definitor, bool Quoted = false, bool UniversalDate = true, bool NullAsEmpty = true)
        {
            var Value = this.GetStoredValue(Definitor);
            string Result = Value.ToString();

            if (Result == null)
                if (NullAsEmpty)
                    Result = String.Empty;
                else
                    return Result;

            if (Definitor.FieldType.IsEqual(DataType.DataTypeDate))
                Result = ((DateTime)Value).ToString("yyyy-MM-dd");
            else
                if (Definitor.FieldType.IsEqual(DataType.DataTypeTime))
                    Result = ((DateTime)Value).ToString("hh:mm:ss");
                else
                    if (Definitor.FieldType.IsEqual(DataType.DataTypeDateTime))
                        Result = ((DateTime)Value).ToString("yyyy-MM-dd hh:mm:ss");
                    else
                        if (Definitor.FieldType.IsEqual(DataType.DataTypeTableRecordRef))
                            Result = ((TableRecord)Value).Label;

            if (Quoted)
                Result = "\"" + Result.Replace("\"", "\"\"") + "\"";

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Value associated with the specified Field Tech-Name.
        /// If no field hast the specified Field Tech-Name, then null is returned.
        /// </summary>
        public object this[string FieldTechName]
        {
            get
            {
                var FieldDef = this.GetFieldDefinitionFromTechName(FieldTechName);
                if (FieldDef == null)
                    return null;

                var Result = this.GetStoredValue(FieldDef);
                return Result;
            }
        }

        /// <summary>
        /// Collection of Values contained in this Table Record.
        /// </summary>
        protected EditableList<object> Values { get; private set; }
        public static ModelListDefinitor<TableRecord, object> __Values =
                   new ModelListDefinitor<TableRecord, object>("Values", EEntityMembership.InternalCoreExclusive, ins => ins.Values, (ins, coll) => ins.Values = coll, "Values", "Collection of Values contained in this Table Record.");

        /// <summary>
        /// Updates the values collection size based on the current storage-structure of the associated Table-Structure Definition.
        /// </summary>
        public void UpdateCapacity()
        {
            this.Values.SetCapacity(this.OwnerTable.Definition.StorageFieldDefinitions.Count);
        }

        /// <summary>
        /// Removes unused field values.
        /// </summary>
        internal void RemoveFieldAt(int FieldIndex)
        {
            if (FieldIndex >= 0 && FieldIndex < this.Values.Count)
                this.Values.RemoveAt(FieldIndex);
        }

        // -----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Value stored in the position related to the supplied Field Definition, converted to string (even if null) and formatted.
        /// Plus, if the value is a reference (such as an choice/option or foreign-key type) then the referenced value is returned.
        /// </summary>
        public string GetStoredValueForDisplay(FieldDefinition FieldDef)
        {
            var BasicKind = FieldDef.FieldType as BasicDataType;
            var Value = GetStoredValue(FieldDef);
            var Result = Value.ToStringAlways();

            if (Value != null)
                if (BasicKind != null)
                {
                    var ChoiceKind = BasicKind as ChoiceType;

                    if (ChoiceKind != null)
                        Result = ChoiceKind.GetOptionRegister(Convert.ToInt32(Value)).Value.ToStringAlways();
                    else
                        if (Value is IFormattable)
                            if (BasicKind is NumberType)
                                Result = ((IFormattable)Value).ToString("##,###,###,###,###,##0.###############", null);
                            else
                                if (!BasicKind.DisplayFormat.IsAbsent())
                                    try
                                    {
                                        Result = ((IFormattable)Value).ToString(BasicKind.DisplayFormat, null);
                                    }
                                    catch (Exception Problem)
                                    {
                                        Result = Value.ToStringAlways();
                                    }
                }
                else
                {
                    var TableRecordRefKind = FieldDef.FieldType as TableRecordLinkType;
                    if (TableRecordRefKind != null)
                        Result = ((TableRecord)Value).Label;
                    else
                    {
                        var IdeaRefKind = FieldDef.FieldType as IdeaLinkType;
                        if (IdeaRefKind != null)
                        {
                            var Target = (Idea)Value;
                            Result = (Target.NameCaption + " " + Target.DescriptiveCaption).Trim();
                        }
                        else
                        {
                            var TableKind = FieldDef.FieldType as TableType;
                            if (TableKind != null)
                                Result = ((Table)Value).RecordsLabel;
                            else
                            {
                                var PictureKind = FieldDef.FieldType as PictureType;
                                if (PictureKind != null)
                                    Result = FIELD_PICTURE_INDICATOR;
                            }
                        }
                    }
                }

            return Result;
        }

        /// <summary>
        /// Gets the Value stored in the position related to the supplied Field Tech-Name.
        /// </summary>
        public object GetStoredValue(string FieldTechName)
        {
            return this.GetStoredValue(this.GetFieldDefinitionFromTechName(FieldTechName));
        }

        /// <summary>
        /// Gets the Value stored in the position related to the supplied Field Definition.
        /// </summary>
        internal object GetStoredValue(FieldDefinition FieldDef)
        {
            if (FieldDef == null)
                throw new UsageAnomaly("Specified Field is not declared by the Table-Structure Definition.");

            // Notice that if value-types are returned, then they default value is obtained (e.g.: For "integers" always it will return "0").
            var FieldIndex = this.OwnerTable.Definition.StorageFieldDefinitions.IndexOfMatch(fdef => fdef.GlobalId.IsEqual(FieldDef.GlobalId));
            if (FieldIndex < 0)
                throw new InternalAnomaly("Field not available for retrieve.");

            object Result = ReadStoredValue(FieldIndex, FieldDef.DefaultEmptyValue.NullDefault(FieldDef.FieldType.ContainerType.GetDefaultValue()));
            if (FieldDef.FieldType.StorageReadConverter != null)
                Result = FieldDef.FieldType.StorageReadConverter(Result);

            return Result;
        }

        /// <summary>
        /// INTERNAL: Reads and returns, from final field-storage, the Value stored in the supplied Field Storage Index position, optionally using the specified default-empty value.
        /// </summary>
        internal object ReadStoredValue(int FieldStorageIndex, object DefaultEmptyValue = null)
        {
            object result = null;

            if (FieldStorageIndex >= 0 && FieldStorageIndex < this.Values.Count)
                result = this.Values[FieldStorageIndex];

            return result.NullDefault(DefaultEmptyValue);
        }

        /// <summary>
        /// Sets into the supplied Field (by Tech-Name) position the specified Value.
        /// This stores the Field Definition default-empty value when the supplied Value is null.
        /// Returns indication of success or failure (validation not passed).
        /// </summary>
        public bool SetStoredValue(string FieldTechName, object Value)
        {
            return this.SetStoredValue(this.GetFieldDefinitionFromTechName(FieldTechName), Value);
        }

        /// <summary>
        /// Sets into the supplied Field (by definition) position the specified Value.
        /// Sets into the supplied Field Definition position the specified Value.
        /// This stores the Field Definition default-empty value when the supplied Value is null.
        /// Returns indication of success or failure (validation not passed).
        /// </summary>
        internal bool SetStoredValue(FieldDefinition FieldDef, object Value)
        {
            if (Value is string)
                if (FieldDef.FieldType.ContainerType != typeof(string))
                {
                    object ConvertedValue = null;
                    if (!FieldDef.FieldType.TryParseValueFrom(Value as string, out ConvertedValue))
                    {
                        Console.WriteLine("Field '{0}' cannot interpret Value '{1}' as Data-Type '{2}'.", FieldDef.Name, Value.ToStringAlways(), FieldDef.FieldType.Name);
                        return false;
                    }

                    Value = ConvertedValue;
                }

            var FinalValue = (FieldDef.FieldType.StorageWriteConverter == null ? Value : FieldDef.FieldType.StorageWriteConverter(Value));
            var Problem = this.OwnerTable.ValidateFieldValue(FieldDef, FinalValue);
            if (Problem != null)
            {
                Console.WriteLine("Field '{0}' cannot accept Value '{1}'. Problem: {2}.", FieldDef.Name, FinalValue.ToStringAlways(), Problem);
                return false;
            }

            var FieldIndex = this.OwnerTable.Definition.StorageFieldDefinitions.IndexOfMatch(fdef => fdef.GlobalId.IsEqual(FieldDef.GlobalId));
            if (FieldIndex < 0)
                throw new InternalAnomaly("Field not available for storage.");

            this.WriteStoredValue(FieldIndex, FinalValue, FieldDef.DefaultEmptyValue);

            // IMPORTANT: This announces a change to all the fields, thus enforcing re-evaluation the whole row
            this.NotifyPropertyChange(String.Empty);

            // DO NOT notify changes with this (data is not refreshed to the row):
            // this.NotifyPropertyChange(FieldDef.TechName);

            /* PREVIOUS
            // IMPORTANT: This announces a change to all the fields, thus enforcing re-evaluation the whole row
            this.NotifyPropertyChange(String.Empty);

            // DO NOT notify changes with this (data is not refreshed to the row):
            // this.NotifyPropertyChange(FieldDef.TechName);

            this.WriteStoredValue(FieldIndex, FinalValue, FieldDef.DefaultEmptyValue);
            */

            return true;
        }

        /// <summary>
        /// INTERNAL: Writes, into final field-storage at the supplied Index position, the specified Value, which at this point must be already validated.
        /// This stores a null if the supplied default-empty value matched the specified main Value.
        /// </summary>
        internal void WriteStoredValue(int FieldStorageIndex, object Value, object DefaultEmptyValue)
        {
            var FinalValue = (Value.IsEqual(DefaultEmptyValue) ? null : Value);
            
            //T Console.WriteLine("Writing Record HC={0}. On Field-Storage[{1}] the value '{2}'.", this.GetHashCode(), FieldStorageIndex, Value);

            if (FieldStorageIndex >= this.Values.Count)
                if (FinalValue == null)
                    return; // Store is not necessary.
                else
                    this.UpdateCapacity();  // Capacity increased as needed.

            // Write (or overwrite) value
            this.Values[FieldStorageIndex] = FinalValue;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<TableRecord> Members

        public MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public ModelClassDefinitor<TableRecord> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly ModelClassDefinitor<TableRecord> __ClassDefinitor = null;

        public virtual object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public TableRecord CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((TableRecord)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public TableRecord PopulateFrom(TableRecord SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
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

        // -----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// By-Indexer access helper for getting Field-Definitor.
        /// </summary>
        protected FieldDefinition GetFieldDefinitionFromIndexer(object[] Indexes)
        {
            if (Indexes == null || Indexes.Length != 1
                || !(Indexes[0] is int || Indexes[0] is string))
                throw new UsageAnomaly("Indexer for accessing Field data is not string (for Field-Name) or int (for Field-Index),", Indexes);

            FieldDefinition FieldDef = null;

            var FieldIdentifier = Indexes[0] as string;
            if (FieldIdentifier == null)
            {
                var FieldIndex = (int)Indexes[0];
                // REMEMBER: The final storage index is not related to the FieldDefinitions, else is that of the StorageStructureFieldDefs.
                if (!FieldIndex.IsInRange(0, this.OwnerTable.Definition.FieldDefinitions.Count - 1))
                    throw new UsageAnomaly("Field-Index for accessing Field data is out of defined Fields range,", FieldIndex);

                FieldDef = this.OwnerTable.Definition.FieldDefinitions[FieldIndex];
            }
            else
            {
                FieldDef = this.OwnerTable.Definition.FieldDefinitions.FirstOrDefault(fld => fld.TechName == FieldIdentifier);
                if (FieldDef == null)
                    throw new UsageAnomaly("Field-Name for accessing Field data is not identifying any defined Field,", FieldIdentifier);
            }

            return FieldDef;
        }

        /// <summary>
        /// By-Binder access helper for getting Field-Definitor.
        /// </summary>
        protected FieldDefinition GetFieldDefinitionFromBinder(DynamicMetaObjectBinder Binder, bool IsRequired = true)
        {
            var GetBinder = Binder as GetMemberBinder;
            var SetBinder = Binder as SetMemberBinder;
            if (GetBinder == null && SetBinder == null)
                throw new UsageAnomaly("Provided member binder is not for Get, Set or is null.");

            string FieldTechName = (GetBinder != null ? GetBinder.Name : SetBinder.Name);

            var Result = this.GetFieldDefinitionFromTechName(FieldTechName, IsRequired);
            return Result;
        }

        /// <summary>
        /// By-Tech-Name access helper for getting Field-Definitor.
        /// </summary>
        protected FieldDefinition GetFieldDefinitionFromTechName(string TechName, bool IsRequired = false)
        {
            if (TechName.IsAbsent())
                throw new UsageAnomaly("Provided member binder or member Tech-Name was not supplied.");

            /*T if (Name == "Label")
                Console.WriteLine("Labeling?!"); */

            FieldDefinition FieldDef = null;

            FieldDef = this.OwnerTable.Definition.FieldDefinitions.FirstOrDefault(fld => fld.TechName == TechName);
            if (FieldDef == null && IsRequired)
                throw new UsageAnomaly("Field-Tech-Name for accessing Field data is not identifying any defined Field,", TechName);

            return FieldDef;
        }

        // -----------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the Record field names.
        /// </summary>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.OwnerTable.Definition.FieldDefinitions.Select(fld => fld.TechName);
        }

        /// <summary>
        /// Gets a Record Value by position Index.
        /// </summary>
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this.GetStoredValue(GetFieldDefinitionFromIndexer(indexes));
            return true;
        }

        /// <summary>
        /// Gets a Record value by Name.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var FieldDef = GetFieldDefinitionFromBinder(binder, false);
            if (FieldDef == null)
            {
                result = null;
                return false;
            }

            result = this.GetStoredValue(FieldDef);
            return true;
        }

        /// <summary>
        /// Sets a Record Value accessing by position Index.
        /// </summary>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            //T Console.WriteLine("Trying set at index[{0}] the value '{1}'.", indexes[0], value);
            this.SetStoredValue(GetFieldDefinitionFromIndexer(indexes), value);
            return true;
        }

        /// <summary>
        /// Sets a Record Value accessing by Name.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            //T Console.WriteLine("Trying set at member '{0}' the value '{1}'.", binder.Name, value);
            var FieldDef = GetFieldDefinitionFromBinder(binder, false);
            if (FieldDef == null)
                return false;

            this.SetStoredValue(FieldDef, value);
            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets object data for an explicit serialization (due to DynamicObject class not marked as serializable).
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(__OwnerTable.TechName, this.OwnerTable);
            info.AddValue(__Values.TechName, this.Values);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the record's Unique-Key: Text composed of the fields (definitions) marked as being part of the Unique-Key in the Table-Structure Definition.
        /// If no unique-key fields were specified, the all the fields are returned.
        /// </summary>
        public string UniqueKey
        {
            get
            {
                var FieldDefs = this.OwnerTable.Definition.UniqueKeyFieldDefs;
                if (FieldDefs == null || FieldDefs.Count < 1)
                    FieldDefs = this.OwnerTable.Definition.FieldDefinitions;
                if (FieldDefs == null || FieldDefs.Count < 1)
                    return "";

                var Text = new StringBuilder(this.GetStoredValue(FieldDefs[0]).ToStringAlways());

                foreach (var FieldDef in FieldDefs.Skip(1))
                    Text.Append(RECORD_LABEL_FIELD_SEP + this.GetStoredValue(FieldDef).ToStringAlways());

                return Text.ToString();
            }
        }

        /// <summary>
        /// Returns the record's Label: Text composed of the fields (definitions) marked as being part of the Label in the Table-Structure Definition.
        /// If no label fields were specified, then the unique-key fields are returned.
        /// If no unique-key fields were specified, the all the fields are returned.
        /// </summary>
        // IMPORTANT: Do not rename this field, it is referenced as DisplayMemberPath in Table Editor's record selection combo-box.
        [Description("Returns the record's Label: Text composed of the fields (definitions) marked as being part of the Label in the Table-Structure Definition.")]
        public string Label
        {
            get
            {
                var FieldDefs = this.OwnerTable.Definition.LabelFieldDefs;
                if (FieldDefs == null || FieldDefs.Count < 1)
                    FieldDefs = this.OwnerTable.Definition.UniqueKeyFieldDefs;
                if (FieldDefs == null || FieldDefs.Count < 1)
                    FieldDefs = this.OwnerTable.Definition.FieldDefinitions;
                if (FieldDefs == null || FieldDefs.Count < 1)
                    return "";

                var Text = new StringBuilder(this.GetStoredValue(FieldDefs[0]).ToStringAlways());

                foreach (var FieldDef in FieldDefs.Skip(1))
                    Text.Append(RECORD_LABEL_FIELD_SEP + this.GetStoredValueForDisplay(FieldDef));  // Old: this.GetStoredValue(FieldDef).ToStringAlways());

                var Result = Text.ToString();
                return Result;
            }
        }

        /// <summary>
        /// Returns the record's field values: Text composed of all the fields (definitions) declared in the Table-Structure Definition.
        /// </summary>
        public string WholeRecord
        {
            get
            {
                var FieldDefs = this.OwnerTable.Definition.FieldDefinitions;
                if (FieldDefs == null || FieldDefs.Count < 1)
                    return "";

                var Text = new StringBuilder(this.GetStoredValue(FieldDefs[0]).ToStringAlways());

                foreach (var FieldDef in FieldDefs.Skip(1))
                    Text.Append(RECORD_LABEL_FIELD_SEP + this.GetStoredValue(FieldDef).ToStringAlways());

                return Text.ToString();
            }
        }

        /// <summary>
        /// Index of the record in the owner Table (one based, for users).
        /// </summary>
        [Description("Index of the record in the owner Table (one based, for users).")]
        public int Index { get { return this.OwnerTable.IndexOf(this) + 1; } }

        public override string ToString()
        {
            return "Record (HC=" + this.GetHashCode().ToString() + "): {" + this.Label + "}";
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}