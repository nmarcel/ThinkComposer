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
// File   : Variation.cs
// Object : Instrumind.Common.EntityBase.Variation (Class)
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
    /// Editing change registered to be undone/redone in the future.
    /// Variations will be stored in the right order, either for undo or redo, by the EntityEditEngine.
    /// </summary>
    public abstract class Variation
    {
        /// <summary>
        /// Executes the registered change for the specified edit engine.
        /// </summary>
        public abstract void Execute(EntityEditEngine Engine);

    }
}