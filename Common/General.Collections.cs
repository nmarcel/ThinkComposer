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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Documents;

using Microsoft.Win32;

using Instrumind.Common.EntityBase;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Provides common features such as encryption, serialization, strings handling extensions and global constants. Collections part.
    /// </summary>
    public static partial class General
    {
        /// <summary>
        /// Determines if this supplied object is equal to any one of the others passed.
        /// </summary>
        public static bool IsOneOf<TEvaluated>(this TEvaluated Evaluated, params TEvaluated[] Cases)
        {
            return IsIn(Evaluated, Cases);
        }

        /// <summary>
        /// Determines if this supplied object is equal to any one of the collection passed.
        /// </summary>
        public static bool IsIn<TEvaluated>(this TEvaluated Evaluated, IEnumerable<TEvaluated> Items)
        {
            return Items.Any(item => item.IsEqual(Evaluated));
        }

        /// <summary>
        /// Returns true if any of the elements in the source collection is found in the target collection, optionally a comparer can be specified.
        /// </summary>
        public static bool ContainsAny<TItem>(this IEnumerable<TItem> Source, IEnumerable<TItem> Target, Func<TItem, TItem, bool> Comparer = null)
        {
            if (Comparer == null)
                Comparer = ((srci, trgi) => srci.IsEqual(trgi));

            foreach (var srci in Source)
                foreach (var trgi in Target)
                    if (Comparer(srci, trgi))
                        return true;

            return false;
        }
        /// <summary>
        /// Returns true if any of the elements in the source collection is found in the parameters.
        /// </summary>
        public static bool ContainsAnyOf<TItem>(this IEnumerable<TItem> Source, params TItem[] Target)
        {
            if (Target == null || Target.Length < 1)
                return false;

            return ContainsAny(Source, Target);
        }
        
        /// <summary>
        /// Returns true if this Source collection is null or has no items.
        /// </summary>
        public static bool IsEmpty<TItem>(this IEnumerable<TItem> Source)
        {
            var Result = (Source == null || !Source.Any());
            return Result;
        }

        /// <summary>
        /// Returns this supplied Item encapsulated into an Array.
        /// </summary>
        public static TItem[] IntoArray<TItem>(this TItem Item)
        {
            return (new TItem[] { Item });
        }

        /// <summary>
        /// Returns this supplied Item encapsulated into a List.
        /// </summary>
        public static List<TItem> IntoList<TItem>(this TItem Item)
        {
            var Result = new List<TItem>(1);
            Result.Add(Item);

            return Result;
        }

        /// <summary>
        /// Enumerates the supplied items.
        /// </summary>
        public static IEnumerable<TItem> Enumerate<TItem>(params TItem[] Items)
        {
            foreach (var Item in Items)
                yield return Item;
        }


        /// <summary>
        /// Creates and returns an Array composed of the supplied items.
        /// </summary>
        public static TItem[] CreateArray<TItem>(params TItem[] Items)
        {
            var Result = (Items == null ? new TItem[0] : Items);
            return Result;
        }

        /// <summary>
        /// Creates and returns an Array composed of the supplied not null items.
        /// </summary>
        public static TItem[] CreateArrayWithoutNulls<TItem>(params TItem[] Items)
        {
            var Result = (Items == null ? new TItem[0] : Items.Where(item => item != null).ToArray());
            return Result;
        }

        /// <summary>
        /// Creates and returns a List composed of the supplied items.
        /// </summary>
        public static List<TItem> CreateList<TItem>(params TItem[] Items)
        {
            var Result = (Items == null ? new List<TItem>() : new List<TItem>(Items.Length));

            if (Items != null)
                foreach (var Item in Items)
                    Result.Add(Item);

            return Result;
        }

        /// <summary>
        /// Creates and returns a List composed of the supplied not null items.
        /// </summary>
        public static List<TItem> CreateListWithoutNulls<TItem>(params TItem[] Items)
        {
            var Result = (Items == null ? new List<TItem>() : new List<TItem>(Items.Length));

            if (Items != null)
                foreach (var Item in Items)
                    if (Item != null)
                        Result.Add(Item);

            return Result;
        }

        /// <summary>
        /// Returns this supplied Item encapsulated into an Enumerable.
        /// </summary>
        public static IEnumerable<TItem> IntoEnumerable<TItem>(this TItem Item)
        {
            yield return Item;
        }

        /// <summary>
        /// Returns the supplied Items encapsulated into an Enumerable.
        /// </summary>
        public static IEnumerable<TItem> IntoEnumerable<TItem>(params TItem[] Items)
        {
            foreach(var Item in Items)
                yield return Item;
        }

        /// <summary>
        /// Adds the specified Items to this supplied Target items list.
        /// </summary>
        public static void AddItems<TItem>(this IList<TItem> Target, params TItem[] Items)
        {
            if (Target == null || Items == null)
                return;

            foreach (var Item in Items)
                Target.Add(Item);
        }

        /// <summary>
        /// Returns the enumerable concatenation of this supplied Item with the specified Items.
        /// </summary>
        public static IEnumerable<TItem> Concatenate<TItem>(this TItem Item, params TItem[] Items)
        {
            return Item.IntoEnumerable().Concatenate(Items);
        }

        /// <summary>
        /// Returns the enumerable concatenation of this supplied Source collection with the specified Items.
        /// </summary>
        public static IEnumerable<TItem> Concatenate<TItem>(this IEnumerable<TItem> Source, params TItem[] Items)
        {
            if (Source == null)
                return Items;

            if (Items == null || Items.Length < 1)
                return Source;

            return Source.Concat(Items);
        }

        /// <summary>
        /// Indicates whether this supplied collection has at least the specified Minimum Count of items, optionally with a where-filter.
        /// This avoids to travel all the collection, when only is needed to know the existence of the firt n items.
        /// </summary>
        public static bool CountsAtLeast<TItem>(this IEnumerable<TItem> Source, int MinCount, Func<TItem, bool> Where = null)
        {
            if (Source == null)
                return false;

            var Count = 0;
            foreach (var Item in Source)
            {
                if (Where != null && !Where(Item))
                    continue;

                Count++;
                if (Count >= MinCount)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// For each member item of this supplied (IEnumerable) collection, executes the specified operation.
        /// </summary>
        public static IEnumerable<TItem> ForEach<TItem>(this IEnumerable<TItem> Source, Action<TItem> Operation)
        {
            if (Source == null)
                throw new UsageAnomaly("Enumeration source cannot be null.");

            foreach (var Item in Source)
                Operation(Item);

            return Source;
        }

        /// <summary>
        /// Applies a select operation, passing a zero-based index of the current item to the selector.
        /// Optionally, a count limit can be specified.
        /// </summary>
        public static IEnumerable<TResult> SelectIndexing<TSource, TResult>(this IEnumerable<TSource> Source, Func<TSource, int, TResult> Selector, int CountLimit = -1)
        {
            if (Source == null)
                throw new UsageAnomaly("Enumeration source cannot be null.");

            var Index = 0;
            foreach (var Item in Source)
            {
                if (CountLimit >= 0 && Index >= CountLimit)
                    break;

                yield return Selector(Item, Index);
                Index++;
            }
        }

        /// <summary>
        /// For each member item of this supplied (IEnumerable) collection, executes the specified operation which receives a zero-based index.
        /// Optionally, a count limit can be specified.
        /// </summary>
        public static IEnumerable<TItem> ForEachIndexing<TItem>(this IEnumerable<TItem> Source, Action<TItem, int> Operation, int CountLimit = -1)
        {
            if (Source == null)
                throw new UsageAnomaly("Enumeration source cannot be null.");

            var Index = 0;
            foreach (var Item in Source)
            {
                if (CountLimit >= 0 && Index >= CountLimit)
                    break;

                Operation(Item, Index);
                Index++;
            }

            return Source;
        }

        /// <summary>
        /// Returns a collection of output items, created by a used-defined Selector,
        /// based in the input Items and a sequential number started at a Sequence-Start (by default starting from 0).
        /// </summary>
        public static IEnumerable<TResult> Sequence<TItem, TResult>(this IEnumerable<TItem> Items, Func<int, TItem, TResult> Selector, int SequenceStart = 0)
        {
            foreach (var Item in Items)
            {
                yield return Selector(SequenceStart, Item);
                SequenceStart++;
            }
        }

        /// <summary>
        /// Returns, from the Source collection, the Items that are at every Interval times from the Initial index.
        /// </summary>
        public static IEnumerable<TItem> TakeEvery<TItem>(this IEnumerable<TItem> Source, int Interval, int InitialIndex = 0)
        {
            if (Interval < 1 || InitialIndex < 0)
                yield break;

            if (InitialIndex > 0)
                Source = Source.Skip(InitialIndex);

            int Count = 0;
            foreach (var Item in Source)
            {
                Count++;
                if (Count == Interval)
                {
                    yield return Item;
                    Count = 0;
                }
            }
        }

        /*- /// <summary>
        /// Returns all the items of TItem type casteable to the TCasted type.
        /// Optionally, the non casted items (which may be returned as default(TCasted)) can be discarded.
        /// </summary>
        public static IEnumerable<TCasted> CastAs<TCasted, TItem>(this IEnumerable<TItem> Source, bool DiscardNonCasted = true)
                where TCasted : class
                where TItem : class
        {
            foreach (var Item in Source)
                if (Item is TCasted)
                    yield return Item as TCasted;
                else
                    if (!DiscardNonCasted)
                        yield return default(TCasted);
        } */

        /// <summary>
        /// Returns all the items of TItem type casteable to the TCasted type which also met the specified filter.
        /// </summary>
        public static IEnumerable<TCasted> CastAs<TCasted, TItem>(this IEnumerable<TItem> Source,
                                                                  Func<TCasted, bool> Filter = null)
            where TCasted : class
            where TItem : class
        {
            foreach (var Item in Source)
                if (Item is TCasted)    // Notice that a null value can be accepted
                {
                    var Casted = Item as TCasted;

                    if (Filter == null || Filter(Casted))
                        yield return Item as TCasted;
                }
        }

        /// <summary>
        /// Generates an extended-enumerable from this supplied source collection.
        /// Optionally, an extractor filter function can be specified.
        /// </summary>
        public static IEnumerable<TItem> GetExtendedEnumerable<TItem>(this IEnumerable<TItem> Source,
                                                                      Func<TItem, Tuple<bool, TItem>> Extractor = null)
        {
            if (Extractor == null)
                Extractor = (item => Tuple.Create<bool, TItem>(true, item));

            return (new ExtendedEnumerable<TItem, TItem>(Extractor, Source));
        }

        /// <summary>
        /// Determines if this Source collection contains the same items of the supplied Sample, or if both are null.
        /// </summary>
        public static bool HasEquivalentContent<TItem>(this IEnumerable<TItem> Source, IEnumerable<TItem> Sample)
        {
            if (Source == null && Sample == null)
                return true;

            if (Source == null || Sample == null || Source.Count() != Sample.Count())
                return false;

            var SampleEnumerator = Sample.GetEnumerator();
            SampleEnumerator.MoveNext();

            foreach (var TargetItem in Source)
            {
                if (!TargetItem.IsEqual(SampleEnumerator.Current))
                    return false;

                SampleEnumerator.MoveNext();
            }

            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a range of doubles with an optional step.
        /// </summary>
        public static IEnumerable<double> RangeOfDoubles(double From, double To, double Step = 1.0)
        {
            if (Step <= 0.0)
                Step = ((Step == 0.0) ? 1.0 : -Step);

            if (From <= To)
                for (var Value = From; Value <= To; Value += Step)
                    yield return Value;
            else
                for (var Value = From; Value >= To; Value -= Step)
                    yield return Value;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns, from this supplied IList, the Item at the specified Index position, or its default type value if not found or the List is null.
        /// </summary>
        public static TItem GetItemOrDefault<TItem>(this IList<TItem> Source, int Index)
        {
            if (Source == null || Index < 0 || Index >= Source.Count)
                return default(TItem);

            return Source[Index];
        }

        /// <summary>
        /// Returns, from this supplied IList, the Item at the specified Index position, or its default type value if not found or the List is null.
        /// </summary>
        public static object GetItemOrDefault(this IList Source, int Index)
        {
            if (Source == null || Index < 0 || Index >= Source.Count)
                return null;

            return Source[Index];
        }

        /// <summary>
        /// Adds the supplied Item to this specified Target list only if is new and return true, else ingore it and return false.
        /// </summary>
        public static bool AddNew<TItem>(this IList<TItem> Target, TItem Item)
        {
            if (Target.Contains(Item))
                return false;

            Target.Add(Item);
            return true;
        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Adds the supplied Key and Value pair to this specified Target Dictionary only if is new and return true, else updates it and return false.
        /// </summary>
        public static bool AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> Target, TKey Key, TValue Value)
        {
            if (Target.ContainsKey(Key))
            {
                Target[Key] = Value;
                return false;
            }

            Target.Add(Key, Value);
            return true;
        }

        /// <summary>
        /// Adds the supplied Key and Value pair to this specified Target Dictionary only if is new and return true, else ingores it and return false.
        /// </summary>
        public static bool AddNew<TKey, TValue>(this IDictionary<TKey, TValue> Target, TKey Key, TValue Value)
        {
            if (Target.ContainsKey(Key))
                return false;

            Target.Add(Key, Value);
            return true;
        }

        /// <summary>
        /// Put the supplied Key and Value pair into this specified Target Dictionary. If new, then add it and return false, else (already present) replace it and return true.
        /// </summary>
        public static bool Put<TKey, TValue>(this IDictionary<TKey, TValue> Target, TKey Key, TValue Value)
        {
            if (Target.ContainsKey(Key))
            {
                Target[Key] = Value;
                return true;
            }

            Target.Add(Key, Value);
            return false;
        }

        /// <summary>
        /// For the supplied dictionary, puts the specified value into the list associated to the specified key.
        /// </summary>
        public static void PutIntoSublist<TKey, TValue>(this Dictionary<TKey, List<TValue>> Target, TKey Key, TValue Value)
        {
            List<TValue> Sublist = null;

            if (Target.ContainsKey(Key))
            {
                Sublist = Target[Key];
                if (Sublist == null)
                    Target[Key] = new List<TValue>();
            }
            else
            {
                Sublist = new List<TValue>();
                Target.Add(Key, Sublist);
            }

            Sublist.AddNew(Value);
        }

        // -------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// For this Source list, swaps the items at the specified positions.
        /// </summary>
        public static void SwapItemsAt<TItem>(this IList<TItem> Source, int FirstItemPosition, int SecondItemPosition)
        {
            var OriginalFirst = Source[FirstItemPosition];
            Source[FirstItemPosition] = Source[SecondItemPosition];
            Source[SecondItemPosition] = OriginalFirst;
        }

        // -------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Searches a matching item, of this supplied IList, where the item is equivalent (see IsEquivalent()) to the specified searched one.
        /// Returns a tuple whose Item1 is the indication (true/false) of equivalence, the Item2 is the matching item or its empty default.
        /// Returns null if the source IList is null.
        /// </summary>
        public static Tuple<bool, TItem> SearchEquivalentItem<TItem>(this IList<TItem> Source, TItem SearchItem) where TItem : class
        {
            if (Source == null)
                return null;

            if (SearchItem != null)
                foreach (var Reg in Source)
                    if (Reg.IsEquivalent(SearchItem))
                        return (Tuple.Create<bool, TItem>(true, Reg));

            return (Tuple.Create<bool, TItem>(false, default(TItem)));
        }

        /// <summary>
        /// Searches a matching key-value pair, of this supplied IDictionary, where the Key is equivalent (see IsEquivalent()) to the specified search Key.
        /// Returns a tuple whose Item1 is the indication (true/false) of equivalence, the Item2 is the Key or its empty default, and the Item3 is its Value or its empty default.
        /// Returns null if the source IDictionary is null.
        /// </summary>
        public static Tuple<bool, TKey, TValue> SearchEquivalentKey<TKey, TValue>(this IDictionary<TKey, TValue> Source, TKey SearchKey) where TKey : class
        {
            if (Source == null)
                return null;

            if (SearchKey != null)
                foreach (var Reg in Source)
                    if (Reg.Key.IsEquivalent(SearchKey))
                        return (Tuple.Create<bool, TKey, TValue>(true, Reg.Key, Reg.Value));

            return (Tuple.Create<bool, TKey, TValue>(false, default(TKey), default(TValue)));
        }

        /// <summary>
        /// Returns the matching pair, whose Key is equivalent (see IsEquivalent()) to the supplied search Key.
        /// If no match found, then either throws an exception or returns a default (empty) pair, depeding on the ReturnDefaultIfNotFound supplied option.
        /// </summary>
        public static KeyValuePair<TKey, TValue> GetPairByEquivalentKey<TKey, TValue>(this IDictionary<TKey, TValue> Source, TKey SearchKey, bool ReturnDefaultIfNotFound = false) where TKey : class
        {
            foreach (var Reg in Source)
                if (Reg.Key.IsEquivalent(SearchKey))
                    return Reg;

            if (ReturnDefaultIfNotFound)
                return default(KeyValuePair<TKey, TValue>);

            throw (new KeyNotFoundException());
        }

        /// <summary>
        /// Interprets as the specified type, the item of the supplied collection at the supplied index (0 is assumed when not specified).
        /// </summary>
        public static TInterpreted InterpretItem<TInterpreted>(this IEnumerable<object> Target, int Index = 0)
        {
            if (Target == null || Target.Count() <= Index)
                return default(TInterpreted);

            var Item = Target.ElementAt(Index);
            TInterpreted Result = default(TInterpreted);

            if (Item is TInterpreted)
                Result = (TInterpreted)Item;

            return Result;
        }

        /// <summary>
        /// For a given object, returns the value associated to a matching key from a list of candidates.
        /// If none matches, then a default value can be specified for return.
        /// </summary>
        /// <typeparam name="TEvaluated">Type of the evaluated object and the keys of the candidates.</typeparam>
        /// <typeparam name="TResult">Type of the expected result and the value of the candidates</typeparam>
        /// <param name="Evaluated">The evaluated object.</param>
        /// <param name="Candidates">Collection of key-value pairs whose key will be compared.</param>
        /// <param name="Default">Optional default value to be returned if no candidate matches.</param>
        /// <returns>The value of the associated matching key, or the default value if no matching found.</returns>
        public static TResult SelectCorresponding<TEvaluated, TResult>(this TEvaluated Evaluated, IDictionary<TEvaluated, TResult> Candidates, TResult Default = default(TResult))
        {
            if (Candidates == null || Candidates.Count < 1)
                return Default;

            foreach (var Item in Candidates)
                if (Item.Key.Equals(Evaluated))
                    return Item.Value;

            return Default;
        }

        /// <summary>
        /// Gets this source-items, starting from the explicit supplied ones, thus giving an explicit order.
        /// </summary>
        public static IEnumerable<TValue> OrderedInitiallyWith<TValue>(this IEnumerable<TValue> SourceItems, params TValue[] ExplicitStartingItems)
        {
            if (ExplicitStartingItems == null || ExplicitStartingItems.Length < 1)
                return SourceItems;

            return ExplicitStartingItems.Concat(SourceItems.Except(ExplicitStartingItems));
        }

        /// <summary>
        /// Updates the content of this supplied Target-Items Dictionary, based on the specified Source-Items dictionary.
        /// </summary>
        public static void UpdateDictionaryContentFrom<TKey, TValue>(this IDictionary<TKey, TValue> TargetItems, IDictionary<TKey, TValue> SourceItems)
        {
            var ExistentItems = new Dictionary<TKey, TValue>();
            var NonexistentItemKeys = new List<TKey>();

            // Identificate existents (with different value, for update) and nonexistents (for removal) in source-items.
            foreach (var TargetItem in TargetItems)
                if (!SourceItems.ContainsKey(TargetItem.Key))
                    NonexistentItemKeys.Add(TargetItem.Key);
                else
                {
                    var SourceValue = SourceItems[TargetItem.Key];
                    if (!TargetItem.Value.IsEqual(SourceValue))
                        ExistentItems.Add(TargetItem.Key, SourceValue);
                }

            // Remove the marked as nonexistents in source
            foreach (var NonexistentItemKey in NonexistentItemKeys)
                TargetItems.Remove(NonexistentItemKey);

            // Update the marked as existents with different value in source
            foreach (var ExistentItem in ExistentItems)
                TargetItems[ExistentItem.Key] = ExistentItem.Value;

            // Add items nonexistents in target-items
            foreach (var SourceItem in SourceItems)
                if (!TargetItems.ContainsKey(SourceItem.Key))
                    TargetItems.Add(SourceItem);
        }

        /// <summary>
        /// Updates and sorts the content of this supplied Target-Items List, based on the specified Source-Items collection, plus items equivalence comparer.
        /// </summary>
        public static void UpdateSortListContentFrom<TItem>(this IList<TItem> TargetItems, IEnumerable<TItem> SourceItems, Func<TItem, TItem, bool> ItemsEquivalenceComparer = null)
        {
            int EvaluationIndex = 0;
            ItemsEquivalenceComparer = ItemsEquivalenceComparer.NullDefault(General.IsEquivalent);

            // Add+insert non-existent items, plus repositioning the unsorted.
            foreach (var SourceItem in SourceItems)
            {
                var ExistentTargetIndex = TargetItems.IndexOfMatch(targetitem => ItemsEquivalenceComparer(targetitem, SourceItem));

                if (ExistentTargetIndex < 0)
                    if (EvaluationIndex < TargetItems.Count)
                        TargetItems.Insert(EvaluationIndex, SourceItem);
                    else
                        TargetItems.Add(SourceItem);
                else
                {
                    if (ExistentTargetIndex == EvaluationIndex)
                        TargetItems[EvaluationIndex] = SourceItem;
                    else
                    {
                        var TakenItem = TargetItems[EvaluationIndex];
                        TargetItems[EvaluationIndex] = TargetItems[ExistentTargetIndex];
                        TargetItems[ExistentTargetIndex] = TakenItem;
                    }
                }

                EvaluationIndex++;
            }

            // Remove the remaining items
            while (EvaluationIndex < TargetItems.Count)
                TargetItems.RemoveAt(EvaluationIndex);
        }

        /// <summary>
        /// Updates the content (only addition and deletion of items) of this supplied Target-Items List, based on the specified Source-Items collection,
        /// plus items erasability evaluator and items equivalence comparer.
        /// </summary>
        public static void UpdateOnlyListContentFrom<TItem>(this IList<TItem> TargetItems, IEnumerable<TItem> SourceItems,
                                                            Func<TItem, bool> ItemsErasabilityEvaluator = null,
                                                            Func<TItem, TItem, bool> ItemsEquivalenceComparer = null)
        {
            ItemsErasabilityEvaluator = ItemsErasabilityEvaluator.NullDefault(item => true);
            ItemsEquivalenceComparer = ItemsEquivalenceComparer.NullDefault(General.IsEquivalent);

            // Remove the remaining items.
            var DeleteItems = TargetItems.Where(target => SourceItems.IndexOfMatch(source => ItemsEquivalenceComparer(source, target)) < 0).ToArray();
            foreach (var Item in DeleteItems)
                if (ItemsErasabilityEvaluator(Item))
                    TargetItems.Remove(Item);

            // Add the non-existent items, and update the matching ones.
            foreach (var Source in SourceItems)
            {
                var Index = TargetItems.IndexOfMatch(target => ItemsEquivalenceComparer(target, Source));

                if (Index < 0)
                    TargetItems.AddNew(Source);
                else
                    TargetItems[Index] = Source;
            }
        }

        /// <summary>
        /// Updates the sort/order disposition of this supplied Target-Items list, based on the specified Sorted-Items collection.
        /// </summary>
        public static void UpdateSortFrom<TItem>(this IList<TItem> TargetItems, IEnumerable<TItem> SortedItems)
        {
            UpdateSortFrom<TItem, TItem>(TargetItems, SortedItems, (target, source) => target.IsEqual(source));
        }

        /// <summary>
        /// Updates the sort/order disposition of this supplied Target-Items list, based on the specified Sorted-Items collection plus an Equivalence evaluator.
        /// </summary>
        public static void UpdateSortFrom<TTargetItem, TSortedItem>(this IList<TTargetItem> TargetItems, IEnumerable<TSortedItem> SortedItems, Func<TTargetItem, TSortedItem, bool> EquivalenceEvaluator)
        {
            int SortedIndex = 0;
            foreach (var SortedItem in SortedItems)
            {
                var UnsortedIndex = TargetItems.IndexOfMatch(item => EquivalenceEvaluator(item, SortedItem));

                if (UnsortedIndex >= 0)
                {
                    if (UnsortedIndex != SortedIndex)
                    {
                        var TakenItem = TargetItems[SortedIndex];
                        TargetItems[SortedIndex] = TargetItems[UnsortedIndex];
                        TargetItems[UnsortedIndex] = TakenItem;
                    }

                    SortedIndex++;
                }
            }
        }

        /// <summary>
        /// Detects and informs changes between two lists, considering its collection-related existence and internal data content.
        /// </summary>
        /// <param name="CurrentList">This current list to be compared from.</param>
        /// <param name="ChangedList">The changed list to be compared against.</param>
        /// <param name="ItemsInstanceComparer">Determines, returning true, if the supplied two item instances are/represents the same item. Optional. By default, compared with IsEqual().</param>
        /// <param name="ItemsDataComparer">Determines and returns a set of data-modified fields, after evaluating the two supplied item instances, informing field-name, plus new and previous values. Optional. By default, the General.DetermineDifferences() comparison is used.</param>
        /// <param name="ItemsDataPropertiesComparer">Determines, returning true, if the supplied two property values are/represents the same item. By default, compared with IsEqual().</param>
        /// <returns>Null if no differences are detected. Else, sets of items [1]Created (informing new position),  [2]Deleted (informing previous position) and [3]Moved (informing new and previous positions),
        /// plus those with [4]Modified Data (informing new item position, plus field-name, new and previous values).</returns>
        public static Tuple<Dictionary<int, TItem>, Dictionary<int, TItem>, Dictionary<Tuple<int, int>, TItem>, Dictionary<int, Dictionary<string, Tuple<object, object>>>>
                      GetDifferencesFrom<TItem>(this IList<TItem> CurrentList,
                                                IList<TItem> ChangedList,
                                                Func<TItem, TItem, bool> ItemsInstanceComparer = null,
                                                Func<TItem, TItem, Func<object, object, bool>, Dictionary<string, Tuple<object, object>>> ItemsDataComparer = null,
                                                Func<object, object, bool> ItemsDataPropertiesComparer = null)
        {
            if (CurrentList.IsEqual(ChangedList))
                return null;

            ItemsInstanceComparer = ItemsInstanceComparer.NullDefault(IsEqual);
            ItemsDataComparer = ItemsDataComparer.NullDefault(General.DetermineDifferences);
            ItemsDataPropertiesComparer = ItemsDataPropertiesComparer.NullDefault(IsEqual);

            var CreatedItems = new Dictionary<int, TItem>();
            var DeletedItems = new Dictionary<int, TItem>();
            var MovedItems = new Dictionary<Tuple<int, int>, TItem>();
            var DataModifiedItems = new Dictionary<int, Dictionary<string, Tuple<object, object>>>();

            var Result = Tuple.Create<Dictionary<int, TItem>, Dictionary<int, TItem>,
                                      Dictionary<Tuple<int, int>, TItem>, Dictionary<int, Dictionary<string, Tuple<object, object>>>>
                                        (CreatedItems, DeletedItems, MovedItems, DataModifiedItems);

            // Determine Created items.
            for (int ChangedIndex = 0; ChangedIndex < ChangedList.Count; ChangedIndex++)
            {
                var ChangedItem = ChangedList[ChangedIndex];


                if (!CurrentList.Any(current => ItemsInstanceComparer(current, ChangedItem)))
                    CreatedItems.Add(ChangedIndex, ChangedItem);
            }

            // Determinte Deleted, Potentially-Modified and Moved items.
            for (int CurrentIndex = 0; CurrentIndex < CurrentList.Count; CurrentIndex++)
            {
                var CurrentItem = CurrentList[CurrentIndex];
                var ChangedIndex = ChangedList.IndexOfMatch(changed => ItemsInstanceComparer(CurrentItem, changed));

                if (ChangedIndex < 0)
                    DeletedItems.Add(CurrentIndex, CurrentItem);
                else
                {
                    if (ItemsDataComparer != null)
                    {
                        var DetectedChanges = ItemsDataComparer(ChangedList[ChangedIndex], CurrentItem, ItemsDataPropertiesComparer);
                        if (DetectedChanges != null && DetectedChanges.Count > 0)
                            DataModifiedItems.Add(ChangedIndex, DetectedChanges);
                    }

                    if (ChangedIndex != CurrentIndex)
                        MovedItems.Add(Tuple.Create<int, int>(ChangedIndex, CurrentIndex), CurrentItem);
                }
            }

            if (CreatedItems.Count < 1 && DeletedItems.Count < 1 && MovedItems.Count < 1 && DataModifiedItems.Count < 1)
                return null;

            return Result;
        }

        /// <summary>
        /// Returns, for a provided collection, the zero-based index position of the first item matchig the specified criteria or -1 if none matches.
        /// </summary>
        public static int IndexOfMatch<TItem>(this IEnumerable<TItem> TargetCollection, Func<TItem, bool> Criteria)
        {
            int Index = 0;

            foreach (var Item in TargetCollection)
            {
                if (Criteria(Item))
                    return Index;
                Index++;
            }
            return -1;
        }

        /// <summary>
        /// Returns the position of the specified Key Item into this supplied dictionary.
        /// </summary>
        public static int IndexOfKey<TKey, TValue>(this IDictionary<TKey, TValue> TargetDictionary, TKey Key)
        {
            int Index = -1;

            foreach (var Reg in TargetDictionary)
            {
                Index++;
                if (Reg.Key.IsEqual(Key))
                    return Index;
            }

            return -1;
        }

        /// <summary>
        /// Returns the position of the specified Item into this supplied dictionary.
        /// </summary>
        public static int IndexOfItem<TKey, TValue>(this IDictionary<TKey, TValue> TargetDictionary, KeyValuePair<TKey, TValue> Item)
        {
            int Index = -1;

            foreach (var Reg in TargetDictionary)
            {
                Index++;
                if (Reg.IsEqual(Item))
                    return Index;
            }

            return -1;
        }

        /// <summary>
        /// Returns array containing the keys of the supplied dictionary.
        /// </summary>
        public static TKey[] DictionaryKeysToArray<TKey, TValue>(this IDictionary<TKey, TValue> TargetDictionary)
        {
            TKey[] KeysList = new TKey[TargetDictionary.Count];
            int Ind = 0;
            foreach (KeyValuePair<TKey, TValue> Register in TargetDictionary)
            {
                KeysList[Ind] = Register.Key;
                Ind++;
            }

            return KeysList;
        }

        /// <summary>
        /// Returns the items contained in a dictionary in a keyValuePair array.
        /// </summary>
        public static KeyValuePair<TLlave, TValor>[] DictionaryToArray<TLlave, TValor>(this IDictionary<TLlave, TValor> TargetDictionary)
        {
            KeyValuePair<TLlave, TValor>[] Result = new KeyValuePair<TLlave, TValor>[TargetDictionary.Count];

            ((ICollection<KeyValuePair<TLlave, TValor>>)TargetDictionary).CopyTo(Result, 0);

            return Result;
        }

        /// <summary>
        /// Returns an array containing the values of the supplied dictionary.
        /// </summary>
        public static TValue[] DictionaryValuesToArray<TKey, TValue>(this IDictionary<TKey, TValue> TargetDictionary)
        {
            TValue[] ValuesList = new TValue[TargetDictionary.Count];
            int Ind = 0;
            foreach (KeyValuePair<TKey, TValue> Register in TargetDictionary)
            {
                ValuesList[Ind] = Register.Value;
                Ind++;
            }

            return ValuesList;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Replaces an Old item by a New one and returns indication of found and substituted.
        /// </summary>
        public static bool Replace<TItem>(this IList<TItem> Target, TItem OldItem, TItem NewItem)
        {
            var Index = Target.IndexOf(OldItem);
            if (Index < 0)
                return false;

            Target[Index] = NewItem;
            return true;
        }

        /// <summary>
        /// Updates the content of this list, if determined by a supplied application-evaluator,
        /// with the result of the specified value-replacer.
        /// Returns the number of items replaced.
        /// </summary>
        public static int Replace<TItem>(this IList<TItem> Target, Func<TItem, bool> ApplicationEvaluator,
                                         Func<TItem, TItem> ValueReplacer)
        {
            int Changes = 0;

            for (int Index = 0; Index < Target.Count; Index++)
                if (ApplicationEvaluator(Target[Index]))
                {
                    Target[Index] = ValueReplacer(Target[Index]);
                    Changes++;
                }

            return Changes;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Removes duplicates of the supplied Target list applying replacer and comparer functions.
        /// </summary>
        public static void ReplaceDuplicates<TItem>(this IList<TItem> Target, Func<IEnumerable<TItem>, TItem, TItem> Replacer, Func<TItem, TItem, bool> ItemsEquivalenceComparer = null)
        {
            General.ContractRequiresNotNull(Target, Replacer);

            ItemsEquivalenceComparer = ItemsEquivalenceComparer.NullDefault(General.IsEqual);

            if (Target.Count <= 1)
                return;

            for (int Index = 0; Index < Target.Count; Index++)
                for (int Eval = 0; Eval < Target.Count; Eval++)
                    if (Index != Eval && ItemsEquivalenceComparer(Target[Index], Target[Eval]))
                        Target[Eval] = Replacer(Target, Target[Eval]);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of the items that exists more than once in the source collection.
        /// </summary>
        public static IEnumerable<TItem> Duplicates<TItem>(this IEnumerable<TItem> Source)
        {
            if (Source == null)
                return null;

            var Distincts = Source.Distinct().ToArray();

            if (Distincts.Length == Source.Count())
                return Enumerable.Empty<TItem>();

            var Duplicates = Distincts.Where(ditem => Source.CountsAtLeast(2, sitem => sitem.Equals(ditem)));
            return Duplicates;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Removes the duplicate extra items, from this source collection, preserving the first item of each duplicated items group.
        /// </summary>
        public static IEnumerable<TItem> RemoveDuplicateSurplus<TItem, TKey>(this IEnumerable<TItem> Source, Func<TItem, TKey> KeySelector)
        {
            Source = Source.OrderBy(KeySelector);

            var PreviousItem = default(TItem);
            var AtStart = true;

            foreach (var Item in Source)
            {
                if (AtStart)
                    yield return Item;
                else
                    if (!KeySelector(Item).IsEqual(KeySelector(PreviousItem)))
                        yield return Item;

                PreviousItem = Item;
                AtStart = false;
            }
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns, from the specified Source collection, all the items except those with empty values (the Default value of its base type).
        /// Use carefully. I.e.: This will remove nulls from a collection of objects (instance references), and zeroes from a collection of integers (values).
        /// </summary>
        public static IEnumerable<TItem> ExcludeEmptyItems<TItem>(this IEnumerable<TItem> Source)
        {
            var Result = Source.Where(item => !item.IsEqual(default(TItem)));
            return Result;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns, for this supplied dictionary, the value associated to the specified key or its default if none is.
        /// Optionally the default value can be specified instead of the type default.
        /// CAUTION: This is intended to be used only with dictionary where the stored values cannot be default(TValue).
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> Source, TKey Key, TValue Default = default(TValue))
        {
            if (Key == null || !Source.ContainsKey(Key))
                return Default;

            return Source[Key];
        }

        /// <summary>
        /// Returns, for this supplied dictionary, the value associated to the specified key or the first one.
        /// </summary>
        public static TValue GetValueOrFirst<TKey, TValue>(this IDictionary<TKey, TValue> Source, TKey Key)
        {
            if (Key == null || !Source.ContainsKey(Key))
                return Source.First().Value;

            return Source[Key];
        }

        /// <summary>
        /// Returns, for this source dictionary, the value associated to the matching key-value-pair with the supplied Evaluator or the first one.
        /// </summary>
        public static TValue GetMatchingOrFirst<TKey, TValue>(this IDictionary<TKey, TValue> Source, Func<TKey, TValue, bool> Evaluator)
        {
            if (Evaluator != null)
                foreach (var Pair in Source)
                    if (Evaluator(Pair.Key, Pair.Value))
                        return Pair.Value;

            return Source.First().Value;
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns, for this source collection, the item with matches the specified evaluator, else returns the initial item of the collection.
        /// </summary>
        public static TItem FirstOrInitial<TItem>(this IEnumerable<TItem> Source, Func<TItem, bool> Evaluator)
        {
            var Result = Source.FirstOrDefault(Evaluator);
            if (Result.IsEqual(default(TItem)) && Source.Any())
                Result = Source.First();

            return Result;
        }

        /// <summary>
        /// Returns the first item of the supplied type for this collection.
        /// </summary>
        public static TFind GetFirstItemOfType<TItem, TFind>(this IEnumerable<TItem> TargetCollection)
                where TFind : class
        {
            return (TargetCollection.FirstOrDefault(item => item is TFind) as TFind);
        }

        //------------------------------------------------------------------------------------------
        /// <summary>
        /// Returns the first element of this supplied Source collection having the specified Name.
        /// </summary>
        public static TItem GetByName<TItem>(this IEnumerable<TItem> Source, string Name)
                where TItem : IIdentifiableElement
        {
            return Source.FirstOrDefault(item => item.Name == Name);
        }

        /// <summary>
        /// Returns the first element of this supplied Source collection having the specified Tech-Name.
        /// </summary>
        public static TItem GetByTechName<TItem>(this IEnumerable<TItem> Source, string TechName)
                where TItem : IIdentifiableElement
        {
            return Source.FirstOrDefault(item => item.TechName == TechName);
        }

        /// <summary>
        /// Returns the Name of the first element of this supplied Source collection having the specified Tech-Name.
        /// </summary>
        public static string GetNameByTechName<TItem>(this IEnumerable<TItem> Source, string TechName, string DefaultName = null)
                where TItem : IIdentifiableElement
        {
            var Element = Source.FirstOrDefault(item => item.TechName == TechName);
            return (Element == null ? DefaultName.NullDefault(TechName) : Element.Name);
        }

        //======================================================================================================================================================
        /// <summary>
        /// Comparer for two sets, each representing an instances of the given type.
        /// </summary>
        public static readonly ComparerForSets<IEnumerable> SetsComparer = new ComparerForSets<IEnumerable>();
        
        /// <summary>
        /// Provides a comparison method for two sets, each representing an instance of the given type.
        /// </summary>
        public class ComparerForSets<TSet> : IComparer<TSet> where TSet : IEnumerable
        {
            /// <summary>
            /// Evaluates field by field the two supplied set instances, and returns the first sorting-differentiation found or zero if both are equal.
            /// </summary>
            public int Compare(TSet x, TSet y)
            {
                var XEnumerator = x.GetEnumerator();
                var YEnumerator = y.GetEnumerator();
                bool ExistX = false, ExistY = false;
                int Comparison = 0;

                ExistX = XEnumerator.MoveNext();
                ExistY = YEnumerator.MoveNext();

                do
                {
                    if (ExistX && !ExistY)
                        return 1;

                    if (!ExistX && ExistY)
                        return -1;

                    if (ExistX && ExistY)
                    {
                        // Notice that for non IComparable based comparisons, the ToStringAsAlways() is used,
                        // hence interpreting NULL values as empty string.
                        if (XEnumerator.Current is IComparable)
                            Comparison = ((IComparable)XEnumerator.Current).CompareTo(YEnumerator.Current);
                        else
                            Comparison = XEnumerator.Current.ToStringAlways().CompareTo(YEnumerator.Current.ToStringAlways());

                        if (Comparison != 0)
                            return Comparison;

                        ExistX = XEnumerator.MoveNext();
                        ExistY = YEnumerator.MoveNext();
                    }
                }
                while (ExistX || ExistY);

                return 0;
            }
        }

        //======================================================================================================================================================
        /* IF NEEDED...
        public class TreeItem<TContent>
        {
            public TContent Content;
            public TreeItem<TContent> Parent;
            public IEnumerable<TreeItem<TContent>> Children;
        } */

        //======================================================================================================================================================
    }
}