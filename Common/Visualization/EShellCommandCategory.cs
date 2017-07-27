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
// File   : EShellCommandCategory.cs
// Object : Instrumind.Common.Visualization.EShellCommandCategory (Enum)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2009.07.06 Néstor Sánchez A.  Creation
//

using System;

/// Specialized WPF components and features across Instrumind products.
namespace Instrumind.Common.Visualization
{
    /// <summary>
    /// Typifies the commands for allow different treatment of them.
    /// </summary>
    public enum EShellCommandCategory : byte
    {
        /// <summary>
        /// Document management related commands. Examples: "New", "Open", "Recent files...", etc.
        /// </summary>
        Document = ((byte)'D'),

        /// <summary>
        /// Document edition related commands. Examples: "Copy", "Paste", "Font", etc.
        /// </summary>
        Edition = ((byte)'E'),

        /// <summary>
        /// Global commands, such as application related ones. Examples: "Options", "Environment", etc.
        /// </summary>
        Global = ((byte)'G'),

        /// <summary>
        /// Extra commands, such as variable or non-essentials. Examples: "Help", "ABC Add-In", etc.
        /// </summary>
        Extra = ((byte)'X')
    }
}