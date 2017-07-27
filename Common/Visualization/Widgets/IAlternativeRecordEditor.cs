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
// File   : IAlternativeRecordEditor.cs
// Object : Instrumind.Common.Visualization.Widgets.IAlternativeRecordEditor (Interface)
//
// Date       Author             Changes
// ---------- ------------------ -------------------------------------------------------------
// 20yy.mm.dd Néstor Sánchez A.  Creation
//

using System;
using System.Windows;

using Instrumind.Common.EntityBase;

/// Library of standard Instrumind WPF custom and user controls.
namespace Instrumind.Common.Visualization.Widgets
{
    /// <summary>
    /// Represents a record editor alternative to a DataGridRow, such as a form style editor.
    /// </summary>
    public interface IAlternativeRecordEditor<TEntity, TItem>
               where TEntity : class, IModelEntity
    {
        /// <summary>
        /// Initializes this alternative editor from the passed source DataGrid.
        /// </summary>
        void InitializeFromGrid(ItemsGridMaintainer<TEntity, TItem> SourceGrid);

        /// <summary>
        /// Gets the visual-control exposing the alternative editor.
        /// </summary>
        UIElement VisualControl { get; }

    }
}