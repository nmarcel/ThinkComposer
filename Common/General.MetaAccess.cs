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
// File   : General.cs
// Object : Instrumind.Common.General (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.23 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Documents;

using Microsoft.Win32;

using Instrumind.Common.EntityBase;
using System.Runtime.CompilerServices;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides common features such as encryption, serialization, strings handling extensions and global constants. Meta-access part.
    /// </summary>
    public static partial class General
    {
        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the supplied Assigner if already local, else creates a new one base on the supplied Cloner for the original assigner value.
        /// </summary>
        public static Assignment<TValue> AssignLocal<TValue>(this Assignment<TValue> Assigner, Func<TValue, TValue> Cloner)
        {
            if (Assigner.IsLocal)
                return Assigner;

            Assigner = new Assignment<TValue>(Cloner(Assigner.Value), true);
            return Assigner;
        }

        /// <summary>
        /// Creates and returns a new Assignment for this supplied value, plus its locality indication
        /// (default=false, hence a shared reference that would not be changed).
        /// </summary>
        public static Assignment<TValue> Assign<TValue>(this TValue Value, bool IsLocal = false)
        {
            return (new Assignment<TValue>(Value, IsLocal));
        }

        /// <summary>
        /// Creates and returns a new shared/global Assignment for this supplied instance and member name.
        /// </summary>
        public static Assignment<TValue> Assign<TValue>(this IMModelClass Instance, string MemberName)
        {
            return (new Assignment<TValue>(Instance, MemberName));
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the value of a referenced Variable only if it is empty.
        /// An optional updater function can be specified for post setting invocation.
        /// Returns either the preexistent value, or the just setted.
        /// </summary>
        public static TValue SetEmptyVariable<TValue>(ref TValue Variable, Func<TValue> InitialSetter, Func<TValue, TValue> Updater = null)
        {
            if (Variable.IsEqual(default(TValue)))
                Variable = InitialSetter();

            if (Updater != null)
                Variable = Updater(Variable);

            return Variable;
        }

        /// <summary>
        /// Determines if two objects or references of the same type have equal value, regarding one or both are null.
        /// </summary>
        public static bool IsEqual<TEvaluated>(this TEvaluated Primary, TEvaluated Secondary)
        {
            var Result = (Primary == null && Secondary == null) || (Primary != null && Primary.Equals(Secondary));

            /* T
#if DEBUG
            if (!Result && Primary != null && Secondary != null)
            {
                var H1 = Primary.GetHashCode();
                var H2 = Secondary.GetHashCode();
                var HashCodes = "HC1=" + H1.ToString() + ", HC2=" + H2.ToString();
                Result = (Result && HashCodes.Length > 0);
            }
#endif
             */

            return Result;
        }

        /// <summary>
        /// Determines if two objects or references of the same type are equivalent, regarding one or both are null.
        /// Equivalence is determined based on GlobalId (GUID), depending on the evaluated base type.
        /// </summary>
        public static bool IsEquivalent<TEvaluated>(this TEvaluated Primary, TEvaluated Secondary)
        {
            if (Primary == null && Secondary == null)
                return true;

            if (Primary == null || Secondary == null)
                return false;

            var EvUniquePri = Primary as IUniqueElement;
            if (EvUniquePri != null)
                return EvUniquePri.GlobalId.IsEqual((Secondary as IUniqueElement).GlobalId);

            /*- For a new comparer?
            var EvIdentifPri = Primary as IIdentifiableElement;
            if (EvIdentifPri != null)
            {
                var EvIdentifSec = Secondary as IIdentifiableElement;
                return EvIdentifPri.Name.IsEqual(EvIdentifSec.Name);
            } */

            return Primary.IsEqual(Secondary);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns this instance Hash-Code plus its String representation after a Separator.
        /// Optionally, the new-lines can be included.
        /// </summary>
        public static string ToHashCodeAndString(this object Instance, string Separator = "!", bool IncludeNewLines = false)
        {
            if (Instance == null)
                return "<NULL>";

            var Result = Instance.GetHashCode().ToString() + Separator + Instance.ToString().RemoveNewLines();
            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the default value for a supplied Type variable.
        /// Like the C# "default" keyword, but for runtime.
        /// </summary>
        public static object GetDefaultValue(this Type SourceType)
        {
            return (SourceType.IsValueType ? Activator.CreateInstance(SourceType) : null);
        }

        /// <summary>
        /// Returns true if the type represents an struct.
        /// </summary>
        public static bool IsStruct(this Type SourceType)
        {
            return (SourceType.IsValueType && !SourceType.IsPrimitive && !SourceType.IsEnum);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Tries to populate the Name and Tech-Name values from themselves and Summary.
        /// Returns indication of Name populated (either previously or automatically).
        /// </summary>
        public static bool AutoPopulate(this IIdentifiableElement Target)
        {
            if (Target.Name.IsAbsent() && !Target.TechName.IsAbsent())
                Target.Name = Target.TechName.IdentifierToText();
            else
                if (Target.TechName.IsAbsent() && !Target.Name.IsAbsent())  // Only happens explicitly setting Tech-Name as empty
                    Target.TechName = Target.Name.TextToIdentifier();
                else
                    if (Target.Name.IsAbsent() && !Target.Summary.IsAbsent())
                        Target.Name = Target.Summary.GetLeft(255).TextToIdentifier(); // Tech-Name is autopopulated

            return !Target.Name.IsAbsent();
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the event fields of the specified Source Type.
        /// </summary>
        public static IEnumerable<FieldInfo> GetEventFields(this Type SourceType)
        {
            if (CachedEventFields.ContainsKey(SourceType))
                return CachedEventFields[SourceType];

            var FieldList = new List<FieldInfo>();
            var FieldDefs = SourceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo FieldDef in FieldDefs)
                if (FieldDef.FieldType.BaseType == typeof(System.MulticastDelegate))
                    FieldList.Add(FieldDef);

            CachedEventFields.Add(SourceType, FieldList);
            return FieldList;
        }
        private static Dictionary<Type, List<FieldInfo>> CachedEventFields = new Dictionary<Type, List<FieldInfo>>();

        /// <summary>
        /// Returns the supplied Source object without any event hooks.
        /// </summary>
        public static object WithoutEventHooks(this object Source)
        {
            if (Source == null)
                return null;

            var FieldDefs = Source.GetType().GetEventFields();
            foreach (FieldInfo FieldDef in FieldDefs)
                if (FieldDef.GetValue(Source) != null)
                    FieldDef.SetValue(Source, null);
 
            return Source;
        }

        /// <summary>
        /// Creates an empty clone of the supplied Source object,
        /// without calling constructor nor preserving fields or event handlers.
        /// </summary>
        public static object EmptyClone(this object Source)
        {
            if (Source == null)
                return null;

            var Clone = System.Runtime.Serialization.FormatterServices.GetSafeUninitializedObject(Source.GetType());
            return Clone;
        }

        /// <summary>
        /// Creates and returns a shallow clone for the supplied Source object.
        /// </summary>
        public static object CreateClone(this object Source)
        {
            var SourceType = Source.GetType();
            var ShallowClone = Activator.CreateInstance(SourceType, true);

            var Fields = SourceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var Field in Fields)
                Field.SetValue(ShallowClone, Field.GetValue(Source));

            return ShallowClone;
        }

        /// <summary>
        /// Updates the supplied Target instance with the content of the supplied Source instance.
        /// </summary>
        public static void UpdateInstance(object Target, object Source)
        {
            var SourceType = Source.GetType();
            var Fields = SourceType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var Field in Fields)
                Field.SetValue(Target, Field.GetValue(Source));
        }

        /// <summary>
        /// Compares two object instances, determining its data field (not properties) differences.
        /// Uses reflection or Entity-getters to access instance data, whatever is more appropriate.
        /// </summary>
        /// <typeparam name="TInstance">Type of the instances to compare.</typeparam>
        /// <param name="Primary">First instance to compare.</param>
        /// <param name="Secondary">Second instance to compare.</param>
        /// <param name="ValuesEqualityComparer">Determines, returning true, if the supplied two objects are/represents the same value. Optional. By default, compared with IsEqual().</param>
        /// <returns>Null if instances are equal. Else, the detected differences, informing field-name plus primary and secondary field values.</returns>
        public static Dictionary<string, Tuple<object, object>> DetermineDifferences<TInstance>(TInstance Primary, TInstance Secondary,
                                                                                                Func<object, object, bool> ValuesEqualityComparer = null)
        {
            if (Primary.IsEqual(Secondary))
                return null;

            var Result = new Dictionary<string, Tuple<object, object>>();

            var PrimaryInstance = Primary as IMModelClass;
            var SecondaryInstance = Secondary as IMModelClass;
            ValuesEqualityComparer = ValuesEqualityComparer.NullDefault(IsEqual);

            if (PrimaryInstance != null && SecondaryInstance != null)
                foreach (var MemberDef in PrimaryInstance.ClassDefinition.Properties)
                {
                    var PrimaryValue = MemberDef.Read(PrimaryInstance);
                    var SecondaryValue = MemberDef.Read(SecondaryInstance);

                    if (!ValuesEqualityComparer(PrimaryValue, SecondaryValue))
                        Result.Add(MemberDef.TechName, Tuple.Create<object, object>(PrimaryValue, SecondaryValue));
                }
            else
                foreach (var MemberDef in typeof(TInstance).GetFields())
                {
                    var PrimaryValue = MemberDef.GetValue(Primary);
                    var SecondaryValue = MemberDef.GetValue(Secondary);

                    if (!ValuesEqualityComparer(PrimaryValue, SecondaryValue))
                        Result.Add(MemberDef.Name, Tuple.Create<object, object>(PrimaryValue, SecondaryValue));
                }

            return (Result.Count < 1 ? null : Result);
        }

        /// <summary>
        /// Modifies the public fields values, of a target instance, with the values of another instance of the same type.
        /// Allows to indicate (true) whether the values must be compared in order to apply the change if them are different,
        /// else (false) the change will be forced for all.
        /// Returns whether any change -forced or not- was applied.
        /// </summary>
        public static bool AlterInstanceFields<TType>(ref TType Target, TType Source, bool ComparePriorToChange)
        {
            bool ChangeWasApplied = false;
            object TargetValue, SourceValue;

            if (ComparePriorToChange)
                foreach (FieldInfo FieldData in typeof(TType).GetFields())
                {
                    TargetValue = FieldData.GetValue(Target);
                    SourceValue = FieldData.GetValue(Source);

                    if ((TargetValue != null && SourceValue != null && !TargetValue.Equals(SourceValue))
                        || (TargetValue == null && SourceValue != null) || (TargetValue != null && SourceValue == null))
                    {
                        FieldData.SetValue(Target, SourceValue);
                        ChangeWasApplied = true;
                    }
                }
            else
            {
                foreach (FieldInfo FieldData in typeof(TType).GetFields())
                    FieldData.SetValue(Target, FieldData.GetValue(Source));

                ChangeWasApplied = true;
            }

            return ChangeWasApplied;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns, for a supplied type, the ancestor types and the implemented by itself.
        /// </summary>
        public static Dictionary<string, Type> ExtractImplementedTypes(this Type TargetType)
        {
            Dictionary<string, Type> ImplementedTypes = new Dictionary<string, Type>();

            while (TargetType != null)
            {
                ImplementedTypes.Add(TargetType.Name, TargetType);
                TargetType = TargetType.BaseType;
            }

            return ImplementedTypes;
        }

        /// <summary>
        /// Indicates whether the supplied type is descendant of (or is itself) the specified ancestor.
        /// </summary>
        public static bool InheritsFrom(this Type TargetType, Type AncestorType)
        {
            // IMPORTANT:
            // This cannot detect Delegates: return TargetType.IsAssignableFrom(AncestorType);

            while (TargetType != null)
            {
                if (TargetType == AncestorType)
                    return true;

                TargetType = TargetType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// For this type returns the first ancestor (or itself) that is a non-generic type.
        /// </summary>
        public static Type GetNonGenericType(this Type TargetType)
        {
            if (TargetType == null)
                return null;

            while (TargetType != null)
            {
                if (!TargetType.IsGenericType)
                    return TargetType;

                TargetType = TargetType.BaseType;
            }

            return typeof(object);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the Name of this type, including generic parameters, using C# style.
        /// Optionally, the type name (including generic type parameters) can be returned prepending namespaces.
        /// </summary>
        public static string GetSimplifiedName(this Type Source, bool IncludeNamespaces = false)
        {
            var Root = (Source.IsGenericType
                        ? Source.GetGenericTypeDefinition()
                        : Source);

            var Result = (Root.IsPrimitive || !IncludeNamespaces
                          ? Root.Name
                          : Root.ToString());

            if (!Source.IsGenericType)
                return Result;

            Result = Result.Substring(0, Result.LastIndexOf('`')) +
                         Source.GetGenericArguments().Aggregate("<",
                            (typesstr, currtype) =>
                                typesstr + (typesstr != "<" ? ", " : null) + GetSimplifiedName(currtype)) +
                         ">";

            return Result;
        }
        
        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this field-info is a backing field of a property.
        /// </summary>
        public static bool IsBackingField(this FieldInfo FieldDef)
        {
            return FieldDef.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the value of the supplied static Field name for the specified source type.
        /// </summary>
        public static object GetStaticFieldValue<TSource>(string FieldName)
        {
            return GetStaticFieldValue(typeof(TSource), FieldName);
        }

        /// <summary>
        /// Returns, for the supplied source type, the value of the supplied static Field name.
        /// </summary>
        public static object GetStaticFieldValue(Type TSource, string FieldName)
        {
            var FieldDef = TSource.GetField(FieldName, BindingFlags.Static | BindingFlags.Public);
            if (FieldDef == null)
                return null;

            return FieldDef.GetValue(null);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the value of the supplied static Property name for the specified source type.
        /// </summary>
        public static object GetStaticPropertyValue<TSource>(string PropertyName)
        {
            return GetStaticPropertyValue(typeof(TSource), PropertyName);
        }

        /// <summary>
        /// Returns, for the supplied source type, the value of the supplied static Property name.
        /// </summary>
        public static object GetStaticPropertyValue(Type TSource, string PropertyName)
        {
            var PropertyDef = TSource.GetProperty(PropertyName, BindingFlags.Static | BindingFlags.Public);
            if (PropertyDef == null)
                return null;

            return PropertyDef.GetValue(null, null);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the specified attribute associated to a type. Returns null if not exists.
        /// </summary>
        public static Attribute ExtractAttribute(this Type SourceType, string AttributeName)
        {
            var AssignedAttributes = SourceType.GetCustomAttributes(false);
            foreach (Attribute EvaluatedAtribute in AssignedAttributes)
                if (ExtractImplementedTypes(EvaluatedAtribute.GetType()).ContainsKey(AttributeName))
                    return EvaluatedAtribute;
            return null;
        }

        /// <summary>
        /// Returns indication of member (property or field) found, plus the value of a member for the object and name supplied.
        /// </summary>
        public static Tuple<bool, object> ExtractMemberValue(object SourceObject, string MemberName)
        {
            var PropertyValue = ExtractPropertyValue(SourceObject, MemberName);

            if (PropertyValue.Item1)
                return PropertyValue;

            return ExtractFieldValue(SourceObject, MemberName);
        }

        /// <summary>
        /// Returns indication of field found, plus the value of a field for the object and name supplied.
        /// </summary>
        public static Tuple<bool, object> ExtractFieldValue(object SourceObject, string FieldName)
        {
            if (SourceObject == null)
                throw new UsageAnomaly("Cannot read the member named '" + FieldName + "' from a null object reference.");

            FieldInfo FieldData = SourceObject.GetType().GetField(FieldName);
            Tuple<bool, object> Result = null;

            /* THIS WAS NOT POSSIBLE
            if (SourceObject is DynamicObject)
            {
                var DynamicSource = SourceObject as DynamicObject;
                object Value = null;
                var MemberBinder = new GetMemberBinder();   // CANNOT BE PROVIDED. IS ABSTRACT.
                MemberBinder.Name = FieldName;
                var CanGet = DynamicSource.TryGetMember(MemberBinder, out Value);
                Result = new Tuple<bool, object>(CanGet, Value);
            }
            else */

            if (SourceObject is IDynamicMetaObjectProvider)
            {
                // EFFECTIVE ALTERNATIVE (ALBEIT SLOW, HENCE THE CACHING)
                var QualifiedName = SourceObject.GetType().FullName + General.STR_SEPARATOR + FieldName;

                CallSite<Func<CallSite, object, object>> CallingSite = null;

                if (CachedCallSites.ContainsKey(QualifiedName))
                    CallingSite = CachedCallSites[QualifiedName];
                else
                {
                    var MemberBinder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(
                                        Microsoft.CSharp.RuntimeBinder.CSharpBinderFlags.None,
                                        FieldName, SourceObject.GetType(),
                                        Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create(
                                            Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfoFlags.None, null).IntoArray());
                    CallingSite = CallSite<Func<CallSite, object, object>>.Create(MemberBinder);
                    CachedCallSites.Add(QualifiedName, CallingSite);
                }

                try
                {
                    Result = Tuple.Create(true, CallingSite.Target(CallingSite, SourceObject));
                }
                catch
                {
                    Result = Tuple.Create(false, (object)null);
                }

                return Result;
            }
            else
                Result = (new Tuple<bool, object>(FieldData != null, FieldData != null ? FieldData.GetValue(SourceObject) : null));

            return Result;
        }

        private static Dictionary<string, CallSite<Func<CallSite, object, object>>> CachedCallSites =
                        new Dictionary<string, CallSite<Func<CallSite, object, object>>>();
        
        /// <summary>
        /// Returns indication of property found, plus the value of a property (not indexed) for the object and name supplied.
        /// Optionally, an indication of required extraction, for throwing exception when property not found or accesible, can be supressed.
        /// </summary>
        public static Tuple<bool, object> ExtractPropertyValue(object SourceObject, string PropertyName, bool Required = true)
        {
            if (SourceObject == null)
                throw new UsageAnomaly("Cannot read the member named '" + PropertyName + "' from a null object reference.");

            PropertyInfo PropertyData = SourceObject.GetType().GetProperty(PropertyName);

            Tuple<bool, object> Result = null;

            try
            {
                Result = new Tuple<bool, object>(PropertyData != null, PropertyData != null ? PropertyData.GetValue(SourceObject, null) : null);
            }
            catch(Exception Problem)
            {
                if (Required)
                    throw;

                Result = Tuple.Create<bool, object>(false, null);
            }

            return Result;
        }

        /// <summary>
        /// For a passed object, returns a dictionary with the data (property name and value) of the supplied attribute.
        /// </summary>
        public static Dictionary<string, object> ExtractAttributeProperties(object SourceObject, string AttributeName)
        {
            if (SourceObject == null)
                return null;

            return ExtractAttributeProperties(SourceObject.GetType(), AttributeName);
        }

        /// <summary>
        /// For a passed type, returns a dictionary with the data (property name and value) of the supplied attribute.
        /// </summary>
        public static Dictionary<string, object> ExtractAttributeProperties(this Type SourceType, string AttributeName)
        {
            return ExtractAttributeProperties(ExtractAttribute(SourceType, AttributeName));
        }

        /// <summary>
        /// Returns a dictionary with the data (property name and value) of the supplied attribute.
        /// </summary>
        public static Dictionary<string, object> ExtractAttributeProperties(this Attribute SourceAttribute)
        {
            Dictionary<string, object> PropertiesValues = new Dictionary<string, object>();
            if (SourceAttribute != null)
                foreach (PropertyInfo PropertyData in SourceAttribute.GetType().GetProperties())
                    PropertiesValues.Add(PropertyData.Name, PropertyData.GetValue(SourceAttribute, null));

            return PropertiesValues;
        }

        /// <summary>
        /// Returns public fields with their name and value for the supplied object.
        /// </summary>
        public static Dictionary<string, object> ExtractFieldNameAndValueFromInstance(object Source)
        {
            Dictionary<string, object> Fields = new Dictionary<string, object>();

            foreach (FieldInfo FieldData in Source.GetType().GetFields())
                Fields.Add(FieldData.Name, FieldData.GetValue(Source));

            return Fields;
        }

        /// <summary>
        /// Returns a dictionary with the public fields with their FieldInfo and value for the supplied object.
        /// </summary>
        public static Dictionary<FieldInfo, object> ExtractFieldInfoAndValueFromInstance(object Datum)
        {
            Dictionary<FieldInfo, object> Fields = new Dictionary<FieldInfo, object>();

            foreach (FieldInfo FieldData in Datum.GetType().GetFields())
                Fields.Add(FieldData, FieldData.GetValue(Datum));

            return Fields;
        }

        /* POSTPONED UNTIL DB TREATMENT IMPLEMENTED
         * REQUIRES: Reference to System.Data.Linq.Mapping

        /// Store of previously extracted autoincremental FieldInfos from a -business entity/table- Type
        /// (for avoiding extraction repetition).
        /// </summary>
        private static Dictionary<Type, FieldInfo> StoreOfTypeAutoincrementalFieldInfos =
            new Dictionary<Type, FieldInfo>();

        /// <summary>
        /// Returns FieldInfo for the autoincremental field attributed for the supplied entity type or null if not exists.
        /// It is considered the first field marked with the attributes IsDbGenerated=true and AutoSync=AutoSync.OnInsert.
        /// </summary>
        public static FieldInfo GetAutoincrementalFieldInfoOfType<TEntity>()
        {
            Type TargetType = typeof(TEntity);

            if (StoreOfTypeAutoincrementalFieldInfos.ContainsKey(TargetType))
                return StoreOfTypeAutoincrementalFieldInfos[TargetType];

            FieldInfo AutoincField = null;

            foreach (FieldInfo FieldData in GetFieldInfosOfType(TargetType))
            {
                ColumnAttribute ColumnAtt =
                    Attribute.GetCustomAttribute(FieldData, typeof(ColumnAttribute))
                    as ColumnAttribute;

                if (ColumnAtt != null
                    && ColumnAtt.IsDbGenerated
                    && ColumnAtt.AutoSync == AutoSync.OnInsert)
                {
                    AutoincField = FieldData;
                    break;
                }
            }

            StoreOfTypeAutoincrementalFieldInfos.Add(TargetType, AutoincField);
            return AutoincField;
        } */

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the index of the matching value for the supplied Items-Source, Selection-Value and optional Member-Name.
        /// </summary>
        public static int GetMatchingIndex(this IEnumerable ItemsSource, object SelectionValue, string MemberName = null)
        {
            int Index = -1, SelectionIndex = -1;

            foreach (var Item in ItemsSource)
            {
                Index++;

                var Value = Item;

                if (!MemberName.IsAbsent())
                {
                    var ValueExtraction = General.ExtractMemberValue(Item, MemberName);
                    if (!ValueExtraction.Item1)
                        break;

                    Value = ValueExtraction.Item2;
                }

                if (Value.IsEqual(SelectionValue))
                {
                    SelectionIndex = Index;
                    break;
                }
            }

            return SelectionIndex;
        }

        /// <summary>
        /// Returns, for the supplied Enum type, a collection with its constant members.
        /// Each member is represented by a Tuple with Item1=Value, Item2=FieldName, Item3=Description.
        /// </summary>
        public static IEnumerable<Tuple<TEnum,string,string>> GetEnumMembers<TEnum>()
        {
            var ConstantsNames = Enum.GetNames(typeof(TEnum));
            var ConstantsValues = Enum.GetValues(typeof(TEnum));

            int Index = -1;
            foreach (var ConstantName in ConstantsNames)
            {
                Index++;
                FieldInfo FieldData = typeof(TEnum).GetField(ConstantName);
                var FieldNameAtts = (FieldNameAttribute[])FieldData.GetCustomAttributes(typeof(FieldNameAttribute), false);
                var DescriptionAtts = (DescriptionAttribute[])FieldData.GetCustomAttributes(typeof(DescriptionAttribute), false);

                yield return (new Tuple<TEnum, string, string>((TEnum)ConstantsValues.GetValue(Index),
                                                               FieldNameAtts.Length > 0 ? FieldNameAtts[0].Name : ConstantName,
                                                               DescriptionAtts.Length > 0 ? DescriptionAtts[0].Description : null));
            }
        }

        /// <summary>
        /// Gets an enum value as SimpleElement with Name=FieldName, TechName=Value, Summary=Description.
        /// </summary>
        public static SimpleElement GetEnumValueAsSimpleElement(this Enum Source)
        {
            FieldInfo FieldData = Source.GetType().GetField(Source.ToString());
            var FieldNameAtts = (FieldNameAttribute[])FieldData.GetCustomAttributes(typeof(FieldNameAttribute), false);
            var DescriptionAtts = (DescriptionAttribute[])FieldData.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return (new SimpleElement(FieldNameAtts.Length > 0 ? FieldNameAtts[0].Name : Source.ToString(),
                                      Source.ToString(),
                                      DescriptionAtts.Length > 0 ? DescriptionAtts[0].Description : null));
        }

        /// <summary>
        /// Gets the field-name of the supplied enumeration based on its FieldName attribute.
        /// </summary>
        public static string GetFieldName(this Enum Source)
        {
            FieldInfo FieldData = Source.GetType().GetField(Source.ToString());
            var Attributes = (FieldNameAttribute[])FieldData.GetCustomAttributes(typeof(FieldNameAttribute), false);

            return (Attributes.Length > 0) ? Attributes[0].Name : Source.ToString();
        }

        /// <summary>
        /// Gets the description of the supplied enumeration based on its Description attribute.
        /// </summary>
        public static string GetDescription(this Enum Source)
        {
            FieldInfo FieldData = Source.GetType().GetField(Source.ToString());
            var Attributes = (DescriptionAttribute[])FieldData.GetCustomAttributes(typeof(DescriptionAttribute), false);
            
            return (Attributes.Length > 0) ? Attributes[0].Description : Source.ToString();
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the description of the specified property from the supplied TSource type.
        /// </summary>
        public static string GetDescriptionOfProperty<TSource>(string PropertyName)
        {
            var PropInfo = typeof(TSource).GetProperty(PropertyName);
            if (PropInfo == null)
                return "";

            return PropInfo.GetDescription();
        }

        /// <summary>
        /// Gets the description of the specified property from this supplied property-info.
        /// </summary>
        public static string GetDescription(this PropertyInfo PropInfo)
        {
            var AttribDef = PropInfo.GetCustomAttributes(typeof(DescriptionAttribute), true)
                                            .FirstOrDefault() as DescriptionAttribute;
            if (AttribDef == null)
                return "";

            return AttribDef.Description;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Generates and returns a full deep copy of the specified source.
        /// </summary>
        public static TSource GenerateDeepClone<TSource>(this TSource Source)
        {
            TSource Clone;

            using (var Torrent = new System.IO.MemoryStream())
            {
                var Formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                // Formatter.Binder = BytesHandling.BinderForWeakToStrongNamedAssembly;

                Formatter.Serialize(Torrent, Source);
                Torrent.Position = 0;
                Clone = (TSource)Formatter.Deserialize(Torrent);
            }

            return Clone;
        }

        //------------------------------------------------------------------------------------------
    }
}