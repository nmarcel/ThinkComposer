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
// File   : EditableDictionary.cs
// Object : Instrumind.Common.EditableDictionary (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.12 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Static definitions for EditableDictionary{TKey, TValue}
    /// </summary>
    public static class EditableDictionary
    {
        public const char ALTCOD_ADD = 'A';
        public const char ALTCOD_CLEAR = 'C';
        public const char ALTCOD_POPULATE = 'P';
        public const char ALTCOD_REMOVE = 'R';
        public const char ALTCOD_KEYADD = '+';
        public const char ALTCOD_KEYREMOVE = '-';
        public const char ALTCOD_KEYSET = '=';
    }

    /// <summary>
    /// Editable (undoable/redoable and observable/changes-notifier) generic Dictionary, or a wrapper for an optional supplied one.
    /// </summary>
    [Serializable]
    public class EditableDictionary<TKey, TValue> : EditableCollection, IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private IDictionary<TKey, TValue> SourceDictionary = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EditableDictionary(string Name, IModelEntity VariatingInstance, int InitialCapacity = 0, bool TreatModelClassValuesAsReferences = false)
             : base(Name, VariatingInstance, TreatModelClassValuesAsReferences)
        {
            this.SourceDictionary = (InitialCapacity < 1 ? new Dictionary<TKey, TValue>() : new Dictionary<TKey, TValue>(InitialCapacity));
        }

        /// <summary>
        /// Constructor based on a provided source Dictionary.
        /// </summary>
        public EditableDictionary(string Name, IModelEntity VariatingInstance, IDictionary<TKey, TValue> SourceDictionary, bool TreatModelClassValuesAsReferences = false)
             : base(Name, VariatingInstance, TreatModelClassValuesAsReferences)
        {
            this.SourceDictionary = (SourceDictionary == null ? new Dictionary<TKey, TValue>() : SourceDictionary);
        }

        /// <summary>
        /// Constructor based on a provided referenceable source Dictionary.
        /// </summary>
        public EditableDictionary(IModelEntity VariatingInstance, string Name, ref Dictionary<TKey, TValue> SourceDictionary, bool TreatModelClassValuesAsReferences = false)
            : base(Name, VariatingInstance, TreatModelClassValuesAsReferences)
        {
            this.SourceDictionary = (SourceDictionary == null ? new Dictionary<TKey, TValue>() : SourceDictionary);
        }

        /// <summary>
        /// Initializes the instance for use after deserialization.
        /// </summary>
        [OnDeserialized]
        // This forces the Dictionary content to be read now and not deferred (to make it available in OnDeserialized event handlers)
        // See: http://social.msdn.microsoft.com/Forums/en-US/e5644081-fd01-4646-81c0-c283dc5d0fd4/do-dictionaries-deserialize-later-than-ondeserialized-marked-methods-are-called
        //  and http://stackoverflow.com/questions/457134/strange-behaviour-of-net-binary-serialization-on-dictionarykey-value
        private void Initialize(StreamingContext context = default(StreamingContext))
        {
            var LocalDictionary = this.SourceDictionary as Dictionary<TKey, TValue>;
            if (LocalDictionary != null)
                LocalDictionary.OnDeserialization(this);
        }

        public override object[] GetAlterParameters(char AlterCode, params object[] OriginalParameters)
        {
            object[] Result = null;

            switch (AlterCode)
            {
                case EditableDictionary.ALTCOD_ADD:
                    Result = OriginalParameters;
                    break;
                case EditableDictionary.ALTCOD_CLEAR:
                    Result = new object[] {};
                    break;
                case EditableDictionary.ALTCOD_POPULATE:
                    Result = new object[] { this.SourceDictionary.ToArray() };
                    break;
                case EditableDictionary.ALTCOD_REMOVE:
                    Result = OriginalParameters;
                    break;
                case EditableDictionary.ALTCOD_KEYADD:
                    Result = OriginalParameters;
                    break;
                case EditableDictionary.ALTCOD_KEYREMOVE:
                    Result = OriginalParameters;
                    break;
                case EditableDictionary.ALTCOD_KEYSET:
                    Result = new object[] { OriginalParameters[0], this.SourceDictionary[(TKey)OriginalParameters[0]] };    // Key, Previous-Value
                    break;
                default:
                    throw new UsageAnomaly("Unknown editable dictionary alteration code.", AlterCode);
            }

            return Result;
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey Key, TValue Value)
        {
            // Include the Value for later generate the inverse "KeyAdd".
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_KEYREMOVE, Key, Value);
            this.SourceDictionary.Add(Key, Value);

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(Key, Value)));
        }

        public bool ContainsKey(TKey key)
        {
            return this.SourceDictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return this.SourceDictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            TValue Value = this.SourceDictionary.GetValueOrDefault(key);

            var Index = -1;
            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                Index = this.SourceDictionary.IndexOfKey(key);

            var WasRemoved = this.SourceDictionary.Remove(key);
            if (WasRemoved)
            {
                EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_KEYADD, key, Value);

                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, Value), Index));
            }

            return WasRemoved;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.SourceDictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return this.SourceDictionary.Values; }
        }

        public TValue this[TKey Key]
        {
            get
            {
                return this.SourceDictionary[Key];
            }
            set
            {
                var AlreadyExists = this.SourceDictionary.ContainsKey(Key);
                TValue PreviousValue = (AlreadyExists ? this.SourceDictionary[Key] : default(TValue));

                if (!AlreadyExists || !PreviousValue.IsEqual(value))
                {
                    if (AlreadyExists)
                        EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_KEYSET, Key, PreviousValue);
                    else
                        // Include the PreviousValue for later generate the inverse "KeyAdd".
                        EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_KEYREMOVE, Key, PreviousValue);

                    this.SourceDictionary[Key] = value;

                    if (AlreadyExists)
                    {
                        var NotifyHandler = this.CollectionChanged;
                        if (NotifyHandler != null)
                            NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(Key, value),
                                                                                     new KeyValuePair<TKey, TValue>(Key, PreviousValue)));
                    }
                    else
                    {
                        var NotifyHandler = this.CollectionChanged;
                        if (NotifyHandler != null)
                            NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(Key, value)));
                    }

                }
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_REMOVE, item);
            this.SourceDictionary.Add(item);

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public override void Clear()
        {
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_POPULATE, this.SourceDictionary.ToArray());
            this.SourceDictionary.Clear();

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void PopulateFrom(IEnumerable<KeyValuePair<TKey, TValue>> SourceDictionary = null)
        {
            if (SourceDictionary == null)
                return;

            foreach (var Item in SourceDictionary)
                this.SourceDictionary .Add(Item);

            var NotifyCollectionHandler = this.CollectionChanged;
            if (NotifyCollectionHandler != null)
                NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this.SourceDictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.SourceDictionary).CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)this.SourceDictionary).Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return this.Remove(item);
        }

        private bool Remove(KeyValuePair<TKey, TValue> item, int Index = -1) // The Index will be supplied when (well) known to avoid re-determine it.
        {
            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null && Index < 0)
                Index = this.SourceDictionary.IndexOfItem(item);

            var WasRemoved = this.SourceDictionary.Remove(item);
            if (WasRemoved)
            {
                EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableDictionary.ALTCOD_ADD, item);

                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, Index));
            }

            return WasRemoved;
        }

        public override void RemoveAt(int index)
        {
            if (index < 0 || index >= this.SourceDictionary.Count)
                throw new IndexOutOfRangeException();

            int Pointer = 0;
            KeyValuePair<TKey, TValue> Pointed = default(KeyValuePair<TKey, TValue>);
            foreach (var Item in this.SourceDictionary)
            {
                if (Pointer == index)
                {
                    Pointed = Item;
                    break;
                }
                Pointer++;
            }

            this.Remove(Pointed, index);
        }

        protected override void DeleteItem(object item)
        {
            this.Remove((KeyValuePair<TKey, TValue>)item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.SourceDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.SourceDictionary.GetEnumerator();
        }

        #endregion

        public override void AlterCollection(char AlterCode, object[] Parameters)
        {
            TKey Key;
            KeyValuePair<TKey,TValue> KvpItem;
            int Index = -1;
            var NotifyHandler = this.CollectionChanged;

            switch (AlterCode)
            {
            case EditableDictionary.ALTCOD_ADD:
                this.SourceDictionary.Add((KeyValuePair<TKey,TValue>)Parameters[0]);
                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (KeyValuePair<TKey, TValue>)Parameters[0]));
                break;

            case EditableDictionary.ALTCOD_CLEAR:
                this.SourceDictionary.Clear();
                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                break;

            case EditableDictionary.ALTCOD_POPULATE:
                this.SourceDictionary.Clear();
                var Items = (IEnumerable<KeyValuePair<TKey, TValue>>)Parameters[0];
                this.PopulateFrom(Items);
                break;

            case EditableDictionary.ALTCOD_REMOVE:
                KvpItem = (KeyValuePair<TKey,TValue>)Parameters[0];
                if (NotifyHandler != null)
                    Index = this.SourceDictionary.IndexOfItem(KvpItem);
                this.SourceDictionary.Remove(KvpItem);
                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, KvpItem, Index));
                break;

            case EditableDictionary.ALTCOD_KEYADD:
                this.SourceDictionary.Add((TKey)Parameters[0], (TValue)Parameters[1]);
                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>((TKey)Parameters[0], (TValue)Parameters[1])));
                break;

            case EditableDictionary.ALTCOD_KEYREMOVE:
                Key = (TKey)Parameters[0];
                var Value = this.SourceDictionary[Key];
                if (NotifyHandler != null)
                    Index = this.SourceDictionary.IndexOfKey(Key);
                this.SourceDictionary.Remove(Key);
                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(Key, Value), Index));
                break;

            case EditableDictionary.ALTCOD_KEYSET:
                var AlreadyExists = this.SourceDictionary.ContainsKey((TKey)Parameters[0]);
                var PreviousValue = (AlreadyExists ? this.SourceDictionary[(TKey)Parameters[0]] : default(TValue));

                this.SourceDictionary[(TKey)Parameters[0]] = (TValue)Parameters[1];
                if (NotifyHandler != null)
                    if (AlreadyExists)
                        NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                                                                                 new KeyValuePair<TKey, TValue>((TKey)Parameters[0], (TValue)Parameters[1]),
                                                                                 new KeyValuePair<TKey, TValue>((TKey)Parameters[0], PreviousValue)));
                    else
                        NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>((TKey)Parameters[0], (TValue)Parameters[1])));
                break;

            default:
                throw new UsageAnomaly("Unknown editable dictionary alteration code.", AlterCode);
            }
        }

        /// <summary>
        /// Returns a copy of this collection for the specified target instance, indicating whether to do a deep-copy and with the supplied name.
        /// </summary>
        public override EditableCollection DuplicateFor(IModelEntity TargetInstance, IMModelClass DirectOwner, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, string Name = null)
        {
            return CloneFor(TargetInstance, DirectOwner, CloningScope, Name);
        }

        public EditableDictionary<TKey, TValue> CloneFor(IModelEntity TargetInstance, IMModelClass DirectOwner, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, string Name = null)
        {
            Dictionary<TKey, TValue> Items = null;

            if (CloningScope == ECloneOperationScope.Slight)
                Items = new Dictionary<TKey, TValue>(this.SourceDictionary);
            else
            {
                Items = new Dictionary<TKey, TValue>(this.SourceDictionary.Count);

                foreach (var Item in this.SourceDictionary)
                {
                    var Value = Item.Value;
                    if (Value is IMModelClass && !this.TreatModelClassValuesAsReferences)
                        Value = (TValue)((IMModelClass)Value).CreateCopy(CloningScope == ECloneOperationScope.Core ? ECloneOperationScope.Slight : CloningScope /* Deep */, DirectOwner);

                    Items.Add(Item.Key, Value);
                }
            }
            
            return (new EditableDictionary<TKey, TValue>(Name ?? this.Name, TargetInstance, Items, this.TreatModelClassValuesAsReferences));
        }

        /// <summary>
        ///  Updates the content of this collection based on the supplied one.
        /// </summary>
        public override void UpdateContentFrom(EditableCollection SourceCollection)
        {
            this.UpdateContentFrom((EditableDictionary<TKey,TValue>)SourceCollection);
        }

        /// <summary>
        ///  Updates the content of this collection based on the supplied editable-dictionary.
        /// </summary>
        public void UpdateContentFrom(EditableDictionary<TKey, TValue> SourceCollection)
        {
            this.UpdateDictionaryContentFrom(SourceCollection);
        }

        // ---------------------------------------------------------------------------------------------------------------------
        #region INotifyCollectionChanged Members

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        public void NotifyRefreshItem(TKey Key)
        {
            if (!this.SourceDictionary.ContainsKey(Key))
                return;

            var Value = this.SourceDictionary[Key];

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(Key, Value),
                                                                         null));
        }

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public override string ToString()
        {
            string InstanceName = "<No Attached Variating-Instance>";

            if (this.VariatingInstance != null)
            {
                if (this.VariatingInstance is FormalElement)
                    InstanceName = ((FormalElement)this.VariatingInstance).Name;
                else
                    if (this.VariatingInstance is SimpleElement)
                        InstanceName = ((SimpleElement)this.VariatingInstance).Name;
                    else
                        InstanceName = "HC=" + this.VariatingInstance.GetHashCode().ToString();
            }

            return "EditableDictionary '" + this.Name + "' (of '" + InstanceName + "'). Items count: " + this.Count.ToString() + ".";
        }

        #region IEditableCollection Members

        #endregion
    }
}