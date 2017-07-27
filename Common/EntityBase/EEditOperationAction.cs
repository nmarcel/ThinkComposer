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
// File   : EEditOperationAction.cs
// Object : Instrumind.Common.EntityBase.EEditOperationAction (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.21 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Execution senses for editing operation actions.
    /// </summary>
    public enum EEditOperationAction : byte
    {
        /// <summary>
        /// Action for apply an operation.
        /// </summary>
        Apply = ((byte)'A'),

        /// <summary>
        /// Action for revert an operation.
        /// </summary>
        Revert = ((byte)'R')
    }
}