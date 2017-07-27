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
// File   : IDynamicStoreDataGridEditor.cs
// Object : Instrumind.Common.Visualization.Widgets.IDynamicStoreDataGridEditor (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 2011.03.15 Néstor Sánchez A.  Creation
//

using System;

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Represents a control capable of editing a field of IDynamicStore objects which are items within a row of a DataGrid.
    /// </summary>
    public interface IDynamicStoreDataGridEditor
    {
        /// <summary>
        /// Name of the edited field.
        /// </summary>
        string StorageFieldName { get; set; }

        /// <summary>
        /// Indicates to do direct write and read operations to the IDynamicStore being edited in the current DataGridRow.
        /// </summary>
        bool ApplyDirectAccess { get; set; }
    }
}