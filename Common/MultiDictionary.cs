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
// File   : MultiDictionary.cs
// Object : Instrumind.Common.MultiDictionary (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.29 Néstor Sánchez A.  Creation
//

using System;
using System.Collections.Generic;

using Instrumind.Common;
using Instrumind.Common.EntityBase;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// A Dictionary containing another Dictionary on the value fields.
    /// </summary>
    /// <typeparam name="TMainKey">Type for the Key of the main container dictionary.</typeparam>
    /// <typeparam name="TSubKey">Type for the Key of the value contained dictionaries.</typeparam>
    /// <typeparam name="TSubValue">Type for the Value of the value contained dictionaries.</typeparam>
    [Serializable]
    public class MultiDictionary<TMainKey, TSubKey, TSubValue>
    {
        public readonly EditableDictionary<TMainKey, EditableDictionary<TSubKey, TSubValue>> Container = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MultiDictionary(string Name, IModelEntity VariatingInstance, int InitialCapacity = 0)
        {
            this.Container = new EditableDictionary<TMainKey, EditableDictionary<TSubKey, TSubValue>>(Name, VariatingInstance, InitialCapacity);
        }

        public TSubValue this[TMainKey MainKey, TSubKey SubKey]
        {
            get
            {
                return Container[MainKey][SubKey];
            }
            set
            {
                if (!this.Container.ContainsKey(MainKey))
                    this.Container.Add(MainKey, new EditableDictionary<TSubKey, TSubValue>(this.Container.Name + "." + MainKey.ToString(),
                                                                                           this.Container.VariatingInstance));

                this.Container[MainKey][SubKey] = value;
            }
        }
    }
}
