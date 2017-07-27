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
// File   : UniqueElement.cs
// Object : Instrumind.Common.UniqueElement (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.12 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;
using System.Collections;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Ancestor for editable (undoable/readoable and observable/changes-notifier) collections.
    /// </summary>
    [Serializable]
    public abstract class EditableCollection : IEditableCollectionView
    {
        /// <summary>
        /// Constructor.
        /// Notice that VariatingInstance can be null, thus disabling undo/redo but allowing change notifications.
        /// </summary>
        public EditableCollection(string Name, IModelEntity VariatingInstance, bool TreatModelClassValuesAsReferences)
        {
            General.ContractRequiresNotAbsent(Name);

            this.Name = Name;
            this.VariatingInstance = VariatingInstance;
            this.TreatModelClassValuesAsReferences = TreatModelClassValuesAsReferences;
        }

        /// <summary>
        /// Name of the collection.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Instance owning the changing collection.
        /// </summary>
        public IModelEntity VariatingInstance { get; protected set; }

        /// <summary>
        /// Indicates whether any stored the Model-Class value should be treated as reference (hence non-cloneable).
        /// </summary>
        public bool TreatModelClassValuesAsReferences { get; set; }

        /// <summary>
        /// Modifies this collection based on the specified alteration code and parameters.
        /// </summary>
        /// <param name="AlterCode"></param>
        public abstract void AlterCollection(char AlterCode, object[] Parameters);

        /// <summary>
        /// Gets the parameters for the alteration-code and original parameters provided.
        /// </summary>
        public abstract object[] GetAlterParameters(char AlterCode, params object[] OriginalParameters);

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Operation for adding or inserting a new Item.
        /// It must return a new Item or null if cancelled.
        /// </summary>
        [NonSerialized]
        public Func<object> CreateItemOperation;

        /// <summary>
        /// Operation for deleting an existing Item.
        /// It gets the selected-for-deletion Item and must return true for proceed or false to cancel.
        /// </summary>
        [NonSerialized]
        public Func<object, bool> DeleteItemOperation;

        /// <summary>
        /// Operation for edit an existing Item.
        /// It gets the selected-for-editing Item.
        /// </summary>
        [NonSerialized]
        public Action<object> EditItemOperation = null;

        /// <summary>
        /// Gets the items count.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Removes all the items from the collection and notifies the reset.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Returns a copy of this collection for the specified target instance, indicating whether to do a deep-copy and with the supplied name.
        /// </summary>
        public abstract EditableCollection DuplicateFor(IModelEntity TargetInstance, IMModelClass DirectOwner, ECloneOperationScope CloningScope = ECloneOperationScope.Slight, string Name = null);

        /// <summary>
        ///  Updates the content of this collection based on the supplied one.
        /// </summary>
        public abstract void UpdateContentFrom(EditableCollection SourceCollection);

        // -------------------------------------------------------------------------------------------------------------
        #region IEditableCollectionView Members

        public NewItemPlaceholderPosition NewItemPlaceholderPosition { get; set; }
        // .....................................................................

        public bool IsAddingNew { get; set; }
        public object CurrentAddItem { get; set;  }
        public bool CanAddNew { get { return (this.CreateItemOperation != null); } }
        public object AddNew()
        {
            if (this.CreateItemOperation == null)
                return null;

            return this.CreateItemOperation();
        }
        public void CancelNew()
        {
        }
        public void CommitNew()
        {
        }

        // .....................................................................
        public bool IsEditingItem { get; set; }
        public object CurrentEditItem { get; set;  }
        public void EditItem(object item)
        {
            if (this.EditItemOperation == null)
                return;

            this.EditItemOperation(item);
        }
        public bool CanCancelEdit
        {
            get { return false; }
        }
        public void CancelEdit()
        {
        }
        public void CommitEdit()
        {
        }

        // .....................................................................
        public bool CanRemove { get { return (this.DeleteItemOperation != null); } }
        void IEditableCollectionView.Remove(object item)
        {
            DeleteItem(item);
        }
        public abstract void RemoveAt(int index);
        protected abstract void DeleteItem(object item);

        #endregion
    }
}