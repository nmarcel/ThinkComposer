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
// File   : FieldDefinition.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.FieldDefinition (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.08 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;
using Instrumind.Common.Visualization;

using Instrumind.ThinkComposer.Composer;
using Instrumind.ThinkComposer.Model.InformationModel;
using Instrumind.ThinkComposer.MetaModel.GraphMetaModel;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a table structure field.
    /// </summary>
    [Serializable]
    public class FieldDefinition : MetaDefinition, IModelEntity, IModelClass<FieldDefinition>, IVersionUpdater
    {
        /// <summary>
        /// Estimated field width (for an average descriptive value), in characters.
        /// </summary>
        public const int STD_FIELD_WITDH = 35;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static FieldDefinition()
        {
            __ClassDefinitor = new ModelClassDefinitor<FieldDefinition>("FieldDefinition", MetaDefinition.__ClassDefinitor, "Field Definition",
                                                                        "Defines a table structure field.");
            __ClassDefinitor.DeclareProperty(__OwnerTableDef);
            __ClassDefinitor.DeclareProperty(__StorageIndex);
            __ClassDefinitor.DeclareCollection(__Categories);
            __ClassDefinitor.DeclareProperty(__FieldType);
            __ClassDefinitor.DeclareProperty(__IsRequired);
            __ClassDefinitor.DeclareProperty(__HideInDiagram);
            __ClassDefinitor.DeclareProperty(__InitialStoreValue);
            __ClassDefinitor.DeclareProperty(__DefaultEmptyValue);
            __ClassDefinitor.DeclareProperty(__MinValue);
            __ClassDefinitor.DeclareProperty(__MaxValue);
            __ClassDefinitor.DeclareProperty(__ValuesSource);
            __ClassDefinitor.DeclareProperty(__IdeaReferencingProperty);
            __ClassDefinitor.DeclareProperty(__ContainedTableDesignator);
            __ClassDefinitor.DeclareProperty(__ContainedTableIsSingleRecord);
            //? __ClassDefinitor.DeclareProperty(__RestrictToValuesSource);
            //? __ClassDefinitor.DeclareProperty(__UnitOfMeasure);
            //? __ClassDefinitor.DeclareProperty(__IsInternal);

            __FieldType.ItemsSourceGetter =
                ((ctx) =>
                    {
                        if (ctx == null || ctx.EditEngine == null)
                            return null;

                        var Result = ((CompositionEngine)ctx.EditEngine).TargetComposition.CompositeContentDomain.AvailableDataTypes;
                        return Result;
                    });
            //? (was needed for ConceptDefinition.__AutomaticCreationConceptDef): __FieldType.ItemsSourceSelectedValuePath = "TechName";
            //? (was needed for ConceptDefinition.__AutomaticCreationConceptDef): __FieldType.ItemsSourceDisplayMemberPath = "Name";

            __ValuesSource.ItemsSourceGetter =
                ((ctx) =>
                {
                    if (ctx == null || ctx.EditEngine == null)
                        return null;

                    var Result = Domain.Unassigned_BaseTable.IntoEnumerable()
                                    .Concat(((CompositionEngine)ctx.EditEngine).TargetComposition.CompositeContentDomain.BaseTables);
                    return Result;
                });

            __IdeaReferencingProperty.ItemsSourceGetter =
                ((ctx) =>
                {
                    if (ctx == null || ctx.EditEngine == null)
                        return null;

                    var Result = Domain.Unassigned_IdeaReferencingPropertyProperty.IntoEnumerable()
                                    .Concat(FormalElement.__ClassDefinitor.Properties.Where(prop => prop.IsIdentificator));

                    return Result;
                });
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="OwnerTableDef">Table-Structure Definition owning this Field Definition.</param>
        /// <param name="Name">Name of the FieldDefinition.</param>
        /// <param name="TechName">Technical Name of the FieldDefinition.</param>
        /// <param name="FieldType">Data-type for the objects to be stored in the defined Field.</param>
        /// <param name="Summary">Summary of the FieldDefinition.</param>
        /// <param name="Pictogram">Image representing the FieldDefinition.</param>
        public FieldDefinition(TableDefinition OwnerTableDef, string Name, string TechName, DataType FieldType, string Summary = "", ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            General.ContractRequiresNotNull(OwnerTableDef, FieldType);

            this.OwnerTableDef = OwnerTableDef;
            this.FieldType = FieldType.NullDefault(DataType.DataTypeText);

            /* POSTPONED: Support for store values in a local (of the field-def) list instead of Table reference.
             *            Notice that Values-Soruce is declared as property rather than collection.
            this.ValuesSource = new EditableList<object>(__ValuesSource.TechName, this); */
        }

        /// <summary>
        /// Internal Constructor for Agents and Cloning.
        /// </summary>
        internal FieldDefinition()
        {
        }

        /// <summary>
        /// Table-Structure Definition owning this Field Definition.
        /// </summary>
        public TableDefinition OwnerTableDef { get { return __OwnerTableDef.Get(this); } internal set { __OwnerTableDef.Set(this, value); } }
        protected TableDefinition OwnerTableDef_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, TableDefinition> __OwnerTableDef =
                   new ModelPropertyDefinitor<FieldDefinition, TableDefinition>("OwnerTableDef", EEntityMembership.External, true, EPropertyKind.Common, ins => ins.OwnerTableDef_, (ins, val) => ins.OwnerTableDef_ = val, false, true,
                                                                                "Owner Table-Structure Definition", "Table-Structure Definition owning this Field Definition.");

        /// <summary>
        /// Actual position, within the table record structure, where the values of this field are located.
        /// </summary>
        public int StorageIndex { get { return __StorageIndex.Get(this); } internal set { __StorageIndex.Set(this, value); } }
        protected int StorageIndex_ = -1;
        public static readonly ModelPropertyDefinitor<FieldDefinition, int> __StorageIndex =
                   new ModelPropertyDefinitor<FieldDefinition, int>("StorageIndex", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.StorageIndex_, (ins, val) => ins.StorageIndex_ = val, false, true,
                                                                    "Storage Index", "Actual position, within the table record structure, where the values of this field are located.");

        /// <summary>
        /// Categorizes this field within its pairs of this and other tables.
        /// </summary>
        public EditableList<MetaCategory<FieldDefinition>> Categories { get; protected set; }
        public static ModelListDefinitor<FieldDefinition, MetaCategory<FieldDefinition>> __Categories =
                   new ModelListDefinitor<FieldDefinition, MetaCategory<FieldDefinition>>("Categories", EEntityMembership.External, ins => ins.Categories, (ins, coll) => ins.Categories = coll, "Categories", "Categorizes this field within its pairs of this and other tables.");

        /// <summary>
        /// Data type of the object values stored within this field definition declared fields.
        /// </summary>
        public DataType FieldType
        {
            get { return __FieldType.Get(this); }
            set
            {
                if (value == null)
                    throw new UsageAnomaly("A FieldType of a FieldDefinition cannot be null");

                __FieldType.Set(this, value);
            }
        }
        protected DataType FieldType_ = DataType.DataTypeText;
        public static readonly ModelPropertyDefinitor<FieldDefinition, DataType> __FieldType =
                   new ModelPropertyDefinitor<FieldDefinition, DataType>("FieldType", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.FieldType_, (ins, val) => ins.FieldType_ = val, true, false,
                                                                         "Field Type", "Data type of the values that will be stored in the derived fields.");

        /// <summary>
        /// Indicates whether the field value must be stored or can be let empty (null).
        /// </summary>
        public bool IsRequired { get { return __IsRequired.Get(this); } set { __IsRequired.Set(this, value); } }
        protected bool IsRequired_ = false;
        public static readonly ModelPropertyDefinitor<FieldDefinition, bool> __IsRequired =
                   new ModelPropertyDefinitor<FieldDefinition, bool>("IsRequired", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsRequired_, (ins, val) => ins.IsRequired_ = val, false, true,
                                                                     "Is Required", "Indicates whether the field value must be stored or can be let empty (null).");

        /// <summary>
        /// Indicates that the field values must be hidden in the diagram view.
        /// </summary>
        public bool HideInDiagram { get { return __HideInDiagram.Get(this); } set { __HideInDiagram.Set(this, value); } }
        protected bool HideInDiagram_ = false;
        public static readonly ModelPropertyDefinitor<FieldDefinition, bool> __HideInDiagram =
                   new ModelPropertyDefinitor<FieldDefinition, bool>("HideInDiagram", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.HideInDiagram_, (ins, val) => ins.HideInDiagram_ = val, true, false,
                                                                     "Hide in Diagram", "Indicates that the field values must be hidden in the diagram view.");

        /// <summary>
        /// Value to be stored once, at Record or Field creation, if none is explicitly setted.
        /// </summary>
        public object InitialStoreValue { get { return __InitialStoreValue.Get(this); } set { __InitialStoreValue.Set(this, value); } }
        protected object InitialStoreValue_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, object> __InitialStoreValue =
                   new ModelPropertyDefinitor<FieldDefinition, object>("InitialStoreValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.InitialStoreValue_, (ins, val) => ins.InitialStoreValue_ = val, false, true,
                                                                       "Initial-Store Value", "Value to be stored once, at Record or Field creation, if none is explicitly setted.");

        /// <summary>
        /// Value to be used for get and set operations, instead of empty/null, over this Field. Therefore, this value is actually never stored.
        /// </summary>
        public object DefaultEmptyValue { get { return __DefaultEmptyValue.Get(this); } set { __DefaultEmptyValue.Set(this, value); } }
        protected object DefaultEmptyValue_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, object> __DefaultEmptyValue =
                   new ModelPropertyDefinitor<FieldDefinition, object>("DefaultEmptyValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.DefaultEmptyValue_, (ins, val) => ins.DefaultEmptyValue_ = val, false, true,
                                                                       "Default-Empty Value", "Value to be used for get and set operations, instead of empty/null, over this Field. Therefore, this value is actually never stored.");

        /// <summary>
        /// Minimum acceptable value.
        /// </summary>
        public object MinValue { get { return __MinValue.Get(this); } set { __MinValue.Set(this, value); } }
        protected object MinValue_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, object> __MinValue =
                   new ModelPropertyDefinitor<FieldDefinition, object>("MinValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MinValue_, (ins, val) => ins.MinValue_ = val, false, true,
                                                                       "Minimum Value", "Minimum acceptable value.");

        /// <summary>
        /// Maximum acceptable value.
        /// </summary>
        public object MaxValue { get { return __MaxValue.Get(this); } set { __MaxValue.Set(this, value); } }
        protected object MaxValue_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, object> __MaxValue =
                   new ModelPropertyDefinitor<FieldDefinition, object>("MaxValue", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.MaxValue_, (ins, val) => ins.MaxValue_ = val, false, true,
                                                                       "Maximum Value", "Maximum acceptable value.");

        /// <summary>
        /// Source of Records, either to be referenced or from which get a label/compatible value, to populate this field.
        /// </summary>
        public IList<TableRecord> ValuesSource
        {
            get
            {
                var Result = __ValuesSource.Get(this);
                if (Result == null)
                    return Domain.Unassigned_BaseTable;

                return Result;
            }
            set
            {
                if (value == Domain.Unassigned_BaseTable)
                    value = null;

                __ValuesSource.Set(this, value);
            }
        }
        protected IList<TableRecord> ValuesSource_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, IList<TableRecord>> __ValuesSource =
                  new ModelPropertyDefinitor<FieldDefinition, IList<TableRecord>>("ValuesSource", EEntityMembership.External, null, EPropertyKind.Common, ins => ins.ValuesSource_, (ins, val) => ins.ValuesSource_ = val, false, false,
                                                                                  "Source Base Table", "Source of Records, either to be referenced or from which get a label/compatible value, to populate this field.");

        /// <summary>
        /// If set, indicates the property used to Reference Ideas from this field.
        /// </summary>
        public MModelPropertyDefinitor IdeaReferencingProperty
        {
            get
            {
                var Result = __IdeaReferencingProperty.Get(this);
                if (Result == null)
                    return Domain.Unassigned_IdeaReferencingPropertyProperty;

                return Result;
            }
            set
            {
                if (value == Domain.Unassigned_IdeaReferencingPropertyProperty)
                    value = null;

                __IdeaReferencingProperty.Set(this, value);
            }
        }
        protected StoreBox<MModelPropertyDefinitor> IdeaReferencingProperty_ = new StoreBox<MModelPropertyDefinitor>();
        public static readonly ModelPropertyDefinitor<FieldDefinition, MModelPropertyDefinitor> __IdeaReferencingProperty =
                  new ModelPropertyDefinitor<FieldDefinition, MModelPropertyDefinitor>("IdeaReferencingProperty", EEntityMembership.External, null, EPropertyKind.Common,
                        ins =>
                        {
                            if (ins.IdeaReferencingProperty_ == null)  // Ensure store-box is created on older compositions.
                                ins.IdeaReferencingProperty_ = new StoreBox<MModelPropertyDefinitor>();

                            return ins.IdeaReferencingProperty_;
                        },
                        (ins, val) =>
                        {
                            if (ins.IdeaReferencingProperty_ == null)  // Ensure store-box is created on older compositions.
                                ins.IdeaReferencingProperty_ = new StoreBox<MModelPropertyDefinitor>();

                            ins.IdeaReferencingProperty_ = val;
                        },
                        false, false, "Idea Referencing by", "If set, indicates the property used to Reference Ideas from this field.");

        /// <summary>
        /// If set, stores or references the Table-Structure Definition declaring the type of the Tables contained by the implementing Fields.
        /// </summary>
        public TableDetailDesignator ContainedTableDesignator { get { return __ContainedTableDesignator.Get(this); } set { __ContainedTableDesignator.Set(this, value); } }
        protected TableDetailDesignator ContainedTableDesignator_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, TableDetailDesignator> __ContainedTableDesignator =
                   new ModelPropertyDefinitor<FieldDefinition, TableDetailDesignator>("ContainedTableDesignator", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ContainedTableDesignator_, (ins, val) => ins.ContainedTableDesignator_ = val, false, false,
                                                                                      "Contained Table Designator", "If set, stores or references the Table-Structure Definition declaring the type of the Tables contained by the implementing Fields.");

        /// <summary>
        /// Indicates whether the contained-table, if set, has only one record, else is Multi-Record.
        /// </summary>
        public bool ContainedTableIsSingleRecord { get { return __ContainedTableIsSingleRecord.Get(this); } set { __ContainedTableIsSingleRecord.Set(this, value); } }
        protected bool ContainedTableIsSingleRecord_ = false;
        public static readonly ModelPropertyDefinitor<FieldDefinition, bool> __ContainedTableIsSingleRecord =
                   new ModelPropertyDefinitor<FieldDefinition, bool>("ContainedTableIsSingleRecord", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.ContainedTableIsSingleRecord_, (ins, val) => ins.ContainedTableIsSingleRecord_ = val, false, false,
                                                                     "Contained Table is Single-Record", "Indicates whether the contained-table, if set, has only one record, else is Multi-Record.");

        /*?
        POSTPONED: Allow to store values outside these provided from the Values-Source.
        /// <summary>
        /// Indicates whether to only store a value from the availables, or else accept any other also.
        /// </summary>
        public bool RestrictToValuesSource { get { return __RestrictToValuesSource.Get(this); } set { __RestrictToValuesSource.Set(this, value); } }
        protected bool RestrictToValuesSource_ = true;
        public static readonly ModelPropertyDefinitor<FieldDefinition, bool> __RestrictToValuesSource =
                   new ModelPropertyDefinitor<FieldDefinition, bool>("RestrictToValuesSource", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.RestrictToValuesSource_, (ins, val) => ins.RestrictToValuesSource_ = val, false, true, true,
                                                                     "Restrict To Values-Source", "Indicates whether to only store a value from the values-source, or else accept any other also.");

        /// <summary>
        /// Key of the Unit of Measure used for the declared field.
        /// </summary>
        public string UnitOfMeasure { get { return __UnitOfMeasure.Get(this); } set { __UnitOfMeasure.Set(this, value); } }
        protected string UnitOfMeasure_ = null;
        public static readonly ModelPropertyDefinitor<FieldDefinition, string> __UnitOfMeasure =
                   new ModelPropertyDefinitor<FieldDefinition, string>("UnitOfMeasure", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.UnitOfMeasure_, (ins, val) => ins.UnitOfMeasure_ = val, false, true, true,
                                                                       "Unit of Measure", "Key of the Unit of Measure used for the declared field.");

        /// <summary>
        /// Specialized controller, not simply based on data-type, for when the field is being iteractively edited.
        /// </summary>
        // public EditionController Editor { get; protected set; }

        /// <summary>
        /// Indicates whether the defined field will be for internal usage (for example: an 'IsDisplayed' field for record show/hide).
        /// </summary>
        public bool IsInternal { get { return __IsInternal.Get(this); } set { __IsInternal.Set(this, value); } }
        protected bool IsInternal_ = false;
        public static readonly ModelPropertyDefinitor<FieldDefinition, bool> __IsInternal =
                   new ModelPropertyDefinitor<FieldDefinition, bool>("IsInternal", EEntityMembership.InternalCoreExclusive, null, EPropertyKind.Common, ins => ins.IsInternal_, (ins, val) => ins.IsInternal_ = val, false, false, true,
                                                                     "Is Internal", "Indicates whether the defined field will be for internal usage (for example: an 'IsDisplayed' field for record show/hide).");

        /// <summary>
        /// Gets the function used for read field values.
        /// </summary>
        public Func<object> ExplicitGetter { get{ return null; } }

        /// <summary>
        /// Gets the action used for write field values.
        /// </summary>
        public Action<object> ExplicitSetter { get{ return null; } }
        */

        /// <summary>
        /// Gets Function used to create initial values. By default gets the Field-Type explicit initializer.
        /// </summary>
        public Func<object> ExplicitInitializer { get { return this.FieldType.ExplicitInitializer; } }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets an estimated column pixels width for this specified Field Definition, considering the optionally supplied Character Width.
        /// </summary>
        public double GetEstimatedColumnPixelsWidth(double CharWidth = Display.CHAR_PXWIDTH_DEFAULT,
                                                    double MaxCharacters = FieldDefinition.STD_FIELD_WITDH)
        {
            var EstimatedCharacters = (double)this.Name.Length;

            if (this.FieldType is BasicDataType)
                EstimatedCharacters = ((BasicDataType)this.FieldType).DisplayMinLength;
            else
                if (this.FieldType is TableRecordLinkType || this.FieldType is IdeaLinkType
                    || this.FieldType is TableType)
                    EstimatedCharacters = STD_FIELD_WITDH;
                else
                    if (this.FieldType is PictureType)
                        EstimatedCharacters = STD_FIELD_WITDH * 2;

            var Result = Math.Max(this.Name.Length, EstimatedCharacters).EnforceMaximum(MaxCharacters) * CharWidth;

            return Result;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        #region IModelClass<FieldDefinition> Members

        public new MModelClassDefinitor ClassDefinition { get { return __ClassDefinitor; } }
        public new ModelClassDefinitor<FieldDefinition> ClassDefinitor { get { return __ClassDefinitor; } }
        public static readonly new ModelClassDefinitor<FieldDefinition> __ClassDefinitor = null;

        public override object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner) { return this.CreateClone(CloningScope, DirectOwner); }
        public new FieldDefinition CreateClone(ECloneOperationScope CloningScope, IMModelClass DirectOwner, bool AsActive = true) { return this.ClassDefinitor.PopulateInstance((FieldDefinition)this.MemberwiseClone(), this, DirectOwner, CloningScope, true, AsActive); }
        public FieldDefinition PopulateFrom(FieldDefinition SourceElement, IMModelClass DirectOwner = null, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, params string[] MemberNames) { return this.ClassDefinitor.PopulateInstance(this, SourceElement, DirectOwner, CloningScope, false, true, MemberNames); }

        #endregion

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public void UpdateVersion()
        {
            if (this.Version != null)
                this.Version.Update();

            if (this.OwnerTableDef != null)
                this.OwnerTableDef.UpdateVersion();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    }
}