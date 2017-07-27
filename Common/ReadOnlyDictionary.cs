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
// File   : ReadOnlyDictionary.cs
// Object : Instrumind.Common.ReadOnlyDictionary (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.06.30 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Read-only wrapper for a generic Dictionary.
    /// The read-only constraint applies to the containing collection, not to the keys or items contained.
    /// </summary>
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private IDictionary<TKey, TValue> TargetDictionary = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> TargetDictionary)
        {
            this.TargetDictionary = TargetDictionary;
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            throw new UsageAnomaly("Invaild operation for RestrictedDictionary, because it is read-only.");
        }

        public bool ContainsKey(TKey key)
        {
            return this.TargetDictionary.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return this.TargetDictionary.Keys; }
        }

        public bool Remove(TKey key)
        {
            throw new UsageAnomaly("Invaild operation for RestrictedDictionary, because it is read-only.");
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this.TargetDictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return this.TargetDictionary.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.TargetDictionary[key];
            }
            set
            {
                throw new UsageAnomaly("Invaild operation for RestrictedDictionary, because it is read-only.");
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new UsageAnomaly("Invaild operation for RestrictedDictionary, because it is read-only.");
        }

        public void Clear()
        {
            throw new UsageAnomaly("Invaild operation for RestrictedDictionary, because it is read-only.");
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)this.TargetDictionary).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)this.TargetDictionary).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return ((ICollection<KeyValuePair<TKey, TValue>>)this.TargetDictionary).Count; }
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new UsageAnomaly("Invaild operation for RestrictedDictionary, because it is read-only.");
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.TargetDictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.TargetDictionary.GetEnumerator();
        }

        #endregion
    }
}