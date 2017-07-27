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
// File   : EShellVisualContentType.cs
// Object : Instrumind.Common.Visualization.ECommandExpositorStyle (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2012.10.18 Néstor Sánchez A.  Creation
//

using System;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{

    /// <summary>
    /// Styles for exposing a command in a palette.
    /// </summary>
    public enum ECommandExpositorStyle : byte
    {
        /// <summary>
        /// Button.
        /// </summary>
        Button = ((byte)'B'),

        /// <summary>
        /// Combo-Box.
        /// </summary>
        ComboBox = ((byte)'C'),

        /// <summary>
        /// List-Box.
        /// </summary>
        ListBox = ((byte)'L'),

        /// <summary>
        /// List-View
        /// </summary>
        ListView = ((byte)'V')
    }
}