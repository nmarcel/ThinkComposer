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
// File   : MModelClasstDefinitor.cs
// Object : Instrumind.Common.EntityBase.MModelClassDefinitor (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.23 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Represents a definition for relevant members of a model class type.
    /// (Intended to work around problems by lack of support for contra/co+variance and for internal usage only)
    /// </summary>
    public abstract class MModelClassDefinitor : ModelDefinition
    {
        /// <summary>
        /// Name of the static class definitor field name.
        /// </summary>
        public const string FIELDNAME_CLASSDEF = "__ClassDefinitor";

        /// <summary>
        /// Collection of the declared definitors.
        /// </summary>
        public static readonly ReadOnlyCollection<MModelClassDefinitor> DeclaredClassDefinitors = null;
        private static List<MModelClassDefinitor> DeclaredClassDefinitors_ = new List<MModelClassDefinitor>();

        /// <summary>
        /// Collection of Declared members (properties/collections) definitors. The Key is the "Class.Member" qualified name.
        /// </summary>
        public static readonly ReadOnlyDictionary<string, MModelMemberDefinitor> DeclaredMemberDefinitors = null;
        protected static Dictionary<string, MModelMemberDefinitor> DeclaredMemberDefinitors_ = new Dictionary<string, MModelMemberDefinitor>();

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Static Constructor.
        /// </summary>
        static MModelClassDefinitor()
        {
            DeclaredClassDefinitors = new ReadOnlyCollection<MModelClassDefinitor>(DeclaredClassDefinitors_);
            DeclaredMemberDefinitors = new ReadOnlyDictionary<string, MModelMemberDefinitor>(DeclaredMemberDefinitors_);
        }

        /// <summary>
        /// Indicates whether the specified Instance is registered as Passive.
        /// Meaning that the object will not notify changes or store them for undo/redo.
        /// </summary>
        public static bool IsPassiveInstance(IMModelClass Instance)
        {
            return (PassiveInstances.Count > 0 && PassiveInstances.Contains(Instance));
        }

        /// <summary>
        /// Registers the supplied Instance from the list of Passive instances.
        /// Thus, the object will not notify changes or store them for undo/redo.
        /// </summary>
        public static void RegisterPassiveInstance(IMModelClass Instance)
        {
            PassiveInstances.AddNew(Instance);
        }
        private static List<IMModelClass> PassiveInstances = new List<IMModelClass>();

        /// <summary>
        /// Unregisters the supplied Instance from the list of Passive instances.
        /// Thus, the object will notify changes or store them for undo/redo.
        /// </summary>
        public static void UnregisterPassiveInstance(IMModelClass Instance)
        {
            PassiveInstances.Remove(Instance);
        }

        /// <summary>
        /// Gets the model class Definitor which has the supplied type.
        /// </summary>
        public static MModelClassDefinitor GetDefinitor(Type TClass)
        {
            var Result = DeclaredClassDefinitors_.FirstOrDefault(def => def.DeclaringType == TClass);

            // Retry, considering that the type static constructor could not have been called yet.
            if (Result == null)
                // Access the static class definition field, for -indirectly- invoking its static constructor
                if (General.GetStaticFieldValue(TClass, FIELDNAME_CLASSDEF) != null)
                    Result = DeclaredClassDefinitors_.FirstOrDefault(def => def.DeclaringType == TClass);

            return Result;
        }

        /// <summary>
        /// Returns the detected serializable fields of registered Model-Classes which are not declared.
        /// (Useful to determine what properties would not be automatically serialized by a custom-made serializer).
        /// </summary>
        public static IEnumerable<string> DetectUndeclaredSerializableModelFields()
        {
            foreach (var ModelClass in DeclaredClassDefinitors_)
            {
                var ModelClassType = ModelClass.DeclaringType;

                // Get the class fields, omitting...
                // - Delegates (events, functions, etc.)
                // - Non-Serialized objects.

                var ClassFields = ModelClassType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                    .Where(fld => (fld.FieldType.BaseType == null || !fld.FieldType.BaseType.InheritsFrom(typeof(Delegate)))
                                                   && !fld.IsDefined(typeof(NonSerializedAttribute), false));

                // NOTES:
                // Any "Data" Property is backed by a "Data_" field.
                // Any "Coll" Collection is backed by a "Coll" automatic-property, and...
                // Any standard .NET "Prop" automatic-property is backed by a "<Prop>k__BackingField" field.
                //
                // Think about use: FieldInfoValue.IsDefined(typeof(CompilerGeneratedAttribute), false); 

                foreach (var ClassField in ClassFields)
                    if (!ModelClass.Members.Any(mbr =>
                        {
                            var MemberName = mbr.TechName + "_";

                            if (mbr is MModelCollectionDefinitor)
                                MemberName = "<" + mbr.TechName + ">k__BackingField";
                            
                            return MemberName == ClassField.Name;
                        }))
                        yield return (ModelClassType.Name + "." + ClassField.Name);
            }
        }

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Constructor.
        /// </summary>
        public MModelClassDefinitor(string TechName, string Name, string Summary, Type DeclaringType, MModelClassDefinitor AncestorDefinitor = null)
             : base(TechName, Name, Summary, DeclaringType)
        {
            if ((AncestorDefinitor == null && !DeclaringType.BaseType.IsOneOf(typeof(object), typeof(DynamicObject)))
                || (AncestorDefinitor != null && AncestorDefinitor.DeclaringType != DeclaringType.BaseType))
                throw new UsageAnomaly("The declaring type of the supplied Ancestor Definitor differs from that of the actual ancestor.",
                                       new DataWagon("Ancestor Definitor", AncestorDefinitor).Add("Current class type", DeclaringType));

            this.AncestorDefinitor = AncestorDefinitor;
        }

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Registers the supplied model class Definitor in the shared collection.
        /// </summary>
        protected void RegisterDefinitor(MModelClassDefinitor Definitor)
        {
            if (DeclaredClassDefinitors_.Contains(Definitor))
                throw new UsageAnomaly("Model classes can be defined only once.", Definitor);

            DeclaredClassDefinitors_.Add(Definitor);
        }

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Definitor for the ancestor of the model class.
        /// </summary>
        public MModelClassDefinitor AncestorDefinitor;

        /// <summary>
        /// Gets the definitors of members (properties and collections), including these of the ancestors.
        /// </summary>
        public IEnumerable<MModelMemberDefinitor> Members
        {
            get
            {
                foreach (var Prop in this.Properties)
                    yield return Prop;

                foreach (var Coll in this.Collections)
                    yield return Coll;
            }
        }

        /// <summary>
        /// Gets the definitors of member properties, including these of the ancestors.
        /// </summary>
        public IEnumerable<MModelPropertyDefinitor> Properties { get { return (new PropertiesExtractor(this)); } }
        protected Dictionary<string, MModelPropertyDefinitor> Properties_ = new Dictionary<string, MModelPropertyDefinitor>();

        /// <summary>
        /// Gets the definitors of member collections, including these of the ancestors.
        /// </summary>
        public IEnumerable<MModelCollectionDefinitor> Collections { get { return (new CollectionsExtractor(this)); } }
        protected Dictionary<string, MModelCollectionDefinitor> Collections_ = new Dictionary<string, MModelCollectionDefinitor>();

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Evaluates the supplied entity and returns its validation result.
        /// </summary>
        public abstract IList<string> ValidateEntity(IMModelClass Instance);

        // -----------------------------------------------------------------------------------------
        /// <summary>
        /// Indicates whether this model class definitor is the supplied one, or one of its logic descendants.
        /// </summary>
        public bool IsCompatibleClassDefinition(MModelClassDefinitor Sample)
        {
            if (Sample == null)
                return false;

            if (this == Sample)
                return true;

            return IsCompatibleClassDefinition(Sample.AncestorDefinitor);
        }

        public MModelPropertyDefinitor GetPropertyDef(string TechName, bool IsRequired = true)
        {
            if (this.CachedPropertyDefs.ContainsKey(TechName))
                return this.CachedPropertyDefs[TechName];

            var Result = this.Properties.FirstOrDefault(prop => prop.TechName == TechName);
            if (Result == null && IsRequired)
                throw new UsageAnomaly("Cannot find property named '" + TechName + "'", this.Properties);

            this.CachedPropertyDefs.Add(TechName, Result);

            return Result;
        }
        private Dictionary<string, MModelPropertyDefinitor> CachedPropertyDefs = new Dictionary<string, MModelPropertyDefinitor>();

        public MModelCollectionDefinitor GetCollectionDef(string TechName, bool IsRequired = true)
        {
            if (this.CachedCollectionDefs.ContainsKey(TechName))
                return this.CachedCollectionDefs[TechName];

            var Result = this.Collections.FirstOrDefault(prop => prop.TechName == TechName);
            if (Result == null && IsRequired)
                throw new UsageAnomaly("Cannot find collection named '" + TechName + "'", this.Collections);

            this.CachedCollectionDefs.Add(TechName, Result);

            return Result;
        }
        private Dictionary<string, MModelCollectionDefinitor> CachedCollectionDefs = new Dictionary<string, MModelCollectionDefinitor>();

        public MModelMemberDefinitor GetMemberDef(string TechName, bool IsRequired = true)
        {
            var PropResult = this.Properties.FirstOrDefault(prop => prop.TechName == TechName);
            if (PropResult != null)
                return PropResult;

            var CollResult = this.Collections.FirstOrDefault(prop => prop.TechName == TechName);
            if (CollResult != null)
                return CollResult;

            if (IsRequired)
                throw new UsageAnomaly("Cannot find member named '" + TechName + "'", this.Collections);

            return null;
        }

        // -----------------------------------------------------------------------------------------
        public class PropertiesExtractor : IEnumerable<MModelPropertyDefinitor>
        {
            internal PropertiesExtractor(MModelClassDefinitor Source)
            {
                this.Source = Source;
            }
            MModelClassDefinitor Source = null;

            public IEnumerator<MModelPropertyDefinitor> GetEnumerator()
            {
                // Start emitting the ancestor's properties
                if (this.Source.AncestorDefinitor != null)
                {
                    var AncestorProps = this.Source.AncestorDefinitor.Properties.ToArray();
                    foreach (var Prop in AncestorProps)
                        yield return Prop;
                }

                // Then emit this source's properties
                foreach (var Prop in this.Source.Properties_)
                    yield return Prop.Value;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        // -----------------------------------------------------------------------------------------
        public class CollectionsExtractor : IEnumerable<MModelCollectionDefinitor>
        {
            internal CollectionsExtractor(MModelClassDefinitor Source)
            {
                this.Source = Source;
            }
            MModelClassDefinitor Source = null;

            public IEnumerator<MModelCollectionDefinitor> GetEnumerator()
            {
                // Start emitting the ancestor's collections
                if (this.Source.AncestorDefinitor != null)
                    foreach (var Collec in this.Source.AncestorDefinitor.Collections)
                        yield return Collec;

                // Then emit this source's collections
                foreach (var Collec in Source.Collections_)
                    yield return Collec.Value;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}