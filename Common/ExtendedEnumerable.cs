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
// File   : ExtendedEnumerable.cs
// Object : Instrumind.Common.ExtendedEnumerable (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.03.25 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides enumerable access for:
    /// - Single or multiple-centralized (a group treated as one) source collections.
    /// - Sending of notifications when these source collection(s) are observable.
    /// - Filter selection for the enumerated items.
    /// This particular type is for initialize source collections directly or from an IEnumerable{TItem} collections container.
    /// </summary>
    /// <typeparam name="TSourceItem">Type of the consumed source collection Items</typeparam>
    /// <typeparam name="TResultItem">Type of the provided enumerated Items</typeparam>
    public class ExtendedEnumerable<TSourceItem, TResultItem> : IEnumerable<TResultItem>, INotifyCollectionChanged
    {
        /// <summary>
        /// Constructor for descendants and siblings.
        /// </summary>
        protected ExtendedEnumerable(Func<TSourceItem, Tuple<bool, TResultItem>> ResultExtractor)
        {
            General.ContractRequiresNotNull(ResultExtractor);

            this.ResultExtractor = ResultExtractor;
        }

        /// <summary>
        /// Constructor for register multiple initial collections, within a collection.
        /// </summary>
        /// <param name="ResultExtractor">Result items extractor, which returns: A filter-accepted indicator (bool), plus the extracted/converted TResultItem value.</param>
        /// <param name="InitialCollections">Collection of source Collections to be exposed as enumerable.</param>
        public ExtendedEnumerable(Func<TSourceItem, Tuple<bool, TResultItem>> ResultExtractor, IList<IEnumerable<TSourceItem>> InitialCollections)
            : this(ResultExtractor)
        {
            if (InitialCollections != null)
                foreach (var Collection in InitialCollections)
                    RegisterCollection(Collection);

            this.ChangeNotificationIsEnabled = true;
        }

        /// <summary>
        /// Constructor for register multiple initial collections, directly.
        /// </summary>
        /// <param name="ResultExtractor">Result items extractor, which returns: A filter-accepted indicator (bool), plus the extracted/converted TResultItem value.</param>
        /// <param name="InitialCollections">Collection of source Collections to be exposed as enumerable.</param>
        public ExtendedEnumerable(Func<TSourceItem, Tuple<bool, TResultItem>> ResultExtractor, params IEnumerable<TSourceItem>[] InitialCollections)
            : this(ResultExtractor, InitialCollections.ToList())
        {
        }

        /// <summary>
        /// Reevaluates, and possibly change, the current collection assignments based on the supplied Updating Collections.
        /// Returns this instance.
        /// </summary>
        public ExtendedEnumerable<TSourceItem, TResultItem>
               UpdateCollectionAssignments(params IEnumerable<TSourceItem>[] UpdatingCollections)
        {
            if (UpdatingCollections == null || UpdatingCollections.Length < 1)
            {
                Reset();
                return this;
            }

            if (this.SourceCollections.HasEquivalentContent(UpdatingCollections))
                return this;

            Reset();
            foreach (var Collection in UpdatingCollections)
                RegisterCollection(Collection);

            return this;
        }

        /// <summary>
        /// Clears the current set of source collections and uses the supplied replacement.
        /// </summary>
        public void ReplaceCollections(params IEnumerable<TSourceItem>[] ReplacingCollections)
        {
            Reset();

            if (ReplacingCollections != null)
                foreach (var Collection in ReplacingCollections)
                    RegisterCollection(Collection);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        protected bool ChangeNotificationIsEnabled = false;
        protected int CurrentCollectionIndex = -1;
        protected int CurrentItemIndex = -1;
        protected TResultItem CurrentItem = default(TResultItem);
        protected List<IEnumerable<TSourceItem>> SourceCollections = new List<IEnumerable<TSourceItem>>();

        protected Func<TSourceItem, Tuple<bool, TResultItem>> ResultExtractor { get { return this.ResultExtractor_; } set { this.ResultExtractor_ = value; } }
        [NonSerialized]
        private Func<TSourceItem, Tuple<bool, TResultItem>> ResultExtractor_ = null;
        
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register a new Collection for enumeration/notify-consumption, optionally notifying this change.
        /// </summary>
        public void RegisterCollection(IEnumerable<TSourceItem> Collection, bool NotifyChange = true)
        {
            if (!this.SourceCollections.AddNew(Collection))
                return;

            var NotifiableCollection = Collection as INotifyCollectionChanged;
            if (NotifiableCollection != null)
                NotifiableCollection.CollectionChanged += NotifyCollectionChange;

            IList Items = (Collection is IList ? Collection as IList : Collection.ToList());
            if (this.ChangeNotificationIsEnabled && NotifyChange)
                NotifyCollectionChange(Collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Items));
        }

        /// <summary>
        /// Unregisters the specified Collection from enumeration/notify-consumption, optionally notifying this change.
        /// </summary>
        public void UnregisterCollection(IEnumerable<TSourceItem> Collection, bool NotifyChange = true)
        {
            if (!this.SourceCollections.Remove(Collection))
                return;

            var NotifiableCollection = Collection as INotifyCollectionChanged;
            if (NotifiableCollection != null)
                NotifiableCollection.CollectionChanged -= NotifyCollectionChange;

            if (this.ChangeNotificationIsEnabled && NotifyChange)
                NotifyCollectionChange(Collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, Collection));
        }

        /// <summary>
        /// Unregisters all collections, optionally notifying this change.
        /// </summary>
        public void UnregisterAllCollections(bool NotifyChange = true)
        {
            var Collections = this.SourceCollections.ToList();
            foreach (var Collection in Collections)
                this.UnregisterCollection(Collection, NotifyChange);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Receives the change notification of one source Collection and propagates it to the subscribers.
        /// </summary>
        protected void NotifyCollectionChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            var ChangedCollection = sender as IEnumerable<TResultItem>;
            var Handler = CollectionChanged;

            if (ChangedCollection == null || Handler == null)
                return;

            int IndexOffset = 0;
            bool Found = false;

            foreach (var Collection in this.SourceCollections)
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

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Empties this enumerable and notifies its resetting.
        /// </summary>
        public void Reset()
        {
            this.SourceCollections.Clear();
            NotifyReset();
        }

        public IEnumerator<TResultItem> GetEnumerator()
        {
            // Create local copies of used collections and extractor to avoid clashes while changing these.
            var ExposedCollections = this.SourceCollections.ToList();
            var Extractor = this.ResultExtractor;

            foreach (var Collection in ExposedCollections)
                foreach (var Item in Collection)
                {
                    var Extraction = Extractor(Item);
                    if (Extraction.Item1)
                        yield return Extraction.Item2;
                }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Sends a notification of whole collection reset.
        /// </summary>
        public void NotifyReset()
        {
            var Handler = CollectionChanged;
            if (Handler == null)
                return;

            Handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /*? public void Dispose()
        {
            this.ChangeNotificationIsEnabled = false;

            // Unattachment of events (indirectly) ...
            var RegisteredCollections = this.SourceCollections.ToArray();
            foreach (var Collection in RegisteredCollections)
                this.UnregisterCollection(Collection);
        } */
    }

    // =====================================================================================================================================================================================================
    /// <summary>
    /// Provides enumerable access for:
    /// - Single or multiple-centralized (a group treated as one) source collections.
    /// - Sending of notifications when these source collection(s) are observable.
    /// - Filter selection for the enumerated items.
    /// This particular type is for initialize source collections from an IEnumerable{TContainer} collections container,
    /// where the source and result items are of different types..
    /// </summary>
    /// <typeparam name="TSourceItem">Type of the consumed source collection Items</typeparam>
    /// <typeparam name="TResultItem">Type of the provided enumerated Items</typeparam>
    /// <typeparam name="TContainerItem">Type of the items containing collections to be consumed as source collections.</typeparam>
    public class ExtendedEnumerable<TSourceItem, TResultItem, TContainerItem> : ExtendedEnumerable<TSourceItem, TResultItem>
    {
        /// <summary>
        /// Constructor for register multiple initial collections, within a collection Container.
        /// </summary>
        /// <param name="ResultExtractor">Result items extractor, which returns: A filter-accepted indicator (bool), plus the extracted/converted TResultItem value.</param>
        /// <param name="ContainedCollectionExtractor">Function for extracting a Collection from the supplied Initial Collections Container.</param>
        /// <param name="CollectionsContainers">Collection of Containers having a source Collections to be exposed as enumerable.</param>
        public ExtendedEnumerable(Func<TSourceItem, Tuple<bool, TResultItem>> ResultExtractor,
                                  Func<TContainerItem, IEnumerable<TSourceItem>> ContainedCollectionExtractor,
                                  IList<TContainerItem> CollectionsContainers)
            : base(ResultExtractor)
        {
            General.ContractRequiresNotNull(ContainedCollectionExtractor, CollectionsContainers);

            this.CollectionsContainers = CollectionsContainers;
            this.ContainedCollectionExtractor = ContainedCollectionExtractor;

            if (this.CollectionsContainers is INotifyCollectionChanged)
            {
                this.ChangeableInitialCollectionsContainer = this.CollectionsContainers as INotifyCollectionChanged;
                this.ChangeableInitialCollectionsContainer.CollectionChanged += ChangeableInitialCollectionsContainer_CollectionChanged;
            }

            foreach (var CollectionContainer in this.CollectionsContainers)
                RegisterCollection(this.ContainedCollectionExtractor(CollectionContainer));

            this.ChangeNotificationIsEnabled = true;
        }

        /// <summary>
        /// Reevaluates, and possibly change, the collection containing the current collection assignments based on the supplied Updating Containing Collections.
        /// Returns this instance.
        /// </summary>
        public ExtendedEnumerable<TSourceItem, TResultItem, TContainerItem>
               UpdateCollectionContainmentAssignments(IList<TContainerItem> UpdatingContainingCollections)
        {
            if (UpdatingContainingCollections == null || UpdatingContainingCollections.Count < 1)
            {
                Reset();
                return this;
            }

            if (this.CollectionsContainers.HasEquivalentContent(UpdatingContainingCollections))
                return this;

            Reset();
            foreach (var CollectionContainer in this.CollectionsContainers)
                RegisterCollection(this.ContainedCollectionExtractor(CollectionContainer));

            return this;
        }

        private INotifyCollectionChanged ChangeableInitialCollectionsContainer = null;

        /// <summary>
        /// Set of collections containers, having the collections to be consumed and observed for later enumeration exposition.
        /// </summary>
        public IList<TContainerItem> CollectionsContainers { get; protected set; }

        /// <summary>
        /// Function for extracting a Collection from the supplied Initial Collections Container.
        /// </summary>
        public Func<TContainerItem, IEnumerable<TSourceItem>> ContainedCollectionExtractor { get { return this.ContainedCollectionExtractor_; } protected set { this.ContainedCollectionExtractor_ = value; } }
        [NonSerialized]
        private Func<TContainerItem, IEnumerable<TSourceItem>> ContainedCollectionExtractor_ = null;

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Register/unregister source collections, if they are added/removed from the Initial Collections Container.
        /// </summary>
        void ChangeableInitialCollectionsContainer_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Unregister all registered collections...
                var RegisteredCollections = this.SourceCollections.ToArray();
                foreach (var Collection in RegisteredCollections)
                    this.UnregisterCollection(Collection);

                // Register all sent collections...
                if (sender is IEnumerable<TContainerItem>)
                {
                    var SentCollections = sender as IEnumerable<TContainerItem>;

                    foreach (var CollectionContainer in SentCollections)
                        this.RegisterCollection(ContainedCollectionExtractor(CollectionContainer));
                }

                return;
            }

            if (e.Action.IsOneOf(NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Replace))
                foreach (var Collection in e.OldItems)
                    this.UnregisterCollection(Collection as IEnumerable<TSourceItem>);

            if (e.Action.IsOneOf(NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Replace))
                foreach (var Collection in e.NewItems)
                    this.RegisterCollection(Collection as IEnumerable<TSourceItem>);
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /*? public void Dispose()
        {
            base.Dispose();

            // Unattachment of events (directly) ...
            if (this.ChangeableInitialCollectionsContainer != null)
                this.ChangeableInitialCollectionsContainer.CollectionChanged -= ChangeableInitialCollectionsContainer_CollectionChanged;
        } */
    }
}
