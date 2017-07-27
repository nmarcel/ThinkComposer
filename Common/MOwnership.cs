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
// File   : Assignment.cs
// Object : Instrumind.Common.Assignment (Class)
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
    /// Base class for indicating ownership, Global/Shared or Local/Exclusive, for the consumer instance and referencing an owner instance which can be one of two possible types.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    [Serializable]
    public abstract class MOwnership
    {
        /// <summary>
        /// Object (non-specialized) reference to the assigned owner object.
        /// </summary>
        public IMModelClass Owner { get; protected set; }

        /// <summary>
        /// Indicates whether the ownership is Global/Shared, else is Local/Exclusive.
        /// </summary>
        public bool IsGlobal { get; protected set; }

        /// <summary>
        /// Creates and returns a clone of this Ownership.
        /// </summary>
        public abstract MOwnership CreateClone(IMModelClass NewOwner = null, bool? NewIsGlobal = null);
    }
}