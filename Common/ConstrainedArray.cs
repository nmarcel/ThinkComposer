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
// File   : ConstrainedArray.cs
// Object : Instrumind.Common.ConstrainedArray (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;
using System.Linq;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Fixed size array where new elements are added or overwrited on the oldest ones.
    /// </summary>
    /// <typeparam name="TValue">Type of the values to be stored.</typeparam>
    public class ConstrainedArray<TValue> : IEnumerable<TValue>
    {
        /// <summary>
        /// Internal store of the array.
        /// </summary>
        private TValue[] StoreArray = null;

        /// <summary>
        /// Current number of nodes of the stack.
        /// </summary>
        public int Count { get { return this.StoreArray.Length; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Size">Maximum number of elements to be stored.</param>
        public ConstrainedArray(int Size = 1)
        {
            this.StoreArray = new TValue[Size];
        }

        /// <summary>
        /// Adds the specified value to the array only if new to it.
        /// If no space left, then the oldest stored element is overwritten.
        /// </summary>
        public void AddNew(TValue Value)
        {
            if (!this.StoreArray.Contains(Value))
            {
                this.StoreArray[AddIndex] = Value;
                if (this.AddIndex == this.StoreArray.Length - 1)
                    this.AddIndex = 0;
                else
                    this.AddIndex++;
            }
        }

        private int AddIndex = 0;

        /// <summary>
        /// Discards all the nodes of the stack.
        /// </summary>
        public void Clear()
        {
            this.AddIndex = 0;
            this.StoreArray = new TValue[this.StoreArray.Length];
        }


        #region IEnumerable<TNode> Members

        public IEnumerator<TValue> GetEnumerator()
        {
            return this.StoreArray.Cast<TValue>().GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.StoreArray.GetEnumerator();
        }

        #endregion
    }
}