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
// File   : EComplementManipulationAction.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.EComplementManipulationAction (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.10.06 Néstor Sánchez A.  Creation
//

using System;
using System.ComponentModel;

/// Provides the user-interface components for the Composition Composer.
namespace Instrumind.ThinkComposer.Composer.ComposerUI
{
    /// <summary>
    /// List the available actions to be performed while manipulating view Complements.
    /// </summary>
    public enum EComplementManipulationAction : byte
    {
        /// <summary>
        /// Edit the Complement target.
        /// </summary>
        [Description("Edit the Complement target.")]
        Edit = ((byte)'E'),

        /// <summary>
        /// Move the Complement.
        /// </summary>
        [Description("Move the Complement. For Group Region/Line press [Alt] to not move the owner symbol.")]
        Move = ((byte)'M'),

        /// <summary>
        /// Resize the Complement.
        /// </summary>
        [Description("Resize the Complement. Press [Alt-Left] to perform a symmetric resizing.")]
        Resize = ((byte)'R')

    }
}