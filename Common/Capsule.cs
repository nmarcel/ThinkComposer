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
// File   : Capsule.cs
// Object : Instrumind.Common.Capsule (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.07.14 Néstor Sánchez A.  Creation
//

using System;
using System.Collections;
using System.Text;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    /// Base class for Capsule generic classes.
    /// </summary>
    [Serializable]
    public abstract class Capsule
    {
        public abstract IEnumerable GetValues();

        public override string ToString()
        {
            var Text = new StringBuilder("Capsule{" + this.GetHashCode().ToString() + "}: ");

            int Index = 0;
            foreach (var Value in this.GetValues())
            {
                Text.Append("Value" + Index.ToString() + "=[" + Value.ToStringAlways() + "]"
                            + (Value == null ? General.STR_NULLREF : Value.GetHashCode().ToString()) + "/ ");
                Index++;
            }

            return Text.ToString();
        }

        /// <summary>
        /// Creates and returns a Capsule, which stores a set of 1 generic values that can be later changed.
        /// Similar to Tuple, but as reference-type and mutable members.
        /// Its items are zero-based indexed and are enumerable.
        /// </summary>
        public static Capsule<T0> Create<T0>(T0 Value0 = default(T0))
        {
            return (new Capsule<T0>(Value0));
        }

        /// <summary>
        /// Creates and returns a Capsule, which stores a set of 2 generic values that can be later changed.
        /// Similar to Tuple, but as reference-type and mutable members.
        /// Its items are zero-based indexed and are enumerable.
        /// </summary>
        public static Capsule<T0, T1> Create<T0, T1>(T0 Value0 = default(T0), T1 Value1 = default(T1))
        {
            return (new Capsule<T0, T1>(Value0, Value1));
        }

        /// <summary>
        /// Creates and returns a Capsule, which stores a set of 3 generic values that can be later changed.
        /// Similar to Tuple, but as reference-type and mutable members.
        /// Its items are zero-based indexed and are enumerable.
        /// </summary>
        public static Capsule<T0, T1, T2> Create<T0, T1, T2>(T0 Value0 = default(T0), T1 Value1 = default(T1), T2 Value2 = default(T2))
        {
            return (new Capsule<T0, T1, T2>(Value0, Value1, Value2));
        }

        /// <summary>
        /// Creates and returns a Capsule, which stores a set of 4 generic values that can be later changed.
        /// Similar to Tuple, but as reference-type and mutable members.
        /// Its items are zero-based indexed and are enumerable.
        /// </summary>
        public static Capsule<T0, T1, T2, T3> Create<T0, T1, T2, T3>(T0 Value0 = default(T0), T1 Value1 = default(T1), T2 Value2 = default(T2), T3 Value3 = default(T3))
        {
            return (new Capsule<T0, T1, T2, T3>(Value0, Value1, Value2, Value3));
        }
    }

    /// <summary>
    /// Stores a set of 1 generic values that can be later changed.
    /// Similar to Tuple, but as reference-type and mutable members.
    /// Its items are zero-based indexed and are enumerable.
    /// </summary>
    [Serializable]
    public class Capsule<T0> : Capsule
    {
        public Capsule(T0 Value0 = default(T0))
        {
            this.Value0 = Value0;
        }

        public T0 Value0;

        public override IEnumerable GetValues()
        {
            yield return this.Value0;
        }
    }

    // ---------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores a set of 2 generic values that can be later changed.
    /// Similar to Tuple, but as reference-type and mutable members.
    /// Its items are zero-based indexed and are enumerable.
    /// </summary>
    [Serializable]
    public class Capsule<T0, T1> : Capsule
    {
        public Capsule(T0 Value0 = default(T0), T1 Value1 = default(T1))
        {
            this.Value0 = Value0;
            this.Value1 = Value1;
        }

        public T0 Value0;
        public T1 Value1;

        public override IEnumerable GetValues()
        {
            yield return this.Value0;
            yield return this.Value1;
        }
    }

    // ---------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores a set of 3 generic values that can be later changed.
    /// Similar to Tuple, but as reference-type and mutable members.
    /// Its items are zero-based indexed and are enumerable.
    /// </summary>
    [Serializable]
    public class Capsule<T0, T1, T2> : Capsule
    {
        public Capsule(T0 Value0 = default(T0), T1 Value1 = default(T1), T2 Value2 = default(T2))
        {
            this.Value0 = Value0;
            this.Value1 = Value1;
            this.Value2 = Value2;
        }

        public T0 Value0;
        public T1 Value1;
        public T2 Value2;

        public override IEnumerable GetValues()
        {
            yield return this.Value0;
            yield return this.Value1;
            yield return this.Value2;
        }
    }

    // ---------------------------------------------------------------------------------------------
    /// <summary>
    /// Stores a set of 4 generic values that can be later changed.
    /// Similar to Tuple, but as reference-type and mutable members.
    /// Its items are zero-based indexed and are enumerable.
    /// </summary>
    [Serializable]
    public class Capsule<T0, T1, T2, T3> : Capsule
    {
        public Capsule(T0 Value0 = default(T0), T1 Value1 = default(T1), T2 Value2 = default(T2), T3 Value3 = default(T3))
        {
            this.Value0 = Value0;
            this.Value1 = Value1;
            this.Value2 = Value2;
            this.Value3 = Value3;
        }

        public T0 Value0;
        public T1 Value1;
        public T2 Value2;
        public T3 Value3;

        public override IEnumerable GetValues()
        {
            yield return this.Value0;
            yield return this.Value1;
            yield return this.Value2;
            yield return this.Value3;
        }
    }
}