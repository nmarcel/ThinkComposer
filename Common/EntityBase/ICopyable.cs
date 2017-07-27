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
// File   : ICopyable.cs
// Object : Instrumind.Common.EntityBase.ICopyable (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2010.05.06 Néstor Sánchez A.  Creation
//

using System;

/// Provides a foundation of structures and services for business entities management, considering its life cycle, edition and persistence mapping.
namespace Instrumind.Common.EntityBase
{
    /// <summary>
    /// Represents an entity which can be copied/cloned.
    /// </summary>
    public interface ICopyable
    {
        /// <summary>
        /// Creates and returns a clone object of this source model instance, plus indicating the cloning-scope and direct-owner.
        /// </summary>
        object CreateCopy(ECloneOperationScope CloningScope, IMModelClass DirectOwner);
    }
}