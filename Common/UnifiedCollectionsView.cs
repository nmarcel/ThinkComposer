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
// File   : UnifiedCollectionsView.cs
// Object : Instrumind.Common.UnifiedCollectionsView (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.03.24 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;

using Instrumind.Common;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Centralizes enumerable access and change notifications of multiple collections.
    /// </summary>
    /// <typeparam name="TItem">Type of the Items contained by the unified collections.</typeparam>
    /// <typeparam name="TCollection">Type of the unified collections (must implement IEnumerable{TItem} and INotifyCollectionChanged).</typeparam>
    [Serializable]
    public class UnifiedCollectionsView<TItem, TCollection> : IEnumerable<TItem>, INotifyCollectionChanged
           where TCollection : class, IEnumerable<TItem>, INotifyCollectionChanged
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public UnifiedCollectionsView()
        {
        }

        /// <summary>
        /// Constructor for register initial collections.
        /// </summary>
        /// <param name="InitialCollections"></param>
        public UnifiedCollectionsView(IEnumerable<TCollection> InitialCollections)
             :this()
        {
            foreach (var Collection in InitialCollections)
                RegisterCollection(Collection);
        }

        /// <summary>
        /// Constructor for register multiple initial collections.
        /// </summary>
        /// <param name="InitialCollections"></param>
        public UnifiedCollectionsView(params TCollection[] InitialCollections)
            : this(InitialCollections as IEnumerable<TCollection>)
        {
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register a new Collection for unification.
        /// </summary>
        public void RegisterCollection(TCollection Collection)
        {
            if (!this.UnifiedCollections.AddNew(Collection))
                return;

            Collection.CollectionChanged += NotifyChanges;
        }

        /// <summary>
        /// Unregisters the specified Collection from unification.
        /// </summary>
        public void UnregisterCollection(TCollection Collection)
        {
            if (!this.UnifiedCollections.Remove(Collection))
                return;

            Collection.CollectionChanged -= NotifyChanges;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected List<TCollection> UnifiedCollections = new List<TCollection>();

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        public IEnumerator<TItem> GetEnumerator()
        {
            foreach (var Collection in this.UnifiedCollections)
                foreach (var Item in Collection)
                    yield return Item;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Receives the change notification of one unified Collection and propagates it to the subscribers.
        /// </summary>
        protected void NotifyChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            var ChangedCollection = sender as TCollection;

            var Handler = CollectionChanged;
            if (ChangedCollection == null || Handler == null)
                return;

            int IndexOffset = 0;
            bool Found = false;

            foreach (var Collection in this.UnifiedCollections)
                if (Collection == ChangedCollection)
                {
                    Found = true;
                    break;
                }
                else
                    IndexOffset += Collection.Count();

            if (!Found)
                return;

            var IndexDisplacedEventArgs = General.GenerateDisplacedNotifyCollectionChangedEventArgs(e, IndexOffset);
            Handler(this, IndexDisplacedEventArgs);
        }

        [field: NonSerialized]
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}