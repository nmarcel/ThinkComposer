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
// File   : EExistenceStatus.cs
// Object : Instrumind.Common.EExistenceStatus (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.13 Néstor Sánchez A.  Creation
//

using System;

/// Library of general purpose components and features across Instrumind products.
namespace Instrumind.Common
{
    /// <summary>
    /// Possible existence status of a persistable object.
    /// </summary>
    public enum EExistenceStatus : byte
    {
        /// <summary>
        /// The object has never been persisted (is new).
        /// </summary>
        Created = ((byte)'C'),

        /// <summary>
        /// The already persistent object has not been modified (like just after read or save).
        /// </summary>
        NotModified = ((byte)'N'),

        /// <summary>
        /// The already persistent object has been modified (so, needs to be saved or discarded).
        /// </summary>
        Modified = ((byte)'M'),

        /// <summary>
        /// The already persistent object has been deleted (so, needs to be discarded).
        /// </summary>
        Deleted = ((byte)'D')
    }
}