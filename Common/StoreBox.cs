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
// File   : StoreBox.cs
// Object : Instrumind.Common.StoreBox (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Services for specific Store-Boxes types.
    /// </summary>
    public static class StoreBox
    {
        /// <summary>
        /// Stores this supplied object into an store-box and returns it.
        /// Optionally a references centralizer can be specified.
        /// </summary>
        public static StoreBox<TValue> Store<TValue>(this TValue Value, IUniqueElement ReferencesCentralizer = null)
        {
            var Result = new StoreBox<TValue>(ReferencesCentralizer);
            Result.Value = Value;
            return Result;
        }

        public static StoreBoxBase CreateStoreBoxForType(Type StoreType, object Value)
        {
            Type StoreBoxType = typeof(StoreBox<>).MakeGenericType(StoreType);
            var Result = Activator.CreateInstance(StoreBoxType) as StoreBoxBase;
            Result.StoredObject = Value;

            return Result;
        }

        /// <summary>
        /// Register a supplied Type, with its corresponding Getter and Setter to be storable.
        /// </summary>
        public static void RegisterStorableType<TValue>(Func<byte[], TValue> Getter, Func<TValue, byte[]> Setter)
        {
            General.ContractRequiresNotNull(Getter, Setter);

            RegisteredTypes[typeof(TValue)] = Tuple.Create<object, object>(Getter, Setter);
        }

        /// <summary>
        /// Returns a tuple with the getter and setter of the specified registered storable type, or null if it was not registered.
        /// </summary>
        public static Tuple<Func<byte[], TValue>, Func<TValue, byte[]>> RetrieveAccessors<TValue>()
        {
            var ValueType = typeof(TValue);
            Type RegisteredType = null;

            foreach(var RegType in RegisteredTypes)
                if (RegType.Key.IsAssignableFrom(ValueType))
                {
                    RegisteredType = RegType.Key;
                    break;
                }

            if (RegisteredType == null)
                return null;

            var Register = RegisteredTypes[RegisteredType];
            var Result = Tuple.Create<Func<byte[], TValue>, Func<TValue, byte[]>>(Register.Item1 as Func<byte[], TValue>,
                                                                               Register.Item2 as Func<TValue, byte[]>);
            return Result;
        }

        /// <summary>
        /// Gets the list of registered types.
        /// </summary>
        public static IEnumerable<Type> GetRegisteredTypes()
        {
            return RegisteredTypes.Select(reg => reg.Key);
        }

        /// <summary>
        /// Internal collection of registered types with its accessors.
        /// </summary>
        private static readonly Dictionary<Type, Tuple<object, object>> RegisteredTypes = new Dictionary<Type, Tuple<object, object>>();

        /// <summary>
        /// Centralizes the storage of shared References across Centralizer objects.
        /// The main dictionary contains Centralizer Global-IDs as keys, and the reference dictionaries as values.
        /// Each reference dictionary contains Object identifiers (also GUIDs) as keys, and its binary representations as values.
        /// </summary>
        internal static readonly Dictionary<Guid, Dictionary<Guid, byte[]>> ReferenceCentralizers = new Dictionary<Guid, Dictionary<Guid, byte[]>>();

        /// <summary>
        /// Gets a Shared References dictionary registered with the specified Centralizer Global-ID, or null if none exists.
        /// </summary>
        public static Dictionary<Guid, byte[]> GetSharedReferencesForCentralizer(Guid Centralizer)
        {
            if (ReferenceCentralizers.ContainsKey(Centralizer))
                return ReferenceCentralizers[Centralizer];

            return null;
        }

        /// <summary>
        /// Registers the specified Centralizer Global-ID with the new Shared References dictionary .
        /// </summary>
        public static void RegisterSharedReferencesCentralizer(Guid Centralizer, Dictionary<Guid, byte[]> SharedReferences)
        {
            General.ContractRequiresNotNull(SharedReferences);

            ReferenceCentralizers[Centralizer] = SharedReferences;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Base class for storing non-natively-serializable objects.
    /// </summary>
    [Serializable]
    public abstract class StoreBoxBase
    {
        /// <summary>
        /// Gets or sets the stored object.
        /// </summary>
        public abstract object StoredObject { get; set; }

        /// <summary>
        /// Gets the stored bytes.
        /// </summary>
        public abstract byte[] GetStoredValueBytes();

        /// <summary>
        /// Makes this Store-Box to share References on the supplied Centralizer Global-ID object.
        /// Fails if the underlying stored Type is a Value-Type.
        /// </summary>
        public abstract void CentralizeReferencesIn(Guid ReferencesCentralizer);

        /// <summary>
        /// Creates and returns a clone of this store-box.
        /// </summary>
        public abstract StoreBoxBase CreateClone();

        /// <summary>
        /// Associates object instances to global-unique-identificators which are used as store-box IDs for serialization.
        /// </summary>
        // PENDING: Improvement for speed. See: http://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary
        // NOTICE: This is a static memeber, so it is fully shared on this non-generic class
        protected static Dictionary<Guid, object> DesignatedObjectsIdentifiers = new Dictionary<Guid, object>();
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Encapsulates a data that can be converted to byte array for serialization purposes.
    /// This type is intended to facilitate the user-defined serialization of types not marked as Serializable.
    /// The referenced types must be registered with its conversion accessor operations (see static StoreBox class).
    /// </summary>
    [Serializable]
    public class StoreBox<TValue> : StoreBoxBase
    {
        /// <summary>
        /// Constructor.
        /// Parameterless for easy call from MakeGenericType().
        /// </summary>
        public StoreBox()
        {
            AssignAccessors();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public StoreBox(IUniqueElement ReferencesCentralizer)
             : this(ReferencesCentralizer != null && !typeof(TValue).IsValueType
                    ? ReferencesCentralizer.GlobalId : default(Guid))
        {
        }

        /// <summary>
        /// Alternate constructor for cloning.
        /// </summary>
        internal StoreBox(Guid CentralizerId)
            : this()
        {
            this.CentralizerId = CentralizerId;
        }

        /// <summary>
        /// Initializes the instance for use after creation or deserialization (AND CLONING BY DESERIALIZE+SERIALIZE).
        /// </summary>
        [OnDeserialized]
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            // Force reset of the cache (if cloned from another isntance with a live cache)
            this.ValueIsCached = false;
            this.CachedValue = default(TValue);
        }

        /// <summary>
        /// Creates and returns a clone of this store-box.
        /// </summary>
        public override StoreBoxBase CreateClone()
        {
            var Result = new StoreBox<TValue>(this.CentralizerId);
            Result.Value = this.Value;
            return Result;
        }

        /// <summary>
        /// Makes this Store-Box to share References on the supplied Centralizer Global-ID object.
        /// Fails if the underlying stored Type is a Value-Type.
        /// </summary>
        public override void CentralizeReferencesIn(Guid ReferencesCentralizer)
        {
            // Exit if no centralizer is provided. This could happen on constructors.
            if (ReferencesCentralizer == null || this.CentralizerId != null)
                return;

            // Exit if centralization for a Value-Type is intended.
            var TypeOfValue = typeof(TValue);
            if (TypeOfValue.IsValueType)
                return;

            // CENTRALIZE
            this.CentralizerId = ReferencesCentralizer;

            // Get the previous value (only will be retrieved if there was not stored a pair CentralizerId + Stored-Object/Guid).
            TValue PreviousValue = this.Value;

            // Because this centralization could have taken place after a previous storage.
            if (!this.CachedValue.IsEqual(default(TValue)))
                this.Value = this.CachedValue;
            else
                if (!PreviousValue.IsEqual(default(TValue)))
                    this.Value = PreviousValue;
        }

        /// <summary>
        /// Assign accessors based on class type.
        /// </summary>
        protected void AssignAccessors()
        {
            var Register = StoreBox.RetrieveAccessors<TValue>();

            if (Register == null)
                throw new UsageAnomaly("Cannot create Store-Box for an unregistered type", typeof(TValue));

            this.Getter = Register.Item1;
            this.Setter = Register.Item2;
        }

        /// <summary>
        /// Gets and sets the exposed value of this store-box.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (!this.ValueIsCached)
                {
                    if (this.Getter == null)
                        AssignAccessors();

                    var TypeOfValue = typeof(TValue);

                    /*T if (TypeOfValue == typeof(System.Windows.Media.ImageSource))
                            Console.WriteLine("Store-Box. Getting ImageSource"); */

                    // For value-types or when no references-centralizer exists, then gets the stored whole object binary representation.
                    if (TypeOfValue.IsValueType || this.CentralizerId == Guid.Empty)
                    {
                        this.CachedValue = this.Getter(this.StoredValue);
                        this.ValueIsCached = true;
                    }
                    else
                    {
                        this.CachedValue = default(TValue);

                        // For reference-types, use references-centralizer and gets the stored binary representations of the Guid and the object.
                        if (this.StoredValue != null)
                            if (StoreBox.ReferenceCentralizers.ContainsKey(this.CentralizerId))
                            {
                                var SharedReferences = StoreBox.ReferenceCentralizers[this.CentralizerId];
                                if (this.StoredValue.Length != 16)
                                    throw new UsageAnomaly("An stored Guid must have a size of 16 bytes.");

                                var ValueKey = new Guid(this.StoredValue);

                                if (SharedReferences.ContainsKey(ValueKey))
                                {
                                    this.CachedValue = this.Getter(SharedReferences[ValueKey]);
                                    DesignatedObjectsIdentifiers.AddNew(ValueKey, this.CachedValue);
                                }
                            }
                            else
                            {
                                // Console.WriteLine("FAIL: An stored References-Centralizer code must be associated to existing shared storage.");
                                throw new UsageAnomaly("An stored References-Centralizer code must be associated to existing shared storage.", this.CentralizerId);
                            }
                    }
                }

                return this.CachedValue;
            }
            set
            {
                if (this.Setter == null)
                    AssignAccessors();

                var TypeOfValue = typeof(TValue);

                /*T if (TypeOfValue == typeof(System.Windows.Media.ImageSource))
                        Console.WriteLine("Store-Box. Setting ImageSource"); */

                // For value-types or when no references-centralizer exists, then store the whole object binary representation.
                if (TypeOfValue.IsValueType || this.CentralizerId == Guid.Empty)
                    this.StoredValue = this.Setter(value);
                else
                {
                    // For reference-types, use references-centralizer and store the binary representations of the Guid and the object.
                    Dictionary<Guid, byte[]> SharedReferences = null;

                    if (StoreBox.ReferenceCentralizers.ContainsKey(this.CentralizerId))
                        SharedReferences = StoreBox.ReferenceCentralizers[this.CentralizerId];
                    else
                    {
                        SharedReferences = new Dictionary<Guid, byte[]>();
                        StoreBox.ReferenceCentralizers.Add(this.CentralizerId, SharedReferences);
                    }

                    if (value == null)
                        this.StoredValue = null;
                    else
                    {
                        Guid ObjectId = DesignatedObjectsIdentifiers.FirstOrDefault(reg => reg.Value.IsEqual(value)).Key;

                        if (ObjectId == default(Guid))
                        {
                            ObjectId = Guid.NewGuid();
                            DesignatedObjectsIdentifiers.Add(ObjectId, value);
                        }

                        SharedReferences[ObjectId] = this.Setter(value);
                        this.StoredValue = ObjectId.ToByteArray();
                    }
                }

                this.CachedValue = value;
                this.ValueIsCached = true;
            }
        }

        /// <summary>
        /// Gets or sets the stored object.
        /// </summary>
        public override object StoredObject
        {
            get { return this.Value; }
            set { this.Value = (TValue)value; }
        }

        /// <summary>
        /// Indicates whether the CachedValue variable has the last stored value, else is the initial default.
        /// </summary>
        [NonSerialized]
        protected bool ValueIsCached = false;

        /// <summary>
        /// Contains the last stored value ready to be getted, hence avoiding re-creation from the Getter.
        /// </summary>
        [NonSerialized]
        protected TValue CachedValue = default(TValue);

        /// <summary>
        /// Contains the Global-ID of a References centralizer.
        /// Only useful for referece-type stored objects.
        /// Must be null for non-centralized stored objects.
        /// </summary>
        // NOTE: The main centralization happens at EntityEditEngine.ObtainEditEngine().
        internal Guid CentralizerId { get; private set; }

        /// <summary>
        /// The actually contained byte array representing the exposed value (can be null).
        /// If CentralizerCode is null, then contains the stored object in the Setter provided binary representation.
        /// Else, if CentralizerCode is not zero, then contains the object's Guid binary representation.
        /// </summary>
        protected byte[] StoredValue { get; set; }
        /* IMPORTANT: Don't do this!... deserializes null values
           (for the Compositions+Domains that are based on the underlying auto-gen Field)
        {
            get { return this.StoredValue_; }
            set
            {
                if (this.StoredValue_ == value)
                    return;

                this.StoredValue_ = value;
                if (value.BytesToString().StartsWith("For"))
                    Console.WriteLine("For");
            }
        }
        private byte[] StoredValue_ = null; */

        public override byte[] GetStoredValueBytes() { return StoredValue; }

        /// <summary>
        /// Conversion function to read the value from the store-box.
        /// </summary>
        [NonSerialized]
        protected Func<byte[], TValue> Getter = null;

        /// <summary>
        /// Conversion function to write the value into the store-box.
        /// </summary>
        [NonSerialized]
        protected Func<TValue, byte[]> Setter = null;

        public override string ToString()
        {
            return "StoreBox (HC=" + this.GetHashCode().ToString() + "). Value: " + this.Value.ToStringAlways();
        }
    }
}