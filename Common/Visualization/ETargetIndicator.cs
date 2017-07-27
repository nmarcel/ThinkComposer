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
// File   : ECursorIndicator.cs
// Object : Instrumind.Common.Visualization.ECursorIndicator (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.09.13 Néstor Sánchez A.  Creation
//

using System;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Indicator for target of cursor about current action.
    /// </summary>
    public enum ETargetIndicator : byte
    {
        /// <summary>
        /// The target is just being normally pointed.
        /// </summary>
        Point = ((byte)'P'),

        /// <summary>
        /// The target will be selected.
        /// </summary>
        Select = ((byte)'S'),

        /// <summary>
        /// The target will be modified.
        /// </summary>
        Modify = ((byte)'C'),

        /// <summary>
        /// The target will be deleted.
        /// </summary>
        Delete = ((byte)'D')
    }
}