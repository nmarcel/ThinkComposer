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
// File   : DataType.cs
// Object : Instrumind.ThinkComposer.MetaModel.InformationMetaModel.DataType (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.11.07 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

using Instrumind.Common;
using Instrumind.Common.Visualization;

/// Base abstractions for the user metadefinition of Information schemas
namespace Instrumind.ThinkComposer.MetaModel.InformationMetaModel
{
    /// <summary>
    /// Defines a type for declaring table fields.
    /// </summary>
    [Serializable]
    public abstract class DataType : MetaDefinition     //- , IEquatable<DataType>
    {
        public const int MAX_SINGLELINE_TEXT_LENGTH = 255;
        public const int MAX_MULTILINE_TEXT_LENGTH = 50000;

        public static readonly TextType DataTypeText = null;
        public static readonly TextType DataTypeTextLong = null;
        public static readonly NumberType DataTypeNumber = null;
        public static readonly NumberType DataTypePositive = null;
        public static readonly NumberType DataTypeInteger = null;
        public static readonly NumberType DataTypePositiveInteger = null;
        public static readonly DateTimeType DataTypeDate = null;
        public static readonly DateTimeType DataTypeTime = null;
        public static readonly DateTimeType DataTypeDateTime = null;
        public static readonly ChoiceType DataTypeSwitch = null;
        public static readonly TableRecordLinkType DataTypeTableRecordRef = null;
        public static readonly IdeaLinkType DataTypeIdeaRef = null;
        public static readonly TableType DataTypeTable = null;
        public static readonly PictureType DataTypePicture = null;

        //P public static readonly IdeaLinkType DataTypeConceptRef = null;
        //P public static readonly IdeaLinkType DataTypeRelationshipRef = null;

        /// <summary>
        /// Static Constructor.
        /// </summary>
        static DataType()
        {
            // IMPORTANT: For enable the comparison after deserialization,
            // the GUIDs of these Predefined Data-Types should not change in the lifetime of the product.

            DataTypeText = new TextType("Text", "Text", "Accepts simple plain Text values (i.e. for names or codes), with a limit of "
                                                        + MAX_SINGLELINE_TEXT_LENGTH.ToString() + " characters.", MAX_SINGLELINE_TEXT_LENGTH);
            DataTypeText.GlobalId = new Guid("813d565a-5481-4faf-9084-578090f392bc");
            DataTypeText.Pictogram = Display.GetLibImage("type_text.png");
            PredefinedDataTypes.Add(DataTypeText);

            DataTypeTextLong = new TextType("Text-Long", "TextLong", "Accepts long plain Text values (i.e. for summaries), with a limit of "
                                                                     + MAX_MULTILINE_TEXT_LENGTH.ToString() + " characters.", MAX_MULTILINE_TEXT_LENGTH);
            DataTypeTextLong.GlobalId = new Guid("a90f022c-2fb0-4038-a367-2ee761f28bf2");
            DataTypeTextLong.Pictogram = Display.GetLibImage("type_textlong.png");
            PredefinedDataTypes.Add(DataTypeTextLong);

            DataTypeNumber = new NumberType("Number", "Number", "Accepts Numeric values (up to 10 integer digits, with 5 decimals, positive or negative).");
            DataTypeNumber.GlobalId = new Guid("2ddc27bd-d4a2-4577-92bf-8bc7fd1a08d3");
            DataTypeNumber.Pictogram = Display.GetLibImage("type_numsigdec.png");
            PredefinedDataTypes.Add(DataTypeNumber);

            DataTypePositive = new NumberType("Positive", "Positive", "Accepts Positive values (up to 10 integer digits, with 5 decimal, positive only).", 10, 5, 0);
            DataTypePositive.GlobalId = new Guid("0cb26944-ee4f-4d49-ae99-8f47f9bad2f7");
            DataTypePositive.Pictogram = Display.GetLibImage("type_numunsdec.png");
            PredefinedDataTypes.Add(DataTypePositive);

            DataTypeInteger = new NumberType("Integer", "Integer", "Accepts Integer values (up to 15 integer digits, with 0 decimals, positive or negative).", 15, 0);
            DataTypeInteger.GlobalId = new Guid("5a5f4617-2a15-43fa-9567-e30a78f276ad");
            DataTypeInteger.Pictogram = Display.GetLibImage("type_numsigint.png");
            PredefinedDataTypes.Add(DataTypeInteger);

            DataTypePositiveInteger = new NumberType("Positive-Integer", "PositiveInteger", "Accepts Positive Integer values (up to 15 integer digits, with 0 decimals, positive only).", 15, 0, 0);
            DataTypePositiveInteger.GlobalId = new Guid("521531a1-1570-42cf-8cd4-37a76048ca2e");
            DataTypePositiveInteger.Pictogram = Display.GetLibImage("type_numunsint.png");
            PredefinedDataTypes.Add(DataTypePositiveInteger);

            DataTypeDate = new DateTimeType("Date", "Date", "Accepts Date values", true, false);
            DataTypeDate.GlobalId = new Guid("bc801cf0-a7bd-4650-bf82-27ef7e5ca379");
            DataTypeDate.Pictogram = Display.GetLibImage("type_date.png");
            DataTypeDate.ExplicitInitializer = (() => DateTime.Now.Date);
            PredefinedDataTypes.Add(DataTypeDate);

            DataTypeTime = new DateTimeType("Time", "Time", "Accepts Time values", false, true);
            DataTypeTime.GlobalId = new Guid("8ce60fdc-d61e-4801-be88-3374937c5aa6");
            DataTypeTime.Pictogram = Display.GetLibImage("type_time.png");
            PredefinedDataTypes.Add(DataTypeTime);

            DataTypeDateTime = new DateTimeType("Date-Time", "DateTime", "Accepts Date-Time values");
            DataTypeDateTime.GlobalId = new Guid("9d3119c6-7846-453e-8610-196646dab7fb");
            DataTypeDateTime.Pictogram = Display.GetLibImage("type_datetime.png");
            DataTypeDateTime.ExplicitInitializer = (() => DateTime.Now);
            PredefinedDataTypes.Add(DataTypeDateTime);

            DataTypeSwitch = new ChoiceType("Switch", "Switch", "Accepts only one of two values: Yes or No (which can be interpreted as true/false, on/off, etc.).");
            DataTypeSwitch.GlobalId = new Guid("033746a9-60c5-4e67-99ad-69b2b93029a4");
            DataTypeSwitch.RegisterOption(0, "No");
            DataTypeSwitch.RegisterOption(255, "Yes");
            DataTypeSwitch.Pictogram = Display.GetLibImage("type_switch.png");
            DataTypeSwitch.DisplayAlignment = TextAlignment.Center;
            DataTypeSwitch.StorageReadConverter =
                (value =>
                    {
                        byte Value = 0;
                        byte.TryParse(value.ToStringAlways(), out Value);
                        return (Value == 255);
                    });
            DataTypeSwitch.StorageWriteConverter =
                (value =>
                    {
                        bool Value = false;
                        bool.TryParse(value.ToStringAlways(), out Value);
                        return (byte)(Value ? 255 : 0);
                    });
            PredefinedDataTypes.Add(DataTypeSwitch);

            /* JUST AS AN EXAMPLE...
            //            CHOICE TYPES ARE NOT USER-DEFINABLE BECAUSE OF NO EDITING (UNDO/REDO) SUPPORT.
            var ChoiceKind = new ChoiceType("Validation-State", "ValidationState", "Validation states.");
            ChoiceKind.GlobalId = new Guid("841f89ac-cd79-4bb3-bfed-83f8869668f4");
            ChoiceKind.DisplayMinLength = TextKind.DisplayMinLength;    // Needed because the Name of the choice/option is shown.
            ChoiceKind.RegisterOption(0, "Unknown");
            ChoiceKind.RegisterOption(1, "Valid");
            ChoiceKind.RegisterOption(2, "Invalid");
            ChoiceKind.RegisterOption(3, "In-Evaluation");
            ChoiceKind.Pictogram = Display.GetLibImage("type_choice.png");
            PredefinedDataTypes.Add(ChoiceKind); */

            DataTypeIdeaRef = new IdeaLinkType("Idea Reference", "IdeaReference", "References a Composition Idea.");
            DataTypeIdeaRef.GlobalId = new Guid("1a604a39-c3a3-427a-9ffb-c8eafd4b0616");
            DataTypeIdeaRef.Pictogram = Display.GetAppImage("ref_idea.png");
            PredefinedDataTypes.Add(DataTypeIdeaRef);

            DataTypeTableRecordRef = new TableRecordLinkType("Table-Record Reference", "TableRecordRef", "References a Record of a Base Table.");
            DataTypeTableRecordRef.GlobalId = new Guid("8ec14293-b74f-410f-b703-3458eeb53b80");
            DataTypeTableRecordRef.Pictogram = Display.GetAppImage("record_ref.png");
            PredefinedDataTypes.Add(DataTypeTableRecordRef);

            DataTypeTable = new TableType("Table", "Table", "Contains a nested Table.");
            DataTypeTable.GlobalId = new Guid("c79c5afe-1e16-489a-b5c1-73a5c49ed520");
            DataTypeTable.Pictogram = Display.GetAppImage("table.png");
            PredefinedDataTypes.Add(DataTypeTable);

            DataTypePicture = new PictureType("Picture", "Picture", "Stores a photograph or graphic representation.");
            DataTypePicture.GlobalId = new Guid("2af512e9-dcfe-4850-a499-ac69eee2cdd2");
            DataTypePicture.Pictogram = Display.GetAppImage("picture.png");
            PredefinedDataTypes.Add(DataTypePicture);

            /* POSTPONED: Difficult to filter tree-view items.
            DataTypeConceptRef = new IdeaLinkType("Concept Reference", "ConceptReference", "References a Composition Concept.", true, false);
            DataTypeConceptRef.GlobalId = new Guid(CREATE);
            DataTypeConceptRef.Pictogram = Display.GetAppImage("ref_concept.png");
            PredefinedDataTypes.Add(DataTypeConceptRef);

            DataTypeRelationshipRef = new IdeaLinkType("Relationship Reference", "RelationshipReference", "References a Composition Relationship.", false, true);
            DataTypeRelationshipRef.GlobalId = new Guid("6416527d-f6a2-40ba-b87a-7b82a0c1f6c2");
            DataTypeRelationshipRef.Pictogram = Display.GetAppImage("ref_relationship.png");
            PredefinedDataTypes.Add(DataTypeRelationshipRef); */

            //T var rr = Guid.NewGuid(); Console.WriteLine(rr);
        }

        /// <summary>
        /// Collection of standard predefined data types.
        /// </summary>
        public static readonly List<DataType> PredefinedDataTypes = new List<DataType>();

        /// <summary>
        /// Functions to initialize Data Type based Values, indexed by Data Type tech-names.
        /// </summary>
        // IMPORTANT: This is indexed by key (string) to be re-attached after deserialization.
        private static readonly Dictionary<string, Func<object>> ValueInitializers = new Dictionary<string, Func<object>>();

        /// <summary>
        /// Functions to convert Values to be Read, from storage-type to final-type, indexed by Data Type tech-names.
        /// In each function: The input parameter is the value in the storage-type form. The output result is the value in the final-type form.
        /// </summary>
        // IMPORTANT: This is indexed by key (string) to be re-attached after deserialization.
        private static readonly Dictionary<string, Func<object, object>> StorageReadConverters = new Dictionary<string, Func<object, object>>();

        /// <summary>
        /// Functions to convert Values to be Written, from final-type to storage-type, indexed by Data Type tech-names.
        /// In each function: The input parameter is the value in the final-type form. The output result is the value in the storage-type form.
        /// </summary>
        // IMPORTANT: This is indexed by key (string) to be re-attached after deserialization.
        private static readonly Dictionary<string, Func<object, object>> StorageWriteConverters = new Dictionary<string, Func<object, object>>();

        /// <summary>
        /// Gets the supplied Source string converted to a value with the most well suited basic data-type.
        /// Optionally, an indication of consider empty-string as null can be specified.
        /// </summary>
        public static Tuple<object, BasicDataType> ConvertToMostSuitedBasicDataType(string Source, bool EmptyIsNull = false)
        {
            object Value = null;    // Do not assign Value here, it is overwritten by the "TryParses outs" even when not returning a value
            BasicDataType Kind = null;

            // POSTPONED: Add more specific types (also improve TryParseValueFrom() methods)

            if (Source.IsAbsent())
            {
                Kind = DataType.DataTypeText;

                if (EmptyIsNull)
                    Value = null;
            }
            else
                if (DataTypeNumber.TryParseValueFrom(Source, out Value))
                    Kind = DataTypeNumber;
                else
                    if (DataTypeDateTime.TryParseValueFrom(Source, out Value))
                        Kind = DataTypeDateTime;
                    else
                    {
                        Value = Source;
                        Kind = DataType.DataTypeText;
                    }

            return Tuple.Create(Value, Kind);
        }

        /// <summary>
        /// Returns the wider basic data-type of this Original respect the Alternative.
        /// </summary>
        public static BasicDataType GetWiderBasicDataTypeRespect(BasicDataType Original, BasicDataType Alternative)
        {
            // POSTPONED: Extend being more specific.

            if (Original == Alternative)
                return Original;

            return DataType.DataTypeText;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public DataType(string Name, string TechName, string Summary = "", ImageSource Pictogram = null)
            : base(Name, TechName, Summary, Pictogram)
        {
            // IMPORTANT: If Owner is added, then implement IVersionUpdater
        }

        /// <summary>
        /// References the .NET type of the final container for the objects declared with this data type.
        /// </summary>
        // IMPORTANT: If this is changed, any dependant Field-Def should update its default-value.
        public abstract Type ContainerType { get; }

        /// <summary>
        /// Gets the function used to create initial values.
        /// </summary>
        public Func<object> ExplicitInitializer
        {
            get { return ValueInitializers.GetValueOrDefault(this.TechName); }
            set { ValueInitializers.AddOrReplace(this.TechName, value); }
        }

        /// <summary>
        /// Gets the function used to convert for Read a value, from its storage-type form to its final-type form.
        /// Example: The Switch type, where the user works with booleans (true or false), but internally a byte (0 or 255) is stored for these values.
        /// </summary>
        public Func<object, object> StorageReadConverter
        {
            get { return StorageReadConverters.GetValueOrDefault(this.TechName); }
            set { StorageReadConverters.AddOrReplace(this.TechName, value); }
        }

        /// <summary>
        /// Gets the function used to convert for Write a value, from its final-type form to its storage-type form.
        /// Example: The Switch type, where the user works with booleans (true or false), but internally a byte (0 or 255) is stored for these values.
        /// </summary>
        public Func<object, object> StorageWriteConverter
        {
            get { return StorageWriteConverters.GetValueOrDefault(this.TechName); }
            set { StorageWriteConverters.AddOrReplace(this.TechName, value);  }
        }

        /// <summary>
        /// If possible, gets a value of this data-type from the supplied Source string and puts it in the specified Result out-reference.
        /// Returns indication of success (true) or failure (false) of the parsing.
        /// </summary>
        public virtual bool TryParseValueFrom(string Source, out object Result)
        {
            Result = null;
            return false;
        }

        /// <summary>
        /// Validates the supplied Value against this data-type.
        /// Returns null if succeeded, or an error message if validation is not passed.
        /// </summary>
        public virtual string Validate(object Value)
        {
            if (Value == null && this.ContainerType.IsStruct())
                return "Value is null";

            if (Value != null && !this.ContainerType.IsAssignableFrom(Value.GetType()))
                return "Value is not of type '" + this.Name + "'.";

            return null;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public bool Equals(DataType other)
        {
            /* T
            if (other != null && other.GlobalId == this.GlobalId
                && !Object.ReferenceEquals(this, other))
                Console.WriteLine("DataType instances are different, but equivalent."); */

            var Result = (other != null && other.GlobalId == this.GlobalId);
            return Result;
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as DataType);
        }

        public override int GetHashCode()
        {
            var Bytes = this.GlobalId.ToByteArray();
            var Result = BitConverter.ToInt32(General.CreateArray(Bytes[0], Bytes[7], Bytes[13], Bytes[15]), 0);
            return Result;
        }
    }
}