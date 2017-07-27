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
// File   : IDynamicStore.cs
// Object : Instrumind.Common.EntityBase.IDynamicStore (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.03.15 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Represents an object whose field values can be accessed dynamically.
    /// </summary>
    public interface IDynamicStore
    {
        /// <summary>
        /// Returns the value contained within the field with the specified Tech-Name.
        /// </summary>
        /// <returns></returns>
        object GetStoredValue(string FieldTechName);

        /// <summary>
        /// Sets the supplied Value into the field with the specified Tech-Name. Returns indication of success.
        /// </summary>
        bool SetStoredValue(string FieldTechName, object Value);
    }
}