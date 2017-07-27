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
// 2010.01.09 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Base class for referencing an object value, or extraction-parameters tuple to it, which is either stored locally or shared with other instances, hence it depends (or is owned) externally.
    /// Of course, the stored object cannot be an extraction-parameters tuple.
    /// (Intended to work around problems by lack of support for contra/co+variance)
    /// </summary>
    [Serializable]
    public abstract class MAssignment
    {
        /// <summary>
        /// Indicates whether this instance owns the referenced Value.
        /// </summary>
        public bool IsLocal { get; protected set; }

        /// <summary>
        /// Object (non-specialized) reference to the assigned object which is local or external (shared).
        /// </summary>
        public abstract object AssignedValue { get; }

        /// <summary>
        /// Creates and returns a clone of this Assignment.
        /// </summary>
        public abstract MAssignment CreateClone(object NewAssignedValue = null, bool? NewIsLocal = null);
    }
}