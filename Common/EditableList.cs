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
// File   : EditableList.cs
// Object : Instrumind.Common.EditableList (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.12 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

using Instrumind.Common;
using Instrumind.Common.EntityBase;
using Instrumind.Common.EntityDefinition;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Static definitions for EditableList{TItem}
    /// </summary>
    public static class EditableList
    {
        public const char ALTCOD_ADD = 'A';
        public const char ALTCOD_CLEAR = 'C';
        public const char ALTCOD_INSERT = 'I';
        public const char ALTCOD_POPULATE = 'P';
        public const char ALTCOD_REMOVE = 'R';
        public const char ALTCOD_REMOVEAT = 'E';
        public const char ALTCOD_RESIZE = 'Z';
        public const char ALTCOD_SET = 'S';
    }

    /// <summary>
    /// Editable (undoable/redoable and observable/changes-notifier) generic List, or a wrapper for an optional supplied one.
    /// </summary>
    [Serializable]
    public class EditableList<TItem> : EditableCollection, IList<TItem>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private List<TItem> SourceList = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public EditableList(string Name, IModelEntity VariatingInstance, int InitialCapacity = 0, bool TreatModelClassValuesAsReferences = false)
            : base(Name, VariatingInstance, TreatModelClassValuesAsReferences)
        {
            this.SourceList = (InitialCapacity < 1 ? new List<TItem>() : new List<TItem>(InitialCapacity));
        }

        /// <summary>
        /// Constructor based on a provided source List.
        /// </summary>
        public EditableList(string Name, IModelEntity VariatingInstance, List<TItem> SourceList, bool TreatModelClassValuesAsReferences = false)
             : base(Name, VariatingInstance, TreatModelClassValuesAsReferences)
        {
            this.SourceList = (SourceList == null ? new List<TItem>() : SourceList);
        }

        public override object[] GetAlterParameters(char AlterCode, params object[] OriginalParameters)
        {
            object[] Result = null;

            switch (AlterCode)
            {
                case EditableList.ALTCOD_ADD:
                    Result = OriginalParameters;
                    break;
                case EditableList.ALTCOD_CLEAR:
                    Result = new object[] {};
                    break;
                case EditableList.ALTCOD_INSERT:
                    Result = OriginalParameters;
                    break;
                case EditableList.ALTCOD_SET:
                    Result = new object[] { OriginalParameters[0] /*Index*/, this.SourceList[(int)OriginalParameters[0]] };
                    break;
                case EditableList.ALTCOD_POPULATE:
                    Result = new object[] { this.SourceList.ToArray() };
                    break;
                case EditableList.ALTCOD_REMOVE:
                    Result = OriginalParameters;
                    break;
                case EditableList.ALTCOD_REMOVEAT:
                    Result = OriginalParameters;
                    break;
                case EditableList.ALTCOD_RESIZE:
                    Result = new object[] { this.SourceList.Capacity, this.SourceList.ToArray() };
                    break;
                default:
                    throw new UsageAnomaly("Unknown editable list alteration code.", AlterCode);
            }

            return Result;
        }

        public void SetCapacity(int NewCapacity)
        {
            if (NewCapacity == this.SourceList.Count)
                return;

            if (this.SourceList.Count < NewCapacity)
                this.AddRange(new TItem[NewCapacity - this.SourceList.Count]);
            else
                this.RemoveRange(NewCapacity, this.SourceList.Count - NewCapacity);

            // Include the previous items for later generate the inverse "Resize".
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_RESIZE, this.SourceList.Capacity, this.SourceList.ToArray());
            this.SourceList.Capacity = NewCapacity;
        }

        #region IList<TItem> Members

        public int IndexOf(TItem item)
        {
            return this.SourceList.IndexOf(item);
        }

        public void Insert(int index, TItem item)
        {
            // Include the previous value for later generate the inverse "Insert".
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_REMOVEAT, index, item);
            this.SourceList.Insert(index, item);

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public override void RemoveAt(int index)
        {
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_INSERT, index, this.SourceList[index]);
            var Target = this.SourceList[index];
            this.SourceList.RemoveAt(index);

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Target, index));
        }

        public TItem this[int index]
        {
            get
            {
                return this.SourceList[index];
            }
            set
            {
                var PreviousValue = this.SourceList[index];

                if (!PreviousValue.IsEqual(value))
                {
                    EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_SET, index, PreviousValue);

                    if (index < 0 || index >= this.SourceList.Count)
                        throw new InternalAnomaly("Item cannot be replaced with an out of range index.");

                    this.SourceList[index] = value;

                    var NotifyHandler = this.CollectionChanged;
                    if (NotifyHandler != null)
                        NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, PreviousValue));
                }
            }
        }

        #endregion

        public void NotifyRefreshItem(int Index)
        {
            if (Index < 0 || Index >= this.SourceList.Count)
                return;

            var Value = this.SourceList[Index];

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Value, null));
        }

        #region ICollection<TItem> Members

        public void Add(TItem item)
        {
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_REMOVE, item);
            this.SourceList.Add(item);

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void AddRange(IEnumerable<TItem> Items)
        {
            if (Items == null)
                throw new ArgumentException();

            foreach (var Item in Items)
                this.Add(Item);
        }

        public override void Clear()
        {
            EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_POPULATE, this.SourceList.ToArray());
            this.SourceList.Clear();

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void PopulateFrom(IEnumerable<TItem> SourceList = null)
        {
            if (SourceList == null)
                return;

            foreach (var Item in SourceList)
                this.SourceList.Add(Item);

            var NotifyCollectionHandler = this.CollectionChanged;
            if (NotifyCollectionHandler != null)
                NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ReplaceAll(List<TItem> SourceList = null)
        {
            this.Clear();

            if (SourceList == null)
                SourceList = new List<TItem>();

            this.SourceList = SourceList;

            var NotifyCollectionHandler = this.CollectionChanged;
            if (NotifyCollectionHandler != null)
                NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(TItem item)
        {
            return this.SourceList.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            this.SourceList.CopyTo(array, arrayIndex);
        }

        public override int Count
        {
            get { return this.SourceList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TItem item)
        {
            int Index = -1;

            var NotifyHandler = this.CollectionChanged;
            if (NotifyHandler != null)
                Index = this.SourceList.IndexOf(item);

            var WasRemoved = this.SourceList.Remove(item);
            if (WasRemoved)
            {
                EntityEditEngine.RegisterInverseCollectionChange(this.VariatingInstance, this, EditableList.ALTCOD_ADD, item);

                if (NotifyHandler != null)
                    NotifyHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, Index));
            }

            return WasRemoved;
        }

        public void RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException();

            if (index + count >= this.SourceList.Count)
                throw new ArgumentException();

            if (index == 0 && count == this.SourceList.Count)
                this.Clear();
            else
                for (int InverseIndex = (index + count) - 1; InverseIndex >= index; InverseIndex--)
                    this.RemoveAt(InverseIndex);
        }

        protected override void DeleteItem(object item)
        {
            this.Remove((TItem)item);
        }

        #endregion

        #region IEnumerable<TItem> Members

        public IEnumerator<TItem> GetEnumerator()
        {
            return this.SourceList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.SourceList.GetEnumerator();
        }

        #endregion

        public override void AlterCollection(char AlterCode, object[] Parameters)
        {
            var NotifyCollectionHandler = this.CollectionChanged;
            
            switch (AlterCode)
            {
            case EditableList.ALTCOD_ADD:
                this.SourceList.Add((TItem)Parameters[0]);
                if (NotifyCollectionHandler != null)
                    NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Parameters[0]));
                break;

            case EditableList.ALTCOD_CLEAR:
                this.SourceList.Clear();
                if (NotifyCollectionHandler != null)
                    NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                break;

            case EditableList.ALTCOD_INSERT:
                this.SourceList.Insert((int)Parameters[0], (TItem)Parameters[1]);
                if (NotifyCollectionHandler != null)
                    NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Parameters[1]));
                break;

            case EditableList.ALTCOD_POPULATE:
                this.SourceList.Clear();
                this.PopulateFrom((IEnumerable<TItem>)Parameters[0]);
                break;

            case EditableList.ALTCOD_REMOVE:
                int Index = -1;
                if (NotifyCollectionHandler != null)
                    Index = this.SourceList.IndexOfMatch(item => item.IsEqual((TItem)Parameters[0]));

                if (this.SourceList.Remove((TItem)Parameters[0]))
                    if (NotifyCollectionHandler != null)
                        NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Parameters[0], Index));
                break;

            case EditableList.ALTCOD_REMOVEAT:
                var Target = this.SourceList[(int)Parameters[0]];
                this.SourceList.RemoveAt((int)Parameters[0]);
                if (NotifyCollectionHandler != null)
                    NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Target, (int)Parameters[0]));
                break;

            case EditableList.ALTCOD_RESIZE:
                this.SourceList.Capacity = (int)Parameters[0];
                var Items = Parameters[1] as TItem[];
                if (Items != null)
                    for (int Ind = 0; Ind < Math.Min(this.SourceList.Capacity, Items.Length); Ind++)
                        this.SourceList[Ind] = Items[Ind];
                break;

            case EditableList.ALTCOD_SET:
                var PreviousValue = this.SourceList[(int)Parameters[0]];
                this.SourceList[(int)Parameters[0]] = (TItem)Parameters[1];
                if (NotifyCollectionHandler != null)
                    NotifyCollectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Parameters[1], PreviousValue));
                break;

            default:
                throw new UsageAnomaly("Unknown editable list alteration code.", AlterCode);
            }
        }

        /// <summary>
        /// Returns a copy of this collection for the specified target instance, indicating whether to do a deep-copy and with the supplied name.
        /// </summary>
        public override EditableCollection DuplicateFor(IModelEntity TargetInstance, IMModelClass DirectOwner, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, string Name = null)
        {
            return CloneFor(TargetInstance, DirectOwner, CloningScope, Name);
        }

        public EditableList<TItem> CloneFor(IModelEntity TargetInstance, IMModelClass DirectOwner, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, string Name = null)
        {
            List<TItem> Items = null;

            if (CloningScope == ECloneOperationScope.Slight)
                Items = new List<TItem>(this.SourceList);
            else
            {
                Items = new List<TItem>(this.SourceList.Count);

                foreach (var Item in this.SourceList)
                {
                    var Value = Item;
                    if (Value is IMModelClass && !this.TreatModelClassValuesAsReferences)
                        Value = (TItem)((IMModelClass)Value).CreateCopy(CloningScope == ECloneOperationScope.Core ? ECloneOperationScope.Slight : CloningScope /* Deep */, DirectOwner);

                    Items.Add(Value);
                }
            }

            return (new EditableList<TItem>(Name ?? this.Name, TargetInstance, Items, this.TreatModelClassValuesAsReferences));
        }

        /// <summary>
        ///  Updates the content of this collection based on the supplied one.
        /// </summary>
        public override void UpdateContentFrom(EditableCollection SourceCollection)
        {
            this.UpdateContentFrom(SourceCollection, null);
        }

        /// <summary>
        ///  Updates the content of this collection based on the supplied one, plus items equivalence comparer.
        /// </summary>
        public void UpdateContentFrom(EditableCollection SourceCollection, Func<TItem, TItem, bool> ItemsEquivalenceComparer)
        {
            this.UpdateContentFrom((EditableList<TItem>)SourceCollection, ItemsEquivalenceComparer);
        }

        /// <summary>
        ///  Updates the content of this collection based on the supplied editable-list, plus items equivalence comparer.
        /// </summary>
        public void UpdateContentFrom(EditableList<TItem> SourceCollection, Func<TItem, TItem, bool> ItemsEquivalenceComparer = null)
        {
            this.UpdateSortListContentFrom(SourceCollection, ItemsEquivalenceComparer);
        }

        // ---------------------------------------------------------------------------------------------------------------------
        #region INotifyCollectionChanged Members

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

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

            return "EditableList '" + this.Name + "' (of '" + InstanceName + "'). Items count: " + this.Count.ToString() + ".";
        }

        #region IList implementation

        int IList.Add(object value)
        {
            this.Add((TItem)value);

            for (int Idx = this.SourceList.Count - 1; Idx >= 0; Idx--)
                if (this.SourceList[Idx].IsEqual((TItem)value))
                    return Idx;

            return -1;
        }
        
        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((TItem)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((TItem)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (TItem)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return this.IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            this.Remove((TItem)value);
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (TItem)value;
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo(array.Cast<TItem>().ToArray(), index);
        }

        int ICollection.Count
        {
            get { return this.Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this.SourceList; }
        }

        #endregion
    }
}