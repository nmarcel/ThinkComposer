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
// File   : ECloneOperationScope.cs
// Object : Instrumind.Common.EntityBase.ECloneOperationScope (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.04.06 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Scopes to be considered when cloning an Entity.
    /// The cloning operation only can Clone classes implementing IModelClass/IMModelClass.
    /// </summary>
    public enum ECloneOperationScope : byte
    {
        /// <summary>
        /// The root instance, plus collection-heads will be cloned.
        /// All other references will point to the same original instances.
        /// </summary>
        Slight = ((byte)'S'),

        /// <summary>
        /// The root instance and its entity core-level graph will be cloned.
        /// Only entity core-level graph references will point to clone instances, beyond references will point to the same original instances.
        /// </summary>
        Core = ((byte)'C'),

        /// <summary>
        /// The complete root depending graph, now including the entity bulk-level, will be cloned.
        /// All GUIDs will be regenerated, to allow the creation of differentiable entity clones (do not use for equivalence comparison of pre-edit-change copies).
        /// All graph references will point to clone instances.
        /// </summary>
        Deep = ((byte)'D'),

        /// <summary>
        /// The complete root depending graph, now including the entity bulk-level, will be cloned.
        /// All GUIDs will remain the same, to allow creation of pre-edit-change copies, comparable by Global-Id equivalence (do not use for create new entities by cloning).
        /// All graph references will point to clone instances.
        /// </summary>
        DeepAndEquivalent = ((byte)'E')
    }
}