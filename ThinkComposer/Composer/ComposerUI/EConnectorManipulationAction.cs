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
// File   : EConnectorManipulationAction.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.EConnectorManipulationAction (Enum)
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
    /// List the available actions to be performed while manipulating view Connectors.
    /// </summary>
    public enum EConnectorManipulationAction : byte
    {
        /// <summary>
        /// Edit in-place the connector title.
        /// </summary>
        [Description("Edit in-place the connector title.")]
        EditInPlace = ((byte)'E'),

        /// <summary>
        /// Show Properties editor.
        /// </summary>
        [Description("Show Properties editor.")]
        EditProperties = ((byte)'P'),

        /// <summary>
        /// Displace the connector.
        /// </summary>
        [Description("Displace the connector.")]
        Displace = ((byte)'D'),

        /// <summary>
        /// Remove the connector.
        /// </summary>
        [Description("Remove the connector.")]
        Remove = ((byte)'R'),

        /// <summary>
        /// Straighten the connection line.
        /// </summary>
        [Description("Straighten the connection line.")]
        StraightenLine = ((byte)'F'),

        /// <summary>
        /// Cycle through available Variants for the target link.
        /// </summary>
        [Description("Cycle through available Variants for the target link.")]
        CycleThroughVariants = ((byte)'V'),

        /// <summary>
        /// Re-Link the connector to/from other symbol.
        /// </summary>
        [Description("Re-Link the connector to/from other symbol or allows precise connection (press [Ctrl] when not available by default).")]
        ReLink = ((byte)'L'),
    }
}