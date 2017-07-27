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
// File   : ESymbolManipulationAction.cs
// Object : Instrumind.ThinkComposer.Composer.ComposerUI.ESymbolManipulationAction (Enum)
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
    /// List the available actions to be performed while manipulating view symbols.
    /// </summary>
    public enum ESymbolManipulationAction : byte
    {
        /// <summary>
        /// Edit in-place the symbol title.
        /// </summary>
        [Description("Edit in-place the symbol title.")]
        EditInPlace = ((byte)'E'),

        /// <summary>
        /// Edit Marker assignment.
        /// </summary>
        [Description("Edit Marker assignment.")]
        MarkerEdit = ((byte)'K'),

        /// <summary>
        /// Access the detail content (for link goes to address, for attachment invokes the default associated application).
        /// </summary>
        [Description("Access the detail content (for link goes to address, for attachment invokes the default associated application, for tables presents the editing grid).")]
        IndividualDetailAccess = ((byte)'X'),

        /// <summary>
        /// Change the detail content.
        /// </summary>
        [Description("Change the detail content.")]
        IndividualDetailChange = ((byte)'H'),

        /// <summary>
        /// Designate the detail.
        /// </summary>
        [Description("Designate the detail.")]
        IndividualDetailDesignation = ((byte)'N'),

        /// <summary>
        /// Expand/Collapse an individual pointed detail.
        /// </summary>
        [Description("Expand/Collapse the pointed detail.")]
        SwitchIndividualDetail = ((byte)'W'),

        /// <summary>
        /// Move the symbol.
        /// </summary>
        [Description("Move the symbol. Press [Ctrl] to include Targets and [Shift] to include Origins. On simple Relationships [Alt-Right] locks position.")]
        Move = ((byte)'M'),

        /// <summary>
        /// Resize the symbol.
        /// </summary>
        [Description("Resize the symbol. Press [Alt-Left] to perform a symmetric resizing.")]
        Resize = ((byte)'S'),

        /// <summary>
        /// Open/Close the symbol Details.
        /// </summary>
        [Description("Open/Close the Idea Details.")]
        ActionSwitchDetails = ((byte)'D'),

        /// <summary>
        /// Expand/Collapse the symbol visually Related Targets/Origins.
        /// </summary>
        [Description("Expand/Collapse the symbol Related Targets (Press [Alt-Left] for the Origins).")]
        ActionSwitchRelated = ((byte)'R'),

        /// <summary>
        /// Show the symbol Composite-Content as View.
        /// </summary>
        [Description("Show the Idea Composite-Content as View.")]
        ActionShowCompositeAsView = ((byte)'C'),

        /// <summary>
        /// Show the Properties editor.
        /// </summary>
        [Description("Show the Properties editor.")]
        ActionEditProperties = ((byte)'P'),

        /// <summary>
        /// Display/Hide the Composite-Content View instead of Details.
        /// </summary>
        [Description("Display/Hide the Composite-Content View instead of Details.")]
        ActionShowCompositeAsDetail = ((byte)'V'),

        /// <summary>
        /// Add New Detail.
        /// </summary>
        [Description("Add New Detail.")]
        ActionAddDetail = ((byte)'A'),

        /// <summary>
        /// Go to Shortcut Target.
        /// </summary>
        [Description("Go to Shortcut Target.")]
        GoToShortcutTarget = ((byte)'G')
    }
}