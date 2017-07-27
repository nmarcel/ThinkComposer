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
// File   : Ownership.cs
// Object : Instrumind.Common.Ownership (Class)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.01.22 Néstor Sánchez A.  Creation
//

using System;

using Instrumind.Common.EntityBase;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Static methods for create Ownership objects, which:
    /// Indicates ownership, Global/Global or Local/Local, for the consumer instance and references an owner instance which can be one of two possible types.
    /// </summary>
    public static class Ownership
    {
        /// <summary>
        /// Creates a new Ownership object, with the supplied Owner and scope indication.
        /// In this case the Global and Local owner types are the same.
        /// </summary>
        public static Ownership<TOwner, TOwner> Create<TOwner>(TOwner Owner, bool IsGlobal)
                where TOwner : IMModelClass
        {
            return (new Ownership<TOwner, TOwner>(Owner, IsGlobal));
        }

        /// <summary>
        /// Creates a new Ownership object, with the supplied Owner of the Global type.
        /// </summary>
        public static Ownership<TGlobal, TLocal> Create<TGlobal, TLocal>(TGlobal GlobalOwner)
            where TGlobal : IMModelClass
            where TLocal : IMModelClass
        {
            if (typeof(TGlobal) == typeof(TLocal))
                throw new UsageAnomaly("Cannot use this method to create a Ownership object when both types, Global and Local, are the same.");

            return (new Ownership<TGlobal, TLocal>(GlobalOwner, true));
        }

        /// <summary>
        /// Creates a new Ownership object, with the supplied Owner of the Local type.
        /// </summary>
        public static Ownership<TGlobal, TLocal> Create<TGlobal, TLocal>(TLocal LocalOwner)
            where TGlobal : IMModelClass
            where TLocal : IMModelClass
        {
            if (typeof(TGlobal) == typeof(TLocal))
                throw new UsageAnomaly("Cannot use this method to create a Ownership object when both types, Global and Local, are the same.");

            return (new Ownership<TGlobal, TLocal>(LocalOwner, false));
        }
    }

    /// <summary>
    /// Indicates ownership, Global or Local, for the consumer instance and references an owner instance which can be based on one of two possible types.
    /// </summary>
    [Serializable]
    public class Ownership<TGlobal, TLocal> : MOwnership
            where TGlobal : IMModelClass
            where TLocal : IMModelClass
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        internal Ownership(IMModelClass Owner, bool IsGlobal = true)
        {
            var TypeGlobal = typeof(TGlobal);
            var TypeLocal = typeof(TLocal);

            if (!Owner.GetType().InheritsFrom(TypeGlobal) &&
                !Owner.GetType().InheritsFrom(TypeLocal))
                throw new UsageAnomaly("Owner must be of (Global) type '" + typeof(TGlobal).ToString() + "' or (Local) type '" + typeof(TLocal).ToString() + "'", Owner);

            this.Owner = Owner;
            this.IsGlobal = IsGlobal;
        }

        /// <summary>
        /// Gets the owner as global (or its default if the owner is not global).
        /// </summary>
        public TGlobal OwnerGlobal
        {
            get { return (this.IsGlobal ? (TGlobal)this.Owner : default(TGlobal));  }
            set
            {
                this.Owner = value;
                this.IsGlobal = true;
            }
        }

        /// <summary>
        /// Gets the owner as local (or its default if the owner is global).
        /// </summary>
        public TLocal OwnerLocal
        {
            get { return (!this.IsGlobal ? (TLocal)this.Owner : default(TLocal)); }
            set
            {
                this.Owner = value;
                this.IsGlobal = false;
            }
        }

        /// <summary>
        /// Gets the referenced owner object.
        /// </summary>
        public TOwner GetOwner<TOwner>()
        {
            if (!typeof(TOwner).IsOneOf(typeof(TGlobal), typeof(TLocal)))
                throw new UsageAnomaly("Owner must be of (Global) type '" + typeof(TGlobal).ToString() + "' or (Local) type '" + typeof(TLocal).ToString() + "'", Owner);

            return (TOwner)Owner;
        }

        /// <summary>
        /// Gets a value from the underlying owner, using one of two supplied functions, where the applied one is that matching the ownership kind (Global or Local).
        /// </summary>
        public TValue GetValue<TValue>(Func<TGlobal, TValue> ExtractorFromGlobal, Func<TLocal, TValue> ExtractorFromLocal)
        {
            if (this.IsGlobal)
                return ExtractorFromGlobal((TGlobal)this.Owner);

            return ExtractorFromLocal((TLocal)this.Owner);
        }

        /// <summary>
        /// Creates and returns a clone of this Ownership.
        /// </summary>
        public override MOwnership CreateClone(IMModelClass NewOwner = null, bool? NewIsGlobal = null)
        {
            var Result = new Ownership<TGlobal, TLocal>(NewOwner.NullDefault(this.Owner),
                                                        (NewIsGlobal == null || !NewIsGlobal.HasValue
                                                         ? this.IsGlobal : NewIsGlobal.Value));

            return Result;
        }
        
        public override string ToString()
        {
            return "Owner (" + (this.IsGlobal ? "Global" : "Local") + ", HC=" + this.GetHashCode().ToString() + "): " + this.Owner.ToStringAlways();
        }
    }
}